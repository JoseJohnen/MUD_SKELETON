using MUD_Skeleton.Commons.Auxiliary;
using MUD_Skeleton.Commons.Comms;
using System.Threading.Channels;

namespace MUD_Skeleton.Client.Controllers
{
    public static class ConnectionManager
    {
        #region Static Utilitary Attributes
        public static Queue<string> cq_instructionsReceived = new Queue<string>();

        public static string receivedMessage = string.Empty;

        private static uint activeChl = 0;
        public static uint ActiveChl { get => activeChl; set => activeChl = value; }

        private static uint name = 0;
        public static uint Name
        {
            get => name; set => name = value;
        }

        //N° de mensaje, Canal
        private static List<Pares<uint, uint>> l_channels = new List<Pares<uint, uint>>();
        public static List<Pares<uint, uint>> L_channels
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
        #endregion

        #region Idempotency Protection
        private static uint idLastSendedId = 1;
        public static uint IdLastSendedId
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

        private static uint idLastReceivedId = 0;
        public static uint IdLastReceivedId
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

        #region Channels Related
        #region Shock absorbers
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
            try
            {
                string tempString = string.Empty;
                while (await ReaderReceive.WaitToReadAsync())
                {
                    string strTemp = await ReaderReceive.ReadAsync();
                    tempString += strTemp.Trim();
                    if (tempString.Contains("{") && tempString.Contains("}"))
                    {
                        tempString = tempString.Replace("\0\0", "").Trim();
                        uint idMsg = Message.GetIdMsgFromJson(tempString);

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
                            Console.Out.WriteLine($"ReadingReceive El servidor ha alcanzado el límite de registro de Idempotencia, reseteando el registro . . . ");
                            IdLastReceivedId = 0;
                            await WriterSend.WriteAsync("/RSTIDMPTC");
                            Console.BackgroundColor = ConsoleColor.Green;
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Out.WriteLine("Protección de idempotencia ha sido reseteado a 1 para el Servidor");
                            Console.ResetColor();
                        }

                        //Si este mensaje id ya ha sido recibido en el pasado
                        //(es decir si es menor o igual) entonces se ignora
                        if (idMsg < IdLastReceivedId)
                        {
                            continue;
                        }

                        if (Message.IsValidMessage(tempString))
                        {
                            await WriterReceiveProcess.WriteAsync(tempString);
                            tempString = string.Empty;
                        }
                    }
                    Console.Out.WriteLine($"ReadingReceive {strTemp}");
                    Console.Out.WriteLine($"ReadingReceive {tempString}");
                    Console.Out.WriteLine($"ReadingReceive {cq_instructionsReceived.Count}");
                    strTemp = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine($"Error ReadingChannelReceive: {ex.Message}");
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
            string tempString = string.Empty;
            while (await ReaderSend.WaitToReadAsync())
            {
                string strTemp = await ReaderSend.ReadAsync();
                tempString += strTemp.Trim();
                
                if (!string.IsNullOrWhiteSpace(tempString))
                {
                    Message Mandar = new Message(ConnectionManager.ActiveChl, tempString);
                    Mandar.IdMsg = IdLastSendedId;
                    Mandar.IdSnd = Name;
                    WriterSendProcess.WriteAsync(Mandar.ToJson());
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
        private static Channel<string> channelReceiveProcess = null;
        public static Channel<string> ChannelReceiveProcess
        {
            get
            {
                if (channelReceiveProcess == null)
                {
                    options.FullMode = BoundedChannelFullMode.Wait;
                    channelReceiveProcess = System.Threading.Channels.Channel.CreateBounded<string>(options);
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

        private static Channel<string> channelSendProcess = null;
        public static Channel<string> ChannelSendProcess
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

        private static ChannelWriter<string> writerSendProcess = null;
        public static ChannelWriter<string> WriterSendProcess
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

        private static ChannelReader<string> readerSendProcess = null;
        public static ChannelReader<string> ReaderSendProcess
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

        #region Data Processing
        public static string ProcessDataFromServer(string info)
        {
            string data = info;
            try
            {
                /*
                 * Do Some Cleaning-Preparing-Decrypting magic here
                 */
                Message msg = Message.CreateFromJson(data);

                string tmpString = msg.TextOriginal;
                string itm = tmpString;
                string content = string.Empty;
                string[] arrStr;

                if (tmpString.Contains("~"))
                {
                    //if it have some content, it prepares it, otherwise keep it as is 
                    if (tmpString.Contains(":"))
                    {
                        arrStr = tmpString.Split(":", StringSplitOptions.RemoveEmptyEntries);
                        itm = arrStr[0];
                        content = arrStr[1];
                    }
                }

                uint tempUint = 0;
                switch (itm)
                {
                    /*case "MV":
                         *
                         * Do Something Specific and return a response to the user
                         * adding it to the OnlineClient.l_onlineClients[position].L_SendQueueMessages.Enqueue
                         *
                        break; */
                    case "~RSTIDMPTC":
                        IdLastSendedId = 1;
                        Console.BackgroundColor = ConsoleColor.Green;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Out.WriteLine("Id de mensaje ha sido reseteado a 1 exitosamente");
                        Console.ResetColor();
                        return string.Empty;
                        break;
                    case "~ACHL":
                        if (uint.TryParse(content, out tempUint))
                        {
                            if (l_channels.Where(c => c.Item2 == tempUint).ToList().Count > 0)
                            {
                                ActiveChl = tempUint;
                                Console.BackgroundColor = ConsoleColor.Green;
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.Out.WriteLine("Canal " + tempUint + " Es el nuevo canal activo");
                                Console.ResetColor();
                            }
                            else if (ActiveChl == tempUint)
                            {
                                Console.BackgroundColor = ConsoleColor.Green;
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.Out.WriteLine("Canal " + tempUint + " Es Ya el Canal Activo");
                                Console.ResetColor();
                            }
                            else
                            {
                                Console.BackgroundColor = ConsoleColor.Green;
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.Out.WriteLine("Canal " + tempUint + " no esta en la lista de canales vinculados");
                                Console.ResetColor();
                            }
                        }
                        return string.Empty;
                        break;
                    case "~ADDCHL":
                        if (uint.TryParse(content, out tempUint))
                        {
                            if (l_channels.Where(c => c.Item2 == tempUint).ToList().Count == 0 && ActiveChl != tempUint)
                            {
                                l_channels.Add(new Pares<uint, uint>(0, tempUint));
                                Console.BackgroundColor = ConsoleColor.Green;
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.Out.WriteLine("Canal " + tempUint + " Agregado");
                                Console.ResetColor();
                            }
                        }
                        return string.Empty;
                        break;
                    case "~REMCHL":
                        if (uint.TryParse(content, out tempUint))
                        {
                            if (l_channels.Where(c => c.Item2 == tempUint).ToList().Count > 0)
                            {
                                l_channels.RemoveAll(c => c.Item2 == tempUint);
                                Console.BackgroundColor = ConsoleColor.Green;
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.Out.WriteLine("Canal " + tempUint + " Removido");
                                Console.ResetColor();
                            }
                        }
                        return string.Empty;
                        break;
                    case "~ISPRSNTCHL":
                        /* Para que se evite volver a enviar en caso de que se automatice en algún punto el auto-enviado */
                        Console.BackgroundColor = ConsoleColor.Green;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Out.WriteLine("Canal Ya Presente En Servidor, Agregando si es que falta en la lista local . . .");
                        Console.ResetColor();
                        if (uint.TryParse(content, out tempUint))
                        {
                            if (l_channels.Where(c => c.Item2 == tempUint).ToList().Count == 0)
                            {
                                l_channels.Add(new Pares<uint, uint>(0, tempUint));
                                Console.BackgroundColor = ConsoleColor.Green;
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.Out.WriteLine("Canal " + tempUint + " Agregado");
                                Console.ResetColor();
                            }
                        }
                        return string.Empty;
                        break;
                    case "~ISNONCHL":
                        /* Para decir que esta acción fue innecesaria pues el canal no existe/no esta registrado por el lado del server*/
                        /* 
                         * Igual es buena idea dado el caso de eliminar el canal por el lado del cliente para sincronizar las listas 
                         * Dado el caso que sea necesario
                         */
                        Console.BackgroundColor = ConsoleColor.Green;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Out.WriteLine("Canal No existe o el cliente no se encuentra vinculado al mismo");
                        Console.ResetColor();
                        if (uint.TryParse(content, out tempUint))
                        {
                            if (l_channels.Where(c => c.Item2 == tempUint).ToList().Count > 0)
                            {
                                l_channels.RemoveAll(c => c.Item2 == tempUint);
                            }
                        }
                        return string.Empty;
                        break;
                    case "~NAMERECEIVED":

                        return string.Empty;
                        break;
                    case "~YOURNAMEIS":
                        /* Asigna el nombre que tendrá en el servidor este cliente
                         * Elemento vital para poder establecer y mantener las comunicaciones
                         */
                        Console.BackgroundColor = ConsoleColor.Green;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Out.WriteLine($"Nombre asignado recibido desde el servidor: {content}");
                        Console.ResetColor();
                        if (uint.TryParse(content, out tempUint))
                        {
                            Name = tempUint;
                        }
                        return string.Empty;
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
                for (int i = 0; i < ConnectionManager.l_channels.Count; i++)
                {
                    strListNum += ConnectionManager.l_channels[i].Item2;
                    if (i < (ConnectionManager.l_channels.Count - 1))
                    {
                        strListNum += ",";
                    }
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
                Console.Out.WriteLineAsync(" Error ProcessDataFromServer(string): " + ex.Message + " data: " + data);
                Console.ResetColor();
                return string.Empty;
            }
        }
        #endregion
    }

}