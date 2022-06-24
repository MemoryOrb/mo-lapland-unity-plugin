using System.Collections;
using System.Collections.Generic;
using MemoryOrbLapland;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class
MemoryOrbManipulator
:
MonoBehaviour,
IPointerEnterHandler,
IPointerExitHandler,
IMixedRealityFocusHandler
{
    private bool focused = false;

    // Manipulating
    [SerializeField]
    private MemoryOrbManager memoryOrbManager;

    [SerializeField]
    private Transform parentWhenManipulating;

    [SerializeField]
    private Transform parentWhenStill;

    private bool manipulating = false;

    public Transform controller;

    private Transform target;

    public GameObject memoryOrbHelper;

    private bool isFocusedByMRTK;

    private char orientationAxis = 'x';

    private bool isOrientationDirectionPositive = true;

    public GameObject xp;

    public GameObject xn;

    public GameObject yp;

    public GameObject yn;

    public GameObject zp;

    public GameObject zn;

    [SerializeField]
    private bool moveConstraint = false;

    [SerializeField]
    private bool scaleConstraint = false;

    [SerializeField]
    private bool rotateConstraint = false;

    [SerializeField]
    private float step = 0.01f;

    [SerializeField]
    private UnityEvent OnManipulationStarted = new UnityEvent();

    [SerializeField]
    private UnityEvent OnManipulationEnded = new UnityEvent();

    [SerializeField]
    private UnityEvent OnRotating = new UnityEvent();

    [SerializeField]
    private UnityEvent OnScaling = new UnityEvent();

    void Start()
    {
        target = transform;
        manipulating = false;
        focused = false;
    }

    void OnEnable()
    {
        memoryOrbManager.GetMemoryOrb().OnButtonChangeState +=
            MemoryOrb_OnButtonChangeState;
        memoryOrbManager.GetMemoryOrb().OnRotaryEncoderChangeState +=
            MemoryOrb_OnRotaryEncoderChangeState;
        memoryOrbManager.GetMemoryOrb().OnPotentiometerChangeState +=
            MemoryOrb_OnPotentiometerChangeState;
    }

    void OnDisable()
    {
        memoryOrbManager.GetMemoryOrb().OnButtonChangeState -=
            MemoryOrb_OnButtonChangeState;
        memoryOrbManager.GetMemoryOrb().OnRotaryEncoderChangeState -=
            MemoryOrb_OnRotaryEncoderChangeState;
        memoryOrbManager.GetMemoryOrb().OnPotentiometerChangeState -=
            MemoryOrb_OnPotentiometerChangeState;
    }

    void Update()
    {
        //Debug.Log(Vector3.Angle(controller.right, target.up));
        float angleY = Vector3.Angle(controller.right, target.up);
        if (angleY > 45 && angleY < 135)
        {
            yp.SetActive(false);
            yn.SetActive(false);

            float angleX = Vector3.Angle(controller.right, target.right);
            if (angleX > 45 && angleX < 135)
            {
                xp.SetActive(false);
                xn.SetActive(false);

                float angleZ = Vector3.Angle(controller.right, target.forward);

                orientationAxis = 'z';
                zp.SetActive(true);
                zn.SetActive(true);
                isOrientationDirectionPositive = angleZ > 90;
            }
            else
            {
                zp.SetActive(false);
                zn.SetActive(false);

                orientationAxis = 'x';
                xp.SetActive(true);
                xn.SetActive(true);
                isOrientationDirectionPositive = angleX > 90;
            }
        }
        else
        {
            xp.SetActive(false);
            xn.SetActive(false);
            zp.SetActive(false);
            zn.SetActive(false);

            orientationAxis = 'y';
            yp.SetActive(true);
            yn.SetActive(true);
            isOrientationDirectionPositive = angleY > 90;
        }

        if (rotateConstraint)
        {
            transform.eulerAngles = Vector3.zero;
        }

        if (moveConstraint)
        {
            // very specific to this scenario
            transform.position = Vector3.up;
        }       
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        focused = true;

        memoryOrbHelper.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        focused = false;

        if (isFocusedByMRTK == false) memoryOrbHelper.SetActive(false);
    }

    private void MemoryOrb_OnButtonChangeState(Hand h, Finger f, ButtonState b)
    {
        if (f != Finger.Thumb) return;

        if (focused) 
        {
            if (b == ButtonState.Pressed)
            {
                if (!manipulating)
                {
                    manipulating = true;
                    OnManipulationStarted?.Invoke();
                    this.transform.SetParent(parentWhenManipulating);
                }
            }
        }

        if (b == ButtonState.Released)
        {
            // if one of the thumb is still pressed, keep manipulating
            if (
                memoryOrbManager
                    .GetMemoryOrb()
                    .IsButtonPressed(Hand.Left, Finger.Thumb) ||
                memoryOrbManager
                    .GetMemoryOrb()
                    .IsButtonPressed(Hand.Right, Finger.Thumb)
            )
            {
                return;
            }

            if (manipulating)
            {
                manipulating = false;
                OnManipulationEnded?.Invoke();
                this.transform.SetParent(parentWhenStill);
            }
        }
    }

    private void MemoryOrb_OnPotentiometerChangeState(
        Potentiometer p,
        int value
    )
    {
        if (manipulating)
        {
            // if both thumb are pressed (while manipulating), we take into account potentiometers' values
            if (
                memoryOrbManager
                    .GetMemoryOrb()
                    .IsButtonPressed(Hand.Left, Finger.Thumb) &&
                memoryOrbManager
                    .GetMemoryOrb()
                    .IsButtonPressed(Hand.Right, Finger.Thumb)
            )
            {
                if (
                    p == Potentiometer.Slide // use to scale
                )
                {
                    if (scaleConstraint)
                        return;

                    float scaleStep =
                        memoryOrbManager
                            .GetMemoryOrb()
                            .GetPotentiometerDelta(p) * -1f *step;
                    switch (orientationAxis)
                    {
                        case 'x':
                            transform.localScale =
                                transform.localScale +
                                Vector3.right * scaleStep;
                            break;
                        case 'y':
                            transform.localScale =
                                transform.localScale + Vector3.up * scaleStep;
                            break;
                        case 'z':
                            transform.localScale =
                                transform.localScale +
                                Vector3.forward * scaleStep;
                            break;
                    }
                    OnScaling?.Invoke();
                } // circular potentiometer, use to rotate
                else
                {
                    if (rotateConstraint)
                        return;

                    float angle =
                        memoryOrbManager
                            .GetMemoryOrb()
                            .GetPotentiometerDelta(p) * 2f;
                    if (isOrientationDirectionPositive)
                    {
                        angle *= -1f;
                    }
                    switch (orientationAxis)
                    {
                        case 'x':
                            transform.Rotate(angle, 0f, 0f);
                            break;
                        case 'y':
                            transform.Rotate(0f, angle, 0f);
                            break;
                        case 'z':
                            transform.Rotate(0f, 0f, angle);
                            break;
                    }
                    OnRotating?.Invoke();
                }
            }
        }
    }

    private void MemoryOrb_OnRotaryEncoderChangeState(Hand h, Direction d)
    {
        if (focused == false && isFocusedByMRTK == false) return;

        // if one of the index finger is pressed, then rotate
        if (
            memoryOrbManager
                .GetMemoryOrb()
                .IsButtonPressed(Hand.Left, Finger.Index) ||
            memoryOrbManager
                .GetMemoryOrb()
                .IsButtonPressed(Hand.Right, Finger.Index)
        )
        {
            if (rotateConstraint)
                return;

            float angle = 1f;
            if (h == Hand.Left)
            {
                if (isOrientationDirectionPositive)
                    angle = d == Direction.Clockwise ? 1f : -1f;
                else
                    angle = d == Direction.Clockwise ? -1f : 1f;
            } // right
            else
            {
                if (isOrientationDirectionPositive)
                    angle = d == Direction.Clockwise ? -1f : 1f;
                else
                    angle = d == Direction.Clockwise ? 1f : -1f;
            }
            switch (orientationAxis)
            {
                case 'x':
                    transform.Rotate(angle, 0f, 0f);
                    break;
                case 'y':
                    transform.Rotate(0f, angle, 0f);
                    break;
                case 'z':
                    transform.Rotate(0f, 0f, angle);
                    break;
                default:
                    break;
            }
            OnRotating?.Invoke();
        }

        // if one of the little finger is pressed
        if (
            memoryOrbManager
                .GetMemoryOrb()
                .IsButtonPressed(Hand.Left, Finger.Little) ||
            memoryOrbManager
                .GetMemoryOrb()
                .IsButtonPressed(Hand.Right, Finger.Little)
        )
        {
            if (scaleConstraint)
                return;

            float signedScaleStep;
            float signedPositionStep;

            if (isOrientationDirectionPositive)
            {
                if (h == Hand.Left)
                {
                    if (d == Direction.Clockwise)
                    {
                        signedPositionStep = step * -1f / 2f;
                        signedScaleStep = step * -1f;
                    }
                    else
                    {
                        signedPositionStep = step / 2f;
                        signedScaleStep = step;
                    }
                } // right hand
                else
                {
                    if (d == Direction.Clockwise)
                    {
                        signedPositionStep = step / 2f;
                        signedScaleStep = step * -1f;
                    }
                    else
                    {
                        signedPositionStep = step * -1f / 2f;
                        signedScaleStep = step * 1f;
                    }
                }
            }
            else
            {
                if (h == Hand.Right)
                {
                    if (d == Direction.Clockwise)
                    {
                        signedPositionStep = step * -1f / 2f;
                        signedScaleStep = step * -1f;
                    }
                    else
                    {
                        signedPositionStep = step / 2f;
                        signedScaleStep = step;
                    }
                } // left hand
                else
                {
                    if (d == Direction.Clockwise)
                    {
                        signedPositionStep = step / 2f;
                        signedScaleStep = step * -1f;
                    }
                    else
                    {
                        signedPositionStep = step * -1f / 2f;
                        signedScaleStep = step * 1f;
                    }
                }
            }

            switch (orientationAxis)
            {
                case 'x':
                    transform.position =
                        transform.position +
                        transform.right * signedPositionStep;
                    transform.localScale =
                        transform.localScale + Vector3.right * signedScaleStep;
                    break;
                case 'y':
                    transform.position =
                        transform.position + transform.up * signedPositionStep;
                    transform.localScale =
                        transform.localScale + Vector3.up * signedScaleStep;
                    break;
                case 'z':
                    transform.position =
                        transform.position +
                        transform.forward * signedPositionStep;
                    transform.localScale =
                        transform.localScale +
                        Vector3.forward * signedScaleStep;
                    break;
            }
            OnScaling?.Invoke();
        }
    }

    void IMixedRealityFocusHandler.OnFocusEnter(FocusEventData eventData)
    {
        memoryOrbHelper.SetActive(true);
        isFocusedByMRTK = true;
    }

    void IMixedRealityFocusHandler.OnFocusExit(FocusEventData eventData)
    {
        if (focused == false) memoryOrbHelper.SetActive(false);
        isFocusedByMRTK = false;
    }
}
