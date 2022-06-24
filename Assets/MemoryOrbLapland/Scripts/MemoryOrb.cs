using System;
using System.Collections;
using System.Collections.Generic;

namespace MemoryOrbLapland
{
    public enum Hand
    {
        Left,
        Right
    }

    public enum Finger
    {
        Thumb,
        Index,
        Middle,
        Ring,
        Little
    }

    public enum Direction
    {
        CounterClockwise,
        Clockwise
    }

    public enum Potentiometer
    {
        Circular,
        Slide
    }

    public enum ButtonState
    {
        Pressed,
        Released
    }

    public class MemoryOrb
    {
        public event Action<Hand, Finger, ButtonState>
            OnButtonChangeState = delegate {};

        public event Action<Hand, ButtonState>
            OnRotaryButtonChangeState = delegate {};

        public event Action<Hand, Direction>
            OnRotaryEncoderChangeState = delegate {};

        public event Action<Potentiometer, int>
            OnPotentiometerChangeState = delegate {};

        private bool[][] isButtonPressed;

        private bool[] isRotaryButtonPressed;

        private int[] potentiometerValues;

        private int[] potentiometerDelta;

        public MemoryOrb()
        {
            isButtonPressed = new bool[2][];
            isButtonPressed[0] = new bool[5];
            isButtonPressed[1] = new bool[5];
            isRotaryButtonPressed = new bool[2];
            potentiometerValues = new int[2] { -1, -1 };
            potentiometerDelta = new int[2] { -1, -1 };
        }

        public bool IsAnyButtonPressed()
        {
            foreach (bool b in isButtonPressed[0])
                if (b)
                    return true;
            foreach (bool b in isButtonPressed[1])
                if (b)
                    return true;
            return false;
        }

        public bool IsButtonPressed(Hand h, Finger f)
        {
            return isButtonPressed[(int) h][(int) f];
        }

        public bool IsRotaryButtonPressed(Hand h)
        {
            return isRotaryButtonPressed[(int) h];
        }

        public int GetPotentiometerValue(Potentiometer p)
        {
            return potentiometerValues[(int) p];
        }

        public int GetPotentiometerDelta(Potentiometer p)
        {
            return potentiometerDelta[(int) p];
        }

        public void Feed(string message)
        {
            string[] data =
                message
                    .TrimEnd()
                    .Replace("\r", "")
                    .Replace("\n", "")
                    .Split(';');
            for (int i = 0; i < data.Length - 1; i++)
            {
                string[] subdata = data[i].Split(':');
                switch (subdata[0])
                {
                    case "B": // Button
                        int buttonID = int.Parse(subdata[1]);
                        ButtonState state = (ButtonState) int.Parse(subdata[2]);
                        if (buttonID >= 10)
                        {
                            isRotaryButtonPressed[(buttonID % 10)] =
                                (state == ButtonState.Pressed);
                            OnRotaryButtonChangeState?
                                .Invoke((Hand)(buttonID % 10), state);
                        }
                        else
                        {
                            isButtonPressed[buttonID < 5 ? 0 : 1][buttonID %
                            5] = state == ButtonState.Pressed;
                            OnButtonChangeState?
                                .Invoke(buttonID < 5 ? Hand.Left : Hand.Right,
                                (Finger)(buttonID % 5),
                                state);
                        }
                        break;
                    case "R": // RotaryEncoder
                        OnRotaryEncoderChangeState?
                            .Invoke((Hand) int.Parse(subdata[1]),
                            (Direction) int.Parse(subdata[2]));
                        break;
                    case "P": // Potentiometer
                        int potentiometerId = int.Parse(subdata[1]);
                        int value = int.Parse(subdata[2]) * 100 / 1024;
                        potentiometerDelta[potentiometerId] = potentiometerValues[potentiometerId] - value;
                        potentiometerValues[potentiometerId] = value;
                        OnPotentiometerChangeState?
                            .Invoke((Potentiometer) potentiometerId, value);
                        break;
                }
            }
        }
    }
}
