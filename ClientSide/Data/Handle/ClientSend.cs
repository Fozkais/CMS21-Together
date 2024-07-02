using System.Net;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;

namespace CMS21Together.ClientSide.Data.Handle;

public class ClientSend
{
    private static void SendData(Packet _packet,bool reliable = true)
    {
        _packet.WriteLength();
        Client.Instance.SendData(_packet, reliable);
    }
    
    public static void ConnectValidationPacket()
    {
        using (Packet packet = new Packet((int)PacketTypes.connect))
        {
            packet.Write(ClientData.UserData.playerID);
            packet.Write(ClientData.UserData.username);
           /* packet.Write(ContentManager.Instance.OwnedContents); TODO: Reimplement those.
            packet.Write(ContentManager.Instance.gameVersion);*/
            packet.Write(MainMod.ASSEMBLY_MOD_VERSION);
                    
            SendData(packet);
        }
    }
}