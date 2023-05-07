using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CMS21MP.DataHandle;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace CMS21MP.ClientSide.Functionnality
{
    public static class CarPart_PreHandling
    {
        public static void AddAllPartToHandleAlt(int carKey)
        {
            if (!ExternalCarPart_Handling.OriginalCarParts.ContainsKey(carKey))
            {
                ExternalCarPart_Handling.PreHandleCarParts(carKey);
            }
            if (!MPGameManager.OriginalParts.ContainsKey(carKey))
            {
                AddPartHandle(carKey);
            }
            if (!MPGameManager.EnginePartsHandle.ContainsKey(carKey))
            {
                AddEnginePartHandle(carKey);
            }
            if (!MPGameManager.SuspensionPartsHandle.ContainsKey(carKey))
            {
                AddSuspensionPartHandle(carKey);
            }
        }
        
        public async static void AddPartHandle(int carHandlerID)
        {
            await Task.Delay(2000);

            int carLoaderID = CarSpawn_Handling.CarHandle[carHandlerID].carLoaderID; // carLoaderID
            for (int i = 0; i < MainMod.carLoaders[carLoaderID].Parts.Count; i++) // toute les pieces de la carLoaderID
            {
                var part_object = MainMod.carLoaders[carLoaderID].Parts._items[i].p_handle; // GameObject de la piece[i]
                var _parts = part_object.GetComponentsInChildren<PartScript>().ToList(); // Toute les sous pieces de la piece[i]
                for (int j = 0; j < _parts.Count; j++)  // Parcours les sous-pieces de la piece[i] avec l'index[j]
                {
                    if (!MPGameManager.OriginalParts.ContainsKey(carHandlerID))
                    {
                        MPGameManager.OriginalParts.Add(carHandlerID, new Dictionary<int, List<PartScript_Info>>());
                    }

                    if (!MPGameManager.OriginalParts[carHandlerID].ContainsKey(i))
                    {
                        MPGameManager.OriginalParts[carHandlerID].Add(i, new List<PartScript_Info>());
                    }

                    MPGameManager.OriginalParts[carHandlerID][i]
                        .Add(new PartScript_Info(partType.part, _parts[j], carLoaderID, i, j)); // Ajoute la sous-piece[j] dans la liste d'index [carLoaderID][i]
                    //MelonLogger.Msg("Added PartScript_Info to OriginalParts");
                }
            }
            MelonLogger.Msg($"Finished Prehandling part for carLoader[{carLoaderID}]");
        }
        

        public async static void AddEnginePartHandle(int carHandlerID)
        {
            await Task.Delay(2000);
            
                int carLoaderID = CarSpawn_Handling.CarHandle[carHandlerID].carLoaderID; // carLoaderID
                var enginePartObject = MainMod.carLoaders[carLoaderID].e_engine_h; // Récupère le gameobject du engine
                List<PartScript> engineParts = enginePartObject.GetComponentsInChildren<PartScript>().ToList();

                if (!MPGameManager.OriginalEngineParts.ContainsKey(carHandlerID))
                {
                    MPGameManager.OriginalEngineParts.Add(carHandlerID, new Dictionary<int, PartScript_Info>());
                }

                for (int i = 0; i < engineParts.Count; i++) // toutes les pièces du moteur de carLoaderID
                {
                    if (!MPGameManager.OriginalEngineParts[carHandlerID].ContainsKey(i))
                    {
                        MPGameManager.OriginalEngineParts[carHandlerID]
                            .Add(i, new PartScript_Info(partType.engine, engineParts[i], carLoaderID, 0, i)); // Ajoute la sous-piece[j] dans la liste d'index [carLoaderID][i]
                        //MelonLogger.Msg("Added PartScript_Info to OriginalEngineParts");
                    }
                }
                MelonLogger.Msg($"Finished Prehandling engine for carLoader[{carLoaderID}]");
        }
        

        public async static void AddSuspensionPartHandle(int carHandlerID)
        {
            await Task.Delay(2000);

            int carLoaderID = CarSpawn_Handling.CarHandle[carHandlerID].carLoaderID; // carLoaderID
            List<GameObject> suspensionPartObjects = new List<GameObject>();
            suspensionPartObjects.Add(MainMod.carLoaders[carLoaderID].s_frontCenter_h);
            suspensionPartObjects.Add(MainMod.carLoaders[carLoaderID].s_frontLeft_h);
            suspensionPartObjects.Add(MainMod.carLoaders[carLoaderID].s_frontRight_h);
            suspensionPartObjects.Add(MainMod.carLoaders[carLoaderID].s_rearCenter_h);
            suspensionPartObjects.Add(MainMod.carLoaders[carLoaderID].s_rearLeft_h);
            suspensionPartObjects.Add(MainMod.carLoaders[carLoaderID].s_rearRight_h);
    
            for (int i = 0; i < suspensionPartObjects.Count; i++) // tout les gameobject "suspension" pour carLoaderID
            {
                if (suspensionPartObjects[i] == null) continue; // Vérifie si l'élément de la liste est null
    
                var partInObject = suspensionPartObjects[i].GetComponentsInChildren<PartScript>().ToList(); // Toutes les pieces du gameobject[i]
                for (int j = 0; j < partInObject.Count; j++)  // Parcours les sous-pieces de la piece[i] avec l'index[j]
                {
                    if (!MPGameManager.OriginalSuspensionParts.ContainsKey(carHandlerID))
                    {
                        MPGameManager.OriginalSuspensionParts.Add(carHandlerID, new Dictionary<int, List<PartScript_Info>>());
                    }
                    if (!MPGameManager.OriginalSuspensionParts[carHandlerID].ContainsKey(i))
                    {
                        MPGameManager.OriginalSuspensionParts[carHandlerID].Add(i, new List<PartScript_Info>());
                    }
                    if (!MPGameManager.OriginalSuspensionParts[carHandlerID][i].Any(part => part._partScript == partInObject[j]))
                    {
                        MPGameManager.OriginalSuspensionParts[carHandlerID][i].Add(new PartScript_Info(partType.suspensions, partInObject[j], carLoaderID, i, j)); // Ajoute la piece[j] dans la liste d'index [carLoaderID][i]
                    }
                }
            }
    
            MelonLogger.Msg($"Finished Prehandling suspension for carLoader[{carLoaderID}]");
        }
    }
}