using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Unity.Properties.Tests
{
    public class TestRequires_ENABLE_IL2CPP : Attribute, ITestAction
    {
        public ActionTargets Targets { get; }
        
        readonly string m_Reason;
        
        public TestRequires_ENABLE_IL2CPP(string reason)
        {
            m_Reason = reason;
        }
        
        public void BeforeTest(ITest test)
        {
#if !ENABLE_IL2CPP
            Assert.Ignore($"Test requires Define=[ENABLE_IL2CPP] Reason=[{m_Reason}]");
#endif
        }

        public void AfterTest(ITest test)
        {
            
        }
    }
    
    public class TestRequires_NET_4_6 : Attribute, ITestAction
    {
        public ActionTargets Targets { get; }

        readonly string m_Reason;
        
        public TestRequires_NET_4_6(string reason)
        {
            m_Reason = reason;
        }
        
        public void BeforeTest(ITest test)
        {
#if !NET_4_6
            Assert.Ignore($"Test requires Define=[NET_4_6] Reason=[{m_Reason}]");
#endif
        }

        public void AfterTest(ITest test)
        {
            
        }
    }
}