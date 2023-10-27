using System.Net.Sockets;
using System.Net;
using System.Text;
using MUD_Skeleton.Server.Controllers;
using MUD_Skeleton.Commons.Comms;
using System.Collections.Concurrent;
using System.Threading.Channels;

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
        static Thread eThread;
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
                Console.Out.WriteLine("Starting ReadingChannelTcpListener");
                string tempString = string.Empty;
                while (await ReaderTcpListener.WaitToReadAsync())
                {
                    TcpClient tcpClient = await ReaderTcpListener.ReadAsync();
                    Console.Out.WriteLine($"Receiving connection from {tcpClient.Client.LocalEndPoint.ToString()}");
                    //cq_tcpClientsReceived.Enqueue(tcpClient)
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
            finally
            {
                Console.Out.WriteLine("Finalized ReadingChannelTcpListener");
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
                Console.Out.WriteLine("Starting ReadingChannelOnlineClient");
                string tempString = string.Empty;
                bool tryadd = false;
                while (await ReaderOnlineClient.WaitToReadAsync())
                {
                    OnlineClient onClient = await ReaderOnlineClient.ReadAsync();
                    do
                    {
                        tryadd = OnlineClient.cq_tcpOnlineClientsReceived.TryAdd(onClient);
                    }
                    while (!tryadd);
                    //OnlineClient.L_onlineClients.Add(onClient);

                    Console.Out.WriteLine($"ReadingChannelOnlineClient {onClient.Name} added successfully");
                }
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine($"Error ReadingChannelOnlineClient: {ex.Message}");
            }
            finally
            {
                Console.Out.WriteLine("Finalized ReadingChannelOnlineClient");
            }
        }
        #endregion

        #region Socket Connection
        static void MainWithConnectionManager()
        {
            try
            {
                Console.Out.WriteLine("Starting MainWithConnectionManager");
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

                eThread = new Thread(() => HandleFromClientReceivedCommunication());
                eThread.Start();

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
                Console.Out.WriteLine("Error MainWithConnectionManager: " + ex.ToString());
            }
            finally
            {
                Console.Out.WriteLine("Finalized MainWithConnectionManager");
            }
        }

        static bool ThreadDefinition(OnlineClient onlineClient)
        {
            try
            {
                Console.Out.WriteLine("Starting ThreadDefinition");
                Thread clientThread;
                clientThread = new Thread(() => HandleToClientSendCommunication(onlineClient));
                clientThread.Start();
                onlineClient.dic_threads.Add(onlineClient.Name + "_SEND", clientThread);

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
                Console.Out.WriteLine("Error bool ThreadDefinition(OnlineClient): " + ex.ToString());
                return false;
            }
            finally
            {
                Console.Out.WriteLine("Finalized ThreadDefinition");
            }
        }

        static void ConnectConnectionsWithClient(TcpClient tcpClient, string name)
        {
            try
            {
                Console.Out.WriteLine("Starting ConnectConnectionsWithClient");
                byte[] buffer = new byte[255];
                NetworkStream serverToClientStream = tcpClient.GetStream();
                OnlineClient onClient = null;
                while (tcpClient.Connected)
                {
                    Console.Out.WriteLine("TCP Socket is Connected");
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
                    Console.Out.WriteLine("TCP Socket CCWC: Received " + receivedMessage);

                    if (receivedMessage.Contains("NAME"))
                    {
                        //If it is requesting a NAME, then, it doesn't have a name Yet
                        Console.Out.WriteLine("ConnectConnectionsWithClient NAME Request RECEIVED");
                        Random rand = new Random();
                        do
                        {
                            name = rand.Next(1000000, 10000000).ToString();
                        }
                        while (OnlineClient.L_onlineClients.Any(c => c.Name == name));

                        onClient = new OnlineClient(name);

                        bool isRegistered = false;
                        do
                        {
                            isRegistered = cDic_tcpClientsReceived.TryAdd(name, onClient);
                        }
                        while (!isRegistered);
                        Console.Out.WriteLine("ConnectConnectionsWithClient NEW ONLINE-CLIENT CREATED");


                        if (cDic_tcpClientsReceived.Values.Where(c => c.Name == name).Count() > 0)
                        {
                            if (onClient.serverToClientClient == null)
                            {
                                onClient.serverToClientClient = tcpClient;
                                WriterOnlineClient.WriteAsync(onClient);

                                Console.Out.WriteLine("ConnectConnectionsWithClient serverToClientClient Has been Created Successfully");
                                if (ThreadDefinition(onClient))
                                {
                                    onClient.WriterSend.WriteAsync("~YOURNAMEIS:" + name);
                                    Thread tempThread = null;
                                    cDic_clientThreads.Remove(name, out tempThread);
                                    Console.Out.WriteLine($"OnlineClient {onClient.Name} creation process finished, thread {name} ended");
                                    return;
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
                Console.Out.WriteLine("Error ConnectConnectionsWithClient: " + ex.Message);
            }
            finally
            {
                Console.Out.WriteLine("Finalized ConnectConnectionsWithClient");
            }
        }

        static async void HandleToClientSendCommunication(OnlineClient onlineClient)
        {
            try
            {
                Console.Out.WriteLine("Starting HandleToClientSendCommunication");
                while (true)
                {
                    while (await onlineClient.ReaderSendProcess.WaitToReadAsync())
                    {
                        string instruction = await onlineClient.ReaderSendProcess.ReadAsync();
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
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Clean up
                onlineClient.serverToClientClient.Close();
                //dic_clientThreads.Remove(onlineClient.Name + "_SEND");
                //dic_clientThreads.Remove(onlineClient.Name + "_RECEIVE");
                l_onlineClients.Remove(onlineClient);
                Console.WriteLine("Error: " + ex.ToString());
            }
            finally
            {
                Console.Out.WriteLine("Finalized HandleToClientSendCommunication");
            }
        }

        static void HandleFromClientReceivedCommunication()
        {
            try
            {
                Console.Out.WriteLine("Starting HandleFromClientReceivedCommunication");
                UdpClient udpServer = new UdpClient(11000);
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 11000);
                while (true)
                {
                    byte[] data = udpServer.Receive(ref remoteEP); // listen on port 11000

                    //Reading the message . . . And publishing in console to be read
                    string receivedMessage = Encoding.ASCII.GetString(data);
                    string[] strArr = null;
                    Console.Out.WriteLine("Received UDP: " + receivedMessage);

                    Message msg = Message.CreateFromJson(receivedMessage);
                    receivedMessage = msg.TextOriginal;
                    uint idSender = msg.IdSnd;
                    Console.Out.WriteLine("Message Contain: " + receivedMessage);

                    if (receivedMessage.Contains("NAME"))
                    {
                        strArr = receivedMessage.Split(":", StringSplitOptions.RemoveEmptyEntries);
                        if (OnlineClient.L_onlineClients.Where(c => c.Name == strArr[1]).Count() > 0)
                        {
                            Console.Out.WriteLine("Name request received,");
                            OnlineClient onClt = OnlineClient.L_onlineClients.Where(c => c.Name == strArr[1]).First();
                            onClt.IpPort = remoteEP.ToString();
                            onClt.WriterSend.WriteAsync("~NAMERECEIVED");
                        }
                        continue;
                    }

                    //So, or it send the wrong message, or it send the message and receive the right answer
                    if (OnlineClient.L_onlineClients.Where(c => c.Name == Convert.ToString(idSender)).Count() > 0)
                    {
                        Console.Out.WriteLine("Sending back " + receivedMessage);
                        OnlineClient onClte = OnlineClient.L_onlineClients.Where(c => c.Name == Convert.ToString(idSender)).First();
                        onClte.WriterReceive.WriteAsync(msg.ToJson());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine("Error: " + ex.ToString());
            }
            finally
            {
                Console.Out.WriteLine("Finalized HandleFromClientReceivedCommunication");
                eThread = new Thread(() => HandleFromClientReceivedCommunication());
                eThread.Start();
            }
        }
        #endregion
    }
}