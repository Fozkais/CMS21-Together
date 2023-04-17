using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CMS21MP.DataHandle;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace CMS21MP.ClientSide.Functionnality
{
    public static class CarPart_Handling
    {
        public static void HandleAllParts()
        {
            HandleParts();
            HandleEngineParts();
            HandleSuspensionParts();
        }
        

        public async static void HandleParts()
        {
            for (int i = 0; i < MPGameManager.OriginalParts.Count; i++) // Nombre de carLoaderID dans OriginalPart
            {
                if (!CarSpawn_Handling.carHandler[i].fromServer)
                {
                    var part = MPGameManager.OriginalParts[i]; // Toutes les pieces[i] ou i = carLoaderID
                        
                    for (int j = 0; j < part.Values.Count; j++) // Parcours des pieces pour i = carLoaderID
                    {
                        for (int k = 0; k < part[j].Count; k++) // Parcours des sous-pieces pour i = carLoaderID, j = pieces, k = sous-pieces
                        {
                            if(!MPGameManager.PartsHandle.ContainsKey(i))
                                MPGameManager.PartsHandle.Add(i, new Dictionary<int, List<PartScriptInfo>>());
                            if(!MPGameManager.PartsHandle[i].ContainsKey(j))
                                MPGameManager.PartsHandle[i].Add(j, new List<PartScriptInfo>());

                            #region AddingToHandle
                            
                            if (!MPGameManager.PartsHandle[i][j].Exists(s => s._UniqueID == part[j][k]._UniqueID)) // Vérifie la préscense de la sous-piece[j] dans le handle
                            {
                                MelonLogger.Msg("New part Found ! sending new pieces");
                                MPGameManager.PartsHandle[i][j].Add(part[j][k]._PartScriptInfo);
                                ClientSend.carParts(part[j][k]._PartScriptInfo);
                            }

                            // MPGameManager.PartsHandle[carID].BinarySearch(part[j]._PartScriptInfo) -> pour trouver l'index d'un élément dans une liste
                            #endregion
                            
                            #region CheckForDifferences

                            if (MPGameManager.PartsHandle[i][j].Exists(s => s._UniqueID == part[j][k]._UniqueID)) // Vérifie la préscense de la sous-piece[j][k] dans le handle
                            {
                                int index = MPGameManager.PartsHandle[i][j].FindIndex(s => s._UniqueID == part[j][k]._UniqueID); 
                                PartScriptInfo partInfo = MPGameManager.PartsHandle[i][j][index]; // sous piece a l'index [i][j][index]
                                PartScriptData partToCompare = new PartScriptData(part[j][k]._partScript);  // sous-piece originalPart[i][j][k]
                                if (HasDifferences(partToCompare, partInfo._partScriptData))
                                {
                                    MelonLogger.Msg("Differences Found ! sending new pieces");
                                    partInfo._partScriptData = partToCompare;
                                    ClientSend.carParts(partInfo);
                                }
                            }

                            #endregion
                        }
                        

                    }
                }
                else
                {
                    await Task.Delay(4000);
                    CarSpawn_Handling.carHandler[i].fromServer = false;
                }
            }
        }



        public async static void HandleEngineParts()
        {
             for (int i = 0; i < MPGameManager.OriginalEngineParts.Count; i++) // Nombre de carLoaderID dans OriginalEngineParts
            {
                if (!CarSpawn_Handling.carHandler[i].fromServer)
                {
                    var part = MPGameManager.OriginalEngineParts[i]; // Toutes les pieces[i] ou i = carLoaderID
                        
                    for (int j = 0; j < part.Count; j++) // Parcours des pieces pour i = carLoaderID
                    {
                        if(!MPGameManager.EnginePartsHandle.ContainsKey(i))
                                MPGameManager.EnginePartsHandle.Add(i, new Dictionary<int, PartScriptInfo>());

                        #region AddingToHandle
                            
                        if (!MPGameManager.EnginePartsHandle[i].Any(s => s.Value._UniqueID == part[j]._UniqueID)) // Vérifie la préscense de la sous-piece[j] dans le handle
                            {
                                MelonLogger.Msg("New part Found ! sending new pieces");
                                MPGameManager.EnginePartsHandle[i].Add(j, part[j]._PartScriptInfo);
                                ClientSend.carParts(part[j]._PartScriptInfo);
                            }
                            
                            #endregion
                            
                            #region CheckForDifferences

                            if (MPGameManager.EnginePartsHandle[i].Any(s => s.Value._UniqueID == part[j]._UniqueID)) // Vérifie la préscense de la sous-piece[j] dans le handle
                            {
                                int index = MPGameManager.EnginePartsHandle[i].FirstOrDefault(s => s.Value._UniqueID == part[j]._UniqueID).Key;
                                PartScriptInfo partInfo = MPGameManager.EnginePartsHandle[i][j]; // sous piece a l'index [i][j][index]
                                PartScriptData partToCompare = new PartScriptData(part[j]._partScript);  // sous-piece originalPart[i][j][k]
                                if (HasDifferences(partToCompare, partInfo._partScriptData))
                                {
                                    MelonLogger.Msg("Differences Found ! sending new pieces");
                                    partInfo._partScriptData = partToCompare;
                                    ClientSend.carParts(partInfo);
                                }
                            }

                            #endregion
                    }
                }
                else
                {
                    await Task.Delay(4000);
                    CarSpawn_Handling.carHandler[i].fromServer = false;
                }
            }
        }

        public async static void HandleSuspensionParts()
        {
            for (int i = 0; i < MPGameManager.OriginalSuspensionParts.Count; i++) // Nombre de carLoaderID dans OriginalPart
            {
                if (!CarSpawn_Handling.carHandler[i].fromServer)
                {
                    var part = MPGameManager.OriginalSuspensionParts[i]; // Toutes les pieces[i] ou i = carLoaderID
                        
                    for (int j = 0; j < part.Values.Count; j++) // Parcours des pieces pour i = carLoaderID
                    {
                        for (int k = 0; k < part[j].Count; k++) // Parcours des sous-pieces pour i = carLoaderID, j = gameobject, k = pieces
                        {
                            if(!MPGameManager.SuspensionPartsHandle.ContainsKey(i))
                                MPGameManager.SuspensionPartsHandle.Add(i, new Dictionary<int, List<PartScriptInfo>>());
                            if(!MPGameManager.SuspensionPartsHandle[i].ContainsKey(j))
                                MPGameManager.SuspensionPartsHandle[i].Add(j, new List<PartScriptInfo>());

                            #region AddingToHandle
                            
                            if (!MPGameManager.SuspensionPartsHandle[i][j].Exists(s => s._UniqueID == part[j][k]._UniqueID)) // Vérifie la préscense de la sous-piece[j] dans le handle
                            {
                                MelonLogger.Msg("New part Found ! sending new pieces");
                                MPGameManager.SuspensionPartsHandle[i][j].Add(part[j][k]._PartScriptInfo);
                                ClientSend.carParts(part[j][k]._PartScriptInfo);
                            }

                            // MPGameManager.PartsHandle[carID].BinarySearch(part[j]._PartScriptInfo) -> pour trouver l'index d'un élément dans une liste
                            #endregion
                            
                            #region CheckForDifferences

                            if (MPGameManager.SuspensionPartsHandle[i][j].Exists(s => s._UniqueID == part[j][k]._UniqueID)) // Vérifie la préscense de la sous-piece[j][k] dans le handle
                            {
                                int index = MPGameManager.SuspensionPartsHandle[i][j].FindIndex(s => s._UniqueID == part[j][k]._UniqueID); 
                                PartScriptInfo partInfo = MPGameManager.SuspensionPartsHandle[i][j][index]; // sous piece a l'index [i][j][index]
                                PartScriptData partToCompare = new PartScriptData(part[j][k]._partScript);  // sous-piece originalPart[i][j][k]
                                if (HasDifferences(partToCompare, partInfo._partScriptData))
                                {
                                    MelonLogger.Msg("Differences Found ! sending new pieces");
                                    partInfo._partScriptData = partToCompare;
                                    ClientSend.carParts(partInfo);
                                }
                            }

                            #endregion
                        }
                        

                    }
                }
                else
                {
                    await Task.Delay(4000);
                    CarSpawn_Handling.carHandler[i].fromServer = false;
                }
            }
        }
        
        public static bool HasDifferences(PartScriptData original, PartScriptData handle)
        {
            bool hasDifferences = false;
            //if (original.color != handle.color)
              //  hasDifferences = true;
           // if (original.quality != handle.quality)
               // hasDifferences = true;
            //if (original.condition != handle.condition)
              //  hasDifferences = true;
            //if (original.dust != handle.dust)
              //  hasDifferences = true;
            if ( original.unmounted != handle.unmounted)
                hasDifferences = true;
                
            return hasDifferences;
        }
    }
}