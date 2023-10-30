using MUD_Skeleton.Commons.Auxiliary;
using MUD_Skeleton.Commons.Comms;

namespace MUD_Skeleton.Server.Controllers
{
    public class InputController
    {
        private static InputController instance;
        public static InputController Instance { get => instance; private set => instance = value; }
        private static Thread InputControllerRunning;

        public static void InputController_Start()
        {
            Instance = new InputController();

            InputControllerRunning = new Thread(() => InputController_Running());
            InputControllerRunning.Start();
        }

        public static async void InputController_Running()
        {
            while (true)
            {
                if (OnlineClient.L_onlineClients.Count == 0)
                {
                    continue;
                }

                uint i = 0;
                string strInstruction = string.Empty;

                //ReaderReceiveProcess compiles all the messages post-shock absortion of every client in just
                //one single pool to work them out
                while (await OnlineClient.ReaderReceiveProcess.WaitToReadAsync())
                {
                    strInstruction = await OnlineClient.ReaderReceiveProcess.ReadAsync();
                    if (!string.IsNullOrWhiteSpace(strInstruction))
                    {
                        i = Message.GetIdSndFromJson(strInstruction);
                        Console.Out.WriteLine("strInstruction: " + strInstruction);
                        string strInst = ProcessCommandsFromPlayer(strInstruction, i);
                        ProcessDataFromPlayers(strInst, i);
                    }
                }
            }
        }

        public static string ProcessDataFromPlayers(string info, uint nameSender)
        {
            string data = info;
            try
            {
                if (string.IsNullOrWhiteSpace(data))
                {
                    return String.Empty;
                }

                Console.BackgroundColor = ConsoleColor.Green;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("¡¡¡ ProcessDataFromPlayers !!!");
                Console.ResetColor();

                /*
                 * Do Some Cleaning-Preparing-Decrypting magic here
                 */

                Message msg = Message.CreateFromJson(data);
                string itm = msg.TextOriginal;
                //Console.Out.WriteLineAsync("TextOriginal: " + msg.TextOriginal);
                //Console.Out.WriteLineAsync("itm: " + itm);
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
                result = "RESULT: " + content;

                /*
                 * ENVIAR Data a la gente que corresponda según el canal
                 */
                foreach (OnlineClient onClt in OnlineClient.L_onlineClients.Where(a => a.L_channels.Any(x => x.Item2 == ChlMsg)).Reverse())
                {
                    onClt.WriterSend.WriteAsync(result);
                }
                Console.BackgroundColor = ConsoleColor.Green;
                Console.ForegroundColor = ConsoleColor.White;
                Console.Out.WriteLine("Enviado " + result + " A todos los suscritos al canal " + ChlMsg);
                Console.ResetColor();

                return String.Empty;
            }
            catch (Exception ex)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Out.WriteLineAsync(" Error ProcessDataFromPlayers(string): " + ex.Message + " data: " + data);
                Console.ResetColor();
                return String.Empty;
            }
        }

        public static string ProcessCommandsFromPlayer(string data, uint nameSender)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(data))
                {
                    return String.Empty;
                }

                Console.BackgroundColor = ConsoleColor.Green;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("¡¡¡ ProcessCommandsFromPlayer !!!");
                Console.ResetColor();

                /*
                 * Do Some Cleaning-Preparing-Decrypting magic here
                 */

                Message msg = Message.CreateFromJson(data);
                string itm = msg.TextOriginal;

                string content = string.Empty;
                string[] arrStr;
                if (itm.Contains("/"))
                {
                    if (itm.Contains(":"))
                    {
                        arrStr = itm.Split(":", StringSplitOptions.RemoveEmptyEntries);
                        itm = arrStr[0];
                        content = arrStr[1];
                    }
                }

                uint tempUint = 0;
                OnlineClient clt = OnlineClient.L_onlineClients.Where(c => c.Name == Convert.ToString(nameSender)).FirstOrDefault();
                switch (itm)
                {
                    //case "MV":
                    /*
                     * Do Something Specific and return a response to the user
                     * adding it to the OnlineClient.l_onlineClients[position].L_SendQueueMessages.Enqueue
                     */
                    //break;
                    case "/RSTIDMPTC":
                        clt.IdLastSendedId = 1;
                        Console.BackgroundColor = ConsoleColor.Green;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Out.WriteLine($"Id de mensaje ha sido reseteado para {clt.Name} exitosamente");
                        Console.ResetColor();
                        return string.Empty;
                        break;
                    case "/ACHL":
                        if (uint.TryParse(content, out tempUint))
                        {
                            if (clt.L_channels.Where(c => c.Item2 == tempUint).ToList().Count > 0 && clt.ActiveChl != tempUint)
                            {
                                clt.ActiveChl = tempUint;
                                clt.WriterSend.WriteAsync("~ACHL:" + tempUint);
                                Console.BackgroundColor = ConsoleColor.Green;
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.Out.WriteLine("Canal " + tempUint + " Es el nuevo canal activo para " + clt.Name);
                                Console.ResetColor();
                            }
                            else if (clt.ActiveChl == tempUint)
                            {
                                Console.BackgroundColor = ConsoleColor.Green;
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.Out.WriteLine("Canal " + tempUint + " Es Ya el Canal Activo de " + clt.Name);
                                Console.ResetColor();
                            }
                            else
                            {
                                Console.BackgroundColor = ConsoleColor.Green;
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.Out.WriteLine("Canal " + tempUint + " No esta en la lista de canales vinculados para " + clt.Name);
                                Console.ResetColor();
                            }
                        }
                        return String.Empty;
                        break;
                    case "/ADD":
                        if (uint.TryParse(content, out tempUint))
                        {
                            //Add the channel, and make it the active one
                            if (clt.L_channels.Where(c => c.Item2 == tempUint).ToList().Count() <= 0)
                            {
                                //Add the channel
                                clt.L_channels.Add(new Pares<uint, uint>(0, tempUint));
                                clt.WriterSend.WriteAsync("~ADDCHL:" + tempUint);
                                //Change it to the active one
                                clt.ActiveChl = tempUint;
                                clt.WriterSend.WriteAsync("~ACHL:" + tempUint);

                                string strListNum = string.Empty;
                                for (int i = 0; i < clt.L_channels.Count; i++)
                                {
                                    strListNum += clt.L_channels[i].Item2;
                                    if (i < (clt.L_channels.Count - 1))
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
                                clt.WriterSend.WriteAsync("~ISPRSNTCHL:" + tempUint);

                                string strListNum = string.Empty;
                                for (int i = 0; i < clt.L_channels.Count; i++)
                                {
                                    strListNum += clt.L_channels[i].Item2;
                                    if (i < (clt.L_channels.Count - 1))
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
                        return String.Empty;
                        break;
                    case "/REM":
                        if (uint.TryParse(content, out tempUint))
                        {
                            if (clt.L_channels.Where(c => c.Item2 == tempUint).ToList().Count() > 0)
                            {
                                clt.L_channels.RemoveAll(c => c.Item2 == tempUint);
                                clt.WriterSend.WriteAsync("~REMCHL:" + tempUint);

                                string strListNum = string.Empty;
                                for (int i = 0; i < clt.L_channels.Count; i++)
                                {
                                    strListNum += clt.L_channels[i].Item2;
                                    if (i < (clt.L_channels.Count - 1))
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
                                clt.WriterSend.WriteAsync("~ISNONCHL:" + tempUint);

                                string strListNum = string.Empty;
                                for (int i = 0; i < clt.L_channels.Count; i++)
                                {
                                    strListNum += clt.L_channels[i].Item2;
                                    if (i < (clt.L_channels.Count - 1))
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
                        return String.Empty;
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
