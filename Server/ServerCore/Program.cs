using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    class Program
    {
        static Listener listener= new Listener();
        static void OnAcceptHandler(Socket clientSocket)
        {
            try
            {
                //받는다
                byte[] recvBuff = new byte[1024];
                int recvBytes = clientSocket.Receive(recvBuff);
                string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
                Console.WriteLine($"[From Client] {recvData}");

                //보낸다.
                byte[] sendBuff = Encoding.UTF8.GetBytes("welcome to my server");
                clientSocket.Send(sendBuff);

                //쫒아낸다.
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        static void Main(string[] args)
        {
            //우리 네트워크 망에 존재하는 DNS 서버가 정보를 가지고있다.

            string host = Dns.GetHostName();
            //Console.WriteLine($"host {host}");
            
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr =  ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 3001);

            listener.Init(endPoint,OnAcceptHandler);
            Console.WriteLine("Linstening..");

            while (true)
            {
                

                //입장
                //Socket clientSocket = listener.Accept();


            }           
        }
    }
}
