using System.Collections.Generic;
using CMS21Together.CustomData;
using CMS21Together.SharedData;

namespace CMS21Together.ServerSide.Data
{
    public class CarPartHandle
    {
        public static void HandlePart(ModPartScript carPart, ModCar car)
        {
            if (car.partInfo != null)
            {
                var partInfo = car.partInfo;
                switch (carPart.type)
                {
                    case partType.other:
                        if (partInfo.OtherParts == null)
                            partInfo.OtherParts = new Dictionary<int, List<ModPartScript>>();
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
                    case partType.engine:
                        if (partInfo.EngineParts == null)
                            partInfo.EngineParts = new Dictionary<int, ModPartScript>();
                        
                        partInfo.EngineParts[carPart.partID] = carPart;
                        break;
                    case partType.suspension:
                        if (partInfo.SuspensionParts == null)
                            partInfo.SuspensionParts = new Dictionary<int, List<ModPartScript>>();
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
}