using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;

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

        public static List<OnlineClient> l_onlineClients = new List<OnlineClient>();

        #region Attributes
        #region Functional
        public TcpClient clientToServerClient;
        public TcpClient serverToClientClient;

        public List<uint> l_channels = new List<uint>();

        public int bytesRead = 0;

        private byte[] buffer = new byte[1024];
        public byte[] Buffer
        {
            get => buffer;
            set
            {
                buffer = value;
                bytesRead = buffer.Length;
            }
        }
        public string Information
        {
            get
            {
                return Encoding.ASCII.GetString(Buffer);
            }
            set
            {
                Buffer = Encoding.ASCII.GetBytes(value);
            }
        }
        #endregion

        #region Game Related
        private string name = string.Empty;
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                if (l_onlineClients == null)
                {
                    l_onlineClients = new List<OnlineClient>();
                }
                if (l_onlineClients.Where(c => c.Name == name).Count() == 0)
                {
                    l_onlineClients.Add(this);
                }
                name = value;
            }
        }
        #endregion

        #region Data Instructions Administration
        private ConcurrentQueue<string> l_SendQueueMessages;
        public ConcurrentQueue<string> L_SendQueueMessages
        {
            get
            {
                if (l_SendQueueMessages == null)
                {
                    l_SendQueueMessages = new ConcurrentQueue<string>();
                }
                else if (l_SendQueueMessages.Count() == 0)
                {
                    l_SendQueueMessages = new ConcurrentQueue<string>();
                }
                return l_SendQueueMessages;
            }
            set => l_SendQueueMessages = value;
        }

        private ConcurrentQueue<string> l_ReceiveQueueMessages;
        public ConcurrentQueue<string> L_ReceiveQueueMessages
        {
            get
            {
                if (l_ReceiveQueueMessages == null)
                {
                    l_ReceiveQueueMessages = new ConcurrentQueue<string>();
                }
                else if (l_ReceiveQueueMessages.Count() == 0)
                {
                    l_ReceiveQueueMessages = new ConcurrentQueue<string>();
                }
                return l_ReceiveQueueMessages;
            }
            set => l_ReceiveQueueMessages = value;
        }

        #endregion
        #endregion

        #region Constructores
        public OnlineClient(string name) => Name = name;

        public OnlineClient()
        {
            if (l_onlineClients == null)
            {
                l_onlineClients = new List<OnlineClient>();
            }
            if (l_onlineClients.Where(c => c.Name == name).Count() == 0)
            {
                l_onlineClients.Add(this);
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