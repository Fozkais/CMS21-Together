using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CMS21MP.DataHandle;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace CMS21MP.ClientSide.Functionnality
{
    public static class CarPart
    {
        public static async Task HandleAllParts()
        {
            await BodyPart.HandleBodyParts();
            await HandleParts();
            await HandleEngineParts();
            await HandleSuspensionParts();

        }


        public async static Task HandleParts()
        {
            var OriginalPartsCopy = MPGameManager.OriginalParts.ToList();
            foreach (var car in OriginalPartsCopy)
            {
                if (CarSpawn.CarHandle[car.Key].FinishedPreHandlingPart)
                {
                    if (!CarSpawn.CarHandle[car.Key].PartFromServer)
                    {
                        var part = MPGameManager.OriginalParts[car.Key];
                        foreach (var _part in part)
                        {
                            if (!MPGameManager.PartsHandle.ContainsKey(car.Key))
                                MPGameManager.PartsHandle.Add(car.Key, new Dictionary<int, List<PartScriptInfo>>());

                            if (!MPGameManager.PartsHandle[car.Key].ContainsKey(_part.Key))
                                MPGameManager.PartsHandle[car.Key].Add(_part.Key, new List<PartScriptInfo>());

                            for (int k = 0; k < part[_part.Key].Count; k++)
                            {
                                if (k >= 0 && k < part[_part.Key].Count)
                                {
                                    if (!MPGameManager.PartsHandle[car.Key][_part.Key].Exists(s => s._UniqueID == part[_part.Key][k]._UniqueID))
                                    {
                                        MPGameManager.PartsHandle[car.Key][_part.Key].Add(part[_part.Key][k]._PartScriptInfo);
                                        ClientSend.carParts(part[_part.Key][k]._PartScriptInfo);
                                    }

                                    if (MPGameManager.PartsHandle.ContainsKey(car.Key) &&
                                        MPGameManager.PartsHandle[car.Key].ContainsKey(_part.Key) &&
                                        k < MPGameManager.PartsHandle[car.Key][_part.Key].Count)
                                    {
                                        if (MPGameManager.PartsHandle[car.Key][_part.Key].Exists(s => s._UniqueID == part[_part.Key][k]._UniqueID))
                                        {
                                            int index = MPGameManager.PartsHandle[car.Key][_part.Key].FindIndex(s => s._UniqueID == part[_part.Key][k]._UniqueID);
                                            PartScriptInfo partInfo = MPGameManager.PartsHandle[car.Key][_part.Key][index];
                                            PartScriptData partToCompare = new PartScriptData(part[_part.Key][k]._partScript);
                                            if (HasDifferences(partToCompare, partInfo._partScriptData))
                                            {
                                                partInfo._partScriptData = partToCompare;
                                                ClientSend.carParts(partInfo);
                                                MelonLogger.Msg("Differences Found ! sending new pieces");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        await Task.Delay(4000);
                        CarSpawn.CarHandle[car.Key].PartFromServer = false;
                    }
                }
            }
        }


        public async static Task HandleEngineParts()
        {
            var OriginalEnginePartsCopy = MPGameManager.OriginalEngineParts.ToList();
            foreach (var car in OriginalEnginePartsCopy)
            {
                if (CarSpawn.CarHandle[car.Key].FinishedPreHandlingEngine)
                {
                    if (!CarSpawn.CarHandle[car.Key].EngineFromServer)
                    {
                        var part = MPGameManager.OriginalEngineParts[car.Key];
                        foreach (var _part in part)
                        {
                            if (!MPGameManager.EnginePartsHandle.ContainsKey(car.Key))
                                MPGameManager.EnginePartsHandle.Add(car.Key, new Dictionary<int, PartScriptInfo>());

                            if (MPGameManager.EnginePartsHandle.ContainsKey(car.Key))
                            {
                                if (!MPGameManager.EnginePartsHandle[car.Key].Any(s => s.Value._UniqueID == part[_part.Key]._UniqueID))
                                {
                                    if (!MPGameManager.EnginePartsHandle[car.Key].ContainsKey(_part.Key))
                                    {
                                        MPGameManager.EnginePartsHandle[car.Key].Add(_part.Key, part[_part.Key]._PartScriptInfo);
                                        ClientSend.carParts(part[_part.Key]._PartScriptInfo);
                                    }
                                }

                                if (MPGameManager.EnginePartsHandle[car.Key].Any(s => s.Value._UniqueID == part[_part.Key]._UniqueID))
                                {
                                    PartScriptInfo partInfo = MPGameManager.EnginePartsHandle[car.Key][_part.Key];
                                    PartScriptData partToCompare = new PartScriptData(part[_part.Key]._partScript);
                                    if (HasDifferences(partToCompare, partInfo._partScriptData))
                                    {
                                        MelonLogger.Msg("Differences Found ! sending new pieces");
                                        partInfo._partScriptData = partToCompare;
                                        ClientSend.carParts(partInfo);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        await Task.Delay(4000);
                        CarSpawn.CarHandle[car.Key].EngineFromServer = false;
                    }
                }
            }
        }


        public async static Task HandleSuspensionParts()
        {
            var OriginalSuspensionPartsCopy = MPGameManager.OriginalSuspensionParts.ToList();

            foreach (var car in OriginalSuspensionPartsCopy)
            {
                if (CarSpawn.CarHandle[car.Key].FinishedPreHandlingPart)
                {
                    if (!CarSpawn.CarHandle[car.Key].SuspensionFromServer)
                    {
                        var part = MPGameManager.OriginalSuspensionParts[car.Key];

                        if (!MPGameManager.SuspensionPartsHandle.ContainsKey(car.Key))
                            MPGameManager.SuspensionPartsHandle.Add(car.Key,
                                new Dictionary<int, List<PartScriptInfo>>());

                        foreach (var _part in part)
                        {
                            if (!MPGameManager.SuspensionPartsHandle[car.Key].ContainsKey(_part.Key))
                                MPGameManager.SuspensionPartsHandle[car.Key].Add(_part.Key, new List<PartScriptInfo>());


                            for (int k = 0; k < _part.Value.Count; k++)
                            {
                                if (MPGameManager.SuspensionPartsHandle[car.Key][_part.Key]
                                    .Exists(s => s._UniqueID == _part.Value[k]._UniqueID))
                                    continue;
                                
                                if (k >= 0 && k < part[_part.Key].Count)
                                {
                                    if (!MPGameManager.SuspensionPartsHandle[car.Key][_part.Key]
                                            .Exists(s => s._UniqueID == part[_part.Key][k]._UniqueID))
                                    {
                                        MPGameManager.SuspensionPartsHandle[car.Key][_part.Key]
                                            .Add(part[_part.Key][k]._PartScriptInfo);
                                        ClientSend.carParts(part[_part.Key][k]._PartScriptInfo);
                                    }

                                    int index = MPGameManager.SuspensionPartsHandle[car.Key][_part.Key]
                                        .FindIndex(s => s._UniqueID == part[_part.Key][k]._UniqueID);
                                    PartScriptInfo partInfo =
                                        MPGameManager.SuspensionPartsHandle[car.Key][_part.Key][index];
                                    PartScriptData partToCompare = new PartScriptData(part[_part.Key][k]._partScript);

                                    if (HasDifferences(partToCompare, partInfo._partScriptData))
                                    {
                                        partInfo._partScriptData = partToCompare;
                                        ClientSend.carParts(partInfo);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        await Task.Delay(4000);
                        CarSpawn.CarHandle[car.Key].SuspensionFromServer = false;
                    }
                }
            }
        }


        public static bool HasDifferences(PartScriptData original, PartScriptData handle)
        {
            bool hasDifferences = false;
            //  if (original.color != handle.color)
            //  hasDifferences = true;
            if (original.unmounted != handle.unmounted)
                hasDifferences = true;

            return hasDifferences;
        }
    }
}