using System.Net.Sockets;
using System.Net;
using System.Text;
using MUD_Skeleton.Server.Controllers;
using MUD_Skeleton.Commons.Comms;
using System.Collections.Concurrent;
using System.Threading.Channels;
using MUD_Skeleton.Commons.Auxiliary;

namespace MUD_Skeleton.Server
{
    internal class Program
    {
        #region Functional Attributes
        static TcpListener ServerListener;

        static List<OnlineClient> l_onlineClients = new List<OnlineClient>();

        static ConcurrentDictionary<string, Thread> cDic_clientThreads = new ConcurrentDictionary<string, Thread>();
        static Thread cThread;
        static Thread dThread;
        static ConcurrentDictionary<string, OnlineClient> cDic_tcpClientsReceived = new ConcurrentDictionary<string, OnlineClient>();
        #endregion

        static void Main(string[] args)
        {
            MainWithConnectionManager();
        }

        #region ChannelTcpListener
        private static BoundedChannelOptions options = new BoundedChannelOptions(255);

        private static Channel<TcpClient> channelTcpListener = null;
        public static Channel<TcpClient> ChannelTcpListener
        {
            get
            {
                if (channelTcpListener == null)
                {
                    options.FullMode = BoundedChannelFullMode.Wait;
                    channelTcpListener = System.Threading.Channels.Channel.CreateBounded<TcpClient>(options);
                }
                return channelTcpListener;
            }
            set { channelTcpListener = value; }
        }

        private static ChannelWriter<TcpClient> writerTcpListener = null;
        public static ChannelWriter<TcpClient> WriterTcpListener
        {
            get
            {
                if (writerTcpListener == null)
                {
                    writerTcpListener = ChannelTcpListener.Writer;
                }
                return writerTcpListener;
            }
            set => writerTcpListener = value;
        }

        private static ChannelReader<TcpClient> readerTcpListener = null;
        public static ChannelReader<TcpClient> ReaderTcpListener
        {
            get
            {
                if (readerTcpListener == null)
                {
                    readerTcpListener = ChannelTcpListener.Reader;
                }
                return readerTcpListener;
            }
            set => readerTcpListener = value;
        }

        public static async void ReadingChannelTcpListener()
        {
            try
            {
                string tempString = string.Empty;
                while (await ReaderTcpListener.WaitToReadAsync())
                {
                    TcpClient tcpClient = await ReaderTcpListener.ReadAsync();
                    //cq_tcpClientsReceived.Enqueue(tcpClient);
                    string name = "_" + cDic_clientThreads.Count;
                    Thread tempThread = new Thread(() => ConnectConnectionsWithClient(tcpClient, name));
                    tempThread.Start();
                    bool tryAdd = false;
                    do
                    {
                        tryAdd = cDic_clientThreads.TryAdd(name, tempThread);
                    }
                    while (!tryAdd);

                    Console.Out.WriteLine($"ReadingChannelTcpListener {tcpClient.ToString()}");
                }
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine($"Error ReadingChannelTcpListener: {ex.Message}");
            }
        }

        private static Channel<OnlineClient> channelOnlineClient = null;
        public static Channel<OnlineClient> ChannelOnlineClient
        {
            get
            {
                if (channelOnlineClient == null)
                {
                    options.FullMode = BoundedChannelFullMode.Wait;
                    channelOnlineClient = System.Threading.Channels.Channel.CreateBounded<OnlineClient>(options);
                }
                return channelOnlineClient;
            }
            set { channelOnlineClient = value; }
        }

        private static ChannelWriter<OnlineClient> writerOnlineClient = null;
        public static ChannelWriter<OnlineClient> WriterOnlineClient
        {
            get
            {
                if (writerOnlineClient == null)
                {
                    writerOnlineClient = ChannelOnlineClient.Writer;
                }
                return writerOnlineClient;
            }
            set => writerOnlineClient = value;
        }

        private static ChannelReader<OnlineClient> readerOnlineClient = null;
        public static ChannelReader<OnlineClient> ReaderOnlineClient
        {
            get
            {
                if (readerOnlineClient == null)
                {
                    readerOnlineClient = ChannelOnlineClient.Reader;
                }
                return readerOnlineClient;
            }
            set => readerOnlineClient = value;
        }

        public static async void ReadingChannelOnlineClient()
        {
            try
            {
                string tempString = string.Empty;
                while (await ReaderOnlineClient.WaitToReadAsync())
                {
                    OnlineClient onClient = await ReaderOnlineClient.ReadAsync();
                    OnlineClient.cq_tcpOnlineClientsReceived.Enqueue(onClient);
                    //OnlineClient.L_onlineClients.Add(onClient);

                    Console.Out.WriteLine($"ReadingChannelOnlineClient {onClient.Name} added successfully");
                }
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine($"Error ReadingChannelOnlineClient: {ex.Message}");
            }
        }
        #endregion

        #region Socket Connection
        static void MainWithConnectionManager()
        {
            try
            {
                //Preparing to receive data of players
                InputController.InputController_Start();

                IPAddress ipAddress = IPAddress.Parse("127.0.0.1"); // Replace with your desired server IP address
                int portClientToServer = 12345; // Replace with the desired port number for client-to-server communication
                //int portServerToClient = 12346; // Replace with the desired port number for server-to-client communication

                // Set up the listener for client-to-server communication
                ServerListener = new TcpListener(ipAddress, portClientToServer);
                ServerListener.Start();

                // Set up the listener for server-to-client communication
                //serverToClientListener = new TcpListener(ipAddress, portServerToClient);
                //serverToClientListener.Start();

                cThread = new Thread(() => ReadingChannelTcpListener());
                cThread.Start();

                dThread = new Thread(() => ReadingChannelOnlineClient());
                dThread.Start();

                Console.WriteLine("Server started. Waiting for connections...");
                while (true)
                {
                    // Accept client-to-server connectionss
                    TcpClient clientToServerClient = ServerListener.AcceptTcpClient();
                    WriterTcpListener.WriteAsync(clientToServerClient);
                    //onlineClient = new OnlineClient(l_onlineClients.Count + "");
                    //onlineClient.clientToServerClient = clientToServerClient;
                    //Console.WriteLine("Client connected (client-to-server).");

                    // Accept server-to-client connection
                    //TcpClient serverToClientClient = serverToClientListener.AcceptTcpClient();
                    //l_tcpClientsReceived.Add(clientToServerClient);
                    //onlineClient.serverToClientClient = serverToClientClient;
                    //Console.WriteLine("Client connected (server-to-client).");
                    //l_onlineClients.Add(onlineClient);
                    //Console.WriteLine("There is currently " + l_onlineClients.Count + " Connected.");

                    // Start a new thread to handle communication with the connected clients
                    //clientThread = new Thread(() => HandleClientCommunication(clientToServerClient, serverToClientClient));
                    //clientThread = new Thread(() => HandleClientCommunication(onlineClient));
                    //ThreadDefinition(onlineClient);
                }

                ////Start handling communication
                //HandleClientCommunication(clientToServerClient, serverToClientClient);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.ToString());
            }
        }

        static bool ThreadDefinition(OnlineClient onlineClient)
        {
            try
            {
                Thread clientThread;
                clientThread = new Thread(() => HandleClientSendCommunication(onlineClient));
                clientThread.Start();
                onlineClient.dic_threads.Add(onlineClient.Name + "_SEND", clientThread);

                clientThread = new Thread(() => HandleClientReceiveCommunication(onlineClient));
                clientThread.Start();
                onlineClient.dic_threads.Add(onlineClient.Name + "_RECEIVE", clientThread);

                clientThread = new Thread(() => onlineClient.ReadingChannelReceive());
                clientThread.Start();
                onlineClient.dic_threads.Add(onlineClient.Name + "_READCHANNELRECEIVE", clientThread);

                clientThread = new Thread(() => onlineClient.ReadingChannelSend());
                clientThread.Start();
                onlineClient.dic_threads.Add(onlineClient.Name + "_READCHANNELSEND", clientThread);
                return true;
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine("Error bool ThreadDefinition(OnlineClient): "+ex.ToString());
                return false;
            }
        }

        static void ConnectConnectionsWithClient(TcpClient tcpClient, string name)
        {
            try
            {
                byte[] buffer = new byte[255];
                string[] strArr = null;
                NetworkStream serverToClientStream = tcpClient.GetStream();
                OnlineClient onClient = null;
                while (tcpClient.Connected)
                {
                    // Read data from client-to-server connection
                    if (serverToClientStream == null)
                    {
                        Console.WriteLine("serverToClientStream Closed");
                        return;
                    }

                    int bytesRead = serverToClientStream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        break; // Client disconnected
                    }

                    //Reading the message . . . And publishing in console to be read
                    string receivedMessage = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                    if (receivedMessage.Contains("NAME:"))
                    {
                        strArr = receivedMessage.Split(":", StringSplitOptions.RemoveEmptyEntries);
                        if (cDic_tcpClientsReceived.Values.Where(c => c.Name == strArr[1]).Count() == 0)
                        {
                            //It's supposed to add himself to the registry list HOWEVER
                            //i'll be adding it manually, and that's why we use the constructor
                            //without name and instead adding the name afterwards
                            onClient = new OnlineClient();
                            onClient.Name = strArr[1];
                            cDic_tcpClientsReceived.TryAdd(strArr[1], onClient);
                        }

                        foreach (OnlineClient item in cDic_tcpClientsReceived.Values)
                        {
                            if (strArr[1] == item.Name)
                            {
                                string[] strArr2 = strArr[0].Split("+", StringSplitOptions.RemoveEmptyEntries);
                                if (strArr2[0].Contains("Client-To-Server") && item.clientToServerClient == null)
                                {
                                    item.clientToServerClient = tcpClient;
                                    Console.Out.WriteLine($"ConnectConnectionsWithClient clientToServerClient of {item.Name} finished, thread {name} ended");
                                    return;
                                }
                                else if (strArr2[0].Contains("Server-To-Client") && item.serverToClientClient == null)
                                {
                                    item.serverToClientClient = tcpClient;
                                    Thread tempThread = null;
                                    WriterOnlineClient.WriteAsync(item);
                                    //OnlineClient.cq_tcpOnlineClientsReceived.Enqueue(item);
                                    cDic_clientThreads.Remove(name, out tempThread);
                                    if (ThreadDefinition(item))
                                    {
                                        Console.Out.WriteLine($"ConnectConnectionsWithClient serverToClientClient of {item.Name} finished, thread {name} ended");
                                        return;
                                    }

                                }
                            }
                        }


                    }

                    Console.Out.WriteLine("Initial Socket, Received from server: " + receivedMessage);
                }

                // Clean up
                //serverToClientClient.Close();
            }
            catch (Exception ex)
            {
                //serverToClientClient.Close();
                //clientThreadReceive = null;
                //Interlocked.Decrement(ref activeThreads);
                //l_clientThreads.RemoveAll(c => c.Item1 == "clientThreadReceive");
                //Console.WriteLine("Remove HRC activeThreads: " + activeThreads + "\n");
                Console.WriteLine("Error ConnectConnectionsWithClient: " + ex.Message);
            }
        }

        static void HandleClientSendCommunication(OnlineClient onlineClient)
        {
            try
            {
                //NetworkStream serverToClientStream = onlineClient.serverToClientClient.GetStream();
                string instruction = string.Empty;
                while (true)
                {
                    // Echo the received data back to the client using server-to-client connection
                    //if (onlineClient.bytesRead != 0)
                    //{
                    //    //serverToClientStream.Write(onlineClient.Buffer, 0, onlineClient.bytesRead);
                    //    // Send to Every Client all the data than correspond

                    //    Console.WriteLine("client: " + onlineClient.Name);
                    //    //if (client != onlineClient)
                    //    //{

                    while (onlineClient.L_SendQueueMessages.TryDequeue(out instruction))
                    {
                        //Esto deja preparado el Buffer para ser consumido
                        onlineClient.Information = instruction;
                        if (onlineClient.serverToClientClient.Connected != false)
                        {
                            NetworkStream clientStream = onlineClient.serverToClientClient.GetStream();
                            clientStream.Write(onlineClient.BufferSend, 0, onlineClient.bytesLengthSend);
                        }
                        else
                        {
                            // Clean up, because the connection is broken, will need to be redo anyways
                            onlineClient.serverToClientClient.Close();
                            onlineClient.clientToServerClient.Close();
                            return;
                        }
                    }
                    //}
                    //    onlineClient.bytesRead = 0;
                    //}
                }

                // Close the listeners
                //clientToServerListener.Stop();
                //serverToClientListener.Stop();
            }
            catch (Exception ex)
            {
                // Clean up
                onlineClient.serverToClientClient.Close();
                onlineClient.clientToServerClient.Close();
                //dic_clientThreads.Remove(onlineClient.Name + "_SEND");
                //dic_clientThreads.Remove(onlineClient.Name + "_RECEIVE");
                l_onlineClients.Remove(onlineClient);
                Console.WriteLine("Error: " + ex.ToString());
            }
        }

        static void HandleClientReceiveCommunication(OnlineClient onlineClient)
        {
            try
            {
                NetworkStream clientToServerStream = onlineClient.clientToServerClient.GetStream();

                while (true)
                {

                    // Read data from client-to-server connection
                    if (clientToServerStream == null || onlineClient.clientToServerClient.Client.Connected == false)
                    {
                        Console.WriteLine("clientToServerStream Closed");
                        return;
                    }

                    //onlineClient.bytesRead = clientToServerStream.Read(onlineClient.Buffer, 0, onlineClient.Buffer.Length);
                    clientToServerStream.Read(onlineClient.BufferReceive, 0, onlineClient.bytesLengthReceive);
                    if (onlineClient.bytesLengthReceive == 0)
                    {
                        //break; // Client disconnected
                        return; // Client disconnected
                    }

                    Console.Out.WriteLine("ByteRead: " + onlineClient.bytesLengthSend);
                    Console.Out.WriteLine("BufferReceive: " + System.Text.Encoding.Default.GetString(onlineClient.BufferReceive));
                    Console.Out.WriteLine("Buffer Lenght: " + System.Text.Encoding.Default.GetString(onlineClient.BufferReceive).Length);
                    //Reading the message . . . And publishing in console to be read
                    string receivedMessage = Encoding.ASCII.GetString(onlineClient.BufferReceive, 0, onlineClient.bytesLengthReceive);

                    //--Cleaning the buffer-- Thanks to the Channel<T> we can totally use a byte[1] to clean BUT we better use
                    //the last size i thing
                    onlineClient.BufferReceive = new byte[onlineClient.bytesLengthReceive];
                    Console.WriteLine("Received from client: " + receivedMessage);
                    onlineClient.WriterReceive.WriteAsync(receivedMessage);
                    //onlineClient.L_ReceiveQueueMessages.Enqueue(receivedMessage);
                }

                // Close the listeners
                //clientToServerListener.Stop();
                //serverToClientListener.Stop();
            }
            catch (Exception ex)
            {
                // Clean up
                onlineClient.clientToServerClient.Close();
                onlineClient.serverToClientClient.Close();
                //dic_clientThreads.Remove(onlineClient.Name + "_SEND");
                //dic_clientThreads.Remove(onlineClient.Name + "_RECEIVE");
                l_onlineClients.Remove(onlineClient);
                Console.WriteLine("Error: " + ex.ToString());
            }
        }
        #endregion
    }
}