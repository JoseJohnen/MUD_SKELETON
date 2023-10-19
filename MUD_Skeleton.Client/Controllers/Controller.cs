using MUD_Skeleton.Commons.Comms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MUD_Skeleton.Client.Controllers
{
    public class Controller
    {
        private static Controller instance;
        public static Controller Instance { get => instance; private set => instance = value; }
        private static Thread ControllerRunning;
        private static Thread ReadingChannelReceive;
        private static Thread ReadingChannelSend;

        public static void Controller_Start()
        {
            Instance = new Controller();

            ControllerRunning = new Thread(() => Controller_Running());
            ControllerRunning.Start();

            ReadingChannelReceive = new Thread(() => ConnectionManager.ReadingChannelReceive());
            ReadingChannelReceive.Start();

            ReadingChannelSend = new Thread(() => ConnectionManager.ReadingChannelSend());
            ReadingChannelSend.Start();
        }

        public static async void Controller_Running()
        {
            while (true)
            {
                string tmpString = string.Empty;
                string strInstruction = string.Empty;
                //if (ConnectionManager.cq_instructionsReceived.Count > 0)
                //{
                    //while (ConnectionManager.cq_instructionsReceived.TryDequeue(out tmpString))
                    while (await ConnectionManager.ReaderReceiveProcess.WaitToReadAsync())
                    {
                        tmpString = await ConnectionManager.ReaderReceiveProcess.ReadAsync();
                        if (!string.IsNullOrWhiteSpace(tmpString))
                        {
                            strInstruction = ConnectionManager.ProcessDataFromServer(tmpString);
                            if (!string.IsNullOrWhiteSpace(strInstruction))
                            {
                                Console.Out.WriteLine("Received From Server " + strInstruction);
                            }
                        }
                    }
                //}
            }
        }

    }
}
