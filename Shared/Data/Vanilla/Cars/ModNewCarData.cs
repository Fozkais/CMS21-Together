using System;
using System.Collections.Generic;
using System.Linq;
using CMS21Together.Shared.Data.Vanilla.GarageTool;
using MelonLoader;

namespace CMS21Together.Shared.Data.Vanilla.Cars;

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
	public int jobID;
	public ModTuningData ecuData;
	public ModEngineData EngineData;
	public string engineSwap;
	public ModColor factoryColor;
	public ModPaintType factoryPaintType;
	public float finalDriveRatio;
	public ModFluidsData FluidsData;
	public List<float> gearRatio;
	public bool HasCustomPaintType;
	public ModHeadLampAlignmentData HeadLampLeftAlignmentData;
	public ModHeadLampAlignmentData HeadlampRightAlignmentData;
	public int index;
	public ModLicencePlatesData LicencePlatesData;
	public bool LightsOn;
	public int measuredDragIndex;
	public int orderConnection;
	public ModPaintData PaintData;
	public List<ModPartData> PartData;
	public List<int> rimsSize;
	public List<int> tiresET;
	public List<int> tiresSize;
	public ModToolsData ToolsData;
	public string UId;
	public ModWheelsAlignmentData wheelsAlignmentData;
	public List<int> wheelsWidth;

	public int carPosition;

	public ModNewCarData(NewCarData newCarData, int placeNo = 0, int _jobID = -1)
	{
		carPosition = placeNo;

		if (newCarData != null)
		{
			AdditionalCarRot = newCarData.AdditionalCarRot;

			if (newCarData.BodyPartsData != null)
			{
				BodyPartsData = new List<ModBodyPartData>();
				foreach (var bodyPartData in newCarData.BodyPartsData.ToArray())
					if (bodyPartData != null)
						BodyPartsData.Add(new ModBodyPartData(bodyPartData));
			}

			if (newCarData.BonusPartsData != null) 
				BonusPartsData = new ModBonusPartsData(newCarData.BonusPartsData);


			CarInfoData = new ModCarInfoData(newCarData.CarInfoData);


			carToLoad = newCarData.carToLoad;
			configVersion = newCarData.configVersion;
			customerCar = newCarData.customerCar;
			jobID = _jobID;

			ecuData = new ModTuningData(newCarData.ecuData);

			if (newCarData.color != null && newCarData.color.Length >= 4) color = new ModColor(newCarData.color[0], newCarData.color[1], newCarData.color[2], newCarData.color[3]);


			EngineData = new ModEngineData(newCarData.EngineData);


			engineSwap = newCarData.engineSwap;

			if (newCarData.factoryColor != null && newCarData.factoryColor.Length >= 4) factoryColor = new ModColor(newCarData.factoryColor[0], newCarData.factoryColor[1], newCarData.factoryColor[2], newCarData.factoryColor[3]);

			factoryPaintType = (ModPaintType)newCarData.factoryPaintType;
			finalDriveRatio = newCarData.finalDriveRatio;

			FluidsData = new ModFluidsData(newCarData.FluidsData);

			gearRatio = newCarData.gearRatio?.ToList();
			HasCustomPaintType = newCarData.HasCustomPaintType;

			HeadLampLeftAlignmentData = new ModHeadLampAlignmentData(newCarData.HeadlampLeftAlignmentData);
			HeadlampRightAlignmentData = new ModHeadLampAlignmentData(newCarData.HeadlampRightAlignmentData);

			index = newCarData.index;

			LicencePlatesData = new ModLicencePlatesData(newCarData.LicensePlatesData);

			LightsOn = newCarData.LightsOn;
			measuredDragIndex = newCarData.measuredDragIndex;
			orderConnection = newCarData.orderConnection;

			PaintData = new ModPaintData(newCarData.PaintData);

			if (newCarData.PartData != null)
			{
				PartData = new List<ModPartData>();
				foreach (var partData in newCarData.PartData.ToArray())
					if (partData != null)
						PartData.Add(new ModPartData(partData));
			}

			rimsSize = newCarData.rimsSize?.ToList();
			tiresET = newCarData.tiresET?.ToList();
			tiresSize = newCarData.tiresSize?.ToList();

			ToolsData = new ModToolsData(newCarData.TooolsData);

			UId = newCarData.UId;


			wheelsAlignmentData = new ModWheelsAlignmentData(newCarData.WheelsAlignment);

			wheelsWidth = newCarData.wheelsWidth?.ToList();
		}
		else
		{
			MelonLogger.Msg("Error: NewCarData is null in ModNewCarData constructor.");
		}
	}

	public NewCarData ToGame()
	{
		var newData = new NewCarData();

		newData.AdditionalCarRot = AdditionalCarRot;
		if (BodyPartsData != null)
		{
			newData.BodyPartsData = new Il2CppSystem.Collections.Generic.List<BodyPartData>();
			foreach (var bodyPartData in BodyPartsData)
				if (bodyPartData != null)
					newData.BodyPartsData.Add(bodyPartData.ToGame());
		}

		newData.BonusPartsData = BonusPartsData.ToGame();
		newData.CarInfoData = CarInfoData.ToGame(CarInfoData);
		newData.carToLoad = carToLoad;
		newData.configVersion = configVersion;
		newData.customerCar = customerCar;
		newData.ecuData = ecuData.ToGame();
		newData.color = color != null ? new[] { color.r, color.g, color.b, color.a } : null;
		newData.EngineData = EngineData.ToGame();
		newData.engineSwap = engineSwap;
		newData.factoryColor = factoryColor != null ? new[] { factoryColor.r, factoryColor.g, factoryColor.b, factoryColor.a } : null;
		newData.factoryPaintType = (PaintType)factoryPaintType;
		newData.finalDriveRatio = finalDriveRatio;
		newData.FluidsData = FluidsData.ToGame();
		newData.gearRatio = gearRatio != null ? gearRatio.ToArray() : null;
		newData.HasCustomPaintType = HasCustomPaintType;
		newData.HeadlampLeftAlignmentData = HeadLampLeftAlignmentData.ToGame();
		newData.HeadlampRightAlignmentData = HeadlampRightAlignmentData.ToGame();
		newData.index = index;
		newData.LicensePlatesData = LicencePlatesData.ToGame();
		newData.LightsOn = LightsOn;
		newData.measuredDragIndex = measuredDragIndex;
		newData.orderConnection = orderConnection;
		newData.PaintData = PaintData.ToGame(PaintData);
		if (PartData != null)
		{
			newData.PartData = new Il2CppSystem.Collections.Generic.List<PartData>();
			foreach (var modPartData in PartData)
				if (modPartData != null)
					newData.PartData.Add(modPartData.ToGame());
		}

		newData.rimsSize = rimsSize != null ? rimsSize.ToArray() : null;
		newData.tiresET = tiresET != null ? tiresET.ToArray() : null;
		newData.tiresSize = tiresSize != null ? tiresSize.ToArray() : null;
		newData.TooolsData = ToolsData.ToGame();
		newData.UId = UId;
		newData.WheelsAlignment = wheelsAlignmentData.ToGame(wheelsAlignmentData);
		newData.wheelsWidth = wheelsWidth != null ? wheelsWidth.ToArray() : null;

		return newData;
	}
}