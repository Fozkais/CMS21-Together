using System;
using System.Collections.Generic;
using CMS21_Together_Core.Data.Vanilla.Cars;
using Newtonsoft.Json;

namespace CMS21_Together_Core.Data.Vanilla.GarageTool;

[Serializable]
public class ModEngineStand
{
	public ModGroupItem engineGroupItem;
	public Dictionary<int, ModPartScript> parts = new();
	public Vector3Serializable position;
	public QuaternionSerializable rotation;
	
	[NonSerialized][JsonIgnore] public EngineStandLogic reference;
	[JsonIgnore] public bool isHandled;
	[JsonIgnore] public Dictionary<int, PartScript> partReferences = new();

	public ModEngineStand(EngineStandLogic _reference)
	{
		reference = _reference;
		isHandled = false;
		partReferences = new Dictionary<int, PartScript>();
		parts = new Dictionary<int, ModPartScript>();
	}
	
}