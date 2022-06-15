using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MemoryOrbLapland;
using Microsoft.MixedReality.Toolkit.UI;

public class ClippingManager : MonoBehaviour
{
    private NetworkUtils _network;
    private MemoryOrb _memoryOrb;

    public PinchSlider pinchSliderStep;
    public TextMesh textStep;

    public Transform clippingBoxTransform;

    private float step = 0.01f;

    private int spsv = 0;

    void Start()
    {
        _network = new NetworkUtils();
        _network.OnMessageReceived += NetworkUtils_OnMessageReceived;
        _network.StartServer("55666");
        _network.Listen();
        Debug.Log("StartServer");

        _memoryOrb = new MemoryOrb();
        _memoryOrb.OnRotaryEncoderChangeState += MemoryOrb_OnRotaryEncoderChangeState;
        _memoryOrb.OnPotentiometerChangeState += MemoryOrb_OnPotentiometerChangeState;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            spsv += 9;
            if (spsv > 100)
                spsv = 100;
            MemoryOrb_OnPotentiometerChangeState(Potentiometer.Slide, spsv);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            spsv -= 9;
            if (spsv < 0)
                spsv = 0;
            MemoryOrb_OnPotentiometerChangeState(Potentiometer.Slide, spsv);
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            MemoryOrb_OnRotaryEncoderChangeState(Hand.Left, Direction.CounterClockwise);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            MemoryOrb_OnRotaryEncoderChangeState(Hand.Left, Direction.Clockwise);
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            MemoryOrb_OnRotaryEncoderChangeState(Hand.Right, Direction.Clockwise);
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            MemoryOrb_OnRotaryEncoderChangeState(Hand.Right, Direction.CounterClockwise);
        }
    }

    void OnDestroy()
    {
        _network.StopServer();
    }

    private void NetworkUtils_OnMessageReceived(string message)
    {
        Debug.Log(message);
        _memoryOrb.Feed(message);
    }

    private void MemoryOrb_OnRotaryEncoderChangeState(Hand h, Direction d)
    {
        // left - clockwise = move X up / 2, scale X down
        // left - anti = move X down / 2, scale X up
        // right - clockwise = move X down / 2, scale X down, 
        // right - anti = move X up / 2, scale X up

        float signedScaleStep;
        float signedPositionStep;
        if (d == Direction.Clockwise)
        {
            signedScaleStep = step * -1f;
            signedPositionStep = step * (h == Hand.Left ? 1f : -1f) / 2f;
        } else
        {
            signedScaleStep = step;
            signedPositionStep = step * (h == Hand.Left ? -1f : 1f) / 2f;
        }

        clippingBoxTransform.localPosition = new Vector3(
            clippingBoxTransform.localPosition.x + signedPositionStep,
            clippingBoxTransform.localPosition.y,
            clippingBoxTransform.localPosition.z
        );

        clippingBoxTransform.localScale = new Vector3(
            clippingBoxTransform.localScale.x + signedScaleStep, 
            clippingBoxTransform.localScale.y, 
            clippingBoxTransform.localScale.z
        );
    }

    private void MemoryOrb_OnPotentiometerChangeState(Potentiometer p, int value)
    {
        if (p == Potentiometer.Slide)
            pinchSliderStep.SliderValue = (value / 100.0f);
        
        if (value <= 25) {
            step = 0.1f;
        } else 
        if (value >= 75) {
            step = 0.001f;
        } else {
            step = 0.01f;
        }
        textStep.text = step * 100f + " cm";
    }
}