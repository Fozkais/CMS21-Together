using System;
using Il2Cpp;

namespace CMS21Together.Shared.Data;

[Serializable]
public class ModFluidData
{
    public float Level;
    public float Condition;
    public ModCarFluid CarFluid;

    public ModFluidData(FluidData data)
    {
        Level = data.Level;
        Condition = data.Condition;
        if(data.CarFluid != null)
            CarFluid = new ModCarFluid(data.CarFluid);
    }

    public FluidData ToGame()
    {
        FluidData data = new FluidData();
        data.Level = Level;
        data.Condition = Condition;
        data.CarFluid = CarFluid.ToGame();

        return data;
    }
}