using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kesa.Tsushin.Testing
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new PacketServer(26657);
            server.Registry.RegisterFromCurrentAssembly();
            server.StartListening();

            var client = new PacketClient("127.0.0.1", 26657);
            client.Registry.RegisterFromCurrentAssembly();
            if (client.Connect())
            {
                var packet = new TestPacket();
                packet.Value = 36;
                client.Communicator.Send(packet);
            }

            var sw = Stopwatch.StartNew();
            var pc = 0;

            server.PacketReceived += (s, e) =>
            {
                if (sw.ElapsedMilliseconds > 1000)
                {
                    Console.WriteLine($"Mirrored {pc} packets.");
                    sw.Restart();
                    pc = 0;
                }

                e.Connection.Communicator.Send(e.Packet);

                pc++;
            };

            while (true)
            {
                Thread.Sleep(100);
            }
        }
    }

    public class TestPacket : Packet
    {
        public override byte Id => 0x01;

        public int Value { get; set; }

        public override void WriteTo(BinaryWriter stream)
        {
            stream.Write(Value);
        }

        public override void ReadFrom(BinaryReader stream)
        {
            Value = stream.ReadInt32();
        }
    }
}
