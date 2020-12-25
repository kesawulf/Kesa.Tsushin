using System;

namespace Kesa.Tsushin
{
    public class PackerClientPacketReceivedEventArgs : EventArgs
    {
        public PacketClient Connection { get; }
        public Packet Packet { get; }

        public PackerClientPacketReceivedEventArgs(PacketClient connection, Packet packet)
        {
            Connection = connection;
            Packet = packet;
        }
    }
}