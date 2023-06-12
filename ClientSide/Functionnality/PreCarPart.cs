using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CMS21MP.DataHandle;
using Il2Cpp;
using UnityEngine;

namespace CMS21MP.ClientSide.Functionnality
{
    public static class PreCarPart
    {
        
        private static object lockObject = new object();
        
        
        public static async void AddAllPartToHandleAlt(int carKey)
        {
            if (!BodyPart.OriginalBodyParts.ContainsKey(carKey))
            {
                await BodyPart.PreHandleBodyParts(carKey);
            }
            if (!MPGameManager.OriginalParts.ContainsKey(carKey))
            {
                await AddPartHandle(carKey);
            }
            if (!MPGameManager.EnginePartsHandle.ContainsKey(carKey))
            {
                await AddEnginePartHandle(carKey);
            }
            if (!MPGameManager.SuspensionPartsHandle.ContainsKey(carKey))
            {
                await AddSuspensionPartHandle(carKey);
            }
        }
        
        public static async Task AddPartHandle(int carHandlerID)
        {
            lock (lockObject)
            {
                int carLoaderID = CarSpawn.CarHandle[carHandlerID].carLoaderID;
                for (int i = 0; i < MainMod.carLoaders[carLoaderID].Parts.Count; i++)
                {
                    var part_object = MainMod.carLoaders[carLoaderID].Parts._items[i].p_handle;
                    var _parts = part_object.GetComponentsInChildren<PartScript>().ToList();
            
                    if (!MPGameManager.OriginalParts.ContainsKey(carHandlerID))
                    {
                        MPGameManager.OriginalParts.Add(carHandlerID, new Dictionary<int, List<ModPartScript_Info>>());
                    }

                    if (!MPGameManager.OriginalParts[carHandlerID].ContainsKey(i))
                    {
                        MPGameManager.OriginalParts[carHandlerID].Add(i, new List<ModPartScript_Info>());
                    }
            
                    for (int j = 0; j < _parts.Count; j++)
                    {
                        if (!MPGameManager.OriginalParts[carHandlerID][i].Contains(new ModPartScript_Info(partType.part, _parts[j], carLoaderID, i, j)))
                        {
                            MPGameManager.OriginalParts[carHandlerID][i].Add(new ModPartScript_Info(partType.part, _parts[j], carLoaderID, i, j));
                        }
                    }
                }
                CarSpawn.CarHandle[carLoaderID].FinishedPreHandlingPart = true;
            }

            await Task.CompletedTask;
        }


        
        public static async Task AddEnginePartHandle(int carHandlerID)
        {
            lock (lockObject)
            {
                int carLoaderID = CarSpawn.CarHandle[carHandlerID].carLoaderID;
                var enginePartObject = MainMod.carLoaders[carLoaderID].e_engine_h;
                List<PartScript> engineParts = enginePartObject.GetComponentsInChildren<PartScript>().ToList();

                if (!MPGameManager.OriginalEngineParts.ContainsKey(carHandlerID))
                {
                    MPGameManager.OriginalEngineParts.Add(carHandlerID, new Dictionary<int, ModPartScript_Info>());
                }

                int counter = 0;

                for (int i = 0; i < engineParts.Count; i++)
                {
                    if (!MPGameManager.OriginalEngineParts[carHandlerID].ContainsKey(counter))
                    {
                        MPGameManager.OriginalEngineParts[carHandlerID]
                            .Add(counter, new ModPartScript_Info(partType.engine, engineParts[i], carLoaderID, 0, counter));
                        // Ajoute la sous-piece[j] dans la liste d'index [carLoaderID][counter]
                        //MelonLogger.Msg("Added PartScript_Info to OriginalEngineParts");
                    }
                    else
                    {
                        counter++;
                        i--;
                    }
                }
                CarSpawn.CarHandle[carLoaderID].FinishedPreHandlingEngine = true;
            }
            await Task.CompletedTask;
        }

        
        public  static async Task AddSuspensionPartHandle(int carHandlerID)
        {
            lock (lockObject)
            {
                int carLoaderID = CarSpawn.CarHandle[carHandlerID].carLoaderID; // carLoaderID
                List<GameObject> suspensionPartObjects = new List<GameObject>();
                suspensionPartObjects.Add(MainMod.carLoaders[carLoaderID].s_frontCenter_h);
                suspensionPartObjects.Add(MainMod.carLoaders[carLoaderID].s_frontLeft_h);
                suspensionPartObjects.Add(MainMod.carLoaders[carLoaderID].s_frontRight_h);
                suspensionPartObjects.Add(MainMod.carLoaders[carLoaderID].s_rearCenter_h);
                suspensionPartObjects.Add(MainMod.carLoaders[carLoaderID].s_rearLeft_h);
                suspensionPartObjects.Add(MainMod.carLoaders[carLoaderID].s_rearRight_h);
        
                for (int i = 0; i < suspensionPartObjects.Count; i++) // tout les gameobject "suspension" pour carLoaderID
                {
                    if (suspensionPartObjects[i] == null) break; // Vérifie si l'élément de la liste est null
        
                    var partInObject = suspensionPartObjects[i].GetComponentsInChildren<PartScript>().ToList(); // Toutes les pieces du gameobject[i]
                    for (int j = 0; j < partInObject.Count; j++)  // Parcours les sous-pieces de la piece[i] avec l'index[j]
                    {
                        if (!MPGameManager.OriginalSuspensionParts.ContainsKey(carHandlerID))
                        {
                            MPGameManager.OriginalSuspensionParts.Add(carHandlerID, new Dictionary<int, List<ModPartScript_Info>>());
                        }
                        if (!MPGameManager.OriginalSuspensionParts[carHandlerID].ContainsKey(i))
                        {
                            MPGameManager.OriginalSuspensionParts[carHandlerID].Add(i, new List<ModPartScript_Info>());
                        }
                        if (!MPGameManager.OriginalSuspensionParts[carHandlerID][i].Any(part => part._partScript == partInObject[j]))
                        {
                            MPGameManager.OriginalSuspensionParts[carHandlerID][i].Add(new ModPartScript_Info(partType.suspensions, partInObject[j], carLoaderID, i, j)); // Ajoute la piece[j] dans la liste d'index [carLoaderID][i]
                        }
                    }
                }
                
                CarSpawn.CarHandle[carLoaderID].FinishedPreHandlingSuspension = true;
            }
            await Task.CompletedTask;
        }

        public static bool isSuspensionPartReady(int carLoaderID)
        {
            var a = MainMod.carLoaders[carLoaderID].s_frontCenter_h;
            var b = MainMod.carLoaders[carLoaderID].s_frontLeft_h;
            var c = MainMod.carLoaders[carLoaderID].s_frontRight_h;
            var d = MainMod.carLoaders[carLoaderID].s_rearCenter_h;
            var e = MainMod.carLoaders[carLoaderID].s_rearLeft_h;
            var f = MainMod.carLoaders[carLoaderID].s_rearRight_h;
            if (a != null && b != null && c != null && d != null && e != null && f != null)
            {
                return true;
            }

            return false;
        }
    }
}