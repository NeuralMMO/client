using System.Linq;
using UnityEngine;

namespace Unity.Build.Tests
{
    public class ResultContainer : ScriptableObject
    {
        [SerializeField]
        private string[] m_Results;

        [SerializeField]
        private bool m_Completed;

        public string[] Results
        {
            get => m_Results;
            set => m_Results = value;
        }

        public bool Completed
        {
            get => m_Completed;
            set => m_Completed = value;
        }

        private string GetMessage(BuildResult result)
        {
            var msg = result.Succeeded ? "Success" : "Fail";
            return $"{result.BuildConfiguration.name}, {msg}";
        }
        public void SetCompleted(BuildResult[] results)
        {
            m_Results = results.Select(r => GetMessage(r)).ToArray();
            m_Completed = true;
        }
    }
}
