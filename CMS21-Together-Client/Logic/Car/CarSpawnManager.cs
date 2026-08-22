using System.Collections;
using CMS21_Together_Core.Logging;
using CMS21_Together_Core.Network.Packets;
using CMS21Together.Network;

namespace CMS21Together.Logic.Car
{
    public static class CarSpawnManager
    {
        public static IEnumerator RequestCarSpawn(string carToLoad, CarLoader carLoader)
        {
            int carLoaderID = CarLoaderPlaces.Get().GetCarLoaderId(carLoader);

            var request = new CarSpawnRequestPacket
            {
                CarLoaderID = carLoaderID,
                CarToLoad = carToLoad,
                ConfigVersion = carLoader.ConfigVersion,
                PlaceNo = carLoader.placeNo,
                IsJob = carLoader.customerCar,
                JobID = -1 
            };
           
            Client.Instance.Send(request);
            Log.Info($"[CarSpawnManager] Requested spawn for {carToLoad} on Loader {carLoaderID}");
            yield break;
        }

        public static IEnumerator RequestCarDelete(CarLoader carLoader)
        {
            int carLoaderID = CarLoaderPlaces.Get().GetCarLoaderId(carLoader);

            var request = new CarSpawnDeletePacket
            {
                CarLoaderID = carLoaderID
            };

            Client.Instance.Send(request);
            Log.Info($"[CarSpawnManager] Requested delete for Loader {carLoaderID}");
            yield break;
        }
    }
}
