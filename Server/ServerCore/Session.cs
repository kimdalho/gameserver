using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using static System.Collections.Specialized.BitVector32;

namespace ServerCore
{

    public abstract class PacketSession : Session
    {
        public sealed override int OnRecv(ArraySegment<byte> buffer)
        {
            //파싱

            int processLen = 0; 
            while(true)
            {
                //최소한의 데이터 기준값(2는 헤더)
                if (buffer.Count < 2)
                    break;

                //완전하게 받았는지 체크
                ushort dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
                if (buffer.Count < dataSize)
                    break;

                OnRecvPacket(new ArraySegment<byte>(buffer.Array, buffer.Offset, dataSize));

                processLen += dataSize;
                buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset + dataSize, buffer.Count - dataSize);
            }

            return processLen;   
        }

        public abstract void OnRecvPacket(ArraySegment<byte> buffer);

    }


    public abstract class Session
    {
        Socket _socket;
        int _disconnected = 0;
        //최적화 언제든지 받을지 모르기에 멤버 변수로 넣는다.
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

        List<ArraySegment<byte>> pendingList = new List<ArraySegment<byte>>();
        Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();
        object _lock = new object();


        RecvBuffer _recvBuffer = new RecvBuffer(1024);


        public void Start(Socket socket)
        {
            this._socket = socket;

            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            //파람설명: 버퍼사이즈, 시작 좌표, 끝 좌표
            //_recvArgs.SetBuffer(new byte[1024],0,1024);
            RegisterRecv();
        }

        public abstract void OnConnected(EndPoint endPoint);
        public abstract int OnRecv(ArraySegment<byte> buffer);
        public abstract void OnSend(int numOfBytes);
        public abstract void OnDisconnected(EndPoint endPoint);



        public void Send(ArraySegment<byte> sendBuff)
        {
            Console.WriteLine("Send call");
            lock(_lock)
            {
                _sendQueue.Enqueue(sendBuff);
                //대기중인 상태의 보낼 메시지가 없다.
                if (pendingList.Count ==0 )
                    RegisterSend();
            }
        }




        public void Disconnect()
        {
            //쫒아낸다.

            // 현상태는 위험 다른곳에서 셧다운이 동시에 호출될수있음
            //_socket.Shutdown(SocketShutdown.Both);
            //_socket.Close();


            // 1++ 1-- 와는 다르다. 서버의 멀티 쓰레드환경을 위해 존재하는 1증가식 함수
            // 간단 설명 1++이 컴파일러가 해석하고 어셈블리어로 변환되면
            // 위 연산은 기준 3줄짜리 어셈블리어 코드가 된다.
            // 따라서 멀티 쓰레드 환경에서는 연산 인터럽트가 발생할수있기에 밑에와 같은 함수를 지원해준다.
            if (Interlocked.Exchange(ref _disconnected, 1) == 1)
                return;

            OnDisconnected(_socket.RemoteEndPoint);
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();

        }


        //#####################Private Function###############################
        private void RegisterRecv()
        {
            _recvBuffer.Clean();

            ArraySegment<byte> segment = _recvBuffer.WriteSegment;
            _recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

            bool panding = _socket.ReceiveAsync(_recvArgs);
            if (panding == false)
                OnRecvCompleted(null, _recvArgs);
        }

        //패딩 플러그 함수가 막아줘서 동시에 호출될일은 없다.
        private void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            //받은 바이트 체크
            if( args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    // Write 커서 이름
                    if(_recvBuffer.OnWrite(args.BytesTransferred) == false)
                    {
                        Disconnect();
                        return;
                    }

                    //짤린거 체크
                    int processLen = OnRecv(_recvBuffer.ReadSegment);               
                    if (processLen < 0 || _recvBuffer.DataSize <processLen)
                    {
                        Disconnect();
                        return;
                    }

                    if (_recvBuffer.OnRead(processLen) == false)
                    {
                        Disconnect();
                        return;
                    }

                    RegisterRecv();
                }
                catch(Exception e)
                {
                    Console.WriteLine($"OnRecvComplet {e.Message}");
                }
            } 
        }

        //#####################Private Function###############################

        //언제든지 보낼수있는 비동기식 보내기
        private void RegisterSend()
        {

            //한번에 보내기 처리의 가속성 높이는 방법
            while (_sendQueue.Count > 0)
            {
                ArraySegment<byte> buff = _sendQueue.Dequeue();
                //배열의 일부! new는 해당 함수의 구조가 구조체로 이루어져 콜바이 벨류로 값을 생성해줘야함
                pendingList.Add(buff);
            }
            _sendArgs.BufferList = pendingList;
            bool pending = _socket.SendAsync(_sendArgs);
            if (pending == false)
                OnSendCompleted(null, _sendArgs);
        }

        private void OnSendCompleted(object _object ,SocketAsyncEventArgs args)
        {
            lock(_lock)
            {
                //보낼때도 값이 적용됨
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {
                        //착각하면안됨 여긴 우채국 휴식공간같은거
                        //보내야할게 모두 없을때 이후를 받을수 있게끔

                        _sendArgs.BufferList = null;
                        pendingList.Clear();

                        OnSend(_sendArgs.BytesTransferred);

                    

                        if(_sendQueue.Count > 0)
                            RegisterSend();
                   
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"On Send Completed Exception {e}");
                    }
                }
                else
                {
                    Disconnect();
                }
            }
        }




    }
}
