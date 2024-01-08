using ServerCore;
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
        static void Main(string[] args)
        {
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 3001);

            Connector connector = new Connector();
            connector.Connect(endPoint, () => { return new ServerSession(); });


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
                Thread.Sleep(1000);
            }

           
        }
    }
}
