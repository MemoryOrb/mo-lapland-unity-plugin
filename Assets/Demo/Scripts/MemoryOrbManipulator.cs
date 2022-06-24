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

    // MOVING
    [SerializeField]
    private MemoryOrbManager memoryOrbManager;

    [SerializeField]
    private Transform parentWhenMoving;

    [SerializeField]
    private Transform parentWhenStill;

    private bool moving = false;

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
    private float step = 0.01f;

    [SerializeField]
    private UnityEvent OnManipulationStarted = new UnityEvent();

    [SerializeField]
    private UnityEvent OnManipulationEnded = new UnityEvent();

    void Start()
    {
        target = transform;
        moving = false;
        focused = false;
    }

    void OnEnable()
    {
        memoryOrbManager.GetMemoryOrb().OnButtonChangeState +=
            MemoryOrb_OnButtonChangeState;
        memoryOrbManager.GetMemoryOrb().OnRotaryEncoderChangeState +=
            MemoryOrb_OnRotaryEncoderChangeState;
    }

    void OnDisable()
    {
        memoryOrbManager.GetMemoryOrb().OnButtonChangeState -=
            MemoryOrb_OnButtonChangeState;
        memoryOrbManager.GetMemoryOrb().OnRotaryEncoderChangeState -=
            MemoryOrb_OnRotaryEncoderChangeState;
    }

    void Update()
    {
        //if (focused == false && isFocusedByMRTK == false)
        //    return;
        // not elegant at all, but well, as if it was only that part.. and heh, it is working!
        float dotY = Vector3.Dot(controller.up, target.up);
        if (dotY > 0.7f)
        {
            orientationAxis = 'y';
            isOrientationDirectionPositive = true;
            yp.SetActive(true);

            yn.SetActive(false);
            xp.SetActive(false);
            xn.SetActive(false);
            zp.SetActive(false);
            zn.SetActive(false);
        }
        else
        {
            yp.SetActive(false);
            if (dotY < -0.7f)
            {
                orientationAxis = 'y';
                isOrientationDirectionPositive = false;
                yn.SetActive(true);

                yp.SetActive(false);
                xp.SetActive(false);
                xn.SetActive(false);
                zp.SetActive(false);
                zn.SetActive(false);
            }
            else
            {
                yn.SetActive(false);

                float dotX = Vector3.Dot(controller.right, target.right);
                if (dotX > 0.7f || dotX < -0.7f)
                {
                    xp.SetActive(false);
                    xn.SetActive(false);
                }
                else
                {
                    dotX = Vector3.Dot(controller.up, target.right);
                    if (dotX > 0.7f)
                    {
                        orientationAxis = 'x';
                        isOrientationDirectionPositive = true;
                        xp.SetActive(true);
                        zp.SetActive(false);
                        zn.SetActive(false);
                    }
                    else
                    {
                        xp.SetActive(false);
                        if (dotX < -0.7f)
                        {
                            orientationAxis = 'x';
                            isOrientationDirectionPositive = false;
                            xn.SetActive(true);
                            zp.SetActive(false);
                            zn.SetActive(false);
                        }
                        else
                        {
                            xn.SetActive(false);

                            float dotZ =
                                Vector3.Dot(controller.forward, target.forward);
                            if (dotZ > 0.7f || dotZ < -0.7f)
                            {
                                // do something
                            }
                            else
                            {
                                dotZ =
                                    Vector3.Dot(controller.up, target.forward);
                                if (dotZ > 0.7f)
                                {
                                    orientationAxis = 'z';
                                    isOrientationDirectionPositive = true;
                                    zp.SetActive(true);
                                }
                                else
                                {
                                    zp.SetActive(false);
                                }

                                if (dotZ < -0.7f)
                                {
                                    orientationAxis = 'z';
                                    isOrientationDirectionPositive = false;
                                    zn.SetActive(true);
                                }
                                else
                                {
                                    zn.SetActive(false);
                                }
                            }
                        }
                    }
                }
            }
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

        if (!focused) return;

        if (b == ButtonState.Pressed)
        {
            if (!moving)
            {
                moving = true;
                OnManipulationStarted?.Invoke();
                this.transform.SetParent(parentWhenMoving);
            }
        }
        else
        {
            if (moving)
            {
                moving = false;
                OnManipulationEnded?.Invoke();
                this.transform.SetParent(parentWhenStill);
            }
        }
    }

    private void MemoryOrb_OnRotaryEncoderChangeState(Hand h, Direction d)
    {
        if (focused == false && isFocusedByMRTK == false) return;

        if (
            memoryOrbManager
                .GetMemoryOrb()
                .IsButtonPressed(Hand.Left, Finger.Index) ||
            memoryOrbManager
                .GetMemoryOrb()
                .IsButtonPressed(Hand.Right, Finger.Index)
        )
        {
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
        }

        if (
            memoryOrbManager
                .GetMemoryOrb()
                .IsButtonPressed(Hand.Left, Finger.Little) ||
            memoryOrbManager
                .GetMemoryOrb()
                .IsButtonPressed(Hand.Right, Finger.Little)
        )
        {
            // left - clockwise = move X up / 2, scale X down
            // left - anti = move X down / 2, scale X up
            // right - clockwise = move X down / 2, scale X down,
            // right - anti = move X up / 2, scale X up
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
