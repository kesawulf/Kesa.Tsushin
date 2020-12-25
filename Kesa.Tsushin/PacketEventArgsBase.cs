using System;

namespace Kesa.Tsushin
{
    public abstract class PacketEventArgsBase : EventArgs
    {
        public abstract PacketConnectionBase Connection { get; }

        public abstract Packet Packet { get; }
    }
}