using CMS21_Together_Core;
using CMS21_Together_Core.Network;
using CMS21_Together_Core.Network.Packets;

namespace CMS21Together.Network.Handlers;

public static class WorldStatesPackets
{
	[PacketHandler(PacketTypes.WorldState)]
	public static void HandleWorldState(long senderId, WorldState packet)
	{
		
	}
}