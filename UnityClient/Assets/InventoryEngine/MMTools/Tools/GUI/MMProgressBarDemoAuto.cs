using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.Tools
{	
	public class MMProgressBarDemoAuto : MonoBehaviour 
	{
        public enum TestModes { Permanent, OneTime }
        public TestModes TestMode = TestModes.Permanent;

        [MMEnumCondition("TestMode", (int)TestModes.Permanent)]
		public float CurrentValue = 0f;
        [MMEnumCondition("TestMode", (int)TestModes.Permanent)]
        public float MinValue = 0f;
        [MMEnumCondition("TestMode", (int)TestModes.Permanent)]
        public float MaxValue = 100f;
        [MMEnumCondition("TestMode", (int)TestModes.Permanent)]
        public float Speed = 1f;

        [MMEnumCondition("TestMode", (int)TestModes.OneTime)]
        public float OneTimeNewValue;
        [MMEnumCondition("TestMode", (int)TestModes.OneTime)]
        public float OneTimeMinValue;
        [MMEnumCondition("TestMode", (int)TestModes.OneTime)]
        public float OneTimeMaxValue;
        [MMEnumCondition("TestMode", (int)TestModes.OneTime)]
        [MMInspectorButton("OneTime")]
        public bool OneTimeButton;

        protected float _direction = 1f;
		protected MMProgressBar _progressBar;

		protected virtual void Start()
		{
			Initialization ();
		}

		protected virtual void Initialization()
		{
			_progressBar = GetComponent<MMProgressBar> ();
		}

		protected virtual void Update()
		{
            if (TestMode == TestModes.Permanent)
            {
                _progressBar.UpdateBar(CurrentValue, MinValue, MaxValue);
                CurrentValue += Speed * Time.deltaTime * _direction;
                if ((CurrentValue <= MinValue) || (CurrentValue >= MaxValue))
                {
                    _direction *= -1;
                }
            }
		}

        protected virtual void OneTime()
        {
            _progressBar.UpdateBar(OneTimeNewValue, OneTimeMinValue, OneTimeMaxValue);
        }
	}
}
