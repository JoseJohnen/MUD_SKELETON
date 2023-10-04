using MUD_Skeleton.Commons.Comms;
using System.Collections.Concurrent;

namespace MUD_Skeleton.Server.Controllers
{
    public class InputController
    {
        private static InputController instance;
        public static InputController Instance { get => instance; private set => instance = value; }
        private static Thread InputControllerRunning;

        private static ConcurrentDictionary<string, InputController> _a;

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
                foreach (OnlineClient oNclt in OnlineClient.l_onlineClients.Reverse<OnlineClient>())
                {
                    while (oNclt.L_ReceiveQueueMessages.TryDequeue(out strInstruction))
                    {
                        if (!string.IsNullOrWhiteSpace(strInstruction))
                        {
                            ProcessDataFromPlayers(strInstruction, i);
                            Console.Out.WriteLine("oNclt.L_ReceiveQueueMessages count: " + oNclt.L_ReceiveQueueMessages.Count);
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
                Console.WriteLine("\n¡¡¡ PROCESS DATA FROM PLAYERS !!!");
                Console.ResetColor();

                /*
                 * Do Some Cleaning-Preparing-Decrypting magic here
                 */
                string item = data;

                switch (item)
                {
                    case "MV:":
                        /*
                         * Do Something Specific and return a response to the user
                         * adding it to the OnlineClient.l_onlineClients[position].L_SendQueueMessages.Enqueue
                         */
                        break;
                    default:
                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Out.WriteLineAsync("ProcessDataFromPlayers Switch Default; " + item + "\nData: " + data);
                        Console.ResetColor();
                        break;
                }
                
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
