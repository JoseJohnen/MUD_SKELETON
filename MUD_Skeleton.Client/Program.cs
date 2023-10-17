using MUD_Skeleton.Client.Controllers;
using MUD_Skeleton.Commons.Auxiliary;
using MUD_Skeleton.Commons.Comms;
using System.Net.Sockets;
using System.Text;

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

        static Dictionary<string, Trios<TypeOfMethod, TcpClient, int>> dic_use_typeOfMethode = new Dictionary<string, Trios<TypeOfMethod, TcpClient, int>>()
        {
            ["clientThreadSend"] = new Trios<TypeOfMethod, TcpClient, int>(TypeOfMethod.HandleClientSendCommunication, null, 12345),
            ["clientThreadReceive"] = new Trios<TypeOfMethod, TcpClient, int>(TypeOfMethod.HandleClientReceiveCommunication, null, 12346)
        };

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
                        Message Mandar = new Message(ConnectionManager.ActiveChl, externalMessage);
                        ConnectionManager.WriterSend.WriteAsync(Mandar.ToJson());
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
                do
                {
                    /*if (string.IsNullOrWhiteSpace(externalMessage))
                    {
                        externalMessage = Console.ReadLine();
                    }*/

                    foreach (KeyValuePair<string, Trios<TypeOfMethod, TcpClient, int>> item in dic_use_typeOfMethode.Reverse())
                    {
                        while (l_clientThreads.Where(c => c.Item1 == item.Key).ToList().Count == 0)
                        {
                            // Connect to the server's client-to-server port
                            item.Value.Item2 = new TcpClient(serverIp, item.Value.Item3);
                            if (item.Value.Item1 == TypeOfMethod.HandleClientSendCommunication)
                            {
                                l_clientThreads.Add(new Pares<string, Thread>(item.Key, new Thread(() => HandleClientSendCommunication(item.Value.Item2))));
                            }
                            else
                            {
                                l_clientThreads.Add(new Pares<string, Thread>(item.Key, new Thread(() => HandleClientReceiveCommunication(item.Value.Item2))));
                            }
                            l_clientThreads.Where(c => c.Item1 == item.Key).First().Item2.Start();
                        }
                    }

                }
                while (true);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error RegenComms: " + ex.Message);
                Thread.Sleep(15000);
                foreach (KeyValuePair<string, Trios<TypeOfMethod, TcpClient, int>> item in dic_use_typeOfMethode)
                {
                    if (item.Value.Item2 != null)
                    {
                        if (item.Value.Item2.Connected == true)
                        {
                            item.Value.Item2.Close();
                        }
                        item.Value.Item2 = null;
                        l_clientThreads.RemoveAll(c => c.Item1 == item.Key);
                    }
                }
                if (l_clientThreads.Count == 1)
                {
                    l_clientThreads.Clear();
                }
                l_clientThreads.Add(new Pares<string, Thread>("RegenComms", new Thread(() => RegenComms(serverIp))));
                l_clientThreads.Where(c => c.Item1 == "RegenComms").First().Item2.Start();
            }
        }
        #endregion

        #region Send - Receive Operations
        static void HandleClientSendCommunication(TcpClient clientToServerClient)
        {
            try
            {
                NetworkStream clientToServerStream = clientToServerClient.GetStream();
                Interlocked.Increment(ref activeThreads);
                Console.WriteLine("Add HSC activeThreads: " + activeThreads + "\n");

                //byte[] buffer = new byte[1024];
                //int bytesRead;
                string message = string.Empty;

                while (true)
                {
                    // Echo the received data back to the client using server-to-client connection
                    //if (!string.IsNullOrWhiteSpace(externalMessage))
                    //{
                    //    message = externalMessage;
                    //    Console.Out.WriteLine("Sending . . . " + message);
                    //    byte[] data = Encoding.ASCII.GetBytes(message);
                    //    /*TODO MIRAR ACA */
                    //    clientToServerStream.Write(data, 0, data.Length);
                    //    externalMessage = string.Empty;
                    //}

                    while (ConnectionManager.cq_instructionsToSend.TryDequeue(out message))
                    {
                        Console.Out.WriteLine("Sending . . . " + message);
                        byte[] data = Encoding.ASCII.GetBytes(message);
                        clientToServerStream.Write(data, 0, data.Length);
                    }
                }

                // Clean up
                //clientToServerClient.Close();
            }
            catch (Exception ex)
            {
                clientToServerClient.Close();
                //clientThreadSend = null;
                Interlocked.Decrement(ref activeThreads);
                l_clientThreads.RemoveAll(c => c.Item1 == "clientThreadSend");
                Console.WriteLine("Remove HSC activeThreads: " + activeThreads + "\n");
                Console.WriteLine("Error HandleClientSendCommunication: " + ex.Message);
            }
        }

        static void HandleClientReceiveCommunication(TcpClient serverToClientClient)
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
                }

                // Clean up
                //serverToClientClient.Close();
            }
            catch (Exception ex)
            {
                serverToClientClient.Close();
                //clientThreadReceive = null;
                Interlocked.Decrement(ref activeThreads);
                l_clientThreads.RemoveAll(c => c.Item1 == "clientThreadReceive");
                Console.WriteLine("Remove HRC activeThreads: " + activeThreads + "\n");
                Console.WriteLine("Error HandleClientReceiveCommunication: " + ex.Message);
            }
        }
        #endregion
        #endregion


    }

}