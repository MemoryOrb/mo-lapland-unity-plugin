using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MemoryOrbLapland;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Microsoft.MixedReality.Toolkit.Input;

public class MemoryOrbManipulator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IMixedRealityFocusHandler
{
    public MemoryOrbManager memoryOrbManager;
    public Transform parentWhenMoving;
    public Transform parentWhenStill;
    

    private bool isMoving;
    private bool isFocused;
    private bool realFocused;

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

    [SerializeField] private UnityEvent OnManipulationStarted = new UnityEvent();
    [SerializeField] private UnityEvent OnManipulationEnded = new UnityEvent();

    void Start()
    {
        target = transform;
        isMoving = false;
        isFocused = false;

        memoryOrbManager.GetMemoryOrb().OnButtonChangeState += MemoryOrb_OnButtonChangeState;
        memoryOrbManager.GetMemoryOrb().OnRotaryButtonChangeState += MemoryOrb_OnRotaryButtonChangeState;
        memoryOrbManager.GetMemoryOrb().OnRotaryEncoderChangeState += MemoryOrb_OnRotaryEncoderChangeState;
        memoryOrbManager.GetMemoryOrb().OnPotentiometerChangeState += MemoryOrb_OnPotentiometerChangeState;
    }

    void Update()
    {
        if (isFocused == false && isFocusedByMRTK == false)
            return;
        // not elegant at all, but well, as if it was only that part.. and heh, it is working!
        float dotY = Vector3.Dot(controller.up, target.up);
        //orientationAxis = 'a';
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
                            
                            float dotZ = Vector3.Dot(controller.forward, target.forward);
                            if (dotZ > 0.7f || dotZ < -0.7f)
                            {
                                // do something   
                            }
                            else
                            {
                                dotZ = Vector3.Dot(controller.up, target.forward);
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
        // Debug.Log(eventData.enterEventCamera.gameObject.name + " enter");
        realFocused = true;
        if (!isMoving)
        {
            PointerEnter();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Debug.Log("exit");
        realFocused = false;
        if (!isMoving)
        {
            PointerExit();
        }
    }

    private void PointerEnter()
    {
        memoryOrbHelper.SetActive(true);
        isFocused = true;
    }

    private void PointerExit()
    {
        if (isFocusedByMRTK == false)
            memoryOrbHelper.SetActive(false);
        isFocused = false;
    }

    private void MemoryOrb_OnButtonChangeState(Hand h, Finger f, ButtonState b)
    {
        if (!isFocused)
            return;

        if (!isMoving)
        {
            isMoving = true;
            OnManipulationStarted?.Invoke();
            this.transform.SetParent(parentWhenMoving);
        }
        else
        {
            isMoving = false;
            OnManipulationEnded?.Invoke();
            this.transform.SetParent(parentWhenStill);
            if (!realFocused) // should very rarely happen
                PointerExit();
        }
    }

    private void MemoryOrb_OnRotaryButtonChangeState(Hand h, ButtonState b)
    {
        if (!isFocused)
            return;
    }

    private void MemoryOrb_OnRotaryEncoderChangeState(Hand h, Direction d)
    {
        if (isFocused == false && isFocusedByMRTK == false)
            return;
        
        if (memoryOrbManager.GetMemoryOrb().IsButtonPressed(Hand.Left, Finger.Index) || memoryOrbManager.GetMemoryOrb().IsButtonPressed(Hand.Right, Finger.Index))
        {
            float angle = 1f;
            if (h == Hand.Left)
            {
                if (isOrientationDirectionPositive)
                    angle = d == Direction.Clockwise ? 1f : -1f;
                else
                    angle = d == Direction.Clockwise ? -1f : 1f;
            } else // right
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
    }

    private void MemoryOrb_OnPotentiometerChangeState(Potentiometer p, int value)
    {
        if (!isFocused)
            return;
    }

    void IMixedRealityFocusHandler.OnFocusEnter(FocusEventData eventData)
    {
        memoryOrbHelper.SetActive(true);
        isFocusedByMRTK = true;
    }

    void IMixedRealityFocusHandler.OnFocusExit(FocusEventData eventData)
    {
        if (isFocused == false)
            memoryOrbHelper.SetActive(false);
        isFocusedByMRTK = false;
    }
}
