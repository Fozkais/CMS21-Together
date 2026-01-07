using System;

namespace CMS21Together.Shared.Data.Vanilla.Cars;

[Serializable]
public class ModWheelData
{
	public int ET;
	public bool IsBalanced;
	public int Profile;
	public int Size;
	public int Width;

	public ModWheelData(WheelData itemWheelData)
	{
		ET = itemWheelData.ET;
		IsBalanced = itemWheelData.IsBalanced;
		Profile = itemWheelData.Profile;
		Size = itemWheelData.Size;
		Width = itemWheelData.Width;
	}

	public ModWheelData()
	{
	}

	public WheelData ToGame(ModWheelData itemWheelData)
	{
		var data = new WheelData();
		data.ET = itemWheelData.ET;
		data.IsBalanced = itemWheelData.IsBalanced;
		data.Profile = itemWheelData.Profile;
		data.Size = itemWheelData.Size;
		data.Width = itemWheelData.Width;
		return data;
	}
}