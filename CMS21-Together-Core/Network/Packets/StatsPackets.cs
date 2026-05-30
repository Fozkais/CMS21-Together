using System;
using CMS21_Together_Core.Data.Enum;

namespace CMS21_Together_Core.Network.Packets;

[Serializable]
[NetworkPacket(PacketTypes.StatsAction)]
public class StatsActionPacket : INetworkData
{
    public int ExpDelta;
    public int ScrapsDelta;
}
