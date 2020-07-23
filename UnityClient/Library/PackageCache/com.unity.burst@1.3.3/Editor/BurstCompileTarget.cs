using System;
using System.Reflection;
using System.Text;

namespace Unity.Burst.Editor
{
    internal class BurstCompileTarget
    {
        public BurstCompileTarget(MethodInfo method, Type jobType, Type interfaceType, bool isStaticMethod)
        {
            Method = method ?? throw new ArgumentNullException(nameof(method));
            JobType = jobType ?? throw new ArgumentNullException(nameof(jobType));
            JobInterfaceType = interfaceType; // can be null
            // This is important to clone the options as we don't want to modify the global instance
            Options = BurstCompiler.Options.Clone();
            Options.EnableBurstCompilation = true;
            // Enable safety checks by default to match inspector default behavior
            Options.EnableBurstSafetyChecks = true;
            TargetCpu = TargetCpu.Auto;
            // The BurstCompilerAttribute can be either on the type or on the method
            IsStaticMethod = isStaticMethod;
        }

        /// <summary>
        /// <c>true</c> if the <see cref="Method"/> is directly tagged with a [BurstCompile] attribute
        /// </summary>
        public readonly bool IsStaticMethod;

        /// <summary>
        /// The Execute method of the target's producer type.
        /// </summary>
        public readonly MethodInfo Method;

        /// <summary>
        /// The type of the actual job (i.e. BoidsSimulationJob).
        /// </summary>
        public readonly Type JobType;

        /// <summary>
        /// The interface of the job (IJob, IJobParallelFor...)
        /// </summary>
        public readonly Type JobInterfaceType;

        /// <summary>
        /// The default compiler options
        /// </summary>
        public readonly BurstCompilerOptions Options;

        public TargetCpu TargetCpu { get; set; }

        /// <summary>
        /// Set to true if burst compilation is actually requested via proper `[BurstCompile]` attribute:
        /// - On the job if it is a job only
        /// - On the method and parent class it if is a static method
        /// </summary>
        public bool HasRequiredBurstCompileAttributes => BurstCompilerOptions.HasBurstCompileAttribute(JobType) && (!IsStaticMethod || BurstCompilerOptions.HasBurstCompileAttribute(Method));

        /// <summary>
        /// Generated raw disassembly (IR, IL, ASM...), or null if disassembly failed (only valid for the current TargetCpu)
        /// </summary>
        public string RawDisassembly;

        /// <summary>
        /// Formatted disassembly for the associated <see cref="RawDisassembly"/>, currently only valid for <see cref="Unity.Burst.Editor.DisassemblyKind.Asm"/>
        /// </summary>
        public string FormattedDisassembly;

        public DisassemblyKind DisassemblyKind;

        public bool IsDarkMode { get; set; }

        public string GetDisplayName()
        {
            var displayName = IsStaticMethod ? Pretty(Method) : $"{Pretty(JobType)} - ({Pretty(JobInterfaceType)})";

            // Remove the '<>c__DisplayClass_' part of the name - this is only added for C# Entities.ForEach jobs to trick the C# debugging tools into
            // treating them like lambdas. This is removed wherever possible from user facing tools (like the Unity profiler), so we should do the same.
            return displayName.Replace("<>c__DisplayClass_", "");
        }

        private static string Pretty(MethodInfo method)
        {
            var builder = new StringBuilder();
            builder.Append(Pretty(method.DeclaringType));
            builder.Append(".");
            builder.Append(method.Name);
            builder.Append("(");
            var parameters = method.GetParameters();
            for (var i = 0; i < parameters.Length; i++)
            {
                var param = parameters[i];
                if (i > 0) builder.Append(", ");
                builder.Append(Pretty(param.ParameterType));
            }

            builder.Append(")");
            return builder.ToString();
        }

        private static string Pretty(Type type)
        {
            if (type == typeof(bool))
            {
                return "bool";
            }
            if (type == typeof(int))
            {
                return "int";
            }
            if (type == typeof(long))
            {
                return "long";
            }
            if (type == typeof(uint))
            {
                return "uint";
            }
            if (type == typeof(ulong))
            {
                return "ulong";
            }
            if (type == typeof(short))
            {
                return "short";
            }
            if (type == typeof(ushort))
            {
                return "ushort";
            }
            if (type == typeof(byte))
            {
                return "byte";
            }
            if (type == typeof(sbyte))
            {
                return "sbyte";
            }
            if (type == typeof(float))
            {
                return "float";
            }
            if (type == typeof(double))
            {
                return "double";
            }
            if (type == typeof(string))
            {
                return "string";
            }
            if (type == typeof(object))
            {
                return "object";
            }
            if (type == typeof(char))
            {
                return "char";
            }

            // When displaying job interface type, display the interface name of Unity.Jobs namespace
            var typeName = type.IsInterface && type.Name.StartsWith("IJob") ? type.Name : type.ToString();
            return typeName.Replace("+", ".");
        }
    }
}
