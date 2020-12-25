namespace Kesa.Tsushin
{
    public class PacketServerConnectionDroppedEventArgs
    {
        public PacketServerConnection Connection { get; }

        public PacketServerConnectionDroppedEventArgs(PacketServerConnection connection)
        {
            Connection = connection;
        }
    }
}