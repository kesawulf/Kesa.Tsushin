namespace Kesa.Tsushin
{
    public class PacketSentEventArgs : PacketEventArgsBase
    {
        public override PacketConnectionBase Connection { get; }

        public override Packet Packet { get; }
        
        public PacketSentEventArgs(PacketConnectionBase connection, Packet packet)
        {
            Connection = connection;
            Packet = packet;
        }
    }
}