using System.Collections.Generic;
using CMS21Together.ClientSide.Data;
using CMS21Together.ClientSide.Data.Player;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using CMS21Together.Shared.Data.Vanilla;
using CMS21Together.Shared.Data.Vanilla.Cars;
using CMS21Together.Shared.Data.Vanilla.Jobs;
using MelonLoader;

namespace CMS21Together.ServerSide.Data;

public static class ServerSend
{
    #region Functions

        private static void SendData(int _toClient,  Packet _packet, bool reliable = true)
        {
            _packet.WriteLength();
            //MelonLogger.Msg($"SendData[{_toClient}]");
            Server.Instance.clients[_toClient].SendData(_packet, reliable);
        }
        private static void SendDataToAll(Packet _packet, bool reliable = true)
        {
            _packet.WriteLength();
            foreach (KeyValuePair<int,ServerConnection> serverClient in Server.Instance.clients)
            {
                serverClient.Value.SendData(_packet, reliable);
            }
                
        }
        private static void SendDataToAll(int _exceptClient,Packet _packet, bool reliable = true)
        {
            _packet.WriteLength();
            foreach (KeyValuePair<int,ServerConnection> serverClient in Server.Instance.clients)
            {
                if(serverClient.Key != _exceptClient)
                    serverClient.Value.SendData(_packet, reliable);
            }
        }

    #endregion

    #region User

        public static void ConnectPacket(int clientId, string message)
        {
            using (Packet packet = new Packet((int)PacketTypes.connect))
            {
                packet.Write(message);
                packet.Write(clientId);
                
                SendData(clientId, packet);
            }
        }
        public static void DisconnectPacket(int fromClient, string message)
        {
            using (Packet packet = new Packet((int)PacketTypes.disconnect))
            {
                packet.Write(message);
                    
                SendData(fromClient, packet);
            }
        }
        
        public static void UserDataPacket(UserData userData, int id=-1)
        {
            using (Packet packet = new Packet((int)PacketTypes.userData))
            {
                packet.Write(userData);
                    
                    
                if(id == -1)
                    SendDataToAll(packet);
                else
                    SendData(id, packet);
            }
        }
        public static void ReadyPacket(int fromClient, bool ready, int id)
        {
            using (Packet packet = new Packet((int)PacketTypes.readyState))
            {
                packet.Write(id);
                packet.Write(ready);
                    
                SendDataToAll(fromClient, packet);
            }
        }
        
        public static void StartPacket(Gamemode gamemode)
        {
            using (Packet packet = new Packet((int)PacketTypes.start))
            {
                packet.Write(gamemode);
                
                SendDataToAll(ClientData.UserData.playerID, packet);
            }
        }

    #endregion

    public static void PositionPacket(int fromClient, Vector3Serializable position)
    {
        using (Packet packet = new Packet((int)PacketTypes.position))
        {
            packet.Write(fromClient);
            packet.Write(position);
                    
            SendDataToAll(fromClient, packet);
        }
    }

    public static void RotationPacket(int fromClient, QuaternionSerializable rotation)
    {
        using (Packet packet = new Packet((int)PacketTypes.rotation))
        {
            packet.Write(fromClient);
            packet.Write(rotation);
                    
            SendDataToAll(fromClient, packet);
        }
    }

    public static void ItemPacket(int fromClient, ModItem item, InventoryAction action)
    {
        using (Packet packet = new Packet((int)PacketTypes.item))
        {
            packet.Write(action);
            packet.Write(item);
                    
            SendDataToAll(fromClient, packet);
        }
    }
    
    public static void GroupItemPacket(int fromClient, ModGroupItem item, InventoryAction action)
    {
        using (Packet packet = new Packet((int)PacketTypes.groupItem))
        {
            packet.Write(action);
            packet.Write(item);
                    
            SendDataToAll(fromClient, packet);
        }
    }

    public static void StatPacket(int fromClient, int value, ModStats type, bool initial)
    {
        using (Packet packet = new Packet((int)PacketTypes.stat))
        {
            packet.Write(initial);
            packet.Write(value);
            packet.Write(type);
                    
            SendDataToAll(fromClient, packet);
        }
    }

    public static void LifterPacket(int fromClient, ModLifterState state, int carLoaderID)
    {
        using (Packet packet = new Packet((int)PacketTypes.lifter))
        {
            packet.Write(state);
            packet.Write(carLoaderID);

            SendDataToAll(fromClient, packet);
        }
    }
    
    public static void LoadCarPacket(int fromClient, ModNewCarData carData, int carLoaderID)
    {
        using (Packet packet = new Packet((int)PacketTypes.loadCar))
        {
            packet.Write(carData);
            packet.Write(carLoaderID);
            
            SendDataToAll(fromClient, packet);
        }
    }

    public static void BodyPartPacket(int fromClient, ModCarPart carPart, int carLoaderID)
    {
        using (Packet packet = new Packet((int)PacketTypes.bodyPart))
        {
            packet.Write(carPart);
            packet.Write(carLoaderID);
            
            SendDataToAll(fromClient, packet);
        }
        MelonLogger.Msg("[ServerSend->PartScriptPacket] Sent BodyPart.");
    }
    
    public static void PartScriptPacket(int fromClient,ModPartScript partScript, int carLoaderID)
    {
        using (Packet packet = new Packet((int)PacketTypes.partScript))
        {
            packet.Write(partScript);
            packet.Write(carLoaderID);
            
            SendDataToAll(fromClient, packet);
        }
        MelonLogger.Msg("[ServerSend->PartScriptPacket] Sent PartScript.");
    }

    public static void DeleteCarPacket(int fromClient, int carLoaderID)
    {
        using (Packet packet = new Packet((int)PacketTypes.deleteCar))
        {
            packet.Write(carLoaderID);
            
            SendDataToAll(fromClient, packet);
        }
    }

    public static void CarPositionPacket(int fromClient, int carLoaderID, int placeNo)
    {
        using (Packet packet = new Packet((int)PacketTypes.carPosition))
        {
            packet.Write(placeNo);
            packet.Write(carLoaderID);
            
            SendDataToAll(fromClient, packet);
        }
    }

    public static void GarageUpgradePacket(int fromClient, GarageUpgrade upgrade)
    {
        using (Packet packet = new Packet((int)PacketTypes.garageUpgrade))
        {
            packet.Write(upgrade);
            
            SendDataToAll(fromClient, packet);
        }
    }

    public static void JobPacket(int fromClient, ModJob job)
    {
        using (Packet packet = new Packet((int)PacketTypes.newJob))
        {
            packet.Write(job);
            
            SendDataToAll(fromClient, packet);
        }
    }

    public static void JobActionPacket(int fromClient, int jobID, bool takeJob)
    {
        using (Packet packet = new Packet((int)PacketTypes.jobAction))
        {
            packet.Write(jobID);
            packet.Write(takeJob);
            
            SendDataToAll(fromClient, packet);
        }
    }

    public static void SelectedJobPacket(int fromClient, ModJob job, bool action)
    {
        using (Packet packet = new Packet((int)PacketTypes.selectedJob))
        {
            packet.Write(job);
            packet.Write(action);
            
            SendDataToAll(fromClient, packet);
        }
    }

    public static void SceneChangePacket(int fromClient, GameScene scene)
    {
        using (Packet packet = new Packet((int)PacketTypes.sceneChange))
        {
            packet.Write(scene);
            packet.Write(fromClient);
            
            SendDataToAll(fromClient, packet);
        }
    }
}