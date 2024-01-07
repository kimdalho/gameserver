using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;


/// 처음은 이해하기 어려움
/// 계속 천천히 하나하나 보면 이해가능
/// 매우중요

namespace ServerCore
{
    public class Listener
    {
        Socket _listenSocket;
        Func<Session> _sessionFactory;

        public void Init(IPEndPoint endPoint, Func<Session> onAcceptHanlder)
        {
            //아이피 버전 ipv4 
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _sessionFactory= onAcceptHanlder;
            //교육
            _listenSocket.Bind(endPoint);
            // 시작 최대 대기수
            _listenSocket.Listen(10);

            for(int i =0; i <4; i++)
            {
               /* SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                //완료 되었을시 처리해야할 콜백 함수를 지정 -> OnAcceptCompleted
                args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
                RegisterAccept(args);*/
            }


            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            //완료 되었을시 처리해야할 콜백 함수를 지정 -> OnAcceptCompleted
            args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
            RegisterAccept(args);
        }

        //비동기 이 함수가 끝날때까지 기다리지 않고 진행가능
        //Accept 함수의 비효율적 처리방식의 상위호환
        void RegisterAccept(SocketAsyncEventArgs args)
        {
           args.AcceptSocket = null;
           bool pending =  _listenSocket.AcceptAsync(args);
            //만약 우연히 즉시 처리되었을 시 OnAcceptCompleted 호출
           if (pending == false)
                OnAcceptCompleted(null,args);
        }

        void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
        {
            if(args.SocketError == SocketError.Success)
            {
                //TODO
                Session session = _sessionFactory.Invoke();
                session.Start(args.AcceptSocket);
                session.OnConnected(args.AcceptSocket.RemoteEndPoint);
            }
            else
              Console.WriteLine(args.SocketError.ToString());

            //다시 비동기로 클라이언트의 신호를 받는상태로 대기
            RegisterAccept(args);
        }

        //동기식함수
        //순차적으로 코드처리가 완료되기에
        //응답을 줄때까지 영원히 대기한다.
        //블록킹 함수
        public Socket Accept()
        {
            return _listenSocket.Accept();
        }
    }
}
