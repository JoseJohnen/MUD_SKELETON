using MUD_Skeleton.Commons.Auxiliary;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
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
         * 2) Hacer que funcione Con Canales Virtuales (i.e. cada cliente esta en un canal y solo habla y escucha con el resto de los clientes en dicho canal, no escucha nada fuera del canal donde se encuentra)
         * 
         * 3) Hacer que cada cliente pueda tener mas de un socket de envío y mas de un socket de recepción simultáneamente
         * 
         * 4) Adicionar Protección contra Idempotencia
         */


        #region Channel (Msg) Related
        private uint activeChl = 0;
        public uint ActiveChl { get => activeChl; set => activeChl = value; }
        #endregion

        #region Channels (Thread) Related
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
            string tempString = string.Empty;
            while (await ReaderReceive.WaitToReadAsync())
            {
                string strTemp = await ReaderReceive.ReadAsync();
                tempString += strTemp.Trim();
                if (tempString.Contains("{") && tempString.Contains("}"))
                {
                    tempString = tempString.Replace("\0\0", "").Trim();
                    if (Message.IsValidMessage(tempString))
                    {
                        l_ReceiveQueueMessages.Enqueue(tempString);
                        tempString = string.Empty;
                    }
                }
                Console.Out.WriteLine($"ReadingReceive {strTemp}");
                Console.Out.WriteLine($"ReadingReceive {tempString}");
                Console.Out.WriteLine($"ReadingReceive {l_ReceiveQueueMessages.Count}");
                strTemp = string.Empty;
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
                if (tempString.Contains("{") && tempString.Contains("}"))
                {
                    l_SendQueueMessages.Enqueue(strTemp);
                    Console.BackgroundColor = ConsoleColor.Blue;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Out.WriteLine($"ReadingSend ¡¡SENDED!! {tempString}");
                    Console.ResetColor();
                    tempString = string.Empty;
                }
                Console.Out.WriteLine($"ReadingSend {strTemp}");
                Console.Out.WriteLine($"ReadingSend {tempString}");
                Console.Out.WriteLine($"ReadingSend {l_SendQueueMessages.Count}");
                strTemp = string.Empty;
            }
        }
        #endregion

        public static ConcurrentQueue<OnlineClient> cq_tcpOnlineClientsReceived = new ConcurrentQueue<OnlineClient>();

        private static List<OnlineClient> l_onlineClients = new List<OnlineClient>();
        public static List<OnlineClient> L_onlineClients
        {
            get
            {
                OnlineClient oClte = null;
                while (cq_tcpOnlineClientsReceived.TryDequeue(out oClte))
                //foreach (OnlineClient client in cq_tcpOnlineClientsReceived)
                {
                    if (oClte != null)
                    {
                        if (l_onlineClients.Where(c => c.Name == oClte.Name).Count() == 0)
                        {
                            l_onlineClients.Add(oClte);
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

        #region Attributes
        #region Functional
        public TcpClient clientToServerClient;
        public TcpClient serverToClientClient;

        private List<Pares<uint, uint>> l_channels = new List<Pares<uint, uint>>();
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
        private Queue<string> l_SendQueueMessages;
        public Queue<string> L_SendQueueMessages
        {
            get
            {
                if (l_SendQueueMessages == null)
                {
                    l_SendQueueMessages = new Queue<string>();
                }
                /*else if (l_SendQueueMessages.Count() == 0)
                {
                    l_SendQueueMessages = new ConcurrentQueue<string>();
                }*/
                return l_SendQueueMessages;
            }
            set => l_SendQueueMessages = value;
        }

        private Queue<string> l_ReceiveQueueMessages;
        public Queue<string> L_ReceiveQueueMessages
        {
            get
            {
                if (l_ReceiveQueueMessages == null)
                {
                    l_ReceiveQueueMessages = new Queue<string>();
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
            //if (L_onlineClients.Where(c => c.Name == name).Count() == 0)
            //{
            //    L_onlineClients.Add(this);
            //}
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