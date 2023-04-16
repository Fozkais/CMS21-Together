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
        
        public static void HandleParts_old()
        {
            // foreach (KeyValuePair<int, List<PartScriptInfoHandler>> parts in MPGameManager.PartsHandle)
            // {
            //     if (!MPGameManager.OriginalParts.ContainsKey(parts.Key))
            //     {
            //         MPGameManager.OriginalParts.Add(parts.Key, new Dictionary<int, PartScriptData>());
            //     }
            //
            //     for (var index = 0; index < parts.Value.Count; index++)
            //     {
            //         //MelonLogger.Msg("Looping through part from handle");
            //         PartScriptData equivalentPart = new PartScriptData().CreateData(parts.Value[index]._partScript, index, parts.Key, partType.part, parts.Value[index]._partObjectID);
            //
            //         if (MPGameManager.OriginalParts[parts.Key].Count <= index)
            //         {
            //             MelonLogger.Msg("Sending part throught If");
            //             ClientSend.carParts(parts.Key, equivalentPart);
            //             MPGameManager.OriginalParts[parts.Key].Add(index, equivalentPart);
            //         }
            //         else if (HasDifferences(equivalentPart, MPGameManager.OriginalParts[parts.Key][index]))
            //         {
            //             MelonLogger.Msg("Sending part throught Else If");
            //             //ClientSend.carParts(parts.Key, equivalentPart);
            //             //MPGameManager.OriginalParts[parts.Key][index] = equivalentPart;
            //         }
            //     }
            // }
        }
        
        // Chargement/Spawn du véhicule en décalé , les autres clients attende de recevoir toute les pieces pour faire spawn le véhicule
        // création d'un "size checker" contenant le nombre de piece a recevoir du server
        // création d'une liste temporaire d'object contenant toutes les pieces avant quelle ne sois appliqué
        
        // Parcours CarloaderID
        // Parcours pieces pour CarloaderID
        // Parcours sous-pieces de piece pour CarLoaderID

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



        public static void HandleEngineParts()
        {
        }

        public static void HandleSuspensionParts()
        {
        }
        
        public static bool HasDifferences(PartScriptData original, PartScriptData handle)
        {
            bool hasDifferences = false;
            //if (original.color != handle.color)
              //  hasDifferences = true;
           // if (original.quality != handle.quality)
            //    hasDifferences = true;
           // if (original.condition != handle.condition)
             //   hasDifferences = true;
           // if (original.dust != handle.dust)
              //  hasDifferences = true;
            if ( original.unmounted != handle.unmounted)
                hasDifferences = true;
                
            return hasDifferences;
        }
    }
}