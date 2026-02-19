using CMS21_Together_Core;
using CMS21_Together_Core.Network;
using CMS21_Together_Core.Network.Packets;
using CMS21Together.Logic.Player;

namespace CMS21Together.Network.Handlers;

public static class PlayerHandlers
{
	[PacketHandler(PacketTypes.Movement)]
	public static void OnMovementUpdate(long senderId, MovementPacket packet)
	{
		Movement.UpdateRemotePlayer(packet);
	}
}