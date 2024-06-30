using System;
using System.Collections.Generic;
using System.Linq;
using Il2Cpp;
using MelonLoader;
using UnityEngine.Serialization;

namespace CMS21Together.Shared.Data
{
    [Serializable]
    public class ModNewCarData
    {
        public float AdditionalCarRot;
        public List<ModBodyPartData> BodyPartsData;
        public ModBonusPartsData BonusPartsData;
        public ModCarInfoData CarInfoData;
        public string carToLoad;
        public ModColor color;
        public int configVersion;
        public bool customerCar;
        //public ModTuningData ecuData; TODO: Implement TuningData
        public ModEngineData EngineData;
        public string engineSwap;
        public ModColor factoryColor;
        public ModPaintType factoryPaintType;
        public float finalDriveRatio;
        //public ModFluidsData FluidsData TODO: Implement FluidsData
        public List<float> gearRatio;
        public bool HasCustomPaintType;
        //public ModHeadLampAlignmentData HeadLampLeftAlignmentData; TODO: Implement HeadLampAlignmentData
        //public ModHeadLampAlignmentData HeadLampRightAlignmentData;
        public int index;
        //public ModLicencePlatesData LicencePlatesData; TODO: Implement LicencePlatesData
        public bool LightsOn;
        public int measuredDragIndex;
        public int orderConnection;
        public ModPaintData PaintData;
        public List<ModPartData> PartData;
        public List<int> rimsSize;
        public List<int> tiresET;
        public List<int> tiresSize;
        //public ModToolsData ToolsData;  TODO: Implement ToolsData
        public string UId;
        public ModWheelsAlignment wheelsAlignment;
        public List<int> wheelsWidth;

        public ModNewCarData(NewCarData newCarData)
{
    if (newCarData != null)
    {
        AdditionalCarRot = newCarData.AdditionalCarRot;

        if (newCarData.BodyPartsData != null)
        {
            BodyPartsData = new List<ModBodyPartData>();
            foreach (var bodyPartData in newCarData.BodyPartsData._items)
            {
                if (bodyPartData != null)
                {
                    BodyPartsData.Add(new ModBodyPartData(bodyPartData));
                }
            }
        }
        
        if (newCarData.BonusPartsData != null)
        {
            BonusPartsData = new ModBonusPartsData(newCarData.BonusPartsData);
        }


        CarInfoData = new ModCarInfoData(newCarData.CarInfoData);
        

        carToLoad = newCarData.carToLoad;
        configVersion = newCarData.configVersion;
        customerCar = newCarData.customerCar;

        if (newCarData.color != null && newCarData.color.Length >= 4)
        {
            color = new ModColor(newCarData.color[0], newCarData.color[1], newCarData.color[2], newCarData.color[3]);
        }


        EngineData = new ModEngineData(newCarData.EngineData);
        

        engineSwap = newCarData.engineSwap;

        if (newCarData.factoryColor != null && newCarData.factoryColor.Length >= 4)
        {
            factoryColor = new ModColor(newCarData.factoryColor[0], newCarData.factoryColor[1], newCarData.factoryColor[2], newCarData.factoryColor[3]);
        }

        factoryPaintType = (ModPaintType)newCarData.factoryPaintType;
        finalDriveRatio = newCarData.finalDriveRatio;
        gearRatio = newCarData.gearRatio?.ToList();
        HasCustomPaintType = newCarData.HasCustomPaintType;
        index = newCarData.index;
        LightsOn = newCarData.LightsOn;
        measuredDragIndex = newCarData.measuredDragIndex;
        orderConnection = newCarData.orderConnection;

        if (newCarData.PaintData != null)
        {
            PaintData = new ModPaintData(newCarData.PaintData);
        }

        if (newCarData.PartData != null)
        {
            PartData = new List<ModPartData>();
            foreach (var partData in newCarData.PartData._items)
            {
                if (partData != null)
                {
                    PartData.Add(new ModPartData(partData));
                }
            }
        }

        rimsSize = newCarData.rimsSize?.ToList();
        tiresET = newCarData.tiresET?.ToList();
        tiresSize = newCarData.tiresSize?.ToList();
        UId = newCarData.UId;


        wheelsAlignment = new ModWheelsAlignment(newCarData.WheelsAlignment);

        wheelsWidth = newCarData.wheelsWidth?.ToList();
    }
    else
    {
        MelonLogger.Msg("Error: NewCarData is null in ModNewCarData constructor.");
    }
}


        public NewCarData ToGame()
        {
            NewCarData newData = new NewCarData();

            newData.AdditionalCarRot = AdditionalCarRot;

            if (BodyPartsData != null)
            {
                newData.BodyPartsData = new Il2CppSystem.Collections.Generic.List<BodyPartData>();
                foreach (var bodyPartData in BodyPartsData)
                {
                    if (bodyPartData != null)
                    {
                        newData.BodyPartsData.Add(bodyPartData.ToGame());
                    }
                }
            }


            newData.CarInfoData = CarInfoData.ToGame(CarInfoData);
            
            newData.carToLoad = carToLoad;
            newData.configVersion = configVersion;
            newData.customerCar = customerCar;

            newData.color = color != null ? new float[] { color.r, color.g, color.b, color.a } : null;


            newData.EngineData = EngineData.ToGame();
            
            newData.engineSwap = engineSwap;

            newData.factoryColor = factoryColor != null ? new float[] { factoryColor.r, factoryColor.g, factoryColor.b, factoryColor.a } : null;
            newData.factoryPaintType = (PaintType)factoryPaintType;
            newData.finalDriveRatio = finalDriveRatio;
            newData.gearRatio = gearRatio != null ? gearRatio.ToArray() : null;
            newData.HasCustomPaintType = HasCustomPaintType;
            newData.index = index;
            newData.LightsOn = LightsOn;
            newData.measuredDragIndex = measuredDragIndex;
            newData.orderConnection = orderConnection;

            if (PaintData != null)
            {
                newData.PaintData = PaintData.ToGame(PaintData);
            }
            
            if (PartData != null)
            {
                newData.PartData = new Il2CppSystem.Collections.Generic.List<PartData>();
                foreach (var modPartData in PartData)
                {
                    if (modPartData != null)
                    {
                        newData.PartData.Add(modPartData.ToGame());
                    }
                }
            }

            newData.rimsSize = rimsSize != null ? rimsSize.ToArray() : null;
            newData.tiresET = tiresET != null ? tiresET.ToArray() : null;
            newData.tiresSize = tiresSize != null ? tiresSize.ToArray() : null;
            newData.UId = UId;


            newData.WheelsAlignment = wheelsAlignment.ToGame(wheelsAlignment);

            newData.wheelsWidth = wheelsWidth != null ? wheelsWidth.ToArray() : null;

            return newData;
        }

    }
}