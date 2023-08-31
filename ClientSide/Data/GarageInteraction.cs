using System.Collections.Generic;
using CMS21MP.ClientSide.DataHandle;
using CMS21MP.SharedData;
using Il2Cpp;

namespace CMS21MP.ClientSide.Data
{
    public static class GarageInteraction
    {

        public static void handleInteraction()
        {
            if (SceneChecker.isInGarage())
            {
                LifterSync();
            }
        }

        #region Lifter
        
        public static List<(CarLifterState, CarLoader)> lifterState = new List<(CarLifterState, CarLoader)>();

        public static void LifterSync()
        {
            foreach (ModCar car in ClientData.carOnScene)
            {
                UpdateLifterState(car);
            }
        }

        public static void UpdateLifterState(ModCar car)
        {
            //MelonLogger.Msg("Start updateCheck");
            if (ClientData.carLoaders[car.carLoaderID].lifter != null)
            {
                var carLifterState = ClientData.carLoaders[car.carLoaderID].lifter.currentState;
                
                if ((int)carLifterState != car.CarLifterState)
                {
                    car.CarLifterState = (int)carLifterState;
                    //ClientSend.LifterPos(car.LifterState, car.carLoaderID); TODO: Send lifter state to server
                }
            }
        }
        #endregion
    }
}