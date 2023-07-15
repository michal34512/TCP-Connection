using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TCP_Connection
{
    public class Connection
    {
        public enum ConnectionRole
        {
            Idle, Server, Client
        }

        private static int _bufferSize = 1024; //in bytes
        private static byte[] _buffer; //inaccessable data

        private static ConnectionRole _Role = ConnectionRole.Idle;
        /// <returns>
        /// Current connection state:
        /// <para>Idle -> client not connected</para>
        /// <para>Server -> server mode</para>
        /// <para>Client -> client mode</para>
        /// </returns>
        public static ConnectionRole Role
        {
            get
            {
                return _Role;
            }
        }

        private static TcpClient _TcpClient;
        private static NetworkStream _NetworkStream;
        private static TcpListener _listener;

        //Message box
        private static List<byte[]> _MessBox = new List<byte[]>();
        private static int _Port = 7777;
        private static string _IpServ = "127.0.0.1";


        // PUBLIC FUNCTIONS
        /// <returns> Returns port value for both server and client mode. If it has not been previously set, returns 7777.</returns>
        public static int Port
        {
            get
            {
                return _Port;
            }
            set
            {
                _Port = value;
            }
        }
        /// <returns> Returns IP address of a server you are trying connect to. If it has not been previously set, returns 127.0.0.1.</returns>
        public static string IpServ
        {
            get
            {
                return _IpServ;
            }
            set
            {
                _IpServ = value;
            }
        }
        /// <summary> Function used to establish connection (for both server and client) </summary>
        /// <param name="role">Connection mode: Server / Client</param>
        public static void Start_Connection(ConnectionRole role)
        {
            _Stop();
            try
            {
                if (role == ConnectionRole.Server)
                {
                    Debug.WriteLine("Starting server at: " + IPAddress.Any.ToString() + ":" + Port.ToString());
                    _listener = new TcpListener(IPAddress.Any, Port);
                    _listener.Start();
                    _listener.BeginAcceptTcpClient(_Server_Callback, null); // Calling "_Server_Callback" when client connect
                }
                else if (role == ConnectionRole.Client)
                {
                    Debug.WriteLine("Connecting at: " + IpServ + ":" + Port.ToString());
                    _TcpClient = new TcpClient();
                    _TcpClient.BeginConnect(IpServ, Port, _Client_Callback, null); //Connecting to server
                }
            }
            catch (System.Exception ex)
            {
                Debug.Fail("Error while starting the connection. Error Code: " + ex);
            }
        }
        /// <summary> Stopping current connection & setting Role to "Idle"</summary>
        public static void Stop_Connection()
        {
            _Stop();
        }
        /// <returns> If connection is active returns true</returns>
        public static bool isConnected
        {
            get
            {
                try
                {
                    if (_TcpClient != null && _TcpClient.Client != null && _TcpClient.Client.Connected)
                    {
                        if (_TcpClient.Client.Poll(0, SelectMode.SelectRead))// Detect if client disconnected
                        {
                            byte[] buff = new byte[1];
                            if (_TcpClient.Client.Receive(buff, SocketFlags.Peek) == 0)
                                return false;
                            else
                                return true;
                        }
                        return true;
                    }
                    else
                        return false;
                }
                catch
                {
                    return false;
                }
            }
        }
        /// <returns> Buffer size for data exchange</returns>
        public static int BufferSize
        {
            get
            {
                return _bufferSize;
            }
            set
            {
                _bufferSize = value;
                _TcpClient.ReceiveBufferSize = value; //setting receiving buffer size
                _TcpClient.SendBufferSize = value; //setting sending buffer size
                _buffer = new byte[value]; //allocating new buffer
            }
        }
        /// <returns> Last messages if there were any, else returns null</returns>
        public static List<byte[]> ReceiveMessages()
        {
            if (_MessBox.Count > 0)
            {
                List<byte[]> copy = _MessBox;
                _MessBox = new List<byte[]>();
                return copy;
            }
            return null;
        }
        /// <summary> Sends message to client/server</summary>
        /// <param name="message">message encoded in byte array</param>
        public static void SendMessage(byte[] message)
        {
            try
            {
                if (_TcpClient != null)
                {
                    _NetworkStream.BeginWrite(message, 0, message.Length, null, null);
                }
                else Debug.Fail("Error while sending data. Not connected!");
            }
            catch (System.Exception ex)
            {
                Debug.Fail("Error while sending data. Error Code: " + ex);
            }
        }

        // PRIVATE FUNCTIONS
        private static void _Stop()
        {
            try
            {
                Debug.WriteLine("Stopping connection...");
                _Role = ConnectionRole.Idle;
                if (_listener != null)
                    _listener.Stop();
                if (_TcpClient != null)
                    _TcpClient.Close();
                if (_NetworkStream != null)
                   _NetworkStream.Close();
            }
            catch (System.Exception ex)
            {
                Debug.Fail("Error while stopping the connection. Error Code: " + ex);
            }
        }
        private static void _Server_Callback(System.IAsyncResult Result)
        {
            _TcpClient = _listener.EndAcceptTcpClient(Result);
            if (_TcpClient.Connected) // Checking if connected
            {
                _Role = ConnectionRole.Server;
                Debug.WriteLine("Client " + _TcpClient.Client.RemoteEndPoint.AddressFamily.ToString() + " connected to the server");// Print ip
                _listener.BeginAcceptTcpClient(new System.AsyncCallback(_Server_Callback), null);
                _Start_Listening();
            }
        }
        private static void _Client_Callback(System.IAsyncResult Result)
        {
            _TcpClient.EndConnect(Result);
            if (_TcpClient.Connected) // Checking if connected
            {
                Debug.WriteLine("Connected to Server!");
                _Role = ConnectionRole.Client;
                _Start_Listening();
            }
            else
            {
                Debug.Fail("Error while connecting to server!");
                return;
            }
        }
        private static void _Start_Listening()
        {
            _TcpClient.ReceiveBufferSize = _bufferSize; //setting receiving buffer size
            _TcpClient.SendBufferSize = _bufferSize; //setting sending buffer size
            _buffer = new byte[_bufferSize]; //allocating new buffer
            _NetworkStream = _TcpClient.GetStream();
            _NetworkStream.BeginRead(_buffer, 0, _bufferSize, new System.AsyncCallback(_Receive_Data), null); //Calling "Server_Receive_Data" when recieve data
        }
        private static void _Receive_Data(System.IAsyncResult Result)
        {
            try
            {
                if (_NetworkStream != null)
                {
                    int ReceivedDataLength = _NetworkStream.EndRead(Result); //How many bytes was received
                    if (ReceivedDataLength <= 0)
                    {
                        Debug.WriteLine("Warrning - empty or broken data");
                        return;
                    }
                    _NetworkStream.BeginRead(_buffer, 0, _bufferSize, new System.AsyncCallback(_Receive_Data), null); //Listening for next data
                                                                                                                      //Saving to message box
                    _MessBox.Add(_buffer);
                }
            }
            catch (System.Exception ex)
            {
                if (!ex.Message.Contains("Cannot access a disposed object.")) //Error not to wory about
                    Debug.Fail("Error while receiving data. Error Code: " + ex);
            }
        }
    }
}
