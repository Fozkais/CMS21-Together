using System;
using UnhollowerBaseLib;

namespace CMS21Together.Shared.Data.Vanilla.Cars;

[Serializable]
public class ModNewCarLoaderData
{
	public int[] position;

	public int[] specialState;

	public ModNewCarLoaderData(NewCarLoaderData data)
	{
		position = new int[data.position.Length];
		for (var i = 0; i < position.Length; i++) position[i] = data.position[i];
		specialState = new int[data.specialState.Length];
		for (var i = 0; i < specialState.Length; i++) specialState[i] = data.specialState[i];
	}

	public NewCarLoaderData ToGame()
	{
		var a = new NewCarLoaderData();
		a.position = new Il2CppStructArray<int>(position.Length);
		for (var i = 0; i < position.Length; i++) a.position[i] = position[i];
		a.specialState = new Il2CppStructArray<int>(specialState.Length);
		for (var i = 0; i < specialState.Length; i++) a.specialState[i] = specialState[i];

		return a;
	}
}