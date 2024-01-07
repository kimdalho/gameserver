using ServerCore;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DummyClient
{
    class GameSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected : {endPoint}");

            //보내기
            byte[] sendBuff = Encoding.UTF8.GetBytes($" Hi!");
            Send(sendBuff);

        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected : {endPoint}");
        }

        public override int OnRecv(ArraySegment<byte> buffer)
        {
            string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"[From Server] {recvData}");
            return buffer.Count;
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"BytesTransferred : {numOfBytes}");
        }
    }


    class Program
    {
        static string clientName = "Aron";


        

        static void Main(string[] args)
        {
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 3001);

            Connector connector = new Connector();
            connector.Connect(endPoint, () => { return new GameSession(); });


            while (true)
            {
                try
                {
                  
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                //0.1초
                Thread.Sleep(100);
            }

           
        }
    }
}
