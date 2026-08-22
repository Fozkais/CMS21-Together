using System.Collections;
using CMS21_Together_Core;
using CMS21_Together_Core.Logging;
using CMS21_Together_Core.Network;
using CMS21_Together_Core.Network.Packets;
using CMS21Together.Logic.Hook;
using CMS21Together.Data;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.Network.Handlers
{
    public static class CarHandlers
    {
        [PacketHandler(PacketTypes.CarSpawnResponse)]
        public static void HandleCarSpawnResponse(long clientId, CarSpawnResponsePacket packet)
        {
            MelonCoroutines.Start(ProcessCarSpawnResponse(packet));
        }

        private static IEnumerator ProcessCarSpawnResponse(CarSpawnResponsePacket packet)
        {
            while (!ClientData.IsInventorySynced || !ClientData.IsGarageStateSynced)
                yield return new WaitForSeconds(0.25f);

            yield return new WaitForEndOfFrame();

            // Find CarLoader by its ID from CarLoaderPlaces
            CarLoader carLoader = CarLoaderPlaces.Get().GetCarLoaderByIndex(packet.CarLoaderID);
            
            if (carLoader == null) yield break;

            // Apply variables
            carLoader.placeNo = packet.PlaceNo;
            carLoader.ConfigVersion = packet.ConfigVersion;
            carLoader.customerCar = packet.IsJob;

            CarSpawnHooks.IgnoreCarSpawnHooks = true;
            try
            {
                carLoader.StartCoroutine(carLoader.LoadCar(packet.CarToLoad));
                Log.Info($"[CarHandlers] Loading {packet.CarToLoad} from server into Loader {packet.CarLoaderID}");
            }
            finally
            {
                CarSpawnHooks.IgnoreCarSpawnHooks = false;
            }

            // Wait for native LoadCar to finish (sets carLoader.done = true)
            while (!carLoader.IsCarLoaded())
                yield return new WaitForEndOfFrame();
            
            // Place it at position (critical for clients who didn't call TakeJob)
            carLoader.PlaceAtPosition(true, true);
        }

        [PacketHandler(PacketTypes.CarSpawnDelete)]
        public static void HandleCarSpawnDelete(long clientId, CarSpawnDeletePacket packet)
        {
            MelonCoroutines.Start(ProcessCarSpawnDelete(packet));
        }

        private static IEnumerator ProcessCarSpawnDelete(CarSpawnDeletePacket packet)
        {
            while (!ClientData.IsInventorySynced || !ClientData.IsGarageStateSynced)
                yield return new WaitForSeconds(0.25f);

            CarLoader carLoader = CarLoaderPlaces.Get().GetCarLoaderByIndex(packet.CarLoaderID);
            
            if (carLoader == null || string.IsNullOrEmpty(carLoader.carToLoad)) yield break;

            CarSpawnHooks.IgnoreCarSpawnHooks = true;
            try
            {
                carLoader.DeleteCar();
                Log.Info($"[CarHandlers] Deleted car from Loader {packet.CarLoaderID} as ordered by server.");
            }
            finally
            {
                CarSpawnHooks.IgnoreCarSpawnHooks = false;
            }
        }
    }
}
