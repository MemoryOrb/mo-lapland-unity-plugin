using System.Collections;
using System.Collections.Generic;
using System;

namespace MemoryOrbLapland 
{
    public enum Hand {Left, Right};
    public enum Finger {Thumb, Index, Middle, Ring, Little};
    public enum Direction {CounterClockwise, Clockwise};
    public enum Potentiometer {Circular, Slide};
    public enum ButtonState {Pressed, Released}

    public class MemoryOrb
    {
        public event Action<Hand, Finger, ButtonState> OnButtonChangeState = delegate { };
        public event Action<Hand, ButtonState> OnRotaryButtonChangeState = delegate { };
        public event Action<Hand, Direction> OnRotaryEncoderChangeState = delegate { };
        public event Action<Potentiometer, int> OnPotentiometerChangeState = delegate { };

        public void Feed(string message)
        {
            string[] data = message.TrimEnd().Split(';');
            for (int i = 0; i < data.Length-1; i++)
            {
                string[] subdata = data[i].Split(':');
                switch (subdata[0])
                {
                    case "B": // Button
                        int buttonID = int.Parse(subdata[1]);
                        if (buttonID >= 10)
                        {
                            OnRotaryButtonChangeState?.Invoke(
                                (Hand) (buttonID % 10),
                                (ButtonState) int.Parse(subdata[2])
                            );
                        }
                        else if (buttonID >= 5)
                        {
                            OnButtonChangeState?.Invoke(
                                Hand.Left,
                                (Finger) (9 - buttonID),
                                (ButtonState) int.Parse(subdata[2])
                            );
                        }
                        else 
                        {
                            OnButtonChangeState?.Invoke(
                                Hand.Right,
                                (Finger) buttonID,
                                (ButtonState) int.Parse(subdata[2])
                            );
                        }
                    break;
                    case "R": // RotaryEncoder
                        OnRotaryEncoderChangeState?.Invoke(
                            (Hand) int.Parse(subdata[1]),
                            (Direction) int.Parse(subdata[2])
                        );
                    break;
                    case "P": // Potentiometer
                        OnPotentiometerChangeState?.Invoke(
                            (Potentiometer) int.Parse(subdata[1]), 
                            int.Parse(subdata[2])
                        );
                    break;
                }
            }
        }
    }
}