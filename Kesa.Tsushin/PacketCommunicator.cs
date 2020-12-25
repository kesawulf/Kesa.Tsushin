using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Kesa.Tsushin
{
    public class PacketCommunicatorErrorOccurredEventArgs : EventArgs
    {
    }

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

        public PacketCommunicator(PacketConnectionBase connection, Stream stream, PacketRegistry registry)
        {
            Registry = registry;
            Connection = connection;

            Packets = new BlockingCollection<Packet>();
            Reader = new BinaryReader(stream, Encoding.UTF8, true);
            Writer = new BinaryWriter(stream, Encoding.UTF8, true);

            Task.Factory.StartNew(ReadLoop);
            Task.Factory.StartNew(SendLoop);
        }

        private void SendLoop()
        {
            Try(() =>
            {
                var packet = Packets.Take();

                Writer.Write((byte)0x02);
                Writer.Write(packet.Id);
                packet.WriteTo(Writer);
                Writer.Write((byte)0x03);
                Writer.Flush();

                PacketSent?.Invoke(this, new PacketSentEventArgs(Connection, packet));
                Task.Factory.StartNew(SendLoop);
            });
        }

        private void ReadLoop()
        {
            Try(() =>
            {
                if (Read() is Packet packet)
                {
                    PacketReceived?.Invoke(this, new PacketReceivedEventArgs(Connection, packet));

                    if (!IsDisposed)
                    {
                        Task.Factory.StartNew(ReadLoop);
                    }
                }
            });
        }

        private void Try(Action action)
        {
            try
            {
                action();
            }
            catch
            {
                ErrorOccurred?.Invoke(this, new PacketCommunicatorErrorOccurredEventArgs());
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
            Packets.Add(packet);
        }

        protected override void OnDisposing(bool disposeManagedObjects)
        {
            if (disposeManagedObjects)
            {
                Reader.Dispose();
                Writer.Dispose();
                Packets.Dispose();
            }
        }
    }
}