using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FZ4P
{
    abstract class SocketInterface
    {

        public enum TypeOfString
        {
            UNICODE,
            UTF8
        }


        public delegate void ReceiveMessage(string IPAddress, string sMessage);
        /// <summary>
        /// void Receive(string ip, string msg);
        /// </summary>
        public event ReceiveMessage ReceiveEvent;

        public delegate void ConnectedStateMessage(string IPAddress);

        /// <summary>
        /// TCP/IP 연결 성공시 이벤트
        /// </summary>
        public event ConnectedStateMessage ConnectedEvent;
        /// <summary>
        /// TCP/IP 연결 해제시 이벤트
        /// </summary>
        public event ConnectedStateMessage DisconnectedEvent;
        TypeOfString stringType;

        abstract public void StartSocket();
        abstract public void EndSocket();
        abstract public void SendMessage(string sMessage);


        protected SocketInterface(TypeOfString type)
        {
            stringType = type;
        }
        ~SocketInterface()
        {
        }
        protected void Receive(string IPAddress, string sMessage)
        {
            if (ReceiveEvent != null)
            {
                ReceiveEvent(IPAddress, sMessage);
            }
        }

        protected byte[] StringToByte(string sString)
        {
            byte[] data = null;
            switch (stringType)
            {
                case TypeOfString.UNICODE:
                    data = Encoding.Unicode.GetBytes(sString);
                    break;
                case TypeOfString.UTF8:
                    data = Encoding.UTF8.GetBytes(sString);//new UTF8Encoding().GetBytes(sString);
                    break;
                default:
                    break;
            }
            return data;
        }

        protected void Connected(string sIP)
        {
            if (ConnectedEvent != null)
            {
                ConnectedEvent(sIP);
            }

        }
        protected void Disconnected(string sIP)
        {
            if (DisconnectedEvent != null)
            {
                DisconnectedEvent(sIP);
            }
        }

        protected string ByteToString(byte[] byteString)
        {
            string sString = null;
            switch (stringType)
            {
                case TypeOfString.UNICODE:
                    sString = Encoding.Unicode.GetString(byteString);
                    break;
                case TypeOfString.UTF8:
                    sString = new UTF8Encoding().GetString(byteString);//Encoding.UTF8.GetString(byteString);//new UTF8Encoding().GetString(byteString);
                    break;
                default:
                    break;
            }
            return sString;
        }
    }
    class MySocketClientClass : SocketInterface
    {

        private Socket m_Client;
        private Socket m_cbSock;

        IPEndPoint ip;
        private byte[] recvBuffer;

        System.Timers.Timer Recon = null;
        bool m_bConnected = false;

        private MySocketClientClass(string sIP, int nPort, TypeOfString type) : base(type)
        {
            ip = new IPEndPoint(IPAddress.Parse(sIP), nPort);

            Recon = new System.Timers.Timer();
            Recon.Interval = 10000;
            Recon.Enabled = false;
            Recon.Elapsed += Recon_Elapsed;

        }

        private void Recon_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Recon.Enabled = false;
            if (m_Client != null)
            {
                m_Client.Dispose();
                m_Client = null;
            }

            StartSocket();
        }

        ~MySocketClientClass()
        {
            EndSocket();
        }

        public static MySocketClientClass CreateClientSocket(string sIP, int nPort, TypeOfString type)
        {
            MySocketClientClass clientObject = new MySocketClientClass(sIP, nPort, type);
            return clientObject;
        }

        public override void StartSocket()
        {
            if (m_Client == null)
            {
                m_Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                recvBuffer = new byte[4096];
                BeginConnect();
            }
        }

        public override void EndSocket()
        {
            if (m_Client == null)
                return;

            try
            {
                m_bConnected = false;
                m_Client.Disconnect(false);
            }
            catch (SocketException e)
            {
                Debug.Write(e.Message);
            }
            finally
            {

                m_Client.Dispose();
                m_Client = null;
            }
        }

        public override void SendMessage(string sMessage)
        {
            if (m_Client != null)
            {
                BeginSend(sMessage);
            }
        }

        //[Obsolete("서버용 기능")]

        private void BeginConnect()
        {
            try
            {
                m_Client.BeginConnect(ip, new AsyncCallback(ConnectCallBack), m_Client);
            }
            catch (SocketException e)
            {

                Debug.Write(e.Message);
                m_Client.Disconnect(false);
                m_Client.Dispose();
                m_Client = null;
                //    StartSocket();
                Recon.Enabled = true;
            }
        }

        private void ConnectCallBack(IAsyncResult IAR)
        {
            try
            {
                Socket sock = (Socket)IAR.AsyncState;
                IPEndPoint serverIP = (IPEndPoint)sock.RemoteEndPoint;
                Debug.Write("Server IP : " + serverIP.Address.ToString());
                sock.EndConnect(IAR);
                m_cbSock = sock;
                m_cbSock.BeginReceive(recvBuffer, 0, recvBuffer.Length, SocketFlags.None, new AsyncCallback(OnReceiveCallBack), m_cbSock);
                m_bConnected = true;

                base.Connected(((IPEndPoint)sock.RemoteEndPoint).Address.ToString());


            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode == SocketError.NotConnected)
                {
                    Debug.Write("Not Connected");
                    m_Client.Dispose();
                    m_Client = null;
                    Recon.Enabled = true;

                }
            }
            catch
            {
                m_bConnected = false;
            }
        }

        private void BeginSend(string sSendData)
        {
            try
            {
                if (m_Client.Connected)
                {


                    byte[] buffer = base.StringToByte(sSendData);
                    m_Client.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(SendCallBack), sSendData);

                }
                else
                {
                    throw new SocketException((int)SocketError.NotConnected);
                }
            }
            catch (SocketException e)
            {
                Debug.Write("Send Fail ErrMsg : " + e.Message);
            }
        }


        private void SendCallBack(IAsyncResult IAR)
        {
            //  string sMsg = (string)IAR.AsyncState;
            //  m_Client.EndSend(IAR);

            Debug.Write(" 전송완료");
        }

        private void Receive()
        {
            m_cbSock.BeginReceive(this.recvBuffer, 0, recvBuffer.Length, SocketFlags.None, new AsyncCallback(OnReceiveCallBack), m_cbSock);
        }
        private void OnReceiveCallBack(IAsyncResult IAR)
        {
            if (!m_bConnected)
            {
                //소켓 해제됨.
                return;
            }
            try
            {
                Socket sock = (Socket)IAR.AsyncState;
                if (!sock.Connected)
                {
                    throw new SocketException((int)SocketError.NotConnected);
                }
                int nReadSize = sock.EndReceive(IAR);
                if (nReadSize != 0)
                {
                    string sData = base.ByteToString(recvBuffer);
                    Array.Clear(recvBuffer, 0, recvBuffer.Length);
                    Debug.Write(sData);
                    base.Receive(((IPEndPoint)sock.RemoteEndPoint).Address.ToString(), sData);

                }
                else
                {
                    base.Disconnected(((IPEndPoint)sock.RemoteEndPoint).Address.ToString());

                    throw new SocketException((int)SocketError.NotConnected);
                }

                Receive();
            }
            catch (SocketException e)
            {


                if (e.SocketErrorCode == SocketError.NotConnected)
                {
                    //BeginConnect();
                    Recon.Enabled = true;
                }
                else if (e.SocketErrorCode == SocketError.ConnectionReset)
                {
                    Recon.Enabled = true;
                }
            }
        }
    }
    class MySocketServerClass : SocketInterface
    {


        private Socket m_Server;
        private List<Socket> m_Client;
        IPEndPoint ip;

        private byte[] szData;

        private MySocketServerClass(int nPort, TypeOfString type) : base(type)
        {
            ip = new IPEndPoint(IPAddress.Any, nPort);
        }

        ~MySocketServerClass()
        {
            EndSocket();
        }

        public static MySocketServerClass CreateServerSocket(int nPort, TypeOfString type)
        {
            MySocketServerClass serverObject = new MySocketServerClass(nPort, type);
            return serverObject;
        }
        public override void StartSocket()
        {
            try
            {
                if (m_Server == null)
                {
                    m_Client = new List<Socket>();
                    m_Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    m_Server.Bind(ip);
                    m_Server.Listen(0);
                    SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                    args.Completed += new EventHandler<SocketAsyncEventArgs>(ConnectedClient);
                    //클라이언트 접속 이벤트 발생대기
                    m_Server.AcceptAsync(args);
                }

            }
            catch
            { }

        }

        public override void EndSocket()
        {
            //if (m_Client == null)
            //    return;

            foreach (Socket socket in m_Client)
            {
                if (socket.Connected)
                {
                    socket.Disconnect(false);

                }

                socket.Dispose();
            }

            if (m_Server != null)
                m_Server.Dispose();
            m_Server = null;
        }

        public override void SendMessage(string sMessage)
        {
            SendToClients(sMessage);
        }

        private void ConnectedClient(object sender, SocketAsyncEventArgs e)
        {
            Socket client = e.AcceptSocket;

            try
            {
                if (m_Client != null)
                {
                    SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                    szData = new byte[4096];
                    args.SetBuffer(szData, 0, 4096);
                    args.UserToken = m_Client;
                    args.Completed += new EventHandler<SocketAsyncEventArgs>(ReceiveData);
                    client.ReceiveAsync(args);
                    m_Client.Add(client);

                    base.Connected(((IPEndPoint)client?.RemoteEndPoint)?.Address?.ToString());

                }
            }
            catch (Exception ee)
            {
                Debug.Write(ee.ToString());
            }
            try
            {
                e.AcceptSocket = null;
                m_Server?.AcceptAsync(e);    //다시 클라이언트 접속 요청 대기
            }
            catch (NullReferenceException eNull)
            {
                Debug.Write(eNull.ToString());
            }
        }

        private void ReceiveData(object sender, SocketAsyncEventArgs e)
        {
            //데이터를 수신하거나 클라이언트의 연결이 끊어지면 호출되는 이벤트
            Socket client = (Socket)sender;
            if (client.Connected && e.BytesTransferred > 0)
            {

                byte[] szData = e.Buffer;

                string sReciveData = base.ByteToString(szData);
                //Buffer 비우기
                Array.Clear(szData, 0, szData.Length);

                base.Receive(((IPEndPoint)client.RemoteEndPoint).Address.ToString(), sReciveData);
                e.SetBuffer(szData, 0, 4096);
                client.ReceiveAsync(e);
            }
            else //연결 끊어짐.
            {
                try
                {
                    if (client.Connected)
                        client.Disconnect(false);
                }
                catch (SocketException eSock)
                {
                    Debug.Write("Socket Error, ErrMsg : " + eSock.ToString());
                }
                finally
                {
                    //base.Disconnected(((IPEndPoint)client.RemoteEndPoint).Address.ToString());
                    //Debug.Write(client.RemoteEndPoint.ToString() + "연결이 끊어짐");
                    try
                    {
                        base.Disconnected(((IPEndPoint)client?.RemoteEndPoint)?.Address?.ToString());
                        m_Client?.Remove(client);
                        client = null;

                    }
                    catch { m_Client?.Remove(client); client = null; }

                }
            }
        }

        private void SendToClients(string sSendData)
        {
            if (m_Client == null)
            {
                return;
            }
            byte[] data = base.StringToByte(sSendData);

            foreach (Socket sock in m_Client)
            {
                sock.Send(data, data.Length, SocketFlags.None);
            }
        }

        private void SendToClient(string sIP, string sSendData)
        {
            if (m_Client == null)
            {
                return;
            }
            byte[] data = base.StringToByte(sSendData);

            var client = from ip in m_Client
                         where ((IPEndPoint)ip.RemoteEndPoint).Address.ToString() == sIP
                         select ip;

            foreach (Socket sock in client)
            {
                sock.Send(data, data.Length, SocketFlags.None);
            }
        }

        public bool isConnectedIP(string ip)
        {
            bool bIsConnected = false;

            for (int i = 0; i < m_Client.Count; i++)
            {
                if (((IPEndPoint)m_Client[i].RemoteEndPoint).Address.ToString().CompareTo(ip) == 0)
                {
                    bIsConnected = true;
                    break;
                }
            }
            return bIsConnected;
        }
    }


    public class HandlerConnection
    {
        SocketInterface conn = null;
        string sendMessage = string.Empty;
        public delegate void ConStatus(bool isCon);
        public event ConStatus OnStatus;

        public delegate void Receive(List<string> data);
        public event Receive OnReceive;
        public List<string> testCnt = new List<string>();
        public void connect(int port)
        {
            disconnect();
            conn = MySocketServerClass.CreateServerSocket(port, SocketInterface.TypeOfString.UTF8);
            conn.StartSocket();
            conn.ConnectedEvent += Conn_ConnectedEvent;
            conn.DisconnectedEvent += Conn_DisconnectedEvent;
            conn.ReceiveEvent += Conn_ReceiveEvent;

        }
        public void connect(string ip, int port)
        {
            disconnect();
            conn = MySocketClientClass.CreateClientSocket(ip ,port, SocketInterface.TypeOfString.UTF8);
            conn.StartSocket();
            conn.ConnectedEvent += Conn_ConnectedEvent;
            conn.DisconnectedEvent += Conn_DisconnectedEvent;
            conn.ReceiveEvent += Conn_ReceiveEvent;

        }

        public void disconnect()
        {
            conn?.EndSocket();
            Thread.Sleep(100);
            conn = null;
        }
        private void Conn_ReceiveEvent(string IPAddress, string sMessage)
        {
            try
            {
                sMessage = sMessage.TrimEnd('\0');
                if (sMessage[0] == (char)36)
                {
                    sendMessage = sMessage;
                    if (sMessage[sMessage.Length - 1] == (char)13)
                    {
                        List<string> splitStr = sendMessage.Split((char)36).ToList();
                        splitStr.RemoveAt(0);
                        OnReceive?.Invoke(splitStr.ToList());
                    }

                }
                else if (sMessage[sMessage.Length - 1] == (char)13)
                {
                    sendMessage += sMessage;
                    List<string> splitStr = sendMessage.Split((char)36).ToList();
                    splitStr.RemoveAt(0);
                    OnReceive?.Invoke(splitStr.ToList());
                }
                else { sendMessage += sMessage; }
            }
            catch
            {
                MessageBox.Show(sMessage);
            }



        }

        private void Conn_DisconnectedEvent(string IPAddress)
        {
            OnStatus?.Invoke(false);
        }

        private void Conn_ConnectedEvent(string IPAddress)
        {
            OnStatus?.Invoke(true);
        }
        public void SendMessage(string s)
        {
            string str = (char)36 + s + (char)13;
            conn.SendMessage(str);
        }
    }
}
