namespace Kesa.Tsushin
{
    public abstract class PacketConnectionBase : DisposableBase
    {
        public abstract PacketCommunicator Communicator { get; }
    }
}