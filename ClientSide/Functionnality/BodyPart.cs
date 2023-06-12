using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CMS21MP.DataHandle;
using MelonLoader;

namespace CMS21MP.ClientSide.Functionnality
{
    public static class BodyPart
    {
        public static Dictionary<int, Dictionary<int, carPartsData_info>> OriginalBodyParts = new Dictionary<int, Dictionary<int, carPartsData_info>>();
        public static Dictionary<int, Dictionary<int, carPartsData>> BodyPartsHandle = new Dictionary<int, Dictionary<int, carPartsData>>();
        
        private static object lockObject = new object();

        public static async Task PreHandleBodyParts(int carHandlerID)
        {
            lock (lockObject)
            {
                carData carData = CarSpawn.CarHandle[carHandlerID];
                int carLoaderID = carData.carLoaderID;

                int counter = 0;
                
                for (int i = 0; i < MainMod.carLoaders[carLoaderID].carParts.Count; i++)
                {
                    carPartsData_info part = new carPartsData_info(MainMod.carLoaders[carLoaderID].carParts._items[i],
                        carLoaderID, i);
                    if (!OriginalBodyParts.ContainsKey(carHandlerID))
                    {
                        OriginalBodyParts.Add(carHandlerID, new Dictionary<int, carPartsData_info>());
                    }

                    if (!OriginalBodyParts[carHandlerID].ContainsKey(counter))
                    {
                        OriginalBodyParts[carHandlerID].Add(counter, new carPartsData_info(MainMod.carLoaders[carLoaderID].carParts._items[i], carLoaderID, counter));
                    }
                    else
                    {
                        counter++;
                        i--;
                    }
                }
                CarSpawn.CarHandle[carLoaderID].FinishedPreHandlingBodyPart = true;
            }
            await Task.CompletedTask;
        }
        
        public async static Task HandleBodyParts()
        {
            var OriginalCarPartsCopy = OriginalBodyParts.ToList();
            
            foreach (KeyValuePair<int, Dictionary<int, carPartsData_info>> car in OriginalCarPartsCopy)
            {
                if (CarSpawn.CarHandle[car.Key].FinishedPreHandlingBodyPart)
                {
                    if (CarSpawn.CarHandle.ContainsKey(car.Key))
                    {
                        if (!CarSpawn.CarHandle[car.Key].CarPartFromServer)
                        {
                            var parts = car.Value;

                            foreach (var part in parts)
                            {
                                if (!BodyPartsHandle.ContainsKey(car.Key))
                                    BodyPartsHandle.Add(car.Key, new Dictionary<int, carPartsData>());

                                #region addToHandle

                                if (!BodyPartsHandle[car.Key].ContainsKey(part.Key))
                                {
                                    BodyPartsHandle[car.Key].Add(part.Key, parts[part.Key]._CarPartsData);
                                    ClientSend.bodyParts(BodyPartsHandle[car.Key][part.Key]);
                                }

                                #endregion

                                #region CheckForDifferences

                                if (BodyPartsHandle.ContainsKey(car.Key) &&
                                    BodyPartsHandle[car.Key].ContainsKey(part.Key))
                                {
                                    carPartsData handled = BodyPartsHandle[car.Key][part.Key];
                                    carPartsData original = new carPartsData(parts[part.Key]._originalPart,
                                        parts[part.Key]._partCountID, parts[part.Key]._carLoaderID,
                                        parts[part.Key]._UniqueID);
                                    if (HasDifferences(original, handled))
                                    {
                                        MelonLogger.Msg("Differences Found , sending updatedPart");
                                        BodyPartsHandle[car.Key][part.Key] = original;
                                        ClientSend.bodyParts(BodyPartsHandle[car.Key][part.Key]);
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
            }
        }

        public static bool HasDifferences(carPartsData original,carPartsData handled)
        {
            bool hasDif = false;
            if (original.unmounted != handled.unmounted)
                hasDif = true;
            if (original.switched != handled.switched)
                hasDif = true;

            return hasDif;
        }
    }
}