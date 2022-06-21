using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using MemoryOrbLapland;

public class MemoryOrbButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public MemoryOrbManager memoryOrbManager;

    private bool focused = false;

    [SerializeField] private UnityEvent OnClick = new UnityEvent();

    void Start()
    {
        memoryOrbManager.GetMemoryOrb().OnButtonChangeState += MemoryOrb_OnButtonChangeState;
    }

    void OnDisable()
    {
        focused = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        focused = true;
        Debug.Log(this.name + " Enter, focused = " + focused);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        focused = false;
        Debug.Log(this.name + " Exit, focused = " + focused);
    }

    private void MemoryOrb_OnButtonChangeState(Hand h, Finger f, ButtonState b)
    {
        Debug.Log(this.name + " ButtonChange, focused = " + focused);
        if (focused)
        {
            if (memoryOrbManager.GetMemoryOrb().IsButtonPressed(Hand.Left, Finger.Thumb) && memoryOrbManager.GetMemoryOrb().IsButtonPressed(Hand.Right, Finger.Thumb))
            {
                Debug.Log("Inkoke OnClick, focused = " + focused + ", gameobject = " + this.name);
                OnClick?.Invoke();
            }
        }
    }
}
