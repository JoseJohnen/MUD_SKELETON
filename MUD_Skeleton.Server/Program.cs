using System.Net.Sockets;
using System.Net;
using System.Text;
using MUD_Skeleton.Server.Controllers;
using MUD_Skeleton.Commons.Comms;

namespace MUD_Skeleton.Server
{
    internal class Program
    {
        #region Functional Attributes
        static TcpListener clientToServerListener;
        static TcpListener serverToClientListener;

        static List<OnlineClient> l_onlineClients = new List<OnlineClient>();
        static OnlineClient onlineClient;

        static Dictionary<string, Thread> dic_clientThreads = new Dictionary<string, Thread>();
        static Thread clientThread;
        #endregion

        static void Main(string[] args)
        {
            MainWithConnectionManager();
        }

        #region Socket Connection
        static void MainWithConnectionManager()
        {
            try
            {
                //Preparing to receive data of players
                InputController.InputController_Start();

                IPAddress ipAddress = IPAddress.Parse("127.0.0.1"); // Replace with your desired server IP address
                int portClientToServer = 12345; // Replace with the desired port number for client-to-server communication
                int portServerToClient = 12346; // Replace with the desired port number for server-to-client communication

                // Set up the listener for client-to-server communication
                clientToServerListener = new TcpListener(ipAddress, portClientToServer);
                clientToServerListener.Start();

                // Set up the listener for server-to-client communication
                serverToClientListener = new TcpListener(ipAddress, portServerToClient);
                serverToClientListener.Start();

                Console.WriteLine("Server started. Waiting for connections...");
                while (true)
                {
                    // Accept client-to-server connectionss
                    TcpClient clientToServerClient = clientToServerListener.AcceptTcpClient();
                    onlineClient = new OnlineClient(l_onlineClients.Count + "");
                    onlineClient.clientToServerClient = clientToServerClient;
                    Console.WriteLine("Client connected (client-to-server).");

                    // Accept server-to-client connection
                    TcpClient serverToClientClient = serverToClientListener.AcceptTcpClient();
                    onlineClient.serverToClientClient = serverToClientClient;
                    Console.WriteLine("Client connected (server-to-client).");
                    l_onlineClients.Add(onlineClient);
                    Console.WriteLine("There is currently " + l_onlineClients.Count + " Connected.");

                    // Start a new thread to handle communication with the connected clients
                    //clientThread = new Thread(() => HandleClientCommunication(clientToServerClient, serverToClientClient));
                    //clientThread = new Thread(() => HandleClientCommunication(onlineClient));
                    clientThread = new Thread(() => HandleClientSendCommunication(onlineClient));
                    clientThread.Start();
                    dic_clientThreads.Add(onlineClient.Name + "_SEND", clientThread);

                    clientThread = new Thread(() => HandleClientReceiveCommunication(onlineClient));
                    clientThread.Start();
                    dic_clientThreads.Add(onlineClient.Name + "_RECEIVE", clientThread);

                    clientThread = new Thread(() => onlineClient.ReadingChannelReceive());
                    clientThread.Start();
                    dic_clientThreads.Add(onlineClient.Name + "_READCHANNELRECEIVE", clientThread);

                    clientThread = new Thread(() => onlineClient.ReadingChannelSend());
                    clientThread.Start();
                    dic_clientThreads.Add(onlineClient.Name + "_READCHANNELSEND", clientThread);
                }

                ////Start handling communication
                //HandleClientCommunication(clientToServerClient, serverToClientClient);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.ToString());
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
                            clientStream.Write(onlineClient.Buffer, 0, onlineClient.bytesRead);
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
                dic_clientThreads.Remove(onlineClient.Name + "_SEND");
                dic_clientThreads.Remove(onlineClient.Name + "_RECEIVE");
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

                    onlineClient.bytesRead = clientToServerStream.Read(onlineClient.Buffer, 0, onlineClient.Buffer.Length);
                    if (onlineClient.bytesRead == 0)
                    {
                        //break; // Client disconnected
                        return; // Client disconnected
                    }

                    //Reading the message . . . And publishing in console to be read
                    string receivedMessage = Encoding.ASCII.GetString(onlineClient.Buffer, 0, onlineClient.bytesRead);
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
                dic_clientThreads.Remove(onlineClient.Name + "_SEND");
                dic_clientThreads.Remove(onlineClient.Name + "_RECEIVE");
                l_onlineClients.Remove(onlineClient);
                Console.WriteLine("Error: " + ex.ToString());
            }
        }
        #endregion
    }
}