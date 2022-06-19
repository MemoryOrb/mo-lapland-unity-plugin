// Simplified version (only having Chest and Camera tracker path) of the script from Unity Technologies forum member - thep3000
// https://forum.unity.com/threads/openxr-and-openvr-together.1113136/#post-7803057

using System.Collections.Generic;
using UnityEngine.Scripting;
using UnityEngine.XR.OpenXR.Input;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.XR;
using UnityEngine.InputSystem;
using System.Runtime.InteropServices;
using System;
 
#if UNITY_EDITOR
using UnityEditor;
#endif
 
using PoseControl = UnityEngine.XR.OpenXR.Input.PoseControl;
 
namespace UnityEngine.XR.OpenXR.Features.Interactions
{
    /// <summary>
    /// This <see cref="OpenXRInteractionFeature"/> enables the use of HTC Vive Trackers interaction profiles in OpenXR.
    /// </summary>
#if UNITY_EDITOR
    [UnityEditor.XR.OpenXR.Features.OpenXRFeature(
        UiName = "HTC Vive Tracker Profile",
        BuildTargetGroups = new[] { BuildTargetGroup.Standalone, BuildTargetGroup.WSA },
        Company = "MASSIVE",
        Desc = "Allows for mapping input to the HTC Vive Tracker interaction profile.",
        DocumentationLink = Constants.k_DocumentationManualURL,
        OpenxrExtensionStrings = HTCViveTrackerProfile.extensionName,
        Version = "0.0.1",
        Category = UnityEditor.XR.OpenXR.Features.FeatureCategory.Interaction,
        FeatureId = featureId)]
#endif
    public class HTCViveTrackerProfile : OpenXRInteractionFeature
    {
        /// <summary>
        /// The feature id string. This is used to give the feature a well known id for reference.
        /// </summary>
        public const string featureId = "com.massive.openxr.feature.input.htcvivetracker";
 
        /// <summary>
        /// The interaction profile string used to reference the <a href="https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#:~:text=in%20this%20case.-,VIVE%20Tracker%20interaction%20profile,-Interaction%20profile%20path">HTC Vive Tracker</a>.
        /// </summary>
        public const string profile = "/interaction_profiles/htc/vive_tracker_htcx";
 
        /// <summary>
        /// The name of the OpenXR extension that supports the Vive Tracker
        /// </summary>
        public const string extensionName = "XR_HTCX_vive_tracker_interaction";
 
        private const string kDeviceLocalizedName = "HTC Vive Tracker OpenXR";
 
        /// <summary>
        /// OpenXR user path definitions for the tracker.
        /// </summary>
        public static class TrackerUserPaths
        {
            /// <summary>
            /// Path for user chest
            /// </summary>
            public const string chest = "/user/vive_tracker_htcx/role/chest";
 
            /// <summary>
            /// Path for user custom camera
            /// </summary>
            public const string camera = "/user/vive_tracker_htcx/role/camera";
        }
 
        /// <summary>
        /// OpenXR component path definitions for the tracker.
        /// </summary>
        public static class TrackerComponentPaths
        {
            /// <summary>
            /// Constant for a pose interaction binding '.../input/grip/pose' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
            /// </summary>
            public const string grip = "/input/grip/pose";
        }
 
        /// <summary>
        /// A base Input System device for XR Trackers, based off the TrackedDevice
        /// </summary>
        [InputControlLayout(isGenericTypeOfDevice = true, displayName = "XR Tracker")]
        public class XRTracker : TrackedDevice
        {
        }
 
        /// <summary>
        /// An Input System device based off the <a href="https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#_htc_vive_controller_profile">HTC Vive Tracker</a>.
        /// </summary>
        [Preserve, InputControlLayout(displayName = "HTC Vive Tracker (OpenXR)", commonUsages = new[] { "Chest", "Camera" })]
        public class XRViveTracker : XRTracker
        {
            /// <summary>
            /// A <see cref="PoseControl"/> that represents information from the <see cref="HTCViveTrackerProfile.grip"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(offset = 0, aliases = new[] { "device", "gripPose" }, usage = "Device", noisy = true)]
            public PoseControl devicePose { get; private set; }
 
            /// <summary>
            /// A [Vector3Control](xref:UnityEngine.InputSystem.Controls.Vector3Control) required for back compatibility with the XRSDK layouts. This is the device position. For the Oculus Touch device, this is both the grip and the pointer position. This value is equivalent to mapping devicePose/position.
            /// </summary>
            [Preserve, InputControl(offset = 8, alias = "gripPosition", noisy = true)]
            new public Vector3Control devicePosition { get; private set; }
 
            /// <summary>
            /// A [QuaternionControl](xref:UnityEngine.InputSystem.Controls.QuaternionControl) required for backwards compatibility with the XRSDK layouts. This is the device orientation. For the Oculus Touch device, this is both the grip and the pointer rotation. This value is equivalent to mapping devicePose/rotation.
            /// </summary>
            [Preserve, InputControl(offset = 20, alias = "gripOrientation", noisy = true)]
            new public QuaternionControl deviceRotation { get; private set; }
            
            [Preserve, InputControl(offset = 60)]
            new public ButtonControl isTracked { get; private set; }
            
            [Preserve, InputControl(offset = 64)]
            new public IntegerControl trackingState { get; private set; }

            /// <inheritdoc cref="OpenXRDevice"/>
            protected override void FinishSetup()
            {
                base.FinishSetup();
                devicePose = GetChildControl<PoseControl>("devicePose");
                devicePosition = GetChildControl<Vector3Control>("devicePosition");
                deviceRotation = GetChildControl<QuaternionControl>("deviceRotation");
                isTracked = GetChildControl<ButtonControl>("isTracked");
                trackingState = GetChildControl<IntegerControl>("trackingState");
                
                var capabilities = description.capabilities;
                var deviceDescriptor = XRDeviceDescriptor.FromJson(capabilities);
                
                if ((deviceDescriptor.characteristics & (InputDeviceCharacteristics)InputDeviceTrackerCharacteristics.TrackerChest) != 0)
                    InputSystem.InputSystem.SetDeviceUsage(this, "Chest");
                else if ((deviceDescriptor.characteristics & (InputDeviceCharacteristics)InputDeviceTrackerCharacteristics.TrackerCamera) != 0)
                    InputSystem.InputSystem.SetDeviceUsage(this, "Camera");
                
                Debug.Log("Device added");
            }
        }
 
        /// <summary>
        /// Registers the <see cref="ViveTracker"/> layout with the Input System.
        /// </summary>
        protected override void RegisterDeviceLayout()
        {
            InputSystem.InputSystem.RegisterLayout<XRTracker>();
 
            InputSystem.InputSystem.RegisterLayout(typeof(XRViveTracker),
                        matches: new InputDeviceMatcher()
                        .WithInterface(XRUtilities.InterfaceMatchAnyVersion)
                        .WithProduct(kDeviceLocalizedName));
        }
 
        /// <summary>
        /// Removes the <see cref="ViveTracker"/> layout from the Input System.
        /// </summary>
        protected override void UnregisterDeviceLayout()
        {
            InputSystem.InputSystem.RemoveLayout(nameof(XRViveTracker));
            InputSystem.InputSystem.RemoveLayout(nameof(XRTracker));
        }
 
        //
        // Summary:
        //     A set of bit flags describing XR.InputDevice characteristics.
        // "Chest", "Camera"
        [Flags]
        public enum InputDeviceTrackerCharacteristics : uint
        {
            TrackerChest = 0x200000u,
            TrackerCamera = 0x400000u
        }
 
        /// <inheritdoc/>
        protected override void RegisterActionMapsWithRuntime()
        {
            ActionMapConfig actionMap = new ActionMapConfig()
            {
                name = "htcvivetracker",
                localizedName = kDeviceLocalizedName,
                desiredInteractionProfile = profile,
                manufacturer = "HTC",
                serialNumber = "",
                deviceInfos = new List<DeviceConfig>()
                {
                    new DeviceConfig()
                    {
                        characteristics = (InputDeviceCharacteristics.TrackedDevice) | (InputDeviceCharacteristics)InputDeviceTrackerCharacteristics.TrackerChest,
                        userPath = TrackerUserPaths.chest
                    },
                    new DeviceConfig()
                    {
                        characteristics = (InputDeviceCharacteristics.TrackedDevice) | (InputDeviceCharacteristics)InputDeviceTrackerCharacteristics.TrackerCamera,
                        userPath = TrackerUserPaths.camera
                    }
                },
                actions = new List<ActionConfig>()
                {
                     new ActionConfig()
                    {
                        name = "devicePose",
                        localizedName = "Device Pose",
                        type = ActionType.Pose,
                        usages = new List<string>()
                        {
                            "Device",
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = TrackerComponentPaths.grip,
                                interactionProfileName = profile,
                            }
                        }
                    },
                }
            };
 
            AddActionMap(actionMap);
        }
 
        protected override bool OnInstanceCreate(ulong xrInstance)
        {
            bool res = base.OnInstanceCreate(xrInstance);
 
            if (OpenXRRuntime.IsExtensionEnabled("XR_HTCX_vive_tracker_interaction"))
            {
                Debug.Log("HTC Vive Tracker Extension Enabled");
            }
            else
            {
                Debug.Log("HTC Vive Tracker Extension Not Enabled");
            }
 
            return res;
        }
    }
}