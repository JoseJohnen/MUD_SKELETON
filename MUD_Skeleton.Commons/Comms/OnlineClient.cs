using MUD_Skeleton.Commons.Auxiliary;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using System.Threading.Channels;

namespace MUD_Skeleton.Commons.Comms
{
    public class OnlineClient
    {
        /*
         * TODO:
         * **DONE** 1) Hacer que lo que cada cliente dice, lo reciban todos los demás clientes
         * 
         * **DONE** 2) Hacer que funcione Con Canales Virtuales (i.e. cada cliente esta en un canal y solo habla y escucha con el resto de los clientes en dicho canal, no escucha nada fuera del canal donde se encuentra)
         * 
         * **ELIMINATED** 3) Hacer que cada cliente pueda tener mas de un socket de envío y mas de un socket de recepción simultáneamente
         * 
         * **DONE** 4) Adicionar Protección contra Idempotencia
         */

        #region Channel (Msg) Related
        private uint activeChl = 0;
        public uint ActiveChl { get => activeChl; set => activeChl = value; }
        #endregion

        #region Idempotency Protection
        private uint idLastSendedId = 1;
        public uint IdLastSendedId
        {
            get
            {
                return idLastSendedId;
            }
            set
            {
                idLastSendedId = value;
            }
        }

        private uint idLastReceivedId = 0;
        public uint IdLastReceivedId
        {
            get 
            { 
                return idLastReceivedId; 
            }
            set
            {
                idLastReceivedId = value;
            }
        }
        #endregion

        #region Channels (Thread & COMMs) Related
        #region Shock absorbers
        public Dictionary<string, Thread> dic_threads = new Dictionary<string, Thread>();
        //IT does create "Back Pressure"
        //It will wait for space to be available in order to wait
        private BoundedChannelOptions options = new BoundedChannelOptions(255);

        private Channel<string> channelReceive = null;
        public Channel<string> ChannelReceive
        {
            get
            {
                if (channelReceive == null)
                {
                    options.FullMode = BoundedChannelFullMode.Wait;
                    channelReceive = System.Threading.Channels.Channel.CreateBounded<string>(options);
                }
                return channelReceive;
            }
            set { channelReceive = value; }
        }

        private ChannelWriter<string> writerReceive = null;
        public ChannelWriter<string> WriterReceive
        {
            get
            {
                if (writerReceive == null)
                {
                    writerReceive = ChannelReceive.Writer;
                }
                return writerReceive;
            }
            set => writerReceive = value;
        }

        private ChannelReader<string> readerReceive = null;
        public ChannelReader<string> ReaderReceive
        {
            get
            {
                if (readerReceive == null)
                {
                    readerReceive = ChannelReceive.Reader;
                }
                return readerReceive;
            }
            set => readerReceive = value;
        }

        public async void ReadingChannelReceive()
        {
            try
            {
                Console.Out.WriteLine($"Starting ReadingChannelReceive");
                string tempString = string.Empty;

                while (await ReaderReceive.WaitToReadAsync())
                {
                    string strTemp = await ReaderReceive.ReadAsync();
                    tempString += strTemp.Trim();

                    if (tempString.Contains("{") && tempString.Contains("}"))
                    {
                        tempString = tempString.Replace("\0\0", "").Trim();
                        uint idMsg = Message.GetIdMsgFromJson(tempString);
                        Console.Out.WriteLine($"ReadingChannelReceive MSG: {tempString} \nProt Idem: LastReceived:{idLastReceivedId} \nLastSended:{IdLastSendedId}\n");

                        //Si este mensaje id ya ha sido recibido en el pasado
                        //(es decir si es menor o igual) entonces se ignora
                        Console.Out.WriteLine($"idMsg {idMsg} IdLastReceivedId {IdLastReceivedId}");
                        if (idMsg <= IdLastReceivedId)
                        {
                            tempString = string.Empty;
                            continue;
                        }

                        //If it is bigger however
                        IdLastReceivedId = idMsg;

                        //If it happends to be close to the limit that uint can count
                        if (IdLastReceivedId >= 4294967000)
                        {
                            Console.Out.WriteLine($"ReadingReceive {Name} ha alcanzado el límite de registro de Idempotencia, reseteando el registro . . . ");
                            IdLastReceivedId = 0;
                            await WriterSend.WriteAsync("~RSTIDMPTC");
                            Console.BackgroundColor = ConsoleColor.Green;
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Out.WriteLine("Protección de idempotencia ha sido reseteado a 1 para " + Name);
                            Console.ResetColor();
                        }

                        if (Message.IsValidMessage(tempString))
                        {
                            Console.Out.WriteLine("ReadingChannelReceive Writing Async: " + tempString);
                            await WriterReceiveProcess.WriteAsync(tempString);
                            tempString = string.Empty;
                        }
                    }

                    Console.Out.WriteLine($"ReadingReceive {strTemp}");
                    Console.Out.WriteLine($"ReadingReceive {tempString}");
                    Console.Out.WriteLine($"ReadingReceive {L_ReceiveQueueMessages.Count}");
                    strTemp = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine($"Error ReadingChannelReceive: {ex.Message}");
            }
            finally
            {

                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Out.WriteLine($"Finalize ReadingChannelReceive");
                Console.ResetColor();
                //TODO: Desconozco si esto va a funcionar para resetarlo pero, Teoricamente DEBERIA, hay que confirmar en la consola.
                //Si no funciona, o tira algún error, lo que habría que hacer es cargar un nuevo thread en el mismo espacio del
                //dictionary ya ocupado, y tirarlo a correr.
                //
                //Sin embargo, creo que dado que la función terminará su funcionamiento pero seguirá guardada en memoria, creo
                //que bastará con tirar un "Start" nuevamente y listo, habrá que ver
                dic_threads.Where(c => c.Key == Name + "_READCHANNELRECEIVE").First().Value.Start();
            }
        }

        private Channel<string> channelSend = null;
        public Channel<string> ChannelSend
        {
            get
            {
                if (channelSend == null)
                {
                    options.FullMode = BoundedChannelFullMode.Wait;
                    channelSend = System.Threading.Channels.Channel.CreateBounded<string>(options);
                }
                return channelSend;
            }
            set { channelSend = value; }
        }

        private ChannelWriter<string> writerSend = null;
        public ChannelWriter<string> WriterSend
        {
            get
            {
                if (writerSend == null)
                {
                    writerSend = ChannelSend.Writer;
                }
                return writerSend;
            }
            set => writerSend = value;
        }

        private ChannelReader<string> readerSend = null;
        public ChannelReader<string> ReaderSend
        {
            get
            {
                if (readerSend == null)
                {
                    readerSend = ChannelSend.Reader;
                }
                return readerSend;
            }
            set => readerSend = value;
        }

        public async void ReadingChannelSend()
        {
            string tempString = string.Empty;
            while (await ReaderSend.WaitToReadAsync())
            {
                string strTemp = await ReaderSend.ReadAsync();
                tempString += strTemp.Trim();
                if (!string.IsNullOrWhiteSpace(tempString))
                {
                    Message Mandar = new Message(ActiveChl, tempString);
                    Mandar.IdMsg = IdLastSendedId;
                    WriterSendProcess.TryWrite(Mandar.ToJson());
                    IdLastSendedId++;
                    Console.BackgroundColor = ConsoleColor.Blue;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Out.WriteLine($"ReadingSend ¡¡SENDED!! {tempString}");
                    Console.ResetColor();
                    tempString = string.Empty;
                }
                Console.Out.WriteLine($"ReadingSend {strTemp}");
                Console.Out.WriteLine($"ReadingSend {tempString}");
                strTemp = string.Empty;
            }
        }
        #endregion

        #region Distributers
        //IT does create "Back Pressure"
        //It will wait for space to be available in order to wait

        //This distributor centralize comms post shock-absorvers of all the clients
        //in just one nice channel, so to process them more efficiently
        private static BoundedChannelOptions options_static = new BoundedChannelOptions(255);

        private static Channel<string> channelReceiveProcess = null;
        public static Channel<string> ChannelReceiveProcess
        {
            get
            {
                if (channelReceiveProcess == null)
                {
                    options_static.FullMode = BoundedChannelFullMode.Wait;
                    channelReceiveProcess = System.Threading.Channels.Channel.CreateBounded<string>(options_static);
                }
                return channelReceiveProcess;
            }
            set { channelReceiveProcess = value; }
        }

        private static ChannelWriter<string> writerReceiveProcess = null;
        public static ChannelWriter<string> WriterReceiveProcess
        {
            get
            {
                if (writerReceiveProcess == null)
                {
                    writerReceiveProcess = ChannelReceiveProcess.Writer;
                }
                return writerReceiveProcess;
            }
            set => writerReceiveProcess = value;
        }

        private static ChannelReader<string> readerReceiveProcess = null;
        public static ChannelReader<string> ReaderReceiveProcess
        {
            get
            {
                if (readerReceiveProcess == null)
                {
                    readerReceiveProcess = ChannelReceiveProcess.Reader;
                }
                return readerReceiveProcess;
            }
            set => readerReceiveProcess = value;
        }

        //This channel however is used for each client, so is not static
        //this is because is for sending things back to them
        private Channel<string> channelSendProcess = null;
        public Channel<string> ChannelSendProcess
        {
            get
            {
                if (channelSendProcess == null)
                {
                    options.FullMode = BoundedChannelFullMode.Wait;
                    channelSendProcess = System.Threading.Channels.Channel.CreateBounded<string>(options);
                }
                return channelSendProcess;
            }
            set { channelSendProcess = value; }
        }

        private ChannelWriter<string> writerSendProcess = null;
        public ChannelWriter<string> WriterSendProcess
        {
            get
            {
                if (writerSendProcess == null)
                {
                    writerSendProcess = ChannelSendProcess.Writer;
                }
                return writerSendProcess;
            }
            set => writerSendProcess = value;
        }

        private ChannelReader<string> readerSendProcess = null;
        public ChannelReader<string> ReaderSendProcess
        {
            get
            {
                if (readerSendProcess == null)
                {
                    readerSendProcess = ChannelSendProcess.Reader;
                }
                return readerSendProcess;
            }
            set => readerSendProcess = value;
        }
        #endregion
        #endregion

        #region Static Utilitary Attributes
        public static BlockingCollection<OnlineClient> cq_tcpOnlineClientsReceived = new BlockingCollection<OnlineClient>();

        private static List<OnlineClient> l_onlineClients = new List<OnlineClient>();
        public static List<OnlineClient> L_onlineClients
        {
            get
            {
                OnlineClient oClte = null;
                while (cq_tcpOnlineClientsReceived.TryTake(out oClte))
                //foreach (OnlineClient client in cq_tcpOnlineClientsReceived)
                {
                    if (oClte != null)
                    {
                        if (l_onlineClients.Where(c => c.Name == oClte.Name).Count() == 0)
                        {
                            l_onlineClients.Add(oClte);
                            Console.Out.WriteLine("L_onlineClients: agregado " + oClte.Name);
                        }
                    }
                }
                return l_onlineClients;
            }
            set
            {
                l_onlineClients = value;
            }
        }
        #endregion

        #region Attributes
        #region Functional
        public TcpClient clientToServerClient;
        public TcpClient serverToClientClient;

        //Para Canales
        //El primer número es tipo (Confirmar)
        //El segundo es el número del canal
        private List<Pares<uint, uint>> l_channels = new List<Pares<uint, uint>>()
        {
            new Pares<uint, uint>(0,0)
        };

        public List<Pares<uint, uint>> L_channels
        {
            get
            {
                return l_channels;
            }
            set
            {
                l_channels = value;
            }
        }

        //Para UDP
        private string ipPort = string.Empty;
        public string IpPort
        {
            get { return ipPort; }
            set { ipPort = value; }
        }

        //Para TCP
        public int bytesLengthSend = 0;
        private byte[] bufferSend = new byte[1024];
        public byte[] BufferSend
        {
            get
            {
                bytesLengthSend = bufferSend.Length;
                return bufferSend;
            }
            set
            {
                bufferSend = value;
                bytesLengthSend = bufferSend.Length;
            }
        }

        public int bytesLengthReceive = 0;
        private byte[] bufferReceive = new byte[1024];
        public byte[] BufferReceive
        {
            get
            {
                bytesLengthReceive = bufferReceive.Length;
                return bufferReceive;
            }
            set
            {
                bufferReceive = value;
                bytesLengthReceive = bufferReceive.Length;
            }
        }
        public string Information
        {
            get
            {
                return Encoding.ASCII.GetString(BufferSend);
            }
            set
            {
                BufferSend = Encoding.ASCII.GetBytes(value);
            }
        }
        #endregion

        #region Game Related
        private string name = string.Empty;
        public string Name
        {
            get
            {
                if (L_onlineClients == null)
                {
                    L_onlineClients = new List<OnlineClient>();
                }
                if (string.IsNullOrWhiteSpace(name))
                {
                    name = "_PROVISORY_" + L_onlineClients.Count.ToString();
                }
                return name;
            }
            set
            {
                if (L_onlineClients == null)
                {
                    L_onlineClients = new List<OnlineClient>();
                }
                else if (L_onlineClients.Where(c => c.Name == name).Count() == 0)
                {
                    L_onlineClients.Add(this);
                }
                name = value;
            }
        }
        #endregion

        #region Data Instructions Administration
        private BlockingCollection<string> l_ReceiveQueueMessages = null;
        public BlockingCollection<string> L_ReceiveQueueMessages
        {
            get
            {
                if (l_ReceiveQueueMessages == null)
                {
                    l_ReceiveQueueMessages = new BlockingCollection<string>();
                }
                /*else if (l_ReceiveQueueMessages.Count() == 0)
                {
                    l_ReceiveQueueMessages = new ConcurrentQueue<string>();
                }*/
                return l_ReceiveQueueMessages;
            }
            set => l_ReceiveQueueMessages = value;
        }
        #endregion
        #endregion

        #region Constructores
        public OnlineClient(string name)
        {
            Name = name;
            options.FullMode = BoundedChannelFullMode.Wait;
            ChannelReceive = System.Threading.Channels.Channel.CreateBounded<string>(options);
            WriterReceive = ChannelReceive.Writer;
            ReaderReceive = ChannelReceive.Reader;
            ChannelSend = System.Threading.Channels.Channel.CreateBounded<string>(options);
            WriterSend = ChannelSend.Writer;
            ReaderSend = ChannelSend.Reader;
            if (L_onlineClients == null)
            {
                L_onlineClients = new List<OnlineClient>();
            }
            if (L_onlineClients.Where(c => c.Name == name).Count() == 0)
            {
                L_onlineClients.Add(this);
            }
        }

        public OnlineClient()
        {
            options.FullMode = BoundedChannelFullMode.Wait;
            ChannelReceive = System.Threading.Channels.Channel.CreateBounded<string>(options);
            WriterReceive = ChannelReceive.Writer;
            ReaderReceive = ChannelReceive.Reader;
            ChannelSend = System.Threading.Channels.Channel.CreateBounded<string>(options);
            WriterSend = ChannelSend.Writer;
            ReaderSend = ChannelSend.Reader;
            if (L_onlineClients == null)
            {
                L_onlineClients = new List<OnlineClient>();
            }
        }
        #endregion

        #region Equalizer
        public override bool Equals(object obj)
        {
            // If the passed object is null
            if (obj == null)
            {
                return false;
            }
            if (!(obj is OnlineClient))
            {
                return false;
            }
            if (ReferenceEquals(obj, null))
            {
                return false;
            }

            if (
                clientToServerClient == null ||
                ((OnlineClient)obj).clientToServerClient == null ||
                serverToClientClient == null ||
                ((OnlineClient)obj).serverToClientClient == null
                )
            {
                return Name == ((OnlineClient)obj).Name &&
                clientToServerClient == ((OnlineClient)obj).clientToServerClient &&
                serverToClientClient == ((OnlineClient)obj).serverToClientClient;
            }

            if (
                clientToServerClient.Client == null ||
                ((OnlineClient)obj).clientToServerClient.Client == null ||
                serverToClientClient.Client == null ||
                ((OnlineClient)obj).serverToClientClient.Client == null
                )
            {
                return Name == ((OnlineClient)obj).Name &&
                clientToServerClient.Client == ((OnlineClient)obj).clientToServerClient.Client &&
                serverToClientClient.Client == ((OnlineClient)obj).serverToClientClient.Client;
            }

            return Name == ((OnlineClient)obj).Name
                && clientToServerClient.Client.RemoteEndPoint == ((OnlineClient)obj).clientToServerClient.Client.RemoteEndPoint
                && serverToClientClient.Client.RemoteEndPoint == ((OnlineClient)obj).serverToClientClient.Client.RemoteEndPoint;
        }
        //Overriding the GetHashCode method
        //GetHashCode method generates hashcode for the current object

        public override int GetHashCode()
        {
            //Performing BIT wise OR Operation on the generated hashcode values
            //If the corresponding bits are different, it gives 1.
            //If the corresponding bits are the same, it gives 0.
            return Name.GetHashCode() ^ clientToServerClient.GetHashCode() ^ serverToClientClient.GetHashCode();
        }

        public static bool operator ==(OnlineClient onClt1, OnlineClient onClt2)
        {
            if (ReferenceEquals(onClt1, null))
            {
                return false;
            }
            if (ReferenceEquals(onClt2, null))
            {
                return false;
            }
            return onClt1.Equals(onClt2);
        }

        public static bool operator !=(OnlineClient onClt1, OnlineClient onClt2)
        {
            if (ReferenceEquals(onClt1, null))
            {
                return false;
            }
            if (ReferenceEquals(onClt2, null))
            {
                return false;
            }
            return !onClt1.Equals(onClt2);
        }
        #endregion
    }
}