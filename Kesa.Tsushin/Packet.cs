using System.IO;

namespace Kesa.Tsushin
{
    public abstract class Packet
    {
        public abstract void WriteTo(BinaryWriter stream);

        public abstract void ReadFrom(BinaryReader stream);
    }
}
