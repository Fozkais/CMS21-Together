using System;
using System.Collections.Generic;
using System.Linq;
using Il2Cpp;
using UnityEngine.Serialization;

namespace CMS21Together.Shared.Data
{
    [Serializable]
    public class ModNewCarData
    {
        public float AdditionalCarRot;
        public List<ModBodyPartData> BodyPartsData;
        //public List<ModBonusPartData> BonusPartData; TODO: Implement Bonus part
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
        public ModWheelsAlignment wheelsesAlignment;
        public List<int> wheelsWidth;

        public ModNewCarData(NewCarData newCarData)
        {
            AdditionalCarRot = newCarData.AdditionalCarRot;
            BodyPartsData = new List<ModBodyPartData>();
            for (int i = 0; i < newCarData.BodyPartsData.Count; i++)
            {
                BodyPartsData.Add(new ModBodyPartData(newCarData.BodyPartsData._items[i]));
            }
            CarInfoData = new ModCarInfoData(newCarData.CarInfoData);
            carToLoad = newCarData.carToLoad;
            configVersion = newCarData.configVersion;
            customerCar = newCarData.customerCar;
            color = new ModColor(newCarData.color[0], newCarData.color[1], newCarData.color[2], newCarData.color[3]);
            EngineData = new ModEngineData(newCarData.EngineData);
            engineSwap = newCarData.engineSwap;
            factoryColor = new ModColor(newCarData.factoryColor[0], newCarData.factoryColor[1], newCarData.factoryColor[2], newCarData.factoryColor[3]);
            factoryPaintType = (ModPaintType)newCarData.factoryPaintType;
            finalDriveRatio = newCarData.finalDriveRatio;
            gearRatio = newCarData.gearRatio.ToList();
            HasCustomPaintType = newCarData.HasCustomPaintType;
            index = newCarData.index;
            LightsOn = newCarData.LightsOn;
            measuredDragIndex = newCarData.measuredDragIndex;
            orderConnection = newCarData.orderConnection;
            PaintData = new ModPaintData(newCarData.PaintData);
            PartData = new List<ModPartData>();
            for (int i = 0; i < newCarData.PartData.Count; i++)
            {
                PartData.Add(new ModPartData(newCarData.PartData._items[i]));
            }
            rimsSize = newCarData.rimsSize.ToList();
            tiresET = newCarData.tiresET.ToList();
            tiresSize = newCarData.tiresSize.ToList();
            UId = newCarData.UId;
            wheelsesAlignment = new ModWheelsAlignment(newCarData.WheelsAlignment);
            wheelsWidth = newCarData.wheelsWidth.ToList();
        }

        public NewCarData ToGame()
        {
            NewCarData newData = new NewCarData();

            newData.AdditionalCarRot = AdditionalCarRot;
    
            newData.BodyPartsData = new Il2CppSystem.Collections.Generic.List<BodyPartData>();
            foreach (var bodyPartData in BodyPartsData)
            {
                newData.BodyPartsData.Add(bodyPartData.ToGame());
            }
    
            newData.CarInfoData = CarInfoData.ToGame(CarInfoData);
            newData.carToLoad = carToLoad;
            newData.configVersion = configVersion;
            newData.customerCar = customerCar;
    
            newData.color = new float[] { color.r, color.g, color.b, color.a };
            newData.EngineData = EngineData.ToGame();
            newData.engineSwap = engineSwap;
            newData.factoryColor = new float[] { factoryColor.r, factoryColor.g, factoryColor.b, factoryColor.a };
            newData.factoryPaintType = (PaintType)factoryPaintType;
            newData.finalDriveRatio = finalDriveRatio;
            newData.gearRatio = gearRatio.ToArray();
            newData.HasCustomPaintType = HasCustomPaintType;
            newData.index = index;
            newData.LightsOn = LightsOn;
            newData.measuredDragIndex = measuredDragIndex;
            newData.orderConnection = orderConnection;
            newData.PaintData = PaintData.ToGame(PaintData);
    
            newData.PartData = new Il2CppSystem.Collections.Generic.List<PartData>();
            foreach (var modPartData in PartData)
            {
                newData.PartData.Add(modPartData.ToGame());
            }
    
            newData.rimsSize = rimsSize.ToArray();
            newData.tiresET = tiresET.ToArray();
            newData.tiresSize = tiresSize.ToArray();
            newData.UId = UId;
            newData.WheelsAlignment = wheelsesAlignment.ToGame(wheelsesAlignment);
            newData.wheelsWidth = wheelsWidth.ToArray();

            return newData;
        }
    }
}