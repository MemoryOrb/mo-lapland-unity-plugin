using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using MemoryOrbLapland;

public class MemoryOrbButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private MemoryOrbManager memoryOrbManager;
    [SerializeField] private UnityEvent OnClick = new UnityEvent();

    private bool focused = false;

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

    private void MemoryOrb_OnRotaryButtonChangeState(Hand h, ButtonState b)
    {
        if (focused && b == ButtonState.Pressed)
        {
            OnClick?.Invoke();
        }
    }
}
