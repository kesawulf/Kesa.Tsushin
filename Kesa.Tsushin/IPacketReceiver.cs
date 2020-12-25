using System;
using System.Net.Sockets;

namespace Kesa.Tsushin
{
    public interface IPacketReceiver
    {
        event EventHandler<PacketReceivedEventArgs> PacketReceived;
    }
}