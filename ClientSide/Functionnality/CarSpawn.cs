using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CMS21MP.DataHandle;
using Il2Cpp;

namespace CMS21MP.ClientSide.Functionnality
{
    public class CarSpawn
    {
        public static Dictionary<int, carData> CarHandle = new Dictionary<int, carData>();
        public static bool PauseHandle;

        public static async Task HandleCarSpawn()
        {
            if(!PauseHandle)
                await HandleCar();
        }

        public static Task HandleCar()
        {
            if (PauseHandle) // Vérifier si PauseHandle est true
                return Task.CompletedTask; // Retourner immédiatement la tâche
            
            Dictionary<int, CarLoader> carOnScene = new Dictionary<int, CarLoader>();
            for (int i = 0; i < MainMod.carLoaders.Length; i++)
            {
                var carLoader = MainMod.carLoaders[i];
                if (carLoader.placeNo != -1 && carLoader.modelLoaded && carLoader.e_engine_h != null && PreCarPart.isSuspensionPartReady(i) && carLoader.Parts._items.Length > 6 && carLoader.carParts._items.Length > 20) // y a t'il un véhicule chargé ?
                {
                    carOnScene.Add(i, MainMod.carLoaders[i]);
                }
            }

            foreach (KeyValuePair<int ,CarLoader> car in carOnScene) // Ajouter les véhicule qui ne sont pas handled
            {
                if (PauseHandle) // Vérifier si PauseHandle est true
                    return Task.CompletedTask; // Retourner immédiatement la tâche
                
                
                if (!CarHandle.ContainsKey(car.Key))
                {
                    if (PauseHandle) // Vérifier si PauseHandle est true
                        return Task.CompletedTask; // Retourner immédiatement la tâche
                    
                    CarHandle.Add(car.Key, new carData(car.Value, car.Key, false));
                    PreCarPart.AddAllPartToHandleAlt(car.Key);
                    ClientSend.SpawnCars(new carData(car.Value, car.Key, true));
                }
            }

            foreach (KeyValuePair<int, carData> car in CarHandle.ToList()) // Supprimer les véhicule qui ne sont plus sur la scène
            {
                if (!carOnScene.ContainsKey(car.Key))
                {
                    RemoveCar(car.Key);
                    CarHandle.Remove(car.Key);
                    car.Value.status = false;
                    ClientSend.SpawnCars(car.Value);
                }
            }

            return Task.CompletedTask;
        }

        public static void RemoveCar(int carKey)
        {
            MPGameManager.OriginalParts.Remove(carKey);
            MPGameManager.OriginalEngineParts.Remove(carKey);
            MPGameManager.OriginalSuspensionParts.Remove(carKey);
            BodyPart.OriginalBodyParts.Remove(carKey);
            
            MPGameManager.PartsHandle.Remove(carKey);
            MPGameManager.EnginePartsHandle.Remove(carKey);
            MPGameManager.SuspensionPartsHandle.Remove(carKey);
            BodyPart.BodyPartsHandle.Remove(carKey);
        }
    }
}