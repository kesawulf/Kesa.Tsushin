namespace Kesa.Tsushin
{
    public class PacketReceivedEventArgs : PacketEventArgsBase
    {
        public override PacketConnectionBase Connection { get; }

        public override Packet Packet { get; }
        
        public PacketReceivedEventArgs(PacketConnectionBase connection, Packet packet)
        {
            Connection = connection;
            Packet = packet;
        }
    }
}