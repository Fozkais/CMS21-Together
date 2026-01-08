using CMS21_Together_Core;
using CMS21_Together_Core.Network;

namespace CMS21Together.Network;

public class Client
{
	public static Client Instance;
	public ClientTCP tcp;

	public static void Init()
	{
		Instance = new Client();
		Instance.tcp = new ClientTCP();
	}

	public void ConnectToServer(string ip = "127.0.0.1")
	{
		tcp.Connect(ip, MainMod.PORT);
	}
        
	public void SendToServer<T>(T packetData) where T : INetworkData
	{
		PacketTypes id = PacketRouter.GetPacketId(packetData);
		using (Packet packet = new Packet((int)id))
		{
			packet.Write(packetData); // Sérialisation auto via ton Core/Packet.cs
			tcp.SendData(packet);
		}
	}
}