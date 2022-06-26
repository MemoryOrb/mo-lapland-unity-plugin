using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using MemoryOrbLapland;
using Microsoft.MixedReality.Toolkit.Input;

public class MemoryOrbButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IMixedRealityFocusHandler
{
    [SerializeField] private MemoryOrbManager memoryOrbManager;
    [SerializeField] private UnityEvent OnClick = new UnityEvent();

    private bool focused = false;
    private bool focusedByMRTK = false;

    void OnEnable()
    {
        memoryOrbManager.GetMemoryOrb().OnRotaryButtonChangeState += MemoryOrb_OnRotaryButtonChangeState;
    }

    void OnDisable()
    {
        focused = false;
        memoryOrbManager.GetMemoryOrb().OnRotaryButtonChangeState -= MemoryOrb_OnRotaryButtonChangeState;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        focused = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        focused = false;
    }

    void IMixedRealityFocusHandler.OnFocusEnter(FocusEventData eventData)
    {
        focusedByMRTK = true;
    }

    void IMixedRealityFocusHandler.OnFocusExit(FocusEventData eventData)
    {
        focusedByMRTK = false;
    }

    private void MemoryOrb_OnRotaryButtonChangeState(Hand h, ButtonState b)
    {
        if (focused || focusedByMRTK)
        {
            if (b == ButtonState.Pressed)
            {
                OnClick?.Invoke();
            }
        }
    }
}
