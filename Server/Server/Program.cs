using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ServerCore;

namespace Server
{
    //컨텐츠
    class GameSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected : {endPoint}");
            /*        byte[] sendBuff = Encoding.UTF8.GetBytes("welcome to my server");
                    Send(sendBuff);*/



         /*   byte[] sendBuff = new byte[1024];*/

            //최대 얼마나 사용할거냐
            ArraySegment<byte> openSegment =   SendBufferHelper.Open(4096);
            byte[] buffer = BitConverter.GetBytes(100);
            byte[] buffer2 = BitConverter.GetBytes(10);
            //8byte
            Array.Copy(buffer, 0, openSegment.Array, openSegment.Offset, buffer.Length);
            Array.Copy(buffer2, 0, openSegment.Array, openSegment.Offset + buffer.Length, buffer2.Length);

            ArraySegment<byte> sendBuff = SendBufferHelper.Close(buffer.Length + buffer2.Length);


            Send(sendBuff);


            Thread.Sleep(100);
            Disconnect();
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected : {endPoint}");
        }

        public override int OnRecv(ArraySegment<byte> buffer)
        {
            string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"[From Client] {recvData}");
            return buffer.Count;
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"BytesTransferred : {numOfBytes}");
        }
    }

    //엔진
    class Program
    {
        static Listener listener = new Listener();

        static void Main(string[] args)
        {
            //우리 네트워크 망에 존재하는 DNS 서버가 정보를 가지고있다.

            string host = Dns.GetHostName();
            //Console.WriteLine($"host {host}");

            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 3001);

            listener.Init(endPoint, () => { return new GameSession(); });
            Console.WriteLine("Linstening..");

            while (true)
            {


                //입장
                //Socket clientSocket = listener.Accept();


            }
        }
    }
}
