using System;
using System.Collections.Generic;
using CMS21Together.ClientSide.Data;
using CMS21Together.ClientSide.Data.Garage.Car;

namespace CMS21Together.Shared.Data.Vanilla.Cars;

[Serializable]
public class ModPartScript
{
	public string id;
	public string tunedID;
	public bool isExamined;
	public bool isPainted;
	public ModPaintData paintData;
	public int paintType;
	public ModColor color;
	public int quality;
	public float condition;
	public float dust;
	public List<string> bolts;
	public bool unmounted;
	public List<ModPartScript> unmountWith;

	public int partID;
	public int partIdNumber;
	public ModPartType type;

	public ModPartScript() { }

	public ModPartScript(PartScript data, int _partID, int _partIdNumber, ModPartType _type)
	{
		id = data.id;
		tunedID = data.tunedID;
		isExamined = data.IsExamined;
		isPainted = data.IsPainted;
		paintData = new ModPaintData(data.CurrentPaintData);
		paintType = (int)data.CurrentPaintType;
		color = new ModColor(data.currentColor);
		quality = data.Quality;
		condition = data.Condition;
		dust = data.Dust;
		unmounted = data.IsUnmounted;

		if (_type != ModPartType.engineStand)
		{
			var carLoaderID = data.gameObject.GetComponentsInParent<CarLoaderOnCar>(true)[0].CarLoader.name[10] - '0' - 1;
			var car = ClientData.Instance.loadedCars[carLoaderID];
			unmountWith = new List<ModPartScript>();
			foreach (var part in data.unmountWith)
			{
				PartUpdateHooks.FindPartInDictionaries(car, part, out var partType, out var key, out var index);

				if (index == null)
					unmountWith.Add(new ModPartScript(part, key, -1, partType));
				else
					unmountWith.Add(new ModPartScript(part, key, index.Value, partType));
			}
		}

		partID = _partID;
		partIdNumber = _partIdNumber;
		type = _type;
	}

	public PartScript ToGame()
	{
		var data = new PartScript();

		data.id = id;
		data.tunedID = tunedID;
		data.IsExamined = isExamined;
		data.IsPainted = isPainted;
		data.CurrentPaintData = paintData.ToGame(paintData);
		data.CurrentPaintType = (PaintType)paintType;
		data.currentColor = color.ToGame();
		data.Quality = quality;
		data.Condition = condition;
		data.Dust = dust;
		data.IsUnmounted = unmounted;

		return data;
	}
}