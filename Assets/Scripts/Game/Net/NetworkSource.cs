using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using static Net.NetworkSourceServer;

namespace Net
{
    public class NetworkSource
    {
        public delegate void OnDataReceived(List<byte[]> packages);
        public delegate void OnConnected();

        private const bool IS_DEBUG = true;
        private const int PACKAGE_SIZE = 1024;
        public string IP { get; private set; }
        public string PORT { get; private set; }

        private List<byte[]> packages;

        private readonly OnDataReceived onDataReceived;
        private readonly OnConnected onConnected;
        private bool isOverTransmission = false;
        private bool isConnected = false;
        private bool isClosed = false;

        private Socket socket;

        public NetworkSource(string IP, string PORT, OnConnected onConnected, OnDataReceived onDataReceived)
        {
            this.IP = IP;
            this.PORT = PORT;
            packages = new List<byte[]>();
            this.onDataReceived = onDataReceived;
            this.onConnected = onConnected;

            socket = new Socket(IPAddress.Any.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            StartReception();
        }

        public void StartReception()
        {
            Task _ = receptionThread(this);
        }

        public void Update()
        {
            if (isOverTransmission)
            {
                onDataReceived(packages);
                isOverTransmission = false;
                packages.Clear();
            }

            if (isConnected)
            {
                onConnected();
                isConnected = false;
            }
        }

        public void Connect()
        {
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(IP), int.Parse(PORT));
            socket.Connect(remoteEP);
        }

        public void Connect(string ip, string port)
        {
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(ip), int.Parse(port));
            socket.Connect(remoteEP);
        }

        public void Disconnect()
        {
            socket.Close();
            socket = null;
            isClosed = true;
        }

        public static async Task receptionThread(NetworkSource self)
        {
            while (!self.socket.Connected)
            {
                await Task.Delay(50);
            }

            self.isConnected = true;

            while (true)
            {
                while (true)
                {
                    try
                    {
                        if (self.socket.Available != 0)
                            break;
                    }
                    catch(Exception e)
                    {
                        return;
                    }

                    await Task.Delay(50);
                }

                while (self.isOverTransmission)
                { await Task.Delay(50); }

                try
                {
                    if (self.socket.Available == 0)
                    {
                        continue;
                    }
                }
                catch
                {
                    return;
                }

                List<object> receivedBytes = new List<object>();
                List<byte> currentBytes = new List<byte>();

                Codes currentInputType = Codes.nullData;
                bool isStartWrite = false;
                bool isSignallFull = false;

                ulong bytesLeft = 0;
                bool isBreak = false;

                while (true)
                {
                    while (self.socket.Available == 0)
                    { await Task.Delay(50); }

                    byte[] buffer = new byte[PACKAGE_SIZE];
                    int bytesReaded = self.socket.Receive(buffer, PACKAGE_SIZE, SocketFlags.None);

                    for (int j = 0; j < bytesReaded; j++)
                    {
                        currentBytes.Add(buffer[j]);

                        if (currentBytes.Count >= 16 && !isStartWrite)
                        {

                            float[] codes = Utils.ByteArrToFloat(currentBytes.GetRange(currentBytes.Count - 16, 8).ToArray());
                            if (codes[0] == (float)Codes.byteInput)
                            {
                                bytesLeft = BitConverter.ToUInt64(currentBytes.GetRange(currentBytes.Count - 8, 8).ToArray(), 0);
                                currentBytes.Clear();
                                currentInputType = Codes.byteInput;

                                isStartWrite = true;

                                if (codes[1] == (float)Codes.OverTransmission)
                                    isSignallFull = true;
                                else
                                    isSignallFull = false;

                                continue;
                            }
                        }
                        else
                            if (bytesLeft > 0)
                            bytesLeft--;

                        if (bytesLeft <= 0 && isStartWrite)
                        {
                            if (isSignallFull)
                            {
                                if (currentInputType == Codes.byteInput)
                                {
                                    receivedBytes.Add(new List<byte>(currentBytes));
                                    currentBytes.Clear();
                                    currentInputType = Codes.nullData;
                                    isStartWrite = false;
                                }
                                isBreak = true;
                                break;
                            }
                            else
                            {
                                if (currentInputType == Codes.byteInput)
                                {
                                    receivedBytes.Add(new List<byte>(currentBytes));
                                    currentBytes.Clear();
                                    currentInputType = Codes.nullData;
                                    isStartWrite = false;
                                    continue;
                                }
                            }
                        }
                    }

                    if (isBreak)
                        break;
                }

                for (int i = 0; i < receivedBytes.Count; i++)
                {
                    if (receivedBytes[i].GetType() == typeof(List<byte>))
                    {
                        if (IS_DEBUG)
                            Console.WriteLine("New Byte Package --> " + ((List<byte>)receivedBytes[i]).Count);

                        self.packages.Add(((List<byte>)receivedBytes[i]).ToArray());
                    }
                }

                self.isOverTransmission = true;

                if (IS_DEBUG)
                    Console.WriteLine("OverTransmission | Received packages --> " + self.packages.Count);
            }
        }

        public void SendData(byte[] data, bool isOverTransmission)
        {
            try
            {
                if (socket == null)
                    throw new Exception("tcpClient not connected!");

                List<byte> newData = new List<byte>();
                newData.AddRange(Utils.FloatArrToByte(new float[] { (int)Codes.byteInput }));

                if (isOverTransmission)
                    newData.AddRange(BitConverter.GetBytes((float)Codes.OverTransmission));
                else
                    newData.AddRange(BitConverter.GetBytes(0f));

                newData.AddRange(BitConverter.GetBytes((ulong)data.Length));

                newData.AddRange(data);

                if (IS_DEBUG)
                    Console.WriteLine("Send Byte Data count --> " + data.Length);

                List<List<byte>> packages = getPackeges(newData);

                for (int i = 0; i < packages.Count; i++)
                {
                    socket.Send(packages[i].ToArray(), PACKAGE_SIZE, SocketFlags.None);
                }

                List<List<byte>> getPackeges(List<byte> arr1)
                {
                    List<List<byte>> packets1 = new List<List<byte>>();

                    for (int j = 0; j < arr1.Count;)
                    {
                        List<byte> temp = new List<byte>();
                        for (int i = 0; i < PACKAGE_SIZE; i++, j++)
                        {
                            if (j >= arr1.Count)
                                break;

                            temp.Add(arr1[j]);
                        }

                        while (temp.Count < PACKAGE_SIZE)
                        {
                            temp.Add(0);
                        }

                        packets1.Add(temp);
                    }
                    return packets1;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public enum Codes
        {
            endOfReceiving = -999,
            nullData = -1000,
            OverTransmission = -1001,
            byteInput = -998
        }
    }
}