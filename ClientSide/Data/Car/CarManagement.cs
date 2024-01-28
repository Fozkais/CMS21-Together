using System;
using System.Collections;
using System.Collections.Generic;
using CMS21Together.ClientSide.Handle;
using CMS21Together.Shared.Data;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Car
{
    public static class CarManagement
    {
        private static bool partHandleRunning;
        private static bool detectCarMoveAndUnloadedCar;

        public static void UpdateCars()
        {
            if (!partHandleRunning)
                MelonCoroutines.Start(CarPartUpdateHandle());
            if (!detectCarMoveAndUnloadedCar)
                MelonCoroutines.Start(CarMoveAndLoadHandle());
        }

        private static IEnumerator CarMoveAndLoadHandle()
        {
            detectCarMoveAndUnloadedCar = true;
            MelonCoroutines.Start(DetectUnloadedCar());
            DetectCarMoves();
            yield return new WaitForSeconds(1);
            detectCarMoveAndUnloadedCar = false;
        }

        private static IEnumerator CarPartUpdateHandle()
        {
            partHandleRunning = true;

            foreach (ModCar car in ClientData.LoadedCars.Values)
            {
                MelonCoroutines.Start(PartsHandle.HandleParts(car));
            }

            yield return new WaitForSeconds(0.15f);
            partHandleRunning = false;
        }
        
        public static IEnumerator DetectUnloadedCar()
        {
            foreach (ModCar car in ClientData.LoadedCars.Values)
            {
                int count = 0;
                while (!car.isHandled && count < 20)
                {
                    count += 1;
                    yield return new WaitForSeconds(0.1f);
                }
                if (String.IsNullOrEmpty(GameData.Instance.carLoaders[car.carLoaderID].carToLoad) && GameData.Instance.carLoaders[car.carLoaderID].carParts == null)
                {
                    ClientSend.SendModCar(new ModCar(car));
                    ClientData.LoadedCars.Remove(car.carLoaderID);
                }
            }
        }
        //
        public static void DetectCarMoves()
        {
            foreach (var car in ClientData.LoadedCars)
            { 
                if (!car.Value.isFromServer)
                {
                    if (GameData.Instance.carLoaders[car.Value.carLoaderID].placeNo != car.Value.carPosition 
                        && GameData.Instance.carLoaders[car.Value.carLoaderID].placeNo != -1)
                    {
                        ClientSend.SendCarPosition(car.Value.carLoaderID, GameData.Instance.carLoaders[car.Value.carLoaderID].placeNo);
                        car.Value.carPosition = GameData.Instance.carLoaders[car.Value.carLoaderID].placeNo;
                    }
                }
                    
                        
            }
        }
    }
}