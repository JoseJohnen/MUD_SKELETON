using MUD_Skeleton.Commons.Comms;
using System.Collections.Concurrent;
using System.Reflection.PortableExecutable;
using System.Threading.Channels;

namespace MUD_Skeleton.Client.Controllers
{
    public static class ConnectionManager
    {
        public static Queue<string> cq_instructionsToSend = new Queue<string>();
        public static Queue<string> cq_instructionsReceived = new Queue<string>();

        public static string receivedMessage = string.Empty;

        private static List<uint> l_channels = new List<uint>();
        public static List<uint> L_channels
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

        #region Channels Related
        //IT does create "Back Pressure"
        //It will wait for space to be available in order to wait
        private static BoundedChannelOptions options = new BoundedChannelOptions(255);

        private static Channel<string> channelReceive = null;
        public static Channel<string> ChannelReceive
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

        private static ChannelWriter<string> writerReceive = null;
        public static ChannelWriter<string> WriterReceive
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

        private static ChannelReader<string> readerReceive = null;
        public static ChannelReader<string> ReaderReceive
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

        public static async void ReadingChannelReceive()
        {
            while (await ReaderReceive.WaitToReadAsync())
            {
                string strTemp = await ReaderReceive.ReadAsync();
                cq_instructionsReceived.Enqueue(strTemp);
                Console.Out.WriteLine($"ReadingReceive {strTemp}");
                Console.Out.WriteLine($"ReadingReceive {cq_instructionsReceived.Count}");
            }
        }

        private static Channel<string> channelSend = null;
        public static Channel<string> ChannelSend
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

        private static ChannelWriter<string> writerSend = null;
        public static ChannelWriter<string> WriterSend
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

        private static ChannelReader<string> readerSend = null;
        public static ChannelReader<string> ReaderSend
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

        public static async void ReadingChannelSend()
        {
            while (await ReaderSend.WaitToReadAsync())
            {
                string strTemp = await ReaderSend.ReadAsync();
                cq_instructionsToSend.Enqueue(strTemp);
                Console.Out.WriteLine($"ReadingSend {strTemp}");
                Console.Out.WriteLine($"ReadingSend {cq_instructionsToSend.Count}");
            }
        }
        #endregion


        public static string ProcessDataFromServer(string tmpString)
        {
            try
            {
                /*
                 * Do Some Cleaning-Preparing-Decrypting magic here
                 */
                string itm = tmpString;
                string content = string.Empty;
                string[] arrStr;
                if (tmpString.Contains("~"))
                {
                    arrStr = tmpString.Split(":", StringSplitOptions.RemoveEmptyEntries);
                    itm = arrStr[0];
                    content = arrStr[1];
                }

                uint tempUint = 0;
                switch (itm)
                {
                    case "MV:":
                        /*
                         * Do Something Specific and return a response to the user
                         * adding it to the OnlineClient.l_onlineClients[position].L_SendQueueMessages.Enqueue
                         */
                        break;
                    case "~ADDCHL":
                        if (uint.TryParse(content, out tempUint))
                        {
                            if (!l_channels.Contains(tempUint))
                            {
                                l_channels.Add(tempUint);
                                Console.Out.WriteLine("Canal " + tempUint + " Agregado");
                            }
                        }
                        break;
                    case "~REMCHL":
                        if (uint.TryParse(content, out tempUint))
                        {
                            if (l_channels.Contains(tempUint))
                            {
                                l_channels.Remove(tempUint);
                                Console.Out.WriteLine("Canal "+tempUint+" Removido");
                            }
                        }
                        break;
                    case "~ISPRSNTCHL":
                        /* Para que se evite volver a enviar en caso de que se automatice en algún punto el auto-enviado */
                        break;
                    case "~ISNONCHL":
                        /* Para decir que esta acción fue innecesaria pues el canal no existe/no esta registrado por el lado del server*/
                        /* 
                         * Igual es buena idea dado el caso de eliminar el canal por el lado del cliente para sincronizar las listas 
                         * Dado el caso que sea necesario
                         */
                        Console.Out.WriteLine("Canal No existe o el cliente no se encuentra vinculado al mismo");
                        if (uint.TryParse(content, out tempUint))
                        {
                            if (l_channels.Contains(tempUint))
                            {
                                l_channels.Remove(tempUint);
                            }
                        }
                        break;
                    default:
                        /* 
                         * Quiere decir que no es una instrucción de coordinación cliente-servidor sino que algo que funciona dentro
                         * del juego, y por tanto, la instrucción queda lista para ser procesada por el administrador de fantasía
                         */
                        return tmpString;
                        break;
                }
                string strListNum = string.Empty;
                foreach (uint str in ConnectionManager.l_channels)
                {
                    strListNum += str + ",";
                }
                Console.BackgroundColor = ConsoleColor.Green;
                Console.ForegroundColor = ConsoleColor.White;
                Console.Out.WriteLineAsync("Channel Current List is: [" + strListNum + "]");
                Console.ResetColor();
                return string.Empty;
            }
            catch (Exception ex)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Out.WriteLineAsync(" Error ProcessDataFromPlayers(string): " + ex.Message);
                Console.ResetColor();
                return string.Empty;
            }
        }
    }

}