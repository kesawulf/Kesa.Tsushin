using System;
using System.Net.Sockets;

namespace Kesa.Tsushin
{
    public class PacketClient : PacketConnectionBase, IPacketReceiver
    {
        public event EventHandler<PacketReceivedEventArgs> PacketReceived;

        private PacketCommunicator _communicator;

        public string Ip { get; }

        public int Port { get; }

        public PacketRegistry Registry { get; }

        public override PacketCommunicator Communicator => _communicator;

        public PacketClient(string ip, int port)
        {
            Ip = ip;
            Port = port;
            Registry = new PacketRegistry();
        }

        public bool Connect(int timeoutMs = -1)
        {
            var client = new TcpClient();
            var ar = client.BeginConnect(Ip, Port, null, null);

            ar.AsyncWaitHandle.WaitOne(timeoutMs);

            if (ar.IsCompleted)
            {
                client.EndConnect(ar);

                _communicator = new PacketCommunicator(this, client.GetStream(), Registry);
                _communicator.PacketReceived += OnCommunicatorPacketRecieved;
                _communicator.ErrorOccurred += OnCommunicatorErrorOccurred;

                return true;
            }

            client.Dispose();
            return false;
        }

        private void OnCommunicatorPacketRecieved(object sender, PacketReceivedEventArgs e)
        {
            PacketReceived?.Invoke(this, e);
        }

        private void OnCommunicatorErrorOccurred(object sender, PacketCommunicatorErrorOccurredEventArgs e)
        {
            _communicator?.Dispose();
        }

        protected override void OnDisposing(bool disposeManagedObjects)
        {
            if (disposeManagedObjects)
            {
                Communicator.Dispose();
            }
        }
    }
}