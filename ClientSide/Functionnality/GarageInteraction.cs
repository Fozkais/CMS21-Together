using System.Collections.Generic;
using CMS21MP.DataHandle;
using Il2Cpp;
using MelonLoader;

namespace CMS21MP.ClientSide.Functionnality
{
    public static class GarageInteraction
    {
        public static void LifterSync()
        {
            foreach (KeyValuePair<int, carData> car in CarSpawn.CarHandle)
            {
                UpdateLifterState(car.Value);
            }
        }

        public static void UpdateLifterState(carData car)
        {
            //MelonLogger.Msg("Start updateCheck");
            if (MainMod.carLoaders[car.carLoaderID].lifter != null)
            {
                //MelonLogger.Msg("Lifter reconized");
                var carLifter = MainMod.carLoaders[car.carLoaderID].lifter;
                var carLifterState = MainMod.carLoaders[car.carLoaderID].lifter.currentState;

                if (carLifterState != car.LifterState)
                {
                    car.LifterState = carLifterState;
                    ClientSend.LifterPos(car.LifterState, car.carLoaderID);
                }
            }
        }
    }
}