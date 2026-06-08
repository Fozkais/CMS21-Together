using CMS21_Together_Core;
using CMS21_Together_Core.Network;
using CMS21_Together_Core.Network.Packets;
using CMS21_Together_Server.Data;
using CMS21_Together_Server.Log;

namespace CMS21_Together_Server.Network.Handlers
{
    public static class CarHandlers
    {
        [PacketHandler(PacketTypes.CarSpawnRequest)]
        public static void HandleCarSpawnRequest(long clientId, CarSpawnRequestPacket packet)
        {
            Logger.Debug($"[CarHandlers] Received CarSpawnRequest from client {clientId} for Loader {packet.CarLoaderID} (Car: {packet.CarToLoad}, Job: {packet.IsJob})");

            var carState = GameDataManager.CurrentState.CarState;
            
            // Validate and store the car in the server state
            if (string.IsNullOrEmpty(packet.CarToLoad))
            {
                Logger.Error($"[CarHandlers] CarSpawnRequest from client {clientId} missing CarToLoad!");
                return;
            }

            var response = new CarSpawnResponsePacket
            {
                CarLoaderID = packet.CarLoaderID,
                CarToLoad = packet.CarToLoad,
                ConfigVersion = packet.ConfigVersion,
                PlaceNo = packet.PlaceNo,
                IsJob = packet.IsJob,
                JobID = packet.JobID
            };

            // Store in state
            carState.LoadedCars[packet.CarLoaderID] = response;

            // Broadcast the approved spawn to ALL OTHER clients
            Server.SendToClients(response, (int)clientId);
            Logger.Debug($"[CarHandlers] Broadcasted CarSpawnResponse for Loader {packet.CarLoaderID} to other clients.");
        }

        [PacketHandler(PacketTypes.CarSpawnDelete)]
        public static void HandleCarSpawnDelete(long clientId, CarSpawnDeletePacket packet)
        {
            Logger.Debug($"[CarHandlers] Received CarSpawnDelete from client {clientId} for Loader {packet.CarLoaderID}");

            var carState = GameDataManager.CurrentState.CarState;
            if (carState.LoadedCars.ContainsKey(packet.CarLoaderID))
            {
                carState.LoadedCars.Remove(packet.CarLoaderID);
            }
            
            // Broadcast the deletion to all other clients
            Server.SendToClients(packet, (int)clientId);
        }
    }
}
