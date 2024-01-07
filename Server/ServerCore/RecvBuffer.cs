using System;
using System.Collections.Generic;
using System.Text;
//리시브의 보낸 데이터가 실제 보낸데이터와 다를수있다.
//예를들어 100 바이트로 데이터를 짜고 보냈는데
//사정상 어쩔수없이 쪼개서 보내진 케이스를 해결하기위해 이를
//퍼즐맞추기마냥 정상데이터로 만들어주는 서비스가 이 클래스다
namespace ServerCore
{
    public class RecvBuffer
    {
        ArraySegment<byte> _buffer;
        int _readPos;
        int _writePos;

        public RecvBuffer(int bufferSize)
        {
            _buffer = new ArraySegment<byte>(new byte[bufferSize],0,bufferSize);
        }

        public int DataSize { get { return _writePos - _readPos; } }
        public int FreeSize { get { return _buffer.Count - _writePos; } }

        public ArraySegment<byte> ReadSegment
        {
            get { return new ArraySegment<byte>(_buffer.Array,_buffer.Offset+_readPos,DataSize); }
        }

        public ArraySegment<byte> WriteSegment
        {
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writePos, FreeSize); }
        }

        public void Clean()
        {
            //풀링마냥 고갈을 막기위한 최적화기능
            //풀링은 선택사항이지만 예는 필수

            int dataSize = DataSize;
            if(dataSize == 0)
            {
                _readPos = _writePos = 0;
            }
            else
            {
                Array.Copy(_buffer.Array, _buffer.Offset + _readPos, _buffer.Array, _buffer.Offset, dataSize);
                _readPos = 0;
                _writePos = dataSize;

            }

        }

        public bool OnRead(int numOfBytes)
        {
            if (numOfBytes > DataSize)
                return false;

            _readPos += numOfBytes;
            return true;

        }

        public bool OnWrite(int numOfBytes)
        {
            if (numOfBytes > FreeSize)
                return false;

            _writePos += numOfBytes;
            return true;
        }
            



    }
}
