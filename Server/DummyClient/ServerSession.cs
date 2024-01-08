using ServerCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;



namespace DummyClient
{
    public abstract class Packet
    {
        //2byte
        public ushort size;
        public ushort packetId;
       

        public abstract ArraySegment<byte> Write();
        public abstract void Read(ArraySegment<byte> s);
    }

    class PlayerInfoReq :Packet
    {
        public long playerId;
        public string name;

        public struct SkillInfo
        {
            public int id;
            public short level;
            public float duration;

            public bool Write(Span<byte> s, ref ushort count)
            {
                bool success = true;
                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), id);
                count += sizeof(int);
                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), level);
                count += sizeof(short);
                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), duration);
                count += sizeof(float);

                return success;
            }


            public void Read(ReadOnlySpan<byte> s, ref ushort count)
            { 
                this.id = BitConverter.ToInt32(s.Slice(count, s.Length - count));
                count += sizeof(int);
                this.level = BitConverter.ToInt16(s.Slice(count, s.Length - count));
                count += sizeof(short);
                this.duration = BitConverter.ToSingle(s.Slice(count, s.Length - count));
                count += sizeof(float);

            }

        }
        public List<SkillInfo> skills = new List<SkillInfo>();

        


        public PlayerInfoReq()
        {
            this.packetId = (ushort)PacketID.PlayerInfoReq;
        }


        public override void Read(ArraySegment<byte> segment)
        {
            ushort count = 0;

            ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
            count += sizeof(ushort);
            count += sizeof(ushort);
            this.playerId = BitConverter.ToUInt16(s.Slice(count,s.Length - count));
            count += sizeof(long);

            ushort nameLen = BitConverter.ToUInt16(s.Slice(count,s.Length - count));
            count += sizeof(ushort);

            this.name = Encoding.Unicode.GetString(s.Slice(count, nameLen));
            count += nameLen;

            ushort skillLen = BitConverter.ToUInt16(s.Slice( count, s.Length - count));
            //count += skillLen;
            count += sizeof(ushort);

            skills.Clear();
            for(int i = 0; i < skillLen;i++)
            {
                SkillInfo newSkill = new SkillInfo();
                newSkill.Read(s,ref count);
                skills.Add(newSkill);

            }

        }

        public override ArraySegment<byte> Write()
        {
            //보내기
            ArraySegment<byte> segment = SendBufferHelper.Open(4096);

            ushort count = 0;
            bool success = true;

            Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(s.Slice(count,s.Length - count), this.packetId);
            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
            count += sizeof(long);
            

            // string
            //ushort nameLen = (ushort)Encoding.Unicode.GetByteCount(this.name);
            //success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLen);
            //count += sizeof(ushort);
            //Encoding.Unicode.GetBytes(this.name);
            //Array.Copy(Encoding.Unicode.GetBytes(this.name), 0, segment.Array, count, nameLen);
           

            // 이 방식으로 이미지를 바이트로 보낼수도 있다.
            ushort nameLen = (ushort)Encoding.Unicode.GetBytes(this.name, 0, this.name.Length, segment.Array, segment.Offset + count + sizeof(ushort));
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLen);
            count += sizeof(ushort);
            count += nameLen;


            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)skills.Count);
            count += sizeof(ushort);

            foreach(SkillInfo skill in skills)
                success &= skill.Write(s, ref count);





            success &= BitConverter.TryWriteBytes(s, count);

            if (success == false)
                return null;

            return SendBufferHelper.Close(count); 
        }
    }

    class PlayerInfoOk : Packet
    {
        public int hp;
        public int attack;

        public override void Read(ArraySegment<byte> s)
        {
            throw new NotImplementedException();
        }

        public override ArraySegment<byte> Write()
        {
            return null;
        }
    }

    public enum PacketID
    {
        PlayerInfoReq = 1,
        PlayerInfoOk = 2,
    }


    class ServerSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"나 응애 클라 OnConnected : {endPoint}");

            List<PlayerInfoReq.SkillInfo> testSkills = new List<PlayerInfoReq.SkillInfo>();

            testSkills.Add(new PlayerInfoReq.SkillInfo() { id = 1, level = 4, duration = 0.1f });
            testSkills.Add(new PlayerInfoReq.SkillInfo() { id = 2, level = 4, duration = 1.3f });
            testSkills.Add(new PlayerInfoReq.SkillInfo() { id = 3, level = 4, duration = 12.4f });
            testSkills.Add(new PlayerInfoReq.SkillInfo() { id = 4, level = 4, duration = 12.3f });
            testSkills.Add(new PlayerInfoReq.SkillInfo() { id = 5, level = 4, duration = 1.54f });

            PlayerInfoReq packet = new PlayerInfoReq() { playerId = 1001, name = "용감한 아론",skills = testSkills };


            var sendBuff = packet.Write();

            if (sendBuff != null)
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
}
