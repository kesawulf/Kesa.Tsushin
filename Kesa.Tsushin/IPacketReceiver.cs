using System;

namespace Kesa.Tsushin
{
    public interface IPacketReceiver
    {
        event EventHandler<PacketReceivedEventArgs> PacketReceived;
    }
}