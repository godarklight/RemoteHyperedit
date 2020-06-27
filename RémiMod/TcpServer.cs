using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RémiMod
{
    public class TcpServer
    {
        private const int BUFFER_SIZE = 16000;
        private TcpListener tcpListener;
        private List<TcpClient> tcpClients = new List<TcpClient>();
        private Queue<string> messages = new Queue<string>();

        public TcpServer(int port)
        {
            //Use IPAddress.Any for IPv4 only
            tcpListener = new TcpListener(new IPEndPoint(IPAddress.IPv6Any, port));
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(HandleConnect, null);
        }

        private void HandleConnect(IAsyncResult ar)
        {
            try
            {
                ClientObject co = new ClientObject();
                co.client = tcpListener.EndAcceptTcpClient(ar);
                co.buffer = new byte[BUFFER_SIZE];
                co.writePos = 0;
                co.client.GetStream().BeginRead(co.buffer, co.writePos, co.buffer.Length - co.writePos, HandleRead, co);
            }
            catch (Exception e)
            {
                Debug.Log("Error accepting client: " + e);
            }
            tcpListener.BeginAcceptTcpClient(HandleConnect, null);
        }

        private void HandleRead(IAsyncResult ar)
        {
            ClientObject co = ar.AsyncState as ClientObject;
            try
            {
                int readBytes = co.client.GetStream().EndRead(ar);
                int oldWritePos = co.writePos;
                co.writePos += readBytes;
                if (readBytes == 0)
                {
                    Debug.Log("Client disconnected");
                    return;
                }
                int newLineCopy = -1;
                for (int i = oldWritePos; i < (oldWritePos + readBytes); i++)
                {
                    if (newLineCopy == -1)
                    {
                        int offset = 0;
                        if (i > 1 && co.buffer[i - 1] == 13)
                        {
                            offset = 1;
                        }
                        if (co.buffer[i] == 10)
                        {
                            string newMessage = Encoding.UTF8.GetString(co.buffer, 0, i - offset);
                            lock (messages)
                            {
                                messages.Enqueue(newMessage);
                            }
                            //Message found, reset it back
                            newLineCopy = 0;
                            co.writePos = 0;
                        }
                    }
                    else
                    {
                        //Copy left over data to start of the array
                        co.buffer[newLineCopy] = co.buffer[i];
                        co.writePos++;
                    }
                }

            }
            catch (Exception e)
            {
                Debug.Log("Error reading client: " + e);
                return;
            }
            co.client.GetStream().BeginRead(co.buffer, co.writePos, co.buffer.Length - co.writePos, HandleRead, co);
        }

        public void Pump(Dictionary<Guid, Queue<VesselUpdate>> vesselUpdates)
        {
            lock (messages)
            {
                while (messages.Count > 0)
                {
                    string currentMessage = messages.Dequeue();
                    int firstSpace = currentMessage.IndexOf(' ');
                    string command = currentMessage.Substring(0, firstSpace);
                    string data = currentMessage.Substring(firstSpace + 1);

                    switch (command)
                    {
                        case "TIME":
                            double universeTime = double.Parse(data);
                            Planetarium.SetUniversalTime(universeTime);
                            break;
                        case "ORBIT":
                            string[] orbitDataString = data.Split(' ');
                            double[] orbitData = new double[6];
                            for (int i = 0; i < 6; i++)
                            {
                                orbitData[i] = double.Parse(orbitDataString[i]);
                            }
                            break;
                        case "POS":
                            string[] postDataString = data.Split(' ');
                            double[] posData = new double[6];
                            for (int i = 0; i < 6; i++)
                            {
                                posData[i] = double.Parse(postDataString[i]);
                            }
                            break;
                    }
                }
            }
        }
    }
}
