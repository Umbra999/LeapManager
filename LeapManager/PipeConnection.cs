using LeapManager.Wrappers;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LeapManager
{
    internal class PipeConnection
    {
        private static Socket Client;

        public static void StartPipe()
        {
            IPHostEntry host = Dns.GetHostEntry("localhost");
            IPAddress ipAddress = host.AddressList[0];
            IPEndPoint remoteEP = new(ipAddress, 1337);

            Client = new(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            Client.Connect(remoteEP);

            Logger.Log("Leap connected to Server");

            new Thread(() =>
            {
                while (Client != null)
                {
                    try
                    {
                        byte[] bytes = new byte[5000];
                        int bytesRec = Client.Receive(bytes);
                        string Message = Encoding.ASCII.GetString(bytes, 0, bytesRec);

                        string[] command = Message.Contains('/') ? Message.Split('/') : new string[] { Message };

                        switch (command[0])
                        {
                            case "Shutdown":
                                Boot.UnInitialize();
                                break;
                        }
                    }
                    catch (SocketException)
                    {
                        Boot.UnInitialize();
                    }
                }
            }) { IsBackground = true }.Start();

            while (Client != null)
            {
                try
                {
                    Leap.Frame l_frame = Boot.LeapController.Frame();

                    string RawHands = JsonConvert.SerializeObject(l_frame.Hands, Formatting.Indented, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                    LeapObjects.HandObject[] Hands = JsonConvert.DeserializeObject<LeapObjects.HandObject[]>(RawHands);

                    SendMessage(JsonConvert.SerializeObject(Hands, Formatting.Indented, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));

                    Thread.Sleep(1);
                }
                catch (SocketException)
                {
                    Boot.UnInitialize();
                }
            }
        }

        public static void SendMessage(string Message)
        {
            try
            {
                Client.Send(Encoding.ASCII.GetBytes(Message));
            }
            catch
            {
                Boot.UnInitialize();
            }
        }
    }
}
