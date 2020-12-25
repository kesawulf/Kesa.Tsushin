using System;

namespace Kesa.Tsushin
{
    public class PacketCommunicatorErrorOccurredEventArgs : EventArgs
    {
        public Exception Exception { get; }

        public PacketCommunicatorErrorOccurredEventArgs(Exception ex)
        {
            Exception = ex;
        }
    }
}