using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Kesa.Tsushin
{
    public class PacketServer : IPacketReceiver
    {
        public event EventHandler<PacketReceivedEventArgs> PacketReceived;

        public event EventHandler<PacketServerConnectionAcceptedEventArgs> ConnectionAccepted;

        public event EventHandler<PacketServerConnectionDroppedEventArgs> ConnectionDropped;

        private TcpListener Listener { get; }

        public PacketRegistry Registry { get; }

        private List<PacketServerConnection> ConnectionsList { get; }

        public PacketServer(int port)
        {
            Registry = new PacketRegistry();
            ConnectionsList = new List<PacketServerConnection>();
            Listener = new TcpListener(IPAddress.Any, port);
        }

        public void StartListening()
        {
            Listener.Start();
            Listener.BeginAcceptTcpClient(OnConnectionAccepted, null);
        }

        public void Send(Packet packet)
        {
            foreach (var conn in ConnectionsList.ToArray())
            {
                conn.Communicator.Send(packet);
            }
        }

        private void OnConnectionAccepted(IAsyncResult result)
        {
            var client = Listener.EndAcceptTcpClient(result);
            var clientConnection = new PacketServerConnection(this, client);

            HandleConnectionAccepted(clientConnection);
        }

        internal void HandleReadPacket(PacketServerConnection connection, Packet packet)
        {
            PacketReceived?.Invoke(this, new PacketReceivedEventArgs(connection, packet));
        }

        internal void HandleConnectionAccepted(PacketServerConnection connection)
        {
            ConnectionsList.Add(connection);
            ConnectionAccepted?.Invoke(this, new PacketServerConnectionAcceptedEventArgs(connection));
            Listener.BeginAcceptTcpClient(OnConnectionAccepted, null);
        }

        internal void HandleConnectionDropped(PacketServerConnection connection)
        {
            if (ConnectionsList.Remove(connection))
            {
                connection.Client.Dispose();
                ConnectionDropped?.Invoke(this, new PacketServerConnectionDroppedEventArgs(connection));
            }
        }
    }
}