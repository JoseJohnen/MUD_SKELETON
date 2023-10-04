using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;

namespace MUD_Skeleton.Client
{
    public static class ConnectionManager
    {
        public static ConcurrentQueue<string> cq_instructionsToSend = new ConcurrentQueue<string>();
        public static ConcurrentQueue<string> cq_instructionsReceived = new ConcurrentQueue<string>();

        public static string receivedMessage = string.Empty;
    }

}