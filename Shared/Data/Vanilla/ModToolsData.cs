using System;
using Il2Cpp;

namespace CMS21Together.Shared.Data.Vanilla;

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
        ToolsData data = new ToolsData();
        data.WelderIsConnected = this.WelderIsConnected;
        data.InteriorDetailingToolkitIsConnected = this.InteriorDetailingToolkitIsConnected;
        data.OilbinIsConnected = this.OilbinIsConnected;
        data.EngineCraneIsConnected = this.EngineCraneIsConnected;
        data.HeadlampAlignmentSystemIsConnected = this.HeadlampAlignmentSystemIsConnected;
        data.WindowTintingToolkitIsConnected = this.WindowTintingToolkitIsConnected;
        return data;
    }
}