using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

#if UNITY_DOTSPLAYER
namespace Unity.Entities.CodeGen
{
    /// <summary> Generates code need to run unit tests in ILPP targets without reflection.</summary>
    /// <remarks>
    /// The NUnit test runner runs DOTS-Runtime in the full dotnet framework in order to use reflection
    /// to find and run tests. In any IL2CPP build (windows, mac, mobile, or web) this straight up doesn't
    /// work since there isn't full dotnet support, only the minimal profile.
    ///
    /// This PostProcessor will detect a test framework assembly, scan it, and code-gen calls to test
    /// cases. It is compatible with NUnit, although it only implements a subset of the NUnit functionality.
    ///
    /// </remarks>

    class TestCaseILPP : EntitiesILPostProcessor
    {
        MethodDefinition m_TestRunner;
        MethodReference m_WriteLine;
        FieldDefinition m_TestsRanFld;
        FieldDefinition m_TestsIgnoredFld;
        FieldDefinition m_TestsSkippedFld;
        FieldDefinition m_TestsUnsupportedFld;
        FieldDefinition m_TestsPartiallySupportedFld;

        enum TestStatus
        {
            Okay,
            Ignored,            // [Ignored] in the code, equivalent to skipped.
            Limitation,         // Limitation of test suite.
            NotSupported,       // Test uses unsupported, not cross-platform feature.
            PartiallySupported  // Test case can be run, but not all the asserts in the test case are executed.
        }

        protected override bool PostProcessImpl(TypeDefinition[] componentSystemTypes)
        {
            m_TestRunner = FindCallerMethod();
            if (m_TestRunner == null)
                return false;

            m_WriteLine = AssemblyDefinition.MainModule.ImportReference(typeof(Console).GetMethod("WriteLine", new[] {typeof(string)}));

            // Initially set up the caller.
            m_TestRunner.Body.Instructions.Clear();
            m_TestRunner.Body.InitLocals = true;

            try
            {
                var nUnit = AssemblyDefinition.MainModule.Types.First(t => t.FullName == "NUnit.Framework.Assert");
                m_TestsRanFld = nUnit.Fields.First(f => f.Name == "testsRan");
                m_TestsIgnoredFld = nUnit.Fields.First(f => f.Name == "testsIgnored");
                m_TestsSkippedFld = nUnit.Fields.First(f => f.Name == "testsLimitation");
                m_TestsUnsupportedFld = nUnit.Fields.First(f => f.Name == "testsNotSupported");
                m_TestsPartiallySupportedFld = nUnit.Fields.First(f => f.Name == "testsPartiallySupported");
            }
            catch
            {
                Console.WriteLine($"Failed to find required fields (testsRan, testsIgnored, etc.) of NUnitFrameWork.Assert in the runtime package.");
            }

            foreach (var t in AssemblyDefinition.MainModule.Types)
            {
                if (t.IsClass)
                    ProcessClass(t);
            }

            ILProcessor il = m_TestRunner.Body.GetILProcessor();
            il.Emit(OpCodes.Ret);
            return true;
        }

        protected override bool PostProcessUnmanagedImpl(TypeDefinition[] unmanagedComponentSystemTypes)
        {
            return false;
        }

        MethodDefinition FindCallerMethod()
        {
            TypeDefinition runner = AssemblyDefinition.MainModule.Types.FirstOrDefault(t => t.FullName == "NUnit.Framework.UnitTestRunner");
            MethodDefinition caller = runner?.Methods.First(m => m.Name == "Run");
            return caller;
        }

        static bool HasCustomAttribute(MethodDefinition m, string attributeName)
        {
            var fullAttrName = attributeName + "Attribute";
            var attributes = m.Resolve().CustomAttributes;
            return attributes.FirstOrDefault(ca =>
                ca.AttributeType.FullName == attributeName || ca.AttributeType.FullName == fullAttrName) != null;
        }

        int ProcessClass(TypeDefinition clss)
        {
            int testCount = 0;
            List<MethodDefinition> setup = null;
            List<MethodDefinition> teardown = null;
            VariableDefinition classVar = null;

            foreach (var m in clss.Methods)
            {
                if (HasCustomAttribute(m, "NUnit.Framework.Test"))
                {
                    ++testCount;
                    if (setup == null)
                    {
                        setup = FindSetupTeardown(true, clss);
                        teardown = FindSetupTeardown(false, clss);
                        classVar = new VariableDefinition(AssemblyDefinition.MainModule.ImportReference(clss));
                        m_TestRunner.Body.Variables.Add(classVar);
                    }

                    EmitTestCall(clss, m, classVar, setup, teardown);
                }
            }

            return testCount;
        }

        // Walks the code to look for [NotSupported]/[PartiallySupported] method calls.
        // Recursive, but some of the Asserts are "buried deep" and hard to find,
        // so the use of Ignore may still be required.
        bool HasTaggedCodeRecursive(MethodReference method, out string msg, HashSet<string> visited, int recDepth, string attr)
        {
            msg = "";

            if (recDepth >= 3)
                return false;

            MethodDefinition methodDefinition = method.Resolve();
            if (methodDefinition == null || methodDefinition.Body == null)
                return false;

            foreach (var bc in methodDefinition.Body.Instructions)
            {
                if (bc.OpCode == OpCodes.Call)
                {
                    MethodReference mr = (MethodReference)bc.Operand;
                    MethodDefinition md = mr.Resolve();

                    if (md != null)
                    {
                        CustomAttribute ca = GetAttributeByFullName(md, attr, out msg);
                        if (ca != null)
                        {
                            return true;
                        }

                        // Limit the search to this module; this search doesn't always work (and in [Ignore] needs
                        // to be manually added) so there's benefit to be faster if only mostly correct.
                        if (method.Module == mr.Module && !visited.Contains(mr.FullName))
                        {
                            visited.Add(mr.FullName);
                            bool result = HasTaggedCodeRecursive(mr, out msg, visited, recDepth + 1, attr);
                            if (result)
                                return true;
                        }
                    }
                }
            }
            return false;
        }

        CustomAttribute GetNotSupportedAttribute(MethodDefinition method, out string msg)
        {
            return GetAttributeByFullName(method, "NUnit.Framework.NotSupportedAttribute", out msg);
        }

        CustomAttribute GetAttributeByFullName(MethodDefinition method, string attr, out string msg)
        {
            msg = "";
            if (method.HasCustomAttributes)
            {
                CustomAttribute ca = method.CustomAttributes.FirstOrDefault(a => a.AttributeType.FullName == attr);
                if (ca != null)
                {
                    msg = (string)ca.ConstructorArguments[0].Value;
                    return ca;
                }
            }

            return null;
        }

        bool HasNotSupportedCode(MethodDefinition method, out string msg)
        {
            msg = "";
            HashSet<string> visited = new HashSet<string>();
            return HasTaggedCodeRecursive(method, out msg, visited, 0, "NUnit.Framework.NotSupportedAttribute");
        }

        bool HasPartiallySupportedCode(MethodDefinition method, out string msg)
        {
            msg = "";
            HashSet<string> visited = new HashSet<string>();
            return HasTaggedCodeRecursive(method, out msg, visited, 0, "NUnit.Framework.PartiallySupportedAttribute");
        }

        bool IsIgnored(MethodDefinition method)
        {
            foreach (var attr in method.CustomAttributes)
            {
                var type = attr.AttributeType;
                while (type != null)
                {
                    if (type.Name == "IgnoreAttribute")
                        return true;

                    // TODO: may choose to support in the future.
                    // But for now - since there is no command line interface yet - same as [Ignore]
                    if (type.Name == "ExplicitAttribute")
                        return true;

                    type = type.Resolve().BaseType;
                }
            }

            return false;
        }

        void EmitIncStaticFld(ILProcessor il, FieldDefinition fld)
        {
            il.Emit(OpCodes.Ldsfld, fld);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Stsfld, fld);
        }

        void EmitTestCall(TypeDefinition clss, MethodDefinition testMethod, VariableDefinition classLocal, List<MethodDefinition> setup, List<MethodDefinition> teardown)
        {
            if (testMethod.ReturnType.MetadataType != MetadataType.Void)
                throw new Exception($"Test case '{testMethod.FullName}' has non-void return type.");

            var ctor = clss.Methods.FirstOrDefault(m => m.Name == ".ctor" && m.Parameters.Count == 0);
            if (ctor == null)
                throw new Exception($"Test class '{clss.FullName}' doesn't have a default constructor.");

            var il = m_TestRunner.Body.GetILProcessor();

            string skipMsg = null;
            string msg = "";

            TestStatus status = TestStatus.Okay;
            if (testMethod.Parameters.Count > 0)
            {
                skipMsg = "(TODO) Test method has input parameters.";
                status = TestStatus.Limitation;
            }
            else if (GetNotSupportedAttribute(testMethod, out msg) != null)
            {
                skipMsg = msg;
                status = TestStatus.NotSupported;
            }
            else if (HasNotSupportedCode(testMethod, out msg))
            {
                skipMsg = msg;
                status = TestStatus.NotSupported;
            }
            else if (IsIgnored(testMethod))
            {
                skipMsg = "";
                status = TestStatus.Ignored;
            }
            else if (HasPartiallySupportedCode(testMethod, out msg))
            {
                // Make sure "partial" is the last check - check first that we aren't unsupported, etc.
                skipMsg = msg;
                status = TestStatus.PartiallySupported;
            }

            switch (status)
            {
                case TestStatus.Limitation:
                    il.Emit(OpCodes.Ldstr, $"[Limitation]   '{testMethod.FullName}' {skipMsg}");
                    il.Emit(OpCodes.Call, m_WriteLine);
                    EmitIncStaticFld(il, m_TestsSkippedFld);
                    return;

                case TestStatus.NotSupported:
                    il.Emit(OpCodes.Ldstr, $"[NotSupported] '{testMethod.FullName}' {skipMsg}");
                    il.Emit(OpCodes.Call, m_WriteLine);
                    EmitIncStaticFld(il, m_TestsUnsupportedFld);
                    return;

                case TestStatus.Ignored:
                    il.Emit(OpCodes.Ldstr, $"[Ignored]      '{testMethod.FullName}' {skipMsg}");
                    il.Emit(OpCodes.Call, m_WriteLine);
                    EmitIncStaticFld(il, m_TestsIgnoredFld);
                    return;

                case TestStatus.PartiallySupported:
                    EmitIncStaticFld(il, m_TestsPartiallySupportedFld);
                    break;

                default:
                    break;
            }

            const int RETURN_HEADER_LEN = 12;
            string logMsg = testMethod.FullName.Substring(RETURN_HEADER_LEN);

            if (status == TestStatus.PartiallySupported)
                logMsg = "[Partial]      " + logMsg + " " + skipMsg;

            il.Emit(OpCodes.Ldstr, logMsg);

            il.Emit(OpCodes.Call, m_WriteLine);
            il.Emit(OpCodes.Newobj, ctor);
            il.Emit(OpCodes.Stloc, classLocal);

            foreach (var setupCall in setup)
            {
                il.Emit(OpCodes.Ldloc, classLocal);
                il.Emit(OpCodes.Callvirt, setupCall);
            }
            il.Emit(OpCodes.Ldloc, classLocal);
            il.Emit(OpCodes.Callvirt, testMethod);
            foreach (var teardownCall in teardown)
            {
                il.Emit(OpCodes.Ldloc, classLocal);
                il.Emit(OpCodes.Callvirt, teardownCall);
            }
            EmitIncStaticFld(il, m_TestsRanFld);
        }

        List<MethodDefinition> FindSetupTeardown(bool setup, TypeDefinition clss)
        {
            List<MethodDefinition> list = new List<MethodDefinition>();

            while (clss != null)
            {
                bool found = false;
                foreach (var method in clss.Methods)
                {
                    if (HasCustomAttribute(method, setup ? "NUnit.Framework.SetUp" : "NUnit.Framework.TearDown"))
                    {
                        if (found)
                            throw new Exception($"Cross platform test runner can't process multiple [SetUp] on ${method.FullName}");
                        found = true;
                        list.Add(method);
                    }
                }

                clss = clss.BaseType?.Resolve();
            }

            if (setup)
            {
                // SetUp: Base first, then derived.
                // TearDown: Derived first, then base
                list.Reverse();
            }

            return list;
        }
    }
}

#endif // UNITY_DOTSPLAYER
