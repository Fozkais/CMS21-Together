using System.Net;
using CMS21Together.ClientSide.Data.Player;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using CMS21Together.Shared.Data.Vanilla;
using CMS21Together.Shared.Data.Vanilla.Cars;
using CMS21Together.Shared.Data.Vanilla.GarageTool;
using CMS21Together.Shared.Data.Vanilla.Jobs;
using Il2Cpp;
using MelonLoader;

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
            packet.Write(ContentManager.Instance.ownedContents);
            packet.Write(ContentManager.Instance.gameVersion);
            packet.Write(MainMod.ASSEMBLY_MOD_VERSION);
                    
            SendData(packet);
        }
    }
    
    public static void ReadyPacket(bool isReady, int playerID)
    {
        using (Packet packet = new Packet((int)PacketTypes.readyState))
        {
            packet.Write(playerID);
            packet.Write(isReady);
                    
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

    public static void StatPacket(int diff, ModStats type, bool initial=false)
    {
        using (Packet packet = new Packet((int)PacketTypes.stat))
        {
            packet.Write(initial);
            packet.Write(diff);
            packet.Write(type);
            
            SendData(packet);
        }
    }

    public static void LifterPacket(ModLifterState state, int carLoaderID)
    {
        using (Packet packet = new Packet((int)PacketTypes.lifter))
        {
            packet.Write(state);
            packet.Write(carLoaderID);
            
            SendData(packet);
        }
    }
    
    public static void LoadJobCarPacket(ModCar car)
    {
        using (Packet packet = new Packet((int)PacketTypes.loadJobCar))
        {
            packet.Write(car);
            
            SendData(packet);
        }
    }

    public static void LoadCarPacket(ModNewCarData carData, int carLoaderID)
    {
        using (Packet packet = new Packet((int)PacketTypes.loadCar))
        {
            packet.Write(carData);
            packet.Write(carLoaderID);
            
            SendData(packet);
        }
    }

    public static void BodyPartPacket(ModCarPart carPart, int carLoaderID)
    {
        using (Packet packet = new Packet((int)PacketTypes.bodyPart))
        {
            packet.Write(carPart);
            packet.Write(carLoaderID);
            
            SendData(packet);
        }
    }

    public static void PartScriptPacket(ModPartScript partScript, int carLoaderID)
    {
        using (Packet packet = new Packet((int)PacketTypes.partScript))
        {
            packet.Write(partScript);
            packet.Write(carLoaderID);
            
            SendData(packet);
        }
    }

    public static void DeleteCarPacket(int carLoaderID)
    {
        using (Packet packet = new Packet((int)PacketTypes.deleteCar))
        {
            packet.Write(carLoaderID);
            
            SendData(packet);
        }
    }

    public static void CarPositionPacket(int carLoaderID, int no)
    {
        using (Packet packet = new Packet((int)PacketTypes.carPosition))
        {
            packet.Write(no);
            packet.Write(carLoaderID);
            
            SendData(packet);
        }
    }

    public static void GarageUpgradePacket(GarageUpgrade upgrade)
    {
        using (Packet packet = new Packet((int)PacketTypes.garageUpgrade))
        {
            packet.Write(upgrade);
            
            SendData(packet);
        }
    }

    public static void JobPacket(ModJob job)
    {
        using (Packet packet = new Packet((int)PacketTypes.newJob))
        {
            packet.Write(job);
            
            SendData(packet);
        }
    }

    public static void JobActionPacket(int jobID, bool takeJob)
    {
        using (Packet packet = new Packet((int)PacketTypes.jobAction))
        {
            packet.Write(jobID);
            packet.Write(takeJob);
            
            SendData(packet);
        }
    }

    public static void SelectedJobPacket(ModJob job, bool action)
    {
        using (Packet packet = new Packet((int)PacketTypes.selectedJob))
        {
            packet.Write(job);
            packet.Write(action);
            
            SendData(packet);
        }
    }

    public static void SceneChangePacket(GameScene scene)
    {
        using (Packet packet = new Packet((int)PacketTypes.sceneChange))
        {
            packet.Write(scene);
            
            SendData(packet);
        }
    }

    public static void ToolPositionPacket(IOSpecialType tool, ModCarPlace place, bool playSound = false)
    {
        using (Packet _packet = new Packet((int)PacketTypes.toolMove))
        {
            _packet.Write((ModIOSpecialType)tool);
            _packet.Write(place);
            _packet.Write(playSound);

            SendData(_packet);
        }
    }
}