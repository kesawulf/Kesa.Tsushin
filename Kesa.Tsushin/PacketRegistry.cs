using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;

namespace Kesa.Tsushin
{
    public class PacketRegistry
    {
        private class PacketTypeInfo
        {
            public int Id { get; set; }
            public Type Type { get; set; }
        }

        private Dictionary<string, PacketTypeInfo> PacketInfo { get; }

        private Dictionary<int, PacketTypeInfo> PacketLookup { get; }

        private int _lastId;

        public PacketRegistry()
        {
            PacketInfo = new Dictionary<string, PacketTypeInfo>();
            PacketLookup = new Dictionary<int, PacketTypeInfo>();
            Register(typeof(TypeRegistrationPacket));
        }

        public int Register(Type type)
        {
            var typeName = type.AssemblyQualifiedName;

            if (PacketInfo.TryGetValue(typeName, out var info))
            {
                return info.Id;
            }
            else
            {
                var newId = Interlocked.Increment(ref _lastId);
                var newInfo = new PacketTypeInfo()
                {
                    Id = newId,
                    Type = type
                };

                PacketInfo[type.AssemblyQualifiedName] = newInfo;
                PacketLookup[newId] = newInfo;
                return newId;
            }
        }

        public Packet GetPacketInstance(byte id)
        {
            if (PacketLookup.TryGetValue(id, out var info))
            {
                return (Packet)FormatterServices.GetUninitializedObject(info.Type);
            }

            return null;
        }
    }
}