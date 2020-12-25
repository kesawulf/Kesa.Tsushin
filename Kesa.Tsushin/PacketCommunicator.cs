using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Kesa.Tsushin
{
    public class PacketCommunicator : DisposableBase
    {
        public event EventHandler<PacketReceivedEventArgs> PacketReceived;

        public event EventHandler<PacketSentEventArgs> PacketSent;

        public event EventHandler<PacketCommunicatorErrorOccurredEventArgs> ErrorOccurred;

        public PacketConnectionBase Connection { get; }

        public PacketRegistry Registry { get; }

        private BlockingCollection<Packet> Packets { get; }

        private BinaryReader Reader { get; }

        private BinaryWriter Writer { get; }

        private HashSet<Type> NotifiedTypes { get; }

        public PacketCommunicator(PacketConnectionBase connection, Stream stream, PacketRegistry registry)
        {
            Registry = registry;
            Connection = connection;

            Packets = new BlockingCollection<Packet>();
            Reader = new BinaryReader(stream, Encoding.UTF8, true);
            Writer = new BinaryWriter(stream, Encoding.UTF8, true);
            NotifiedTypes = new HashSet<Type>();

            thread(ReadLoop);
            thread(SendLoop);

            void thread(Action act) => new Thread(new ThreadStart(act)) { IsBackground = true }.Start();
        }

        private void SendLoop()
        {
            Try(() =>
            {
                while (!IsDisposed)
                {
                    var packet = Packets.Take();

                    Writer.Write((byte)0x02);
                    Writer.Write((byte)Registry.Register(packet.GetType()));
                    packet.WriteTo(Writer);
                    Writer.Write((byte)0x03);
                    Writer.Flush();

                    PacketSent?.Invoke(this, new PacketSentEventArgs(Connection, packet));
                }
            });
        }

        private void ReadLoop()
        {
            Try(() =>
            {
                while (!IsDisposed && Read() is Packet packet)
                {
                    if (packet is TypeRegistrationPacket typePacket)
                    {
                        var type = Type.GetType(typePacket.FullTypeName);
                        Registry.Register(type);
                        NotifiedTypes.Add(type);
                    }

                    PacketReceived?.Invoke(this, new PacketReceivedEventArgs(Connection, packet));
                }
            });
        }

        private void Try(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, new PacketCommunicatorErrorOccurredEventArgs(ex));
            }
        }

        private Packet Read()
        {
            while (!IsDisposed)
            {
                if (Reader.ReadByte() == 0x02)
                {
                    var packetId = Reader.ReadByte();
                    var packet = Registry.GetPacketInstance(packetId);
                    packet.ReadFrom(Reader);

                    if (Reader.ReadByte() == 0x03)
                    {
                        return packet;
                    }
                }
            }

            return null;
        }

        public void Send(Packet packet)
        {
            var pType = packet.GetType();

            if (NotifiedTypes.Add(pType))
            {
                Packets.Add(new TypeRegistrationPacket() { FullTypeName = pType.AssemblyQualifiedName });
            }

            Registry.Register(pType);
            Packets.Add(packet);
        }

        protected override void OnDisposing(bool disposeManagedObjects)
        {
            if (disposeManagedObjects)
            {
                PacketReceived = null;
                PacketSent = null;
                ErrorOccurred = null;

                Reader.Dispose();
                Writer.Dispose();
                Packets.Dispose();
            }
        }
    }
}