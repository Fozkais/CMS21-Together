using System.Collections.Generic;
using CMS21Together.Shared.Data.Vanilla.Cars;

namespace CMS21Together.Shared.Data.Vanilla.GarageTool;

public class ModEngineStand
{
	public float angle;
	public ModItem engine;

	public Dictionary<int, ModPartScript> engineStandParts = new();
	public Dictionary<int, PartScript> engineStandPartsReferences = new();
	public bool fromServer;
	public ModGroupItem Groupengine;


	public bool isHandled;
	public bool isReferenced;
	public bool needToResync;
	public Vector3Serializable position;
	public QuaternionSerializable rotation;
}