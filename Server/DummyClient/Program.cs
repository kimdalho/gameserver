using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DummyClient
{
    class Program
    {
        static string clientName = "Aron";


        

        static void Main(string[] args)
        {
            string host = Dns.GetHostName();

            while(true)
            {
                try
                {
                    IPHostEntry ipHost = Dns.GetHostEntry(host);
                    IPAddress ipAddr = ipHost.AddressList[0];
                    IPEndPoint endPoint = new IPEndPoint(ipAddr, 3001);
                    //뒷부분 설정은 세트
                    Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                    //입장 시도
                    socket.Connect(endPoint);
                    Console.WriteLine($"Connected To {socket.RemoteEndPoint.ToString()}");

                    //보내기
                    byte[] sendBuff = Encoding.UTF8.GetBytes($"{clientName}: Hi!");
                    int sendBytes = socket.Send(sendBuff);

                    //받기
                    //받게될 데이터의 사이즈 크기를 모른다는 가정하에  최대한 크게한다.
                    byte[] recvBuff = new byte[1024];
                    int recvBytes = socket.Receive(recvBuff);
                    string recvData = Encoding.UTF8.GetString(recvBuff);
                    Console.WriteLine($"[From Server] {recvData}");

                    //나간다.
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
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
