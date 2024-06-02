using Leap;
using LeapManager.Wrappers;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

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

            while (Client != null)
            {
                try
                {
                    Frame l_frame = Boot.LeapController.Frame();

                    LeapObjects.HandObject[] Hands = new LeapObjects.HandObject[l_frame.Hands.Count];
                    for (int i = 0; i < Hands.Length; i++)
                    {
                        Hands[i] = new LeapObjects.HandObject();
                        Hands[i].IsLeft = l_frame.Hands[i].IsLeft;
                        Hands[i].IsRight = l_frame.Hands[i].IsRight;

                        Hands[i].Position = new LeapObjects.Position()
                        {
                            X = l_frame.Hands[i].PalmPosition.x,
                            Y = l_frame.Hands[i].PalmPosition.y,
                            Z = l_frame.Hands[i].PalmPosition.z
                        };

                        Hands[i].Rotation = new LeapObjects.Rotation()
                        {
                            X = l_frame.Hands[i].Rotation.x,
                            Y = l_frame.Hands[i].Rotation.y,
                            Z = l_frame.Hands[i].Rotation.z,
                            W = l_frame.Hands[i].Rotation.w
                        };

                        Hands[i].Fingers = new LeapObjects.FingerObject[l_frame.Hands[i].Fingers.Count];
                        for (int j = 0; j < Hands[i].Fingers.Length; j++)
                        {
                            Hands[i].Fingers[j] = new LeapObjects.FingerObject();
                            Hands[i].Fingers[j].Type = (LeapObjects.FingerType)l_frame.Hands[i].Fingers[j].Type;

                            Hands[i].Fingers[j].bones = new LeapObjects.BoneObject[l_frame.Hands[i].Fingers[j].bones.Length];
                            for (int k = 0; k < Hands[i].Fingers[j].bones.Length; k++)
                            {
                                Hands[i].Fingers[j].bones[k] = new LeapObjects.BoneObject();
                                Hands[i].Fingers[j].bones[k].Type = (LeapObjects.BoneType)l_frame.Hands[i].Fingers[j].bones[k].Type;

                                Hands[i].Fingers[j].bones[k].Rotation = new LeapObjects.Rotation()
                                {
                                    X = l_frame.Hands[i].Fingers[j].bones[k].Rotation.x,
                                    Y = l_frame.Hands[i].Fingers[j].bones[k].Rotation.y,
                                    Z = l_frame.Hands[i].Fingers[j].bones[k].Rotation.z,
                                    W = l_frame.Hands[i].Fingers[j].bones[k].Rotation.w,
                                };
                            }
                        }
                    }

                    SendMessage(JsonSerializer.Serialize(Hands));

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
