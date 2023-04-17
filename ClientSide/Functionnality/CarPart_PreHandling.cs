using System.Collections.Generic;
using System.Linq;
using CMS21MP.DataHandle;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace CMS21MP.ClientSide.Functionnality
{
    public static class CarPart_PreHandling
    {
        public static void AddAllPartToHandle()
        {
            for (var i = 0; i < CarSpawn_Handling.carHandler.Count; i++)
            {
                if (!MPGameManager.OriginalParts.ContainsKey( CarSpawn_Handling.carHandler[i].carLoaderID))
                {
                    AddPartHandle(i);
                }
                if (!MPGameManager.EnginePartsHandle.ContainsKey( CarSpawn_Handling.carHandler[i].carLoaderID))
                {
                    AddEnginePartHandle(i);
                }
                if (!MPGameManager.SuspensionPartsHandle.ContainsKey( CarSpawn_Handling.carHandler[i].carLoaderID))
                {
                    AddSuspensionPartHandle(i);
                }
            }
        }
        
        public static void AddPartHandle(int carHandlerID)
        {
            int carLoaderID = CarSpawn_Handling.carHandler[carHandlerID].carLoaderID; // carLoaderID
            for (int i = 0; i <  MainMod.carLoaders[carLoaderID].Parts.Count; i++) // toute les pieces de la carLoaderID
            {
                var part_object = MainMod.carLoaders[carLoaderID].Parts._items[i].p_handle; // GameObject de la piece[i]
                var _parts = part_object.GetComponentsInChildren<PartScript>().ToList(); // Toute les sous pieces de la piece[i]
                for (int j = 0; j < _parts.Count; j++)  // Parcours les sous-pieces de la piece[i] avec l'index[j]
                {
                    if (!MPGameManager.OriginalParts.ContainsKey(carLoaderID))
                    {
                        MPGameManager.OriginalParts.Add(carLoaderID, new Dictionary<int, List<PartScript_Info>>());
                    }

                    if (!MPGameManager.OriginalParts[carLoaderID].ContainsKey(i))
                    {
                        MPGameManager.OriginalParts[carLoaderID].Add(i, new List<PartScript_Info>());
                    }
                    
                    MPGameManager.OriginalParts[carLoaderID][i]
                        .Add(new PartScript_Info(partType.part, _parts[j], carLoaderID, i, j)); // Ajoute la sous-piece[j] dans la liste d'index [carLoaderID][i]
                    MelonLogger.Msg("Added PartScript_Info to OriginalParts");
                }

                //CarSpawn_Handling.carHandler[carHandlerID].isReady = true;
            }
        }
        

        public static void AddEnginePartHandle(int carHandlerID)
        {
            int carLoaderID = CarSpawn_Handling.carHandler[carHandlerID].carLoaderID; // carLoaderID
            var enginePartObject = MainMod.carLoaders[carLoaderID].e_engine_h; // Récupère le gameobject du engine
            List<PartScript> engineParts = enginePartObject.GetComponentsInChildren<PartScript>().ToList();
            
            for (int i = 0; i <  engineParts.Count; i++) // toute les pieces du moteur de carLoaderID
            {
                if (!MPGameManager.OriginalEngineParts.ContainsKey(carLoaderID))
                {
                    MPGameManager.OriginalEngineParts.Add(carLoaderID, new Dictionary<int, PartScript_Info>());
                }

                if (!MPGameManager.OriginalEngineParts[carLoaderID].ContainsKey(i))
                {
                    MPGameManager.OriginalEngineParts[carLoaderID]
                        .Add(i, new PartScript_Info(partType.engine, engineParts[i], carLoaderID, 0, i)); // Ajoute la sous-piece[j] dans la liste d'index [carLoaderID][i]
                }
                MelonLogger.Msg("Added PartScript_Info to OriginalEngineParts");

               // CarSpawn_Handling.carHandler[carHandlerID].isReady = true;
            }
        }
        

        public static void AddSuspensionPartHandle(int carHandlerID)
        {
            int carLoaderID = CarSpawn_Handling.carHandler[carHandlerID].carLoaderID; // carLoaderID
            
            List<GameObject> suspensionPartObjects = new List<GameObject>();
            suspensionPartObjects.Add(MainMod.carLoaders[carLoaderID].s_frontCenter_h);
            suspensionPartObjects.Add(MainMod.carLoaders[carLoaderID].s_frontLeft_h);
            suspensionPartObjects.Add(MainMod.carLoaders[carLoaderID].s_frontRight_h);
            suspensionPartObjects.Add(MainMod.carLoaders[carLoaderID].s_rearCenter_h);
            suspensionPartObjects.Add(MainMod.carLoaders[carLoaderID].s_rearLeft_h);
            suspensionPartObjects.Add(MainMod.carLoaders[carLoaderID].s_rearRight_h);
            
            for (int i = 0; i <  suspensionPartObjects.Count; i++) // tout les gameobject "suspension" pour carLoaderID
            {
                var partInObject = suspensionPartObjects[i].GetComponentsInChildren<PartScript>().ToList(); // Toute les piece du gameobject[i]
                for (int j = 0; j < partInObject.Count; j++)  // Parcours les sous-pieces de la piece[i] avec l'index[j]
                {
                    if (!MPGameManager.OriginalSuspensionParts.ContainsKey(carLoaderID))
                    {
                        MPGameManager.OriginalSuspensionParts.Add(carLoaderID, new Dictionary<int, List<PartScript_Info>>());
                    }
                    if (!MPGameManager.OriginalSuspensionParts[carLoaderID].ContainsKey(i))
                    {
                        MPGameManager.OriginalSuspensionParts[carLoaderID].Add(i, new List<PartScript_Info>());
                    }
                    
                    MPGameManager.OriginalSuspensionParts[carLoaderID][i]
                        .Add(new PartScript_Info(partType.suspensions, partInObject[j], carLoaderID, i, j)); // Ajoute la piece[j] dans la liste d'index [carLoaderID][i]
                    MelonLogger.Msg("Added PartScript_Info to OriginalSuspensionsParts");
                }

                //CarSpawn_Handling.carHandler[carHandlerID].isReady = true;
            }
        }
    }
}