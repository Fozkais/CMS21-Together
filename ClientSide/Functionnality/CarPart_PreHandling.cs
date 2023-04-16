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
                    AddPartHandle_bis(i);
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

        public static void AddPartHandle(int i)
        {
            List<PartScriptInfo> _parts = new List<PartScriptInfo>();
            for (int j = 0; j < MainMod.carLoaders[ CarSpawn_Handling.carHandler[i].carLoaderID].Parts.Count; j++)
            {
                var partObject = MainMod.carLoaders[ CarSpawn_Handling.carHandler[i].carLoaderID].Parts._items[j].p_handle;
                var partsInObject = partObject.GetComponentsInChildren<PartScript>().ToList();
                for (int k = 0; k < partsInObject.Count; k++)
                {
                   // PartScriptInfo part = new PartScriptInfo(partType.part, partsInObject[k], j);
                    //_parts.Add(part);
                }
            }

            for (int j = 0; j < _parts.Count; j++)
            {
                if (!MPGameManager.PartsHandle.ContainsKey( CarSpawn_Handling.carHandler[i].carLoaderID))
                {
                   // MPGameManager.PartsHandle.Add( CarSpawn_Handling.carHandler[i].carLoaderID, new List<PartScriptInfo>() { _parts[j] });
                }
                else
                {
                    //MPGameManager.PartsHandle[CarSpawn_Handling.carHandler[i].carLoaderID].Add(_parts[j]);
                }
            }
        }
        
        public static void AddEnginePartHandle(int i)
        {
            List<PartScriptInfo> _parts = new List<PartScriptInfo>();
            var enginePartObject = MainMod.carLoaders[ CarSpawn_Handling.carHandler[i].carLoaderID].e_engine_h;
            List<PartScript> engineParts = enginePartObject.GetComponentsInChildren<PartScript>().ToList();
            for (int j = 0; j < engineParts.Count; j++)
            {
               // PartScriptInfoHandler part = new PartScriptInfoHandler(partType.engine, engineParts[j], j);
                //_parts.Add(part);
            }
            for (int j = 0; j < _parts.Count; j++)
            {
                if (!MPGameManager.EnginePartsHandle.ContainsKey( CarSpawn_Handling.carHandler[i].carLoaderID))
                {
                   // MPGameManager.EnginePartsHandle.Add( CarSpawn_Handling.carHandler[i].carLoaderID, new Dictionary<int,PartScriptInfo>());
                   // MPGameManager.EnginePartsHandle[CarSpawn_Handling.carHandler[i].carLoaderID].Add(j, _parts[j]);
                }
                else
                {
                    //MPGameManager.EnginePartsHandle[CarSpawn_Handling.carHandler[i].carLoaderID].Add(j, _parts[j]);
                }
            }
        }

        public static void AddSuspensionPartHandle(int i)
        { 
            List<PartScriptInfo> _parts = new List<PartScriptInfo>();
            List<GameObject> suspensionPartObjects = new List<GameObject>();
            suspensionPartObjects.Add(MainMod.carLoaders[ CarSpawn_Handling.carHandler[i].carLoaderID].s_frontCenter_h);
            suspensionPartObjects.Add(MainMod.carLoaders[ CarSpawn_Handling.carHandler[i].carLoaderID].s_frontLeft_h);
            suspensionPartObjects.Add(MainMod.carLoaders[ CarSpawn_Handling.carHandler[i].carLoaderID].s_frontRight_h);
            suspensionPartObjects.Add(MainMod.carLoaders[ CarSpawn_Handling.carHandler[i].carLoaderID].s_rearCenter_h);
            suspensionPartObjects.Add(MainMod.carLoaders[ CarSpawn_Handling.carHandler[i].carLoaderID].s_rearLeft_h);
            suspensionPartObjects.Add(MainMod.carLoaders[ CarSpawn_Handling.carHandler[i].carLoaderID].s_rearRight_h);
            
            for (var j = 0; j < suspensionPartObjects.Count; j++)
            {
                var partInObject = suspensionPartObjects[j].GetComponentsInChildren<PartScript>().ToList();
                for (int k = 0; k < partInObject.Count; k++)
                {
                    //PartScriptInfo part = new PartScriptInfo(partType.suspensions, partInObject[k], k, j);
                    //_parts.Add(part);
                }
            }

            for (int j = 0; j < _parts.Count; j++)
            {
                if (!MPGameManager.SuspensionPartsHandle.ContainsKey( CarSpawn_Handling.carHandler[i].carLoaderID))
                {
                    MPGameManager.SuspensionPartsHandle.Add( CarSpawn_Handling.carHandler[i].carLoaderID, new List<PartScriptInfo>() { _parts[j] });
                }
                else
                {
                    MPGameManager.SuspensionPartsHandle[CarSpawn_Handling.carHandler[i].carLoaderID].Add(_parts[j]);
                }
            }
        }


        public static void AddPartHandle_bis(int carHandlerID)
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

                CarSpawn_Handling.carHandler[carHandlerID].isReady = true;
            }
        }
    }
}