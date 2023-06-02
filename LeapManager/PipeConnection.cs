using LeapManager.Wrappers;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LeapManager
{
    internal class PipeConnection
    {
        private static Socket Client;

        public static bool IsLeapActive()
        {
            return Client != null;
        }

        public static void StartPipe()
        {
            IPHostEntry host = Dns.GetHostEntry("localhost");
            IPAddress ipAddress = host.AddressList[0];
            IPEndPoint remoteEP = new(ipAddress, 1337);

            Client = new(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            Client.Connect(remoteEP);

            Logger.Log("Leap connected to Server");

            while (IsLeapActive())
            {
                Leap.Frame l_frame = Boot.LeapController.Frame();

                string RawHands = JsonConvert.SerializeObject(l_frame.Hands, Formatting.Indented, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                LeapObjects.HandObject[] Hands = JsonConvert.DeserializeObject<LeapObjects.HandObject[]>(RawHands);

                SendMessage(JsonConvert.SerializeObject(Hands, Formatting.Indented, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
                Thread.Sleep(1);
            }

            Boot.UnInitialize();
        }

        public static void SendMessage(string Message)
        {
            try
            {
                if (IsLeapActive()) Client.Send(Encoding.ASCII.GetBytes(Message));
            }
            catch
            {
                Boot.UnInitialize();
                Process.GetCurrentProcess().Kill();
            }
        }
    }
}
