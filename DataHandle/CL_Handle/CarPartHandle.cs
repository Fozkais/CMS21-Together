using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using CMS21MP.ClientSide;
using CMS21MP.ClientSide.Functionnality;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace CMS21MP.DataHandle.CL_Handle
{
    public static class CarPartHandle
    {

        public static bool isRunning = false;
        public static List<PartScriptInfo> partTempList = new List<PartScriptInfo>();

        public static async void StartUpdating(PartScriptInfo _part)
        {
            try
            {
                if (CarSpawn.CarHandle.ContainsKey(_part._carLoaderID))
                {
                    if (!CarSpawn.CarHandle[_part._carLoaderID].FinishedPreHandlingEngine ||
                        !CarSpawn.CarHandle[_part._carLoaderID].FinishedPreHandlingPart ||
                        !CarSpawn.CarHandle[_part._carLoaderID].FinishedPreHandlingSuspension)
                        {
                            await PreCarPart.AddPartHandle(_part._carLoaderID);
                            await PreCarPart.AddEnginePartHandle(_part._carLoaderID);
                            await PreCarPart.AddSuspensionPartHandle(_part._carLoaderID);
                        }

                    if (_part._type == partType.part)
                    {
                        if (!MPGameManager.PartsHandle.ContainsKey(_part._carLoaderID))
                        {
                            MPGameManager.PartsHandle.Add(_part._carLoaderID,
                                new Dictionary<int, List<PartScriptInfo>>());
                        }

                        if (!MPGameManager.PartsHandle[_part._carLoaderID].ContainsKey(_part._partItemID))
                        {
                            MPGameManager.PartsHandle[_part._carLoaderID]
                                .Add(_part._partItemID, new List<PartScriptInfo>());
                        }

                        MPGameManager.PartsHandle[_part._carLoaderID][_part._partItemID].Add(_part);
                    }
                    else if (_part._type == partType.engine)
                    {
                        if (!MPGameManager.EnginePartsHandle.ContainsKey(_part._carLoaderID))
                        {
                            MPGameManager.EnginePartsHandle.Add(_part._carLoaderID,
                                new Dictionary<int, PartScriptInfo>());
                        }

                        if (!MPGameManager.EnginePartsHandle[_part._carLoaderID].ContainsKey(_part._partCountID))
                        {
                            MPGameManager.EnginePartsHandle[_part._carLoaderID].Add(_part._partCountID, _part);
                        }
                    }
                    else if (_part._type == partType.suspensions)
                    {
                        if (!MPGameManager.SuspensionPartsHandle.ContainsKey(_part._carLoaderID))
                        {
                            MPGameManager.SuspensionPartsHandle.Add(_part._carLoaderID,
                                new Dictionary<int, List<PartScriptInfo>>());
                        }

                        if (!MPGameManager.SuspensionPartsHandle[_part._carLoaderID].ContainsKey(_part._partItemID))
                        {
                            MPGameManager.SuspensionPartsHandle[_part._carLoaderID]
                                .Add(_part._partItemID, new List<PartScriptInfo>());
                        }

                        MPGameManager.SuspensionPartsHandle[_part._carLoaderID][_part._partItemID].Add(_part);
                    }
                    await UpdateDictionnary(_part);
                }
                else
                {
                    while (!CarSpawn.CarHandle.ContainsKey(_part._carLoaderID))
                        await Task.Delay(100);
                    StartUpdating(_part);
                }
            }
            catch (Exception e)
            {
                MelonLogger.Msg(e);
            }
        }

        public async static Task UpdateDictionnary(PartScriptInfo _part)
        {
            try
            {
                if (!String.IsNullOrEmpty(MainMod.carLoaders[_part._carLoaderID].carToLoad))
                {
                    await Task.Delay(1500);
                    if (_part._type == partType.part)
                    {
                        if (MPGameManager.PartsHandle.ContainsKey(_part._carLoaderID) &&
                            MPGameManager.PartsHandle[_part._carLoaderID].ContainsKey(_part._partItemID) &&
                            _part._partCountID < MPGameManager.PartsHandle[_part._carLoaderID][_part._partItemID].Count)
                        {
                            MPGameManager.OriginalParts[_part._carLoaderID][_part._partItemID][_part._partCountID]._UniqueID = _part._UniqueID;
                            MPGameManager.PartsHandle[_part._carLoaderID][_part._partItemID][_part._partCountID] = _part;
                            UpdatePart(MPGameManager.OriginalParts[_part._carLoaderID][_part._partItemID][_part._partCountID], _part);
                        }
                        else
                        {
                            MelonLogger.Msg("Error while updating part Dict");
                        }
                    }
                    else if (_part._type == partType.engine)
                    {
                        if (MPGameManager.EnginePartsHandle.ContainsKey(_part._carLoaderID) &&
                            MPGameManager.EnginePartsHandle[_part._carLoaderID].ContainsKey(_part._partCountID))
                        {
                            MPGameManager.OriginalEngineParts[_part._carLoaderID][_part._partCountID]._UniqueID = _part._UniqueID;
                            MPGameManager.EnginePartsHandle[_part._carLoaderID][_part._partCountID] = _part;
                            UpdatePart(MPGameManager.OriginalEngineParts[_part._carLoaderID][_part._partCountID], _part);
                        }
                        else
                        {
                            MelonLogger.Msg("Error while updating engine Dict");
                        }
                    }
                    else if (_part._type == partType.suspensions)
                    {
                        if (MPGameManager.SuspensionPartsHandle.ContainsKey(_part._carLoaderID) &&
                            MPGameManager.SuspensionPartsHandle[_part._carLoaderID].ContainsKey(_part._partItemID) &&
                            _part._partCountID < MPGameManager.SuspensionPartsHandle[_part._carLoaderID][_part._partItemID].Count)
                            {
                                MPGameManager.OriginalSuspensionParts[_part._carLoaderID][_part._partItemID][_part._partCountID]._UniqueID = _part._UniqueID;
                                MPGameManager.SuspensionPartsHandle[_part._carLoaderID][_part._partItemID][_part._partCountID] = _part;
                                UpdatePart(MPGameManager.OriginalSuspensionParts[_part._carLoaderID][_part._partItemID][_part._partCountID], _part);
                            }
                    }
                }
            }
            catch (Exception e)
            {
                partTempList.Add(_part);
                throw;
            }
        }

        public static void UpdatePart(ModPartScript_Info Originalpart, PartScriptInfo Newpart)
        {
            try
            {

                PartScript originalPart = Originalpart._partScript;
                PartScriptData newPart = Newpart._partScriptData;

                if (MainMod.carLoaders[Newpart._carLoaderID] != null)
                {
                    if (!String.IsNullOrEmpty(newPart.tunedID))
                    {
                        originalPart.tunedID = newPart.tunedID;
                        // if (originalPart.tunedID != newPart.tunedID)
                        // MainMod.carLoaders[Newpart._carLoaderID].TunePart(originalPart.tunedID, newPart.tunedID); 
                    }
                }
                originalPart.IsExamined = newPart.isExamined;
                if (newPart.unmounted == false)
                {
                    originalPart.IsPainted = newPart.isPainted;
                    if (newPart.isPainted)
                    {
                        originalPart.CurrentPaintType = (PaintType)newPart.paintType;
                        originalPart.CurrentPaintData = new ModPaintData().Convert(newPart.paintData);
                        originalPart.SetColor(new ModColor().Convert(newPart.color));
                        if ((PaintType)newPart.paintType == PaintType.Custom)
                            PaintHelper.SetCustomPaintType(originalPart.gameObject, originalPart.CurrentPaintData, false);
                    }

                    originalPart.Quality = newPart.quality;
                    originalPart.SetCondition(newPart.condition);
                    originalPart.UpdateDust(newPart.dust, true);
                    // Handle Bolts

                    if (originalPart.IsUnmounted)
                    {
                        originalPart.ShowBySaveGame();
                        originalPart.ShowMountAnimation();
                        originalPart.FastMount();
                    }
                    
                    //Wheel Handle
                    var wheelData = MainMod.carLoaders[Newpart._carLoaderID].WheelsData;
                    for (int i = 0; i < MainMod.carLoaders[Newpart._carLoaderID].WheelsData.Wheels.Count; i++)
                    {
                        MainMod.carLoaders[Newpart._carLoaderID].SetWheelSize((int)wheelData.Wheels[i].Width, (int)wheelData.Wheels[i].Size, (int)wheelData.Wheels[i].Profile, (WheelType)i);
                        MainMod.carLoaders[Newpart._carLoaderID].SetET((WheelType)i, wheelData.Wheels[i].ET);
                    }
                    MainMod.carLoaders[Newpart._carLoaderID].SetWheelSizes();
                }
                else
                {
                    originalPart.Quality = newPart.quality;
                    originalPart.SetCondition(newPart.condition);
                    originalPart.UpdateDust(newPart.dust);
                    if (originalPart.IsUnmounted == false)
                    {
                        originalPart.HideBySavegame(false, MainMod.carLoaders[Newpart._carLoaderID]);
                    }
                }
            }
            catch (Exception e)
            {
                MelonLogger.Msg("Error while updating pieces : " + e);
                throw;
            }
        }


        public async static void delayUpdatePart()
        {
            if (partTempList.Count > 0)
            {
                isRunning = true;
                await Task.Delay(2000);

                for (var index = 0; index < partTempList.Count; index++)
                {
                    var part = partTempList[index];
                    await UpdateDictionnary(part);
                    partTempList.Remove(part);
                }

                isRunning = false;
            }

        }
        
    }
}