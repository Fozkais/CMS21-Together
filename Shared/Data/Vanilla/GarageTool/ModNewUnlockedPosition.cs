using System;
using UnhollowerBaseLib;

namespace CMS21Together.Shared.Data.Vanilla.GarageTool;

[Serializable]
public class ModNewUnlockedPosition
{
	public bool[] position;

	public ModNewUnlockedPosition(NewUnlockedPosition data)
	{
		position = new bool[data.position.Length];
		for (var i = 0; i < data.position.Length; i++) position[i] = data.position[i];
	}

	public NewUnlockedPosition ToGame()
	{
		var a = new NewUnlockedPosition();
		a.position = new Il2CppStructArray<bool>(position.Length);
		for (var i = 0; i < position.Length; i++) a.position[i] = position[i];

		return a;
	}
}