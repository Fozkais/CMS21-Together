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

            // By the time IsGarageStateSynced/IsInventorySynced are true, CarLoaderPlaces
            // is already guaranteed populated (LoaderAddition.VanillaLoad calls
            // CarLoaderPlaces.Get().Load() before AskForSync is even sent, in the same
            // coroutine chain). A null result here is a real bug, not a timing race.
            CarLoader carLoader = CarLoaderPlaces.Get().GetCarLoaderByIndex(packet.CarLoaderID);
            if (carLoader == null)
            {
                Log.Error($"[CarHandlers] CarLoader {packet.CarLoaderID} not found, dropping CarSpawnResponse.");
                yield break;
            }

            // Apply variables
            carLoader.placeNo = packet.PlaceNo;
            carLoader.ConfigVersion = packet.ConfigVersion;
            carLoader.customerCar = packet.IsJob;

            CarSpawnHooks.Suppress(packet.CarLoaderID);
            try
            {
                carLoader.StartCoroutine(carLoader.LoadCar(packet.CarToLoad));
                Log.Info($"[CarHandlers] Loading {packet.CarToLoad} from server into Loader {packet.CarLoaderID}");

                // Wait for native LoadCar to finish (sets carLoader.done = true)
                while (!carLoader.IsCarLoaded())
                    yield return new WaitForEndOfFrame();

                // Place it at position (critical for clients who didn't call TakeJob)
                carLoader.PlaceAtPosition(true, true);
            }
            finally
            {
                CarSpawnHooks.Release(packet.CarLoaderID);
            }
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
            if (carLoader == null)
            {
                Log.Error($"[CarHandlers] CarLoader {packet.CarLoaderID} not found, dropping CarSpawnDelete.");
                yield break;
            }

            if (string.IsNullOrEmpty(carLoader.carToLoad)) yield break;

            CarSpawnHooks.Suppress(packet.CarLoaderID);
            try
            {
                carLoader.DeleteCar();
                Log.Info($"[CarHandlers] Deleted car from Loader {packet.CarLoaderID} as ordered by server.");
            }
            finally
            {
                CarSpawnHooks.Release(packet.CarLoaderID);
            }
        }

        [PacketHandler(PacketTypes.CarSpawnRejected)]
        public static void HandleCarSpawnRejected(long clientId, CarSpawnRejectedPacket packet)
        {
            MelonCoroutines.Start(ProcessCarSpawnRejected(packet));
        }

        private static IEnumerator ProcessCarSpawnRejected(CarSpawnRejectedPacket packet)
        {
            Log.Warn($"[CarHandlers] CarSpawnRequest for Loader {packet.CarLoaderID} was rejected by server: {packet.Reason}. Reverting local spawn.");

            CarLoader carLoader = CarLoaderPlaces.Get().GetCarLoaderByIndex(packet.CarLoaderID);
            if (carLoader == null || string.IsNullOrEmpty(carLoader.carToLoad)) yield break;

            CarSpawnHooks.Suppress(packet.CarLoaderID);
            try
            {
                carLoader.DeleteCar();
            }
            finally
            {
                CarSpawnHooks.Release(packet.CarLoaderID);
            }
        }
    }
}
