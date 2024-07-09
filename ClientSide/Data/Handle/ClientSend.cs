using System.Net;
using CMS21Together.ClientSide.Data.Player;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using CMS21Together.Shared.Data.Vanilla;

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

    public static void PositionPacket(Vector3Serializable position)
    {
        using (Packet packet = new Packet((int)PacketTypes.position))
        {
            packet.Write(position);
            SendData(packet);
        }
    }
    
    public static void RotationPacket(QuaternionSerializable rotation)
    {
        using (Packet packet = new Packet((int)PacketTypes.rotation))
        {
            packet.Write(rotation);
            SendData(packet);
        }
    }

    public static void ItemPacket(ModItem item, InventoryAction action)
    {
        using (Packet packet = new Packet((int)PacketTypes.item))
        {
            packet.Write(action);
            packet.Write(item);
            
            SendData(packet);
        }
    }
    
    public static void GroupItemPacket(ModGroupItem groupItem, InventoryAction action)
    {
        using (Packet packet = new Packet((int)PacketTypes.groupItem))
        {
            packet.Write(action);
            packet.Write(groupItem);
            
            SendData(packet);
        }
    }
}