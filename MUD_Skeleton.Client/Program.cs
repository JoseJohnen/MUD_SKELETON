using MUD_Skeleton.Client.Controllers;
using MUD_Skeleton.Commons.Auxiliary;
using MUD_Skeleton.Commons.Comms;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MUD_Skeleton.Client
{
    enum TypeOfMethod { HandleClientSendCommunication, HandleClientReceiveCommunication }

    class Program
    {
        static List<Pares<string, Thread>> l_clientThreads = new List<Pares<string, Thread>>();

        //static Thread clientThreadSend;
        //static Thread clientThreadReceive;

        //static TcpClient clientToServerClient;
        //static TcpClient serverToClientClient;

        static string externalMessage = "hello server";
        static int activeThreads = 0;
        static string name = string.Empty;

        static CancellationToken ctSendingDataUDP = new CancellationToken();
        static CancellationToken ctReceivingDataTCP = new CancellationToken();

        static uint IdSender = 0;
        //static Dictionary<string, Trios<TypeOfMethod, TcpClient, int>> dic_use_typeOfMethode = new Dictionary<string, Trios<TypeOfMethod, TcpClient, int>>()
        //{
        //    ["clientThreadSend"] = new Trios<TypeOfMethod, TcpClient, int>(TypeOfMethod.HandleClientSendCommunication, null, 12345),
        //    ["clientThreadReceive"] = new Trios<TypeOfMethod, TcpClient, int>(TypeOfMethod.HandleClientReceiveCommunication, null, 12345)
        //};

        static void Main()
        {
            try
            {
                //Prepare client for connections and uses
                Controller.Controller_Start();

                string serverIp = "127.0.0.1"; // Replace with the server IP address
                                               //int serverPortClientToServer = 12345; // Replace with the server's client-to-server port number
                                               //int serverPortServerToClient = 12346; // Replace with the server's server-to-client port number

                //RegenComms(serverIp);//, serverPortClientToServer, serverPortServerToClient);
                //Start with Sockets
                l_clientThreads.Add(new Pares<string, Thread>("RegenComms", new Thread(() => RegenComms(serverIp))));
                l_clientThreads.Where(c => c.Item1 == "RegenComms").First().Item2.Start();

                while (true)
                {
                    externalMessage = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(externalMessage))
                    {
                        ConnectionManager.WriterSend.WriteAsync(externalMessage);
                        //ConnectionManager.cq_instructionsToSend.Enqueue(externalMessage);
                        externalMessage = string.Empty;
                    }
                }
                //// Clean up
                //clientToServerClient.Close();
                //serverToClientClient.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Entro acá");
                Console.WriteLine("Error: " + ex.ToString());
            }
        }

        #region Sockets Connection
        #region Listening Socket, Comms preparation and Pipeline of comms availability
        static void RegenComms(string serverIp)//, int serverPortClientToServer, int serverPortServerToClient)
        {
            try
            {
                string messageId = string.Empty;

                NetworkStream serverToClientStream = null;

                // Connect to the server's client-to-server port
                TcpClient tcpClt = new TcpClient(serverIp, 12345);
                serverToClientStream = tcpClt.GetStream();

                if (serverToClientStream != null)
                {
                        //TODO: Change the name for the login name + Motherboard + Process + Mac or something like that
                        //For now, it would be a randomized value randomized outside of this
                        //messageId = "NAME:" + name;
                        messageId = "NAME";

                        byte[] dataName = Encoding.ASCII.GetBytes(messageId);
                        //Sended to create the OnlineClient
                        serverToClientStream.Write(dataName, 0, dataName.Length);
                }

                l_clientThreads.Add(new Pares<string, Thread>("_RECEIVE_COMMS_SOCKET", new Thread(() => HandleClientReceiveCommunication(tcpClt, ctReceivingDataTCP))));
                l_clientThreads.Where(c => c.Item1 == "_RECEIVE_COMMS_SOCKET").First().Item2.Start();
                l_clientThreads.Add(new Pares<string, Thread>("_SENDING_COMMS_UDP", new Thread(() => HandleClientSendCommunication(ctSendingDataUDP))));
                l_clientThreads.Where(c => c.Item1 == "_SENDING_COMMS_UDP").First().Item2.Start();

                bool isNameSetted = false;
                do
                {
                    if(ConnectionManager.Name != 0)
                    {
                        ConnectionManager.WriterSend.WriteAsync(messageId+":"+ConnectionManager.Name);
                        isNameSetted = true;
                    }
                }
                while (!isNameSetted);
                //Sending to register the UDP connection with the same OnlineClient already created

                //After finishing everything, it just close, preparing the conditions to run again
                //if it happends than the 
                l_clientThreads.RemoveAll(c => c.Item1 == "RegenComms");
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error RegenComms: " + ex.Message);
                if (l_clientThreads.Count == 1)
                {
                    l_clientThreads.Clear();
                }
                l_clientThreads.Add(new Pares<string, Thread>("RegenComms", new Thread(() => RegenComms(serverIp))));
                l_clientThreads.Where(c => c.Item1 == "RegenComms").First().Item2.Start();
            }
            finally
            {
                Console.Out.WriteLine("Finalized RegenComms");
            }
        }
        #endregion

        #region Send - Receive Operations
        static async void HandleClientSendCommunication(CancellationToken cancellationToken)
        {
            try
            {
                Interlocked.Increment(ref activeThreads);
                Console.WriteLine("Add HSC activeThreads: " + activeThreads + "\n");

                string message = string.Empty;


                while (await ConnectionManager.ReaderSendProcess.WaitToReadAsync())
                {
                    message = await ConnectionManager.ReaderSendProcess.ReadAsync();
                    UdpClient client = new UdpClient();
                    IPEndPoint ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000); // endpoint where server is listening

                    client.Connect(ep);
                    Console.Out.WriteLine("Sending . . . " + message);
                    // send data
                    int bytesSended = client.Send(Encoding.ASCII.GetBytes(message));
                    Console.Out.WriteLine(bytesSended+" bytes sended successfully . . . ");

                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }
                    message = string.Empty;
                }

                // Clean up
                //clientToServerClient.Close();
            }
            catch (Exception ex)
            {
                Interlocked.Decrement(ref activeThreads);
                l_clientThreads.RemoveAll(c => c.Item1 == "_SENDING_COMMS_UDP");
                Console.Out.WriteLine("Remove HSC activeThreads: " + activeThreads + "\n");
                Console.Out.WriteLine("Error HandleClientSendCommunication: " + ex.Message);
            }
            finally
            {
                Console.Out.WriteLine("Finalized HandleClientSendCommunication");
            }
        }

        static void HandleClientReceiveCommunication(TcpClient serverToClientClient, CancellationToken cancellationToken)
        {
            try
            {
                NetworkStream serverToClientStream = serverToClientClient.GetStream();


                byte[] buffer = new byte[1024];
                int bytesRead;
                Interlocked.Increment(ref activeThreads);
                Console.WriteLine("Add HRC activeThreads: " + activeThreads + "\n");

                while (true)
                {
                    // Read data from client-to-server connection
                    if (serverToClientStream == null)
                    {
                        Console.WriteLine("serverToClientStream Closed");
                        return;
                    }

                    bytesRead = serverToClientStream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        break; // Client disconnected
                    }

                    //Reading the message . . . And publishing in console to be read
                    ConnectionManager.receivedMessage = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    ConnectionManager.WriterReceive.WriteAsync(ConnectionManager.receivedMessage);
                    //ConnectionManager.cq_instructionsReceived.Enqueue(ConnectionManager.receivedMessage);
                    //Console.Out.WriteLine("Received from server: " + receivedMessage);
                    Console.WriteLine("Received: " + ConnectionManager.receivedMessage + " TOTAL : " + ConnectionManager.cq_instructionsReceived.Count);
                    ConnectionManager.receivedMessage = string.Empty;

                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }
                }

                // Clean up
                //serverToClientClient.Close();
            }
            catch (Exception ex)
            {
                Interlocked.Decrement(ref activeThreads);
                l_clientThreads.RemoveAll(c => c.Item1 == "_RECEIVE_COMMS_SOCKET");
                Console.WriteLine("Remove HRC activeThreads: " + activeThreads + "\n");
                Console.WriteLine("Error HandleClientReceiveCommunication: " + ex.Message);
            }
            finally
            {
                Console.Out.WriteLine("Finalized HandleClientReceiveCommunication");
            }
        }
        #endregion
        #endregion


    }

}