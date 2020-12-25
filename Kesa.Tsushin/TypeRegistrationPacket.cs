using System.IO;

namespace Kesa.Tsushin
{
    public class TypeRegistrationPacket : Packet
    {
        public string FullTypeName { get; set; }

        public override void ReadFrom(BinaryReader stream)
        {
            FullTypeName = stream.ReadString();
        }

        public override void WriteTo(BinaryWriter stream)
        {
            stream.Write(FullTypeName);
        }
    }
}