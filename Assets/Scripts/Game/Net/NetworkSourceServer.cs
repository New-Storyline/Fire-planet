using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Net
{
    public class NetworkSourceServer
    {
        public delegate void OnDataReceived(ConnectedPC pc);
        public delegate void OnNewConnection(ConnectedPC pc);
        public delegate void OnConnectionLost(ConnectedPC pc);

        private const bool IS_DEBUG = true;
        private const int TIME_OUT = 10000000;
        private const int PACKAGE_SIZE = 1024;

        public string PORT { get; private set; }

        private Socket socket;
        private OnDataReceived onDataReceived;
        private OnNewConnection onNewConnection;
        private OnConnectionLost onConnectionLost;

        private List<ConnectedPC> connectedPCs = new List<ConnectedPC>();
        private List<Thread> receptionThreads = new List<Thread>();

        public class ConnectedPC
        {
            public Socket handler;
            public string IP;
            public int ID;
            public List<byte[]> packages;

            public int lastResponseTime;
            public bool isOverTransmission;
            public bool isStartedTransmission;

            public ConnectedPC(Socket handler, string IP, int ID)
            {
                this.handler = handler;
                this.IP = IP;
                this.ID = ID;
                lastResponseTime = Environment.TickCount;
                isStartedTransmission = true;
                packages = new List<byte[]>();
                isOverTransmission = false;
            }
        }

        public NetworkSourceServer(string PORT, OnDataReceived onDataReceived, OnNewConnection onNewConnection, OnConnectionLost onConnectionLost)
        {
            this.PORT = PORT;

            this.onDataReceived = onDataReceived;
            this.onNewConnection = onNewConnection;
            this.onConnectionLost = onConnectionLost;

            socket = new Socket(IPAddress.Any.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            StartReception();
        }

        public void Update() {

            for (int i = 0; i < connectedPCs.Count; i++)
            {
                ConnectedPC pc = connectedPCs[i];
                if (pc.isOverTransmission)
                {
                    onDataReceived(pc);
                    pc.packages.Clear();
                    pc.lastResponseTime = Environment.TickCount;
                    pc.isOverTransmission = false;
                }

                if (pc.isStartedTransmission)
                {
                    onNewConnection(pc);
                    pc.isStartedTransmission = false;
                }

                if (Environment.TickCount - pc.lastResponseTime > TIME_OUT)
                {
                    Disconnect(pc);
                    i--;
                }
            }
        }

        public void Disconnect(ConnectedPC pc)
        {
            int i = FindConnectedPCIndex(connectedPCs, pc);
            receptionThreads.RemoveAt(i);

            try
            {
                connectedPCs[i].handler.Close();
            }
            catch
            { }

            connectedPCs.RemoveAt(i);
            onConnectionLost(pc);
        }

        public bool SendData(byte[] data, ConnectedPC pc, bool isOverTransmission)
        {
            try
            {
                Socket currentSocket = FindConnectedPC(connectedPCs, pc).handler;

                if (currentSocket == null)
                {
                    throw new Exception("[SendData()] handler not exsist!");
                }

                List<byte> newData = new List<byte>();
                newData.AddRange(Utils.FloatArrToByte(new float[] { (int)Codes.byteInput }));

                if (isOverTransmission)
                    newData.AddRange(BitConverter.GetBytes((float)Codes.OverTransmission));
                else
                    newData.AddRange(BitConverter.GetBytes(0f));

                newData.AddRange(BitConverter.GetBytes((ulong)data.Length));

                newData.AddRange(data);

                if (IS_DEBUG)
                    Console.WriteLine("Send Data count --> " + newData.Count);

                List<List<byte>> packages = getPackeges(newData);

                for (int i = 0; i < packages.Count; i++)
                {
                    Console.WriteLine("Send Data package --> " + packages[i].Count);
                    currentSocket.Send(packages[i].ToArray(), PACKAGE_SIZE, SocketFlags.None);
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
            catch
            {
                return false;
            }
            return true;
        }

        private void StartReception()
        {
            initSocket();
            StartNewReceptionThread();
        }

        private void StartNewReceptionThread()
        {
            Thread thread = new Thread(() => receptionThreadServer(this));
            thread.Start();
            receptionThreads.Add(thread);
        }

        private void initSocket()
        {

            IPAddress bindAddress = IPAddress.Any;
            IPEndPoint bindEndPoint = new IPEndPoint(bindAddress, int.Parse(PORT));

            socket = new Socket(IPAddress.Any.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            //socket.SetIPProtectionLevel(IPProtectionLevel.Unrestricted);
            socket.Bind(bindEndPoint);
            socket.Listen(10);

        }

        private static void receptionThreadServer(NetworkSourceServer self)
        {
            Socket handler = self.socket.Accept();
            IPEndPoint endP = handler.RemoteEndPoint as IPEndPoint;
            string EndpIP = endP.Address.ToString();
            ConnectedPC pc = new ConnectedPC(handler, EndpIP, self.FindFreePCID());
            self.connectedPCs.Add(pc);

            self.StartNewReceptionThread();

            while (true)
            {
                while (true)
                {
                    try
                    {
                        if (handler.Available != 0)
                            break;
                    }
                    catch
                    {
                        return;
                    }
                }

                while (pc.isOverTransmission)
                { }

                try
                {
                    if (handler.Available == 0)
                        continue;
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
                    try
                    {
                        if (handler.Available == 0)
                        {
                            continue;
                        }
                    }
                    catch
                    {
                        return;
                    }

                    byte[] buffer = new byte[PACKAGE_SIZE];
                    int bytesReaded = handler.Receive(buffer, PACKAGE_SIZE, SocketFlags.None);

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
                    if (receivedBytes[i].GetType() == typeof(List<byte>))
                    {
                        if (IS_DEBUG)
                            Console.WriteLine("New Byte Package --> " + ((List<byte>)receivedBytes[i]).Count);

                        pc.packages.Add(((List<byte>)receivedBytes[i]).ToArray());
                    }
                pc.isOverTransmission = true;

                if (IS_DEBUG)
                    Console.WriteLine("OverTransmission |  Received byte packages --> " + pc.packages.Count);
            }

        }

        public List<ConnectedPC> GetConnectedPCs() { return connectedPCs; }

        #region Utils
        private int FindFreePCID()
        {
            bool isIDFree(int id)
            {

                foreach (ConnectedPC pc in connectedPCs)
                    if (pc.ID == id)
                        return false;
                return true;
            }

            for (int i = 0; i < 999999; i++)
                if (isIDFree(i))
                    return i;

            throw new Exception("No free ID found!");
        }
        private static ConnectedPC FindConnectedPC(List<ConnectedPC> arr, ConnectedPC pc)
        {

            for (int i = 0; i < arr.Count; i++)
                if (arr[i].IP == pc.IP && arr[i].ID == pc.ID)
                    return arr[i];

            throw new Exception($"ConnectedPC not found! IP: {pc.IP}, ID: {pc.ID}");
        }
        private static int FindConnectedPCIndex(List<ConnectedPC> arr, ConnectedPC pc)
        {
            for (int i = 0; i < arr.Count; i++)
                if (arr[i].IP == pc.IP && arr[i].ID == pc.ID)
                    return i;

            throw new Exception($"ConnectedPC index not found! IP: {pc.IP}, ID: {pc.ID}");
        }
        public static int FindOpenPort(int startport, string ip)
        {
            using (TcpClient tcpClient = new TcpClient())
            {
                for (int port = startport; port < 48000; port++)
                {
                    try
                    {
                        tcpClient.Connect("127.0.0.1", port);
                        return port;
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }
            return -1;
        }

        #endregion

        public enum Codes
        {
            endOfReceiving = -999,
            nullData = -1000,
            OverTransmission = -1001,
            byteInput = -998,
        }

    }
}