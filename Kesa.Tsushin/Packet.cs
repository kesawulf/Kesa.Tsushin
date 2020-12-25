using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Kesa.Tsushin
{
    public abstract class Packet
    {
        public abstract void WriteTo(BinaryWriter stream);

        public abstract void ReadFrom(BinaryReader stream);
    }
}
