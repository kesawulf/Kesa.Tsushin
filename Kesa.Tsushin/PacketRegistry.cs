using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Kesa.Tsushin
{
    public class PacketRegistry
    {
        private List<PacketRegistryPacketInfo> Packets { get; }

        public PacketRegistry()
        {
            Packets = new List<PacketRegistryPacketInfo>();
        }

        public void Register<T>() where T : Packet
        {
            Register(typeof(T));
        }

        public void Register(Type type)
        {
            var instance = (Packet)FormatterServices.GetUninitializedObject(type);

            Packets.Add(new PacketRegistryPacketInfo()
            {
                Id = instance.Id,
                PacketType = type
            });
        }

        public void Register(Assembly assembly)
        {
            assembly.GetTypes()
                .Where(t => typeof(Packet).IsAssignableFrom(t))
                .Where(t => t != typeof(Packet))
                .ToList()
                .ForEach(Register);
        }

        public void RegisterFromCurrentAssembly()
        {
            Register(Assembly.GetCallingAssembly());
        }

        public Packet GetPacketInstance(byte id)
        {
            var packetInfo = Packets.FirstOrDefault(p => p.Id == id);
            if (packetInfo != null)
            {
                return (Packet)FormatterServices.GetUninitializedObject(packetInfo.PacketType);
            }

            return null;
        }
    }
}