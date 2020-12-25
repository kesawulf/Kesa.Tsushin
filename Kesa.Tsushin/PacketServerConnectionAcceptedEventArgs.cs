using System;

namespace Kesa.Tsushin
{
    public class PacketServerConnectionAcceptedEventArgs : EventArgs
    {
        public PacketServerConnection Connection { get; }

        public PacketServerConnectionAcceptedEventArgs(PacketServerConnection connection)
        {
            Connection = connection;
        }
    }
}