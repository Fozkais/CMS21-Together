using System.Collections.Generic;
using CMS21Together.Shared.Data;

namespace CMS21Together.ServerSide.Data
{
    public static class CarPartHandle
    {
         public static void HandlePart(ModPartScript carPart, ModCar car)
         {
             if (car.partInfo == null) car.partInfo = new ModPartInfo();
             
            var partInfo = car.partInfo;
            switch (carPart.type)
            {
                case ModPartType.other:
                    if (partInfo.OtherParts.ContainsKey(carPart.partID))
                    {
                        if (!partInfo.OtherParts[carPart.partID].Contains(carPart))
                            partInfo.OtherParts[carPart.partID].Add(carPart);
                        else
                            partInfo.OtherParts[carPart.partID][carPart.partIdNumber] = carPart;
                    }
                    else
                    {
                        partInfo.OtherParts.Add(carPart.partID, new List<ModPartScript>());
                        partInfo.OtherParts[carPart.partID].Add(carPart);
                    }
                    break;
                
                case ModPartType.engine:
                    partInfo.EngineParts[carPart.partID] = carPart;
                    break;
                
                case ModPartType.driveshaft:
                    partInfo.DriveshaftParts[carPart.partID] = carPart;
                    break;
                
                case ModPartType.suspension:
                    if (partInfo.SuspensionParts.ContainsKey(carPart.partID))
                    {
                        if (!partInfo.SuspensionParts[carPart.partID].Contains(carPart))
                            partInfo.SuspensionParts[carPart.partID].Add(carPart);
                        else
                            partInfo.SuspensionParts[carPart.partID][carPart.partIdNumber] = carPart;
                    }
                    else
                    {
                        partInfo.SuspensionParts.Add(carPart.partID, new List<ModPartScript>());
                        partInfo.SuspensionParts[carPart.partID].Add(carPart);
                    }
                    break;
            }
        }
    }
}