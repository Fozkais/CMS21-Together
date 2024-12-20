using System;

namespace CMS21Together.Shared.Data.Vanilla.GarageTool;

[Serializable]
public struct ModToolsData
{
	public bool WelderIsConnected;
	public bool InteriorDetailingToolkitIsConnected;
	public bool OilbinIsConnected;
	public bool EngineCraneIsConnected;
	public bool HeadlampAlignmentSystemIsConnected;
	public bool WindowTintingToolkitIsConnected;

	public ModToolsData(ToolsData data)
	{
		WelderIsConnected = data.WelderIsConnected;
		InteriorDetailingToolkitIsConnected = data.InteriorDetailingToolkitIsConnected;
		OilbinIsConnected = data.OilbinIsConnected;
		EngineCraneIsConnected = data.EngineCraneIsConnected;
		HeadlampAlignmentSystemIsConnected = data.HeadlampAlignmentSystemIsConnected;
		WindowTintingToolkitIsConnected = data.WindowTintingToolkitIsConnected;
	}

	public ToolsData ToGame()
	{
		var data = new ToolsData();
		data.WelderIsConnected = WelderIsConnected;
		data.InteriorDetailingToolkitIsConnected = InteriorDetailingToolkitIsConnected;
		data.OilbinIsConnected = OilbinIsConnected;
		data.EngineCraneIsConnected = EngineCraneIsConnected;
		data.HeadlampAlignmentSystemIsConnected = HeadlampAlignmentSystemIsConnected;
		data.WindowTintingToolkitIsConnected = WindowTintingToolkitIsConnected;
		return data;
	}
}