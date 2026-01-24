using CMS21_Together_Core.Network.Packets;

namespace CMS21_Together_Core.Data;

public class ModGameState
{
	public WorldState WorldState = new WorldState();
	public GarageState GarageState = new GarageState();
	public PlayerState PlayerState = new PlayerState();
}