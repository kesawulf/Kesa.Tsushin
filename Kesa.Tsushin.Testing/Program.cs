using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Kesa.Tsushin.Testing
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            MirrorRateTesting();

            //Test2();
            //Test3();
            //...
        }

        private static void MirrorRateTesting()
        {
            var server = new PacketServer(26657);
            server.StartListening();

            var client = new PacketClient("127.0.0.1", 26657);
            var sw = Stopwatch.StartNew();
            var pc = 0;

            server.PacketReceived += (s, e) => mirror(e);
            client.PacketReceived += (s, e) => mirror(e);

            if (client.Connect())
            {
                client.Communicator.Send(new PingPacket());
            }
            else
            {
                throw new Exception("How?");
            }

            while (true)
            {
                Thread.Sleep(-1);
            }

            void mirror(PacketReceivedEventArgs e)
            {
                if (e.Packet is PingPacket pingPacket)
                {
                    e.Connection.Communicator.Send(new PongPacket() { Value = pingPacket.Value + 1 });
                }
                else if (e.Packet is PongPacket pongPacket)
                {
                    e.Connection.Communicator.Send(new PingPacket() { Value = pongPacket.Value + 1 });
                }

                if (e.Connection is PacketClient && sw.ElapsedMilliseconds > 1000)
                {
                    Console.WriteLine($"Mirrored {pc / 2} packets.");
                    pc = 0;
                    sw.Restart();
                }

                pc++;
            }
        }
    }

    public class JsonPacket<T> : Packet
    {
        public T Data { get; set; }

        public override void WriteTo(BinaryWriter stream) => stream.Write(JsonConvert.SerializeObject(Data));

        public override void ReadFrom(BinaryReader stream) => Data = JsonConvert.DeserializeObject<T>(stream.ReadString());
    }

    public class PingPacket : Packet
    {
        public int Value { get; set; }

        public override void WriteTo(BinaryWriter stream) => stream.Write(Value);

        public override void ReadFrom(BinaryReader stream) => Value = stream.ReadInt32();
    }

    public class PongPacket : Packet
    {
        public int Value { get; set; }

        public override void WriteTo(BinaryWriter stream) => stream.Write(Value);

        public override void ReadFrom(BinaryReader stream) => Value = stream.ReadInt32();
    }
}
