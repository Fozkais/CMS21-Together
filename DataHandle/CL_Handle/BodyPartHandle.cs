using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using CMS21MP.ClientSide;
using CMS21MP.ClientSide.Functionnality;
using Il2Cpp;
using MelonLoader;
using UnityEngine;
using CarPart = Il2Cpp.CarPart;

namespace CMS21MP.DataHandle.CL_Handle
{
    public static class BodyPartHandle
    {

        private static object bodyPartsLock = new object();
        public static async void StartUpdating(carPartsData _bodyPart)
        {
            if (CarSpawn.CarHandle.TryGetValue(_bodyPart.carLoaderID, out var value))
                {
                    if (BodyPart.PreHandleBodyParts(_bodyPart.carLoaderID).Status != TaskStatus.Running)
                    {
                        if (!value.FinishedPreHandlingBodyPart)
                        {
                            await  BodyPart.PreHandleBodyParts(_bodyPart.carLoaderID);
                        }
                        MelonLogger.Msg("Finished Awaiting bodyPart ");
                    }
                     await UpdateDictionnary(_bodyPart);
                }
                else
                {
                    while (!CarSpawn.CarHandle.ContainsKey(_bodyPart.carLoaderID))
                        await Task.Delay(100);
                    StartUpdating(_bodyPart);
                }
            }

        public async static Task UpdateDictionnary(carPartsData _bodyPart)
        {
            await Task.Delay(1000);
            lock (bodyPartsLock) // Utilisation du verrou pour obtenir un accès exclusif
            {
                if (!BodyPart.BodyPartsHandle.ContainsKey(_bodyPart.carLoaderID))
                {
                    BodyPart.BodyPartsHandle.Add(_bodyPart.carLoaderID, new Dictionary<int, carPartsData>());
                }

                if (!BodyPart.BodyPartsHandle[_bodyPart.carLoaderID].ContainsKey(_bodyPart.carPartID))
                {
                    BodyPart.BodyPartsHandle[_bodyPart.carLoaderID].Add(_bodyPart.carPartID, _bodyPart);
                }

                if (BodyPart.OriginalBodyParts.ContainsKey(_bodyPart.carLoaderID) &&
                    BodyPart.OriginalBodyParts[_bodyPart.carLoaderID].ContainsKey(_bodyPart.carPartID))
                {
                    BodyPart.OriginalBodyParts[_bodyPart.carLoaderID][_bodyPart.carPartID]._UniqueID =
                        _bodyPart.UniqueID;
                    BodyPart.BodyPartsHandle[_bodyPart.carLoaderID][_bodyPart.carPartID] = _bodyPart;
                    UpdateBodyPart(_bodyPart);
                }
            }
        }
        
        public static void UpdateBodyPart(carPartsData _bodyPart)
        {
            if (MainMod.carLoaders != null && MainMod.carLoaders[_bodyPart.carLoaderID] != null)
                {
                    
                    if (!String.IsNullOrEmpty(MainMod.carLoaders[_bodyPart.carLoaderID].carToLoad))
                    {
                        Color color = new ModColor().Convert(_bodyPart.colors);
                        Color tintColor = new ModColor().Convert(_bodyPart.TintColor);

                        CarPart _part = BodyPart.OriginalBodyParts[_bodyPart.carLoaderID][_bodyPart.carPartID]._originalPart;

                        if(_part.TunedID != _bodyPart.tunedID)
                            MainMod.carLoaders[_bodyPart.carLoaderID].TunePart(_part.name, _bodyPart.tunedID);
                        
                        MainMod.carLoaders[_bodyPart.carLoaderID].SetDent(_part, _bodyPart.dent);
                        MainMod.carLoaders[_bodyPart.carLoaderID].EnableDust(_part, _bodyPart.Dust);
                        MainMod.carLoaders[_bodyPart.carLoaderID].SetCondition(_part, _bodyPart.condition);
                        
                        _part.IsTinted = _bodyPart.isTinted;
                        _part.TintColor = tintColor;
                        _part.Color = color;
                        _part.PaintType = (PaintType)_bodyPart.paintType;
                        _part.OutsideRustEnabled = _bodyPart.outsaidRustEnabled;
                        _part.AdditionalString = _bodyPart.additionalString;
                        _part.Quality = _bodyPart.quality;
                        _part.WashFactor = _bodyPart.washFactor;
                        
                        
                        if (!_part.Unmounted && !_part.name.StartsWith("license_plate"))
                        {
                            //MainMod.carLoaders[_carLoaderID].SetCustomCarPaintType(_part, new ModPaintData().Convert(_bodyPart.paintData));  
                           // MainMod.carLoaders[_carLoaderID].SetCarColorAndPaintType(_part, color, (PaintType)_bodyPart.paintType);
                        }
                        MainMod.carLoaders[_bodyPart.carLoaderID].SetCarLivery(_part, _bodyPart.livery, _bodyPart.liveryStrength);
                        
                        if(!_part.Unmounted && _bodyPart.unmounted)
                            MainMod.carLoaders[_bodyPart.carLoaderID].TakeOffCarPartFromSave(_bodyPart.name);
                        if (_part.Unmounted && !_bodyPart.unmounted)
                        {
                            MainMod.carLoaders[_bodyPart.carLoaderID].TakeOnCarPartFromSave(_bodyPart.name);
                        }
                                          
                                     
                        
                        if (_part.Switched != _bodyPart.switched)
                            MainMod.carLoaders[_bodyPart.carLoaderID].SwitchCarPart(_part, false, _bodyPart.switched);


                        if(_bodyPart.isTinted)
                            PaintHelper.SetWindowProperties(_part.handle, (int)(_bodyPart.TintColor.a * 255), _part.TintColor);
                        
                        //MelonLogger.Msg($"Parts[{_bodyPart.carPartID}] updated!, {_bodyPart.name}, -> {MainMod.carLoaders[_carLoaderID].carParts._items[_bodyPart.carPartID].name}, {_bodyPart.unmounted}");
                    }
                    else
                    {
                        MelonLogger.Msg($"Loss of data from bodyPart ! {_bodyPart.name} on car with CarLoaderID{_bodyPart.carLoaderID}");
                    }
                }
        }
    }
}