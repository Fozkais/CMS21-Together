using System;
using System.Collections.Generic;
using CMS21MP.DataHandle;
using Il2Cpp;

namespace CMS21MP.ClientSide.Functionnality
{
    public static class ExternalCarPart_Handling
    {
        public static List<C_carPartsData> CarPartsHandler = new List<C_carPartsData>();
        public static List<C_carPartsData> CarPartsToHandle = new List<C_carPartsData>();
        
        public static void HandleCarParts()
        {
            CarPartsToHandle.Clear();
            
            foreach (carData car in  CarSpawn_Handling.carHandler)
            {
                if (!car.fromServer)
                {
                    foreach (CarPart _part in MainMod.carLoaders[car.carLoaderID].carParts)
                    {
                        C_carPartsData partData = new C_carPartsData();
                        partData.carPartID = CarPartsToHandle.Count;
                        partData.carLoaderID = car.carLoaderID;
                        partData.name = _part.name;
                        partData.switched = _part.Switched;
                        partData.inprogress = _part.Switched;
                        partData.condition = _part.Condition;
                        partData.unmounted = _part.Unmounted;
                        partData.tunedID = _part.TunedID;
                        partData.isTinted = _part.IsTinted;
                        partData.TintColor = new C_Color(_part.TintColor);
                        partData.colors = new C_Color(_part.Color);
                        partData.paintType = (int)_part.PaintType;
                        partData.paintData = new C_PaintData().FromGame(_part.PaintData);
                        partData.conditionStructure = _part.StructureCondition;
                        partData.conditionPaint = _part.ConditionPaint;
                        partData.livery = _part.Livery;
                        partData.liveryStrength = _part.LiveryStrength;
                        partData.outsaidRustEnabled = _part.OutsideRustEnabled;
                        partData.dent = _part.Dent;
                        partData.additionalString = _part.AdditionalString;
                        partData.Dust = _part.Dust;
                        partData.washFactor = _part.WashFactor;

                        foreach (String partAttached in _part.ConnectedParts)
                        {
                            partData.mountUnmountWith.Add(partAttached);
                        }
                        partData.quality = _part.Quality;
                        
                        CarPartsToHandle.Add(partData);
                    }
                }
            }


            for (int i = 0; i < CarPartsToHandle.Count; i++)
            {
                if (!carPartIsExistingList(CarPartsToHandle[i], CarPartsHandler))
                {
                    //MelonLogger.Msg($"CL: Sending carPart to server!");
                    CarPartsHandler.Add(CarPartsToHandle[i]);
                    ClientSend.bodyParts(CarPartsToHandle[i].carLoaderID, CarPartsToHandle[i]);
                }
            }
        }
        
        public static bool carPartIsExistingList(C_carPartsData partToHandle, List<C_carPartsData> otherList)
        {
            foreach (C_carPartsData carPartData in otherList)
            {
                if (carPartData.name == partToHandle.name && carPartData.carLoaderID == partToHandle.carLoaderID && carPartData.quality == partToHandle.quality && partToHandle.unmounted == carPartData.unmounted)
                {
                    //MelonLogger.Msg($"Rejected CarData: {partToHandle.name}");
                    return true;
                }
            }
            return false;
        }
    }
}