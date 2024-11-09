using System.Collections.Generic;
using CMS21Together.Shared.Data.Vanilla.Cars;

namespace CMS21Together.Shared.Data.Vanilla.GarageTool;

public class ModEngineStand
{
	public ModItem engineItem;
	public ModGroupItem engineGroupItem;

	public Dictionary<int, PartScript> partReferences = new();
	public Dictionary<int, ModPartScript> parts = new();
	
	
	public Vector3Serializable position;
	public QuaternionSerializable rotation;
}