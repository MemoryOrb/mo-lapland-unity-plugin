// Class based on the InputActionHandler, filter added for handedness

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Input
{
    /// <summary>
    /// Script used to handle input action events. Invokes Unity events when the configured input action starts or ends. 
    /// </summary>
    public class InputActionHandlerHandedness : BaseInputHandler, IMixedRealityInputActionHandler
    {
        [SerializeField]
        [Tooltip("Input Action to handle")]
        private MixedRealityInputAction InputAction = MixedRealityInputAction.None;

        [SerializeField]
        [Tooltip("Whether input events should be marked as used after handling so other handlers in the same game object ignore them")]
        private bool MarkEventsAsUsed = false;

        // CUSTOM
        [SerializeField]
        [Tooltip("Is it for Left Hand? (if not, then it is right")]
        private bool isItForLeftHand = true;

        /// <summary>
        /// Unity event raised on action start, e.g. button pressed or gesture started. 
        /// Includes the input event that triggered the action.
        /// </summary>
        public InputActionUnityEvent OnInputActionStarted;

        /// <summary>
        /// Unity event raised on action end, e.g. button released or gesture completed.
        /// Includes the input event that triggered the action.
        /// </summary>
        public InputActionUnityEvent OnInputActionEnded;

        #region InputSystemGlobalHandlerListener Implementation

        /// <inheritdoc />
        protected override void RegisterHandlers()
        {
            CoreServices.InputSystem?.RegisterHandler<IMixedRealityInputActionHandler>(this);
        }

        /// <inheritdoc />
        protected override void UnregisterHandlers()
        {
            CoreServices.InputSystem?.UnregisterHandler<IMixedRealityInputActionHandler>(this);
        }

        #endregion InputSystemGlobalHandlerListener Implementation

        void IMixedRealityInputActionHandler.OnActionStarted(BaseInputEventData eventData)
        {
            // CUSTOM
            bool isItFromLeft = eventData.InputSource.SourceName.IndexOf("Left", 0) != -1;
            if (eventData.MixedRealityInputAction == InputAction && !eventData.used && isItForLeftHand == isItFromLeft)
            {
                OnInputActionStarted.Invoke(eventData);
                if (MarkEventsAsUsed)
                {
                    eventData.Use();
                }
            }
        }
        void IMixedRealityInputActionHandler.OnActionEnded(BaseInputEventData eventData)
        {
            // CUSTOM
            bool isItFromLeft = eventData.InputSource.SourceName.IndexOf("Left", 0) != -1;
            if (eventData.MixedRealityInputAction == InputAction && !eventData.used && isItForLeftHand == isItFromLeft)
            {
                OnInputActionEnded.Invoke(eventData);
                if (MarkEventsAsUsed)
                {
                    eventData.Use();
                }
            }
        }
    }
}