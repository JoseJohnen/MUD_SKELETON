using MUD_Skeleton.Commons.Auxiliary;
using MUD_Skeleton.Commons.Comms;

namespace MUD_Skeleton.Server.Controllers
{
    public class InputController
    {
        private static InputController instance;
        public static InputController Instance { get => instance; private set => instance = value; }
        private static Thread InputControllerRunning;

        //private static ConcurrentDictionary<string, InputController> _a;

        public static void InputController_Start()
        {
            Instance = new InputController();

            InputControllerRunning = new Thread(() => InputController_Running());
            InputControllerRunning.Start();
        }

        public static void InputController_Running()
        {
            while (true)
            {
                string strInstruction = string.Empty;
                int i = 0;

                if (OnlineClient.l_onlineClients.Count == 0)
                {
                    continue;
                }

                foreach (OnlineClient oNclt in OnlineClient.l_onlineClients.Reverse<OnlineClient>())
                {
                    while (oNclt.L_ReceiveQueueMessages.TryDequeue(out strInstruction))
                    {
                        if (!string.IsNullOrWhiteSpace(strInstruction))
                        {
                            Console.Out.WriteLine("strInstruction: " + strInstruction);
                            string strInst = ProcessCommandsFromPlayer(strInstruction, i);
                            ProcessDataFromPlayers(strInst, i);
                            Console.Out.WriteLine("oNclt.L_ReceiveQueueMessages count: " + oNclt.L_ReceiveQueueMessages.Count);
                            Console.Out.WriteLine("oNclt.L_SendQueueMessages count: " + oNclt.L_SendQueueMessages.Count);
                            //ProcessDataFromPlayersIrc(strInstruction, i);
                        }
                    }
                    i++;
                }
            }
        }

        public static string ProcessDataFromPlayers(string data, int position)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(data))
                {
                    return String.Empty;
                }

                Console.BackgroundColor = ConsoleColor.Green;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n¡¡¡ ProcessDataFromPlayers !!!");
                Console.ResetColor();

                /*
                 * Do Some Cleaning-Preparing-Decrypting magic here
                 */

                Message msg = Message.CreateFromJson(data);
                string itm = msg.TextOriginal;
                Console.Out.WriteLineAsync("TextOriginal: " + msg.TextOriginal);
                Console.Out.WriteLineAsync("itm: " + itm);
                string content = string.Empty;
                string[] arrStr;
                if (itm.Contains(":"))
                {
                    arrStr = itm.Split(":", StringSplitOptions.RemoveEmptyEntries);
                    itm = arrStr[0];
                    content = arrStr[1];
                }
                else
                {
                    content = itm;
                }

                Console.Out.WriteLineAsync("itm Prepared: " + itm);

                string result = string.Empty;
                switch (itm)
                {
                    case "MV":
                        /*
                         * Do Something Specific and return a response to the user
                         * adding it to the OnlineClient.l_onlineClients[position].L_SendQueueMessages.Enqueue
                         */
                        break;
                    default:
                        break;
                }

                uint ChlMsg = msg.IdChl;

                /*** Dejo esto acá para finjir procesamiento dentro del switch***/
                result = "RESULT: "+content;


                /*
                 * ENVIAR Data a la gente que corresponda según el canal
                 */
                Message nwMsg = new Message(ChlMsg, result);
                foreach (OnlineClient onClt in OnlineClient.l_onlineClients.Where(a => a.L_channels.Any(x => x.Item2 == ChlMsg)).Reverse())
                {
                    onClt.WriterSend.WriteAsync(nwMsg.ToJson());
                }
                Console.BackgroundColor = ConsoleColor.Green;
                Console.ForegroundColor = ConsoleColor.White;
                Console.Out.WriteLine("Enviado "+result+" A todos los suscritos al canal "+ChlMsg);
                Console.ResetColor();

                return String.Empty;
            }
            catch (Exception ex)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Out.WriteLineAsync(" Error ProcessDataFromPlayers(string): " + ex.Message);
                Console.ResetColor();
                return String.Empty;
            }
        }

        public static string ProcessCommandsFromPlayer(string data, int position)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(data))
                {
                    return String.Empty;
                }

                Console.BackgroundColor = ConsoleColor.Green;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n¡¡¡ ProcessCommandsFromPlayer !!!");
                Console.ResetColor();

                /*
                 * Do Some Cleaning-Preparing-Decrypting magic here
                 */

                Message msg = Message.CreateFromJson(data);
                string itm = msg.TextOriginal;
                Console.Out.WriteLineAsync("TextOriginal: " + msg.TextOriginal);
                Console.Out.WriteLineAsync("itm: " + itm);
                string content = string.Empty;
                string[] arrStr;
                if (itm.Contains("/"))
                {
                    arrStr = itm.Split(":", StringSplitOptions.RemoveEmptyEntries);
                    itm = arrStr[0];
                    content = arrStr[1];
                }
                Console.Out.WriteLineAsync("itm Prepared: " + itm);

                uint tempUint = 0;
                switch (itm)
                {
                    //case "MV":
                        /*
                         * Do Something Specific and return a response to the user
                         * adding it to the OnlineClient.l_onlineClients[position].L_SendQueueMessages.Enqueue
                         */
                        //break;
                    case "/ACHL":
                        if (uint.TryParse(content, out tempUint))
                        {
                            if (OnlineClient.l_onlineClients[position].L_channels.Where(c => c.Item2 == tempUint).ToList().Count > 0 && OnlineClient.l_onlineClients[position].ActiveChl != tempUint)
                            {
                                OnlineClient.l_onlineClients[position].ActiveChl = tempUint;
                                Message ansW = new Message(OnlineClient.l_onlineClients[position].ActiveChl, "~ACHL:" + tempUint);
                                OnlineClient.l_onlineClients[position].WriterSend.WriteAsync(ansW.ToJson());
                                Console.BackgroundColor = ConsoleColor.Green;
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.Out.WriteLine("Canal " + tempUint + " Es el nuevo canal activo para " + OnlineClient.l_onlineClients[position].Name);
                                Console.ResetColor();
                            }
                            else if (OnlineClient.l_onlineClients[position].ActiveChl == tempUint)
                            {
                                Console.BackgroundColor = ConsoleColor.Green;
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.Out.WriteLine("Canal " + tempUint + " Es Ya el Canal Activo de " + OnlineClient.l_onlineClients[position].Name);
                                Console.ResetColor();
                            }
                            else
                            {
                                Console.BackgroundColor = ConsoleColor.Green;
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.Out.WriteLine("Canal " + tempUint + " No esta en la lista de canales vinculados para " + OnlineClient.l_onlineClients[position].Name);
                                Console.ResetColor();
                            }
                        }
                        break;
                    case "/ADD":
                        if (uint.TryParse(content, out tempUint))
                        {
                            if (OnlineClient.l_onlineClients[position].L_channels.Where(c => c.Item2 == tempUint).ToList().Count() <= 0)
                            {
                                OnlineClient.l_onlineClients[position].L_channels.Add(new Pares<uint, uint>(0, tempUint));
                                //OnlineClient.l_onlineClients[position].L_SendQueueMessages.Enqueue("~ADDCHL:" + tempUint);
                                //OnlineClient.l_onlineClients[position].WriterSend.WriteAsync("~ADDCHL:" + tempUint);
                                Message ansW = new Message(OnlineClient.l_onlineClients[position].ActiveChl, "~ADDCHL:" + tempUint);
                                OnlineClient.l_onlineClients[position].WriterSend.WriteAsync(ansW.ToJson());
                                string strListNum = string.Empty;
                                for (int i = 0; i < OnlineClient.l_onlineClients[position].L_channels.Count; i++)
                                {
                                    strListNum += OnlineClient.l_onlineClients[position].L_channels[i].Item2;
                                    if (i < (OnlineClient.l_onlineClients[position].L_channels.Count - 1))
                                    {
                                        strListNum += ",";
                                    }
                                }
                                Console.BackgroundColor = ConsoleColor.Green;
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.Out.WriteLineAsync("Channel " + tempUint + " Added, Current List is: [" + strListNum + "]");
                                Console.ResetColor();
                            }
                            else
                            {
                                /* El canal ya esta registrado al usuario */
                                //OnlineClient.l_onlineClients[position].L_SendQueueMessages.Enqueue("~ISPRSNTCHL:" + tempUint);
                                //OnlineClient.l_onlineClients[position].WriterSend.WriteAsync("~ISPRSNTCHL:" + tempUint);
                                Message ansW = new Message(OnlineClient.l_onlineClients[position].ActiveChl, "~ISPRSNTCHL:" + tempUint);
                                OnlineClient.l_onlineClients[position].WriterSend.WriteAsync(ansW.ToJson());
                                string strListNum = string.Empty;
                                for (int i = 0; i < OnlineClient.l_onlineClients[position].L_channels.Count; i++)
                                {
                                    strListNum += OnlineClient.l_onlineClients[position].L_channels[i].Item2;
                                    if (i < (OnlineClient.l_onlineClients[position].L_channels.Count - 1))
                                    {
                                        strListNum += ",";
                                    }
                                }
                                Console.BackgroundColor = ConsoleColor.Green;
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.Out.WriteLineAsync("Channel " + tempUint + " Is Already Registered, Current List is: [" + strListNum + "]");
                                Console.ResetColor();
                            }
                        }
                        break;
                    case "/REM":
                        if (uint.TryParse(content, out tempUint))
                        {
                            //if (OnlineClient.l_onlineClients[position].L_channels.Contains(tempUint))
                            if (OnlineClient.l_onlineClients[position].L_channels.Where(c => c.Item2 == tempUint).ToList().Count() > 0)
                            {
                                OnlineClient.l_onlineClients[position].L_channels.RemoveAll(c => c.Item2 == tempUint);
                                //OnlineClient.l_onlineClients[position].L_SendQueueMessages.Enqueue("~REMCHL:" + tempUint);
                                //OnlineClient.l_onlineClients[position].WriterSend.WriteAsync("~REMCHL:" + tempUint);
                                Message ansW = new Message(OnlineClient.l_onlineClients[position].ActiveChl, "~REMCHL:" + tempUint);
                                OnlineClient.l_onlineClients[position].WriterSend.WriteAsync(ansW.ToJson());
                                string strListNum = string.Empty;
                                for (int i = 0; i < OnlineClient.l_onlineClients[position].L_channels.Count; i++)
                                {
                                    strListNum += OnlineClient.l_onlineClients[position].L_channels[i].Item2;
                                    if (i < (OnlineClient.l_onlineClients[position].L_channels.Count - 1))
                                    {
                                        strListNum += ",";
                                    }
                                }
                                Console.BackgroundColor = ConsoleColor.Green;
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.Out.WriteLineAsync("Channel " + tempUint + " Removed, Current List is: [" + strListNum + "]");
                                Console.ResetColor();
                            }
                            else
                            {
                                /* No encuentra el canal o no existe */
                                //OnlineClient.l_onlineClients[position].L_SendQueueMessages.Enqueue("~ISNONCHL:" + tempUint);
                                //OnlineClient.l_onlineClients[position].WriterSend.WriteAsync("~ISNONCHL:" + tempUint);
                                Message ansW = new Message(OnlineClient.l_onlineClients[position].ActiveChl, "~ISNONCHL:" + tempUint);
                                OnlineClient.l_onlineClients[position].WriterSend.WriteAsync(ansW.ToJson());
                                string strListNum = string.Empty;
                                for (int i = 0; i < OnlineClient.l_onlineClients[position].L_channels.Count; i++)
                                {
                                    strListNum += OnlineClient.l_onlineClients[position].L_channels[i].Item2;
                                    if (i < (OnlineClient.l_onlineClients[position].L_channels.Count - 1))
                                    {
                                        strListNum += ",";
                                    }
                                }
                                Console.BackgroundColor = ConsoleColor.Green;
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.Out.WriteLineAsync("Channel " + tempUint + " Is Non-Channel, Current List is: [" + strListNum + "]");
                                Console.ResetColor();
                            }
                        }
                        break;
                    default:
                        Console.BackgroundColor = ConsoleColor.Green;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Out.WriteLineAsync("Data Is NOT a Command but is a Valid Message, Returning Data: " + data);
                        Console.ResetColor();
                        return data;
                        break;
                }
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Out.WriteLineAsync("ProcessCommandsFromPlayer Switch Exited Or Skipped; " + itm + "\nData: " + data);
                Console.ResetColor();

                return String.Empty;
            }
            catch (Exception ex)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Out.WriteLineAsync(" Error ProcessDataFromPlayers(string): " + ex.Message);
                Console.ResetColor();
                return String.Empty;
            }
        }
    }
}
