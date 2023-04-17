using System;
using System.Collections.Generic;
using CMS21MP.DataHandle;
using MelonLoader;

namespace CMS21MP.ClientSide.Functionnality
{
    public static class CarSpawn_Handling
    {
        public static List<carData> carHandler = new List<carData>();
        public static List<carData> carsToHandle = new List<carData>();

        public static void HandleCar()
        {
            CarChecker();
            CarSpawning();
            CarRemoving();
        }
        
        public static void CarChecker()
        {
            carsToHandle.Clear();
            for (var i = 0; i < MainMod.carLoaders.Length; i++)
            {
                var carLoader = MainMod.carLoaders[i];
                if (!String.IsNullOrEmpty(carLoader.carToLoad))
                {
                    if (carLoader.placeNo != -1)
                    {
                        carData car = new carData();
                        car.carID = carLoader.carToLoad;
                        car.carLoaderID = i;
                        car.carPosition = carLoader.placeNo;
                        car.status = true;
                        car.configNumber = carLoader.ConfigVersion;
                        car.carColor = new C_Color(carLoader.color.r, carLoader.color.g, carLoader.color.b, carLoader.color.a);
                        car.fromServer = false;
                        carsToHandle.Add(car);
                        //MelonLogger.Msg("CL: Added to ToHandle," + "Car ID: " + car.carID + " Car Loader ID: " +
                                         // car.carLoaderID + " Car Position: " + car.carPosition + " Car Status: " +
                                         // car.status);
                    }
                }
            }
        }
        public static void CarSpawning()
        {
            for (int i = 0; i < carsToHandle.Count; i++)
            {
                if(!isExistingList(carsToHandle[i], carHandler))
                {
                    carHandler.Add(carsToHandle[i]);
                    
                    carData car = new carData();
                    car.carID = carsToHandle[i].carID;
                    car.carLoaderID = i;
                    car.carPosition = carsToHandle[i].carPosition;
                    car.configNumber = carsToHandle[i].configNumber;
                    car.status = true;
                    car.carColor = new C_Color(carsToHandle[i].carColor.r, carsToHandle[i].carColor.g, carsToHandle[i].carColor.b, carsToHandle[i].carColor.a);
                    car.fromServer = true;

                    ClientSend.SpawnCars(car);
                   // HandleParts(carsToHandle[i].carLoaderID);
                   // HandleCarParts(carsToHandle[i].carLoaderID);
                    //HandleEngineParts(carsToHandle[i].carLoaderID);
                   // HandleSuspensionParts(carsToHandle[i].carLoaderID);
                    MelonLogger.Msg("CL: Added to Handler," + "Car ID: " + carsToHandle[i].carID + " Car Loader ID: " + carsToHandle[i].carLoaderID + " Car Status: " + carsToHandle[i].status);
                }
            }
        }
        public static void CarRemoving()
        {
            for (int i = 0; i < carHandler.Count; i++)
            {
                carData car = carHandler[i];
                if (!isExistingList(car, carsToHandle))
                {
                    car.status = false;
                    ClientSend.SpawnCars(car);
                    
                    MelonLogger.Msg("CL: Removed from Handler, " + "Car ID: " + car.carID + " Car Loader ID: " + car.carLoaderID + " Car Status: " + car.status);
                    carHandler.Remove(car);
                }
            }
        }
        
        public static bool isExistingList(carData carToHandle, List<carData> otherList)
        {
            foreach (carData car in otherList)
            {
                if(car.carID == carToHandle.carID && car.carLoaderID == carToHandle.carLoaderID)
                {
                    return true;
                }
            }
            return false;
        }
    }
}