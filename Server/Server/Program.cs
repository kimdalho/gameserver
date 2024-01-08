using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ServerCore;

namespace Server
{


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

            listener.Init(endPoint, () => { return new ClientSession(); });
            Console.WriteLine("Linstening..");

            while (true)
            {


                //입장
                //Socket clientSocket = listener.Accept();


            }
        }
    }
}
