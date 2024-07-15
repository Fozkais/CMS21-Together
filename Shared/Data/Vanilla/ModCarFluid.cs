using System;
using Il2Cpp;

namespace CMS21Together.Shared.Data.Vanilla;

[Serializable]
public class ModCarFluid
{
    public ModCarFluidType FluidType;
    public int ID;
    public bool HasReservoir;


    public ModCarFluid(CarFluid data)
    {
        FluidType = (ModCarFluidType)data.FluidType;
        ID = data.ID;
        HasReservoir = data.HasReservoir;
    }

    public CarFluid ToGame()
    {
        CarFluid carFluid = new CarFluid();
        carFluid.FluidType = (CarFluidType)FluidType;
        carFluid.ID = ID;
        carFluid.HasReservoir = HasReservoir;

        return carFluid;
    }
}