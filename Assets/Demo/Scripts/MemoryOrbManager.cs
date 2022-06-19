using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MemoryOrbLapland;

public class MemoryOrbManager : MonoBehaviour
{
    public Transform memoryOrbRight; // external cylinder
    public Transform memoryOrbLeft; // internal

    public Renderer[] buttonsLeftRenderer; // ordered from thumb to little finger
    public Renderer[] buttonsRightRenderer;
    public Renderer[] rotaryCapsRenderer; // ordered left, right

    private NetworkUtils network;
    private MemoryOrb memoryOrb;

    private float lerpColorDuration = 0.75f;
    private float[] t = {1f, 1f};
    
    void Start()
    {
        network = new NetworkUtils();
        network.OnMessageReceived += NetworkUtils_OnMessageReceived;
        network.StartServer("55666");
        network.Listen();
        Debug.Log("StartServer");

        memoryOrb = new MemoryOrb();
        memoryOrb.OnButtonChangeState += MemoryOrb_OnButtonChangeState;
        memoryOrb.OnRotaryButtonChangeState += MemoryOrb_OnRotaryButtonChangeState;
        memoryOrb.OnRotaryEncoderChangeState += MemoryOrb_OnRotaryEncoderChangeState;
        memoryOrb.OnPotentiometerChangeState += MemoryOrb_OnPotentiometerChangeState;
    }

    void OnDestroy()
    {
        network.StopServer();
    }

    private void NetworkUtils_OnMessageReceived(string message)
    {
        memoryOrb.Feed(message);
    }

    private void MemoryOrb_OnButtonChangeState(Hand h, Finger f, ButtonState b)
    {
        Debug.Log("Button " + h + " " + f + " " + b);
        if (h == Hand.Left)
        {
            buttonsLeftRenderer[(int) f].materials[0].SetColor("_Color", b == ButtonState.Pressed ? Color.cyan : Color.black);
        } else // Right
        {
            buttonsRightRenderer[(int) f].materials[0].SetColor("_Color", b == ButtonState.Pressed ? Color.cyan : Color.black);
        }
    }

    private void MemoryOrb_OnRotaryButtonChangeState(Hand h, ButtonState b)
    {
        Debug.Log("RotaryButton " + h + " " + b);
        rotaryCapsRenderer[(int) h].materials[0].SetColor("_Color", b == ButtonState.Pressed ? Color.cyan : Color.black);
    }

    private void MemoryOrb_OnRotaryEncoderChangeState(Hand h, Direction d)
    {
        Debug.Log("Rotary " + h + " " + d);
        if (!memoryOrb.IsRotaryButtonPressed(h))
        {
            int index = (int) h;
            if (t[index] >= lerpColorDuration)
            {
                t[index] = 0f;
                StartCoroutine(LerpColor(rotaryCapsRenderer[index].material, index));
            } else {
                t[index] = 0f;
            }
        }
    }

    private void MemoryOrb_OnPotentiometerChangeState(Potentiometer p, int value)
    {
        Debug.Log("Potentiometer " + p + " " + value);
        if (p == Potentiometer.Slide)
        {
            float xslide = (0.03f * (value + 1.0f)) / 100.0f;
            memoryOrbLeft.localPosition = new Vector3(xslide *-1.0f, memoryOrbLeft.localPosition.y, memoryOrbLeft.localPosition.z);
            memoryOrbRight.localPosition = new Vector3(xslide, memoryOrbRight.localPosition.y, memoryOrbRight.localPosition.z);
        } else // Circular 
        {
            var circularAngle = memoryOrbLeft.localRotation.eulerAngles;
            circularAngle.x = (value-50)*-1.8f;
            memoryOrbLeft.localRotation = Quaternion.Euler(circularAngle);
        }
    }

    IEnumerator LerpColor(Material m, int tIndex)
    {
        while (t[tIndex] < lerpColorDuration)
        {
            t[tIndex] += Time.deltaTime;
            m.color = Color.Lerp(Color.cyan, Color.black, t[tIndex] / lerpColorDuration);
            yield return null;
        }
    }
}
