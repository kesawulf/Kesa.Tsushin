using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Kesa.Tsushin
{
    public class PacketServerConnection : PacketConnectionBase
    {
        public PacketServer Server { get; }

        public TcpClient Client { get; }

        public override PacketCommunicator Communicator { get; }

        public PacketServerConnection(PacketServer server, TcpClient client)
        {
            Server = server;
            Client = client;

            Communicator = new PacketCommunicator(this, client.GetStream(), server.Registry);
            Communicator.PacketReceived += OnCommunicatorPacketReceived;
        }

        private void OnCommunicatorPacketReceived(object sender, PacketReceivedEventArgs e)
        {
            Server.HandleReadPacket(this, e.Packet);
        }

        protected override void OnDisposing(bool disposeManagedObjects)
        {
            if (disposeManagedObjects)
            {
                Communicator.Dispose();
                Client.Dispose();
            }
        }
    }
}