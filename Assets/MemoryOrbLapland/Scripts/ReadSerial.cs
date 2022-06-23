using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO.Ports;
using System.Threading;

namespace MemoryOrbLapland
{

    public class ReadSerial : MonoBehaviour 
    {
        public enum Baudrate 
        {
            b300 = 300, b600 = 600, b1200 = 1200, b2400 = 2400, b4800 = 4800, b9600 = 9600, b14400 = 14400, b19200 = 19200, b28800 = 28800, b38400 = 38400, b57600 = 57600, b115200 = 115200
        }

        public event Action<string> OnRead = delegate { };

        private SerialPort _serialPort;
        public string port;
        public Baudrate baudrate = Baudrate.b9600;

        [Tooltip("Value in milliseconds.")]
        public int readTimeout = 500;
        
        [Tooltip("Value in milliseconds.")]
        public int writeTimeout = 500;
        private bool _continue = false;

        Thread readThread;

        void Start () 
        {
            readThread = new Thread(Read);

            _serialPort = new SerialPort(port, Convert.ToInt32(baudrate));
            _serialPort.ReadTimeout = readTimeout;
            _serialPort.WriteTimeout = writeTimeout;
            _serialPort.Open();

            _continue = true;

            readThread.Start();
        }

        void OnDestroy()
        {
            readThread.Abort();
            _serialPort.Close();
        }


        public void Read()
        {
            while (_continue)
            {
                try
                {
                    string message = _serialPort.ReadTo(";");
                    UnityMainThreadDispatcher.Instance().Enqueue(ThisWillBeExecutedOnTheMainThread(message));
                }
                catch (TimeoutException)
                {

                }
            }
        }

        public IEnumerator ThisWillBeExecutedOnTheMainThread(string message)
        {
            // Debug.Log("ReadSerial: " + message);
            OnRead(message);
            yield return null;
        }
    }
}