using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CMS21MP.DataHandle;
using Il2Cpp;
using MelonLoader;

namespace CMS21MP.ClientSide.Functionnality
{
    public static class CarSpawn_Handling
    {
        public static Dictionary<int, carData> CarHandle = new Dictionary<int, carData>();
        public static bool HasFinishedUpdatingPre, HasFinishedUpdatingHand;

        public static void HandleCarSpawn()
        {
            HandleCar();
        }

        public static void HandleCar()
        {
            Dictionary<int, CarLoader> hasCar = new Dictionary<int, CarLoader>();
            for (int i = 0; i < MainMod.carLoaders.Length; i++)
            {
                var carLoader = MainMod.carLoaders[i];
                if (!String.IsNullOrEmpty(carLoader.carToLoad) && carLoader.placeNo != -1) // y a t'il un véhicule chargé ?
                {
                    hasCar.Add(i, MainMod.carLoaders[i]);
                }
            }

            foreach (KeyValuePair<int ,CarLoader> car in hasCar) // Ajouter les véhicule qui ne sont pas handled
            {
                if (!CarHandle.ContainsKey(car.Key))
                {
                    CarHandle.Add(car.Key, new carData(car.Value, car.Key));
                    CarPart_PreHandling.AddAllPartToHandleAlt(car.Key);
                    ClientSend.SpawnCars(new carData(car.Value, car.Key));
                }
            }

            foreach (KeyValuePair<int, carData> car in CarHandle.ToList()) // Supprimer les véhicule qui ne sont plus sur la scène
            {
                if (!hasCar.ContainsKey(car.Key))
                {
                    RemoveCar(car.Key);
                    CarHandle.Remove(car.Key);
                    car.Value.status = false;
                    ClientSend.SpawnCars(car.Value);
                }
            }
        }

        public static void RemoveCar(int carKey)
        {
            MPGameManager.OriginalParts.Remove(carKey);
            MPGameManager.OriginalEngineParts.Remove(carKey);
            MPGameManager.OriginalSuspensionParts.Remove(carKey);
            ExternalCarPart_Handling.OriginalCarParts.Remove(carKey);
            
            MPGameManager.PartsHandle.Remove(carKey);
            MPGameManager.EnginePartsHandle.Remove(carKey);
            MPGameManager.SuspensionPartsHandle.Remove(carKey);
            ExternalCarPart_Handling.CarPartsHandle.Remove(carKey);
        }
    }
}