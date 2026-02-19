using System;
using CMS21_Together_Core.Data.Enum;

namespace CMS21_Together_Core.Network.Packets;

[Serializable]
[NetworkPacket(PacketTypes.UpgradeRequest)]
public class UpgradeRequest : INetworkData
{
	public string id;
	public int level;
	public UpgradeType type;
}