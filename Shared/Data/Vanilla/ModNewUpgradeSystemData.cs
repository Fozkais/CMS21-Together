using System;
using System.Collections.Generic;

namespace CMS21Together.Shared.Data.Vanilla;

[Serializable]
public struct ModNewUpgradeSystemData
{
	public string[] id;

	public List<ModBoolArrayWrapper> unlocked;

	public int points;

	public ModNewUpgradeSystemData(NewUpgradeSystemData data)
	{
		id = data.id;
		unlocked = new List<ModBoolArrayWrapper>();
		for (var i = 0; i < data.unlocked.Count; i++) unlocked.Add(new ModBoolArrayWrapper(data.unlocked._items[i]));

		points = data.points;
	}

	public NewUpgradeSystemData ToGame()
	{
		var a = new NewUpgradeSystemData();

		a.id = id;
		a.unlocked = new Il2CppSystem.Collections.Generic.List<BoolArrayWrapper>();
		for (var i = 0; i < unlocked.Count; i++) a.unlocked.Add(unlocked[i].ToGame());

		a.points = points;

		return a;
	}
}

[Serializable]
public struct ModBoolArrayWrapper
{
	public bool[] element;

	public ModBoolArrayWrapper(BoolArrayWrapper item)
	{
		element = new bool[item.element.Length];
		for (var i = 0; i < item.element.Length; i++) element[i] = item.element[i];
	}

	public BoolArrayWrapper ToGame()
	{
		var a = new BoolArrayWrapper();
		a.element = element;
		return a;
	}
}