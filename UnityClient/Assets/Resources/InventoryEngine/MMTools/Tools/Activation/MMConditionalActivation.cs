using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;

namespace MoreMountains.Tools
{
    public class MMConditionalActivation : MonoBehaviour
    {
        public MonoBehaviour[] EnableThese;
        public MonoBehaviour[] AfterTheseAreAllDisabled;

        protected bool _enabled = false;

        protected virtual void Update()
        {
            if (_enabled)
            {
                return;
            }

            bool allDisabled = true;
            foreach (MonoBehaviour component in AfterTheseAreAllDisabled)
            {
                if (component.isActiveAndEnabled)
                {
                    allDisabled = false;
                }
            }
            if (allDisabled)
            {
                foreach (MonoBehaviour component in EnableThese)
                {
                    component.enabled = true;
                }
                _enabled = true;
            }
        }
    }
}
