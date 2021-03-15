using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    /// <summary>
    /// Animator extensions
    /// </summary>
    public static class MMAnimatorExtensions
    {
        /// <summary>
		/// Determines if an animator contains a certain parameter, based on a type and a name
		/// </summary>
		/// <returns><c>true</c> if has parameter of type the specified self name type; otherwise, <c>false</c>.</returns>
		/// <param name="self">Self.</param>
		/// <param name="name">Name.</param>
		/// <param name="type">Type.</param>
		public static bool MMHasParameterOfType(this Animator self, string name, AnimatorControllerParameterType type)
        {
            if (name == null || name == "") { return false; }
            AnimatorControllerParameter[] parameters = self.parameters;
            foreach (AnimatorControllerParameter currParam in parameters)
            {
                if (currParam.type == type && currParam.name == name)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Adds an animator parameter name to a parameter list if that parameter exists.
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="parameterName"></param>
        /// <param name="parameter"></param>
        /// <param name="type"></param>
        /// <param name="parameterList"></param>
        public static void AddAnimatorParameterIfExists(Animator animator, string parameterName, out int parameter, AnimatorControllerParameterType type, List<int> parameterList)
        {
            if (parameterName == "")
            {
                parameter = -1;
                return;
            }

            parameter = Animator.StringToHash(parameterName);

            if (animator.MMHasParameterOfType(parameterName, type))
            {
                parameterList.Add(parameter);
            }
        }

        /// <summary>
        /// Adds an animator parameter name to a parameter list if that parameter exists.
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="parameterName"></param>
        /// <param name="type"></param>
        /// <param name="parameterList"></param>
        public static void AddAnimatorParameterIfExists(Animator animator, string parameterName, AnimatorControllerParameterType type, List<string> parameterList)
        {
            if (animator.MMHasParameterOfType(parameterName, type))
            {
                parameterList.Add(parameterName);
            }
        }

        // <summary>
        /// Updates the animator bool.
        /// </summary>
        /// <param name="animator">Animator.</param>
        /// <param name="parameterName">Parameter name.</param>
        /// <param name="value">If set to <c>true</c> value.</param>
        public static void UpdateAnimatorBool(Animator animator, string parameterName, bool value)
        {
            animator.SetBool(parameterName, value);
        }

        // <summary>
        /// Updates the animator bool.
        /// </summary>
        /// <param name="animator">Animator.</param>
        /// <param name="parameterName">Parameter name.</param>
        /// <param name="value">If set to <c>true</c> value.</param>
        public static void UpdateAnimatorBool(Animator animator, int parameter, bool value, List<int> parameterList)
        {
            if (parameterList.Contains(parameter))
            {
                animator.SetBool(parameter, value);
            }
        }

        // <summary>
        /// Updates the animator bool.
        /// </summary>
        /// <param name="animator">Animator.</param>
        /// <param name="parameterName">Parameter name.</param>
        /// <param name="value">If set to <c>true</c> value.</param>
        public static void UpdateAnimatorBool(Animator animator, string parameterName, bool value, List<string> parameterList)
        {
            if (parameterList.Contains(parameterName))
            {
                animator.SetBool(parameterName, value);
            }
        }

        /// <summary>
        /// Sets an animator's trigger of the int parameter specified
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="parameter"></param>
        /// <param name="parameterList"></param>
        public static void UpdateAnimatorTrigger(Animator animator, int parameter, List<int> parameterList)
        {
            if (parameterList.Contains(parameter))
            {
                animator.SetTrigger(parameter);
            }
        }

        /// <summary>
        /// Sets an animator's trigger of the string parameter name specified
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="parameterName"></param>
        /// <param name="parameterList"></param>
        public static void UpdateAnimatorTrigger(Animator animator, string parameterName, List<string> parameterList)
        {
            if (parameterList.Contains(parameterName))
            {
                animator.SetTrigger(parameterName);
            }
        }

        /// <summary>
        /// Triggers an animator trigger.
        /// </summary>
        /// <param name="animator">Animator.</param>
        /// <param name="parameter">Parameter name.</param>
        /// <param name="value">If set to <c>true</c> value.</param>
        public static void SetAnimatorTrigger(Animator animator, int parameter, List<int> parameterList)
        {
            if (parameterList.Contains(parameter))
            {
                animator.SetTrigger(parameter);
            }
        }

        /// <summary>
		/// Triggers an animator trigger.
		/// </summary>
		/// <param name="animator">Animator.</param>
		/// <param name="parameterName">Parameter name.</param>
		/// <param name="value">If set to <c>true</c> value.</param>
		public static void SetAnimatorTrigger(Animator animator, string parameterName, List<string> parameterList)
        {
            if (parameterList.Contains(parameterName))
            {
                animator.SetTrigger(parameterName);
            }
        }

        /// <summary>
        /// Updates the animator's float 
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="parameterName"></param>
        /// <param name="value"></param>
        public static void UpdateAnimatorFloat(Animator animator, string parameterName, float value)
        {
            animator.SetFloat(parameterName, value);
        }

        /// <summary>
        /// Updates the animator float.
        /// </summary>
        /// <param name="animator">Animator.</param>
        /// <param name="parameter">Parameter name.</param>
        /// <param name="value">Value.</param>
        public static void UpdateAnimatorFloat(Animator animator, int parameter, float value, List<int> parameterList)
        {
            if (parameterList.Contains(parameter))
            {
                animator.SetFloat(parameter, value);
            }
        }

        /// <summary>
		/// Updates the animator float.
		/// </summary>
		/// <param name="animator">Animator.</param>
		/// <param name="parameterName">Parameter name.</param>
		/// <param name="value">Value.</param>
		public static void UpdateAnimatorFloat(Animator animator, string parameterName, float value, List<string> parameterList)
        {
            if (parameterList.Contains(parameterName))
            {
                animator.SetFloat(parameterName, value);
            }
        }

        /// <summary>
        /// Updates the animator integer.
        /// </summary>
        /// <param name="animator">Animator.</param>
        /// <param name="parameter">Parameter name.</param>
        /// <param name="value">Value.</param>
        public static void UpdateAnimatorInteger(Animator animator, string parameterName, int value)
        {
            animator.SetInteger(parameterName, value);
        }

        /// <summary>
        /// Updates the animator integer.
        /// </summary>
        /// <param name="animator">Animator.</param>
        /// <param name="parameter">Parameter name.</param>
        /// <param name="value">Value.</param>
        public static void UpdateAnimatorInteger(Animator animator, int parameter, int value, List<int> parameterList)
        {
            if (parameterList.Contains(parameter))
            {
                animator.SetInteger(parameter, value);
            }
        }

        /// <summary>
        /// Updates the animator integer.
        /// </summary>
        /// <param name="animator">Animator.</param>
        /// <param name="parameterName">Parameter name.</param>
        /// <param name="value">Value.</param>
        public static void UpdateAnimatorInteger(Animator animator, string parameterName, int value, List<string> parameterList)
        {
            if (parameterList.Contains(parameterName))
            {
                animator.SetInteger(parameterName, value);
            }
        }

        // <summary>
        /// Updates the animator bool after checking the parameter's existence.
        /// </summary>
        /// <param name="animator">Animator.</param>
        /// <param name="parameterName">Parameter name.</param>
        /// <param name="value">If set to <c>true</c> value.</param>
        public static void UpdateAnimatorBoolIfExists(Animator animator, string parameterName, bool value)
        {
            if (animator.MMHasParameterOfType(parameterName, AnimatorControllerParameterType.Bool))
            {
                animator.SetBool(parameterName, value);
            }
        }

        /// <summary>
        /// Updates an animator trigger if it exists
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="parameterName"></param>
        public static void UpdateAnimatorTriggerIfExists(Animator animator, string parameterName)
        {
            if (animator.MMHasParameterOfType(parameterName, AnimatorControllerParameterType.Trigger))
            {
                animator.SetTrigger(parameterName);
            }
        }

        /// <summary>
        /// Triggers an animator trigger after checking for the parameter's existence.
        /// </summary>
        /// <param name="animator">Animator.</param>
        /// <param name="parameterName">Parameter name.</param>
        /// <param name="value">If set to <c>true</c> value.</param>
        public static void SetAnimatorTriggerIfExists(Animator animator, string parameterName)
        {
            if (animator.MMHasParameterOfType(parameterName, AnimatorControllerParameterType.Trigger))
            {
                animator.SetTrigger(parameterName);
            }
        }

        /// <summary>
        /// Updates the animator float after checking for the parameter's existence.
        /// </summary>
        /// <param name="animator">Animator.</param>
        /// <param name="parameterName">Parameter name.</param>
        /// <param name="value">Value.</param>
        public static void UpdateAnimatorFloatIfExists(Animator animator, string parameterName, float value)
        {
            if (animator.MMHasParameterOfType(parameterName, AnimatorControllerParameterType.Float))
            {
                animator.SetFloat(parameterName, value);
            }
        }

        /// <summary>
        /// Updates the animator integer after checking for the parameter's existence.
        /// </summary>
        /// <param name="animator">Animator.</param>
        /// <param name="parameterName">Parameter name.</param>
        /// <param name="value">Value.</param>
        public static void UpdateAnimatorIntegerIfExists(Animator animator, string parameterName, int value)
        {
            if (animator.MMHasParameterOfType(parameterName, AnimatorControllerParameterType.Int))
            {
                animator.SetInteger(parameterName, value);
            }
        }
    }
}
