using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CMS21MP.DataHandle;
using Il2Cpp;
using MelonLoader;

namespace CMS21MP.ClientSide.Functionnality
{
    public static class BodyPart
    {
        public static Dictionary<int, Dictionary<int, carPartsData_info>> OriginalCarParts = new Dictionary<int, Dictionary<int, carPartsData_info>>();
        public static Dictionary<int, Dictionary<int, carPartsData>> CarPartsHandle = new Dictionary<int, Dictionary<int, carPartsData>>();

        public async static void PreHandleBodyParts(int carHandlerID)
        {
            await Task.Delay(2000);

            carData carData = CarSpawn.CarHandle[carHandlerID]; //
            if (carData != null)
            {
                int carLoaderID = carData.carLoaderID;//
                if (carLoaderID < MainMod.carLoaders.Length)
                {
                    for (int i = 0; i < MainMod.carLoaders[carLoaderID].carParts.Count; i++)//
                    {
                        carPartsData_info part = new carPartsData_info(MainMod.carLoaders[carLoaderID].carParts._items[i], carLoaderID, i);
                        if (!OriginalCarParts.ContainsKey(carHandlerID))
                        {
                            OriginalCarParts.Add(carHandlerID, new Dictionary<int, carPartsData_info>());//
                        }
                        if (!OriginalCarParts[carHandlerID].ContainsKey(i))
                        {
                            OriginalCarParts[carHandlerID].Add(i, part);
                        }
                    }
                }
                else
                {
                   // MelonLogger.Warning($"PreHandleCarParts: carLoaderID {carLoaderID} is out of range for MainMod.carLoaders.");
                }
               // MelonLogger.Msg($"Finished Prehandling bodyPart for carLoader[{carLoaderID}]");
               CarSpawn.CarHandle[carLoaderID].FinishedPreHandlingCarPart = true;
            }
            else
            {
                await Task.Delay(2000);
                PreHandleBodyParts(carHandlerID);
            }
        }

        public async static void HandleBodyParts()
        {
            var OriginalCarPartsCopy = OriginalCarParts.ToList();
            
            foreach (KeyValuePair<int, Dictionary<int, carPartsData_info>> car in OriginalCarPartsCopy)
            {
                if (CarSpawn.CarHandle[car.Key].FinishedPreHandlingCarPart)
                {
                    if (CarSpawn.CarHandle.ContainsKey(car.Key))
                    {
                        if (!CarSpawn.CarHandle[car.Key].CarPartFromServer)
                        {
                            var parts = car.Value;

                            foreach (var part in parts)
                            {
                                if (!CarPartsHandle.ContainsKey(car.Key))
                                    CarPartsHandle.Add(car.Key, new Dictionary<int, carPartsData>());

                                #region addToHandle

                                if (!CarPartsHandle[car.Key].ContainsKey(part.Key))
                                {
                                    CarPartsHandle[car.Key].Add(part.Key, parts[part.Key]._CarPartsData);
                                    ClientSend.bodyParts(CarPartsHandle[car.Key][part.Key]);
                                }

                                #endregion

                                #region CheckForDifferences

                                if (CarPartsHandle.ContainsKey(car.Key) &&
                                    CarPartsHandle[car.Key].ContainsKey(part.Key))
                                {
                                    carPartsData handled = CarPartsHandle[car.Key][part.Key];
                                    carPartsData original = new carPartsData(parts[part.Key]._originalPart,
                                        parts[part.Key]._partCountID, parts[part.Key]._carLoaderID,
                                        parts[part.Key]._UniqueID);
                                    if (HasDifferences(original, handled))
                                    {
                                        MelonLogger.Msg("Differences Found , sending updatedPart");
                                        CarPartsHandle[car.Key][part.Key] = original;
                                        ClientSend.bodyParts(CarPartsHandle[car.Key][part.Key]);
                                    }
                                }

                                #endregion
                            }
                        }
                        else
                        {
                            await Task.Delay(4000);
                            CarSpawn.CarHandle[car.Key].CarPartFromServer = false;
                        }
                    }
                }
                else
                {
                    MelonLogger.Msg($"Dont contain key {car.Key}");
                }

            }
        }
        
        public static bool carPartIsExistingList(carPartsData partToHandle, List<carPartsData> otherList)
        {
            foreach (carPartsData carPartData in otherList)
            {
                if (carPartData.name == partToHandle.name && carPartData.carLoaderID == partToHandle.carLoaderID && carPartData.quality == partToHandle.quality && partToHandle.unmounted == carPartData.unmounted)
                {
                    //MelonLogger.Msg($"Rejected CarData: {partToHandle.name}");
                    return true;
                }
            }
            return false;
        }

        public static bool HasDifferences(carPartsData original,carPartsData handled)
        {
            bool hasDif = false;
            if (original.unmounted != handled.unmounted)
                hasDif = true;
            if (original.switched != handled.switched)
                hasDif = true;
            //else if (original.colors != handled.colors)
               // hasDif = true;
           /// else if (original.paintType != handled.paintType)
               // hasDif = true;

            return hasDif;
        }
    }
}