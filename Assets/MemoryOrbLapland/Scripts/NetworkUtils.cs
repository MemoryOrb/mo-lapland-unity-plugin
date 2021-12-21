using System.Collections;
using System.Net.Sockets;
using System.Net;
using UnityEngine;
using System.Threading;
using System.Text;
using System;

namespace MemoryOrbLapland
{
    
    public class NetworkUtils
    {
        public event Action<string> OnMessageReceived = delegate { };
        
        private TcpListener _tcpListener;
        private Thread _thread;
        private TcpClient _tcpClient;
        
        public void StartServer(string portNumber)
        {
            _tcpListener = new TcpListener(IPAddress.Any, int.Parse(portNumber));
        }

        public void StopServer()
        {
            _tcpListener.Stop();
        }

        public void Listen()
        {
            _tcpListener.Start();
            _thread = new Thread (new ThreadStart(ThreadListen));
            _thread.IsBackground = true;
            _thread.Start();
        }

        private void ThreadListen()
        {
            try 
            {
                Byte[] bytes = new Byte[1024];
                
                _tcpClient = _tcpListener.AcceptTcpClient();
                
                NetworkStream stream = _tcpClient.GetStream();
                _tcpListener.Stop();
                int i;
                while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(SendMessageToMainThread(
                        Encoding.ASCII.GetString(bytes, 0, i)));
                }
            }
            catch (SocketException socketException)
            {
                Debug.Log("SocketException " + socketException);
            }
            finally
            {
                _tcpClient.Close();
            }
        }

        public void SendMessage(string message)
        {
            if (_tcpClient == null)
            {
                return;
            }

            try
            {
                NetworkStream stream = _tcpClient.GetStream();
                if (stream.CanWrite) {
                    byte[] serverMessageAsByteArray = Encoding.ASCII.GetBytes(message);
                    stream.Write(serverMessageAsByteArray, 0, serverMessageAsByteArray.Length);
                }
            }
            catch (SocketException socketException)
            {
                Debug.Log("SocketException: " + socketException);
            }
        }

        private IEnumerator SendMessageToMainThread(string message)
        {
            OnMessageReceived?.Invoke(message);
            yield return null;
        }
    }

}