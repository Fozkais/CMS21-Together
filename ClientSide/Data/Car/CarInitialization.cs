using System.Collections;
using System.Linq;
using CMS21Together.Shared;
using CMS21Together.ClientSide.Handle;
using CMS21Together.Shared.Data;
using Il2Cpp;
using Il2CppSystem;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Car
{
    public class CarInitialization
    {

        public  static int ConvertCarLoaderID(string carLoaderID) 
        {
            int convertedLoaderID;
            switch (carLoaderID)
            {
                case "1":
                    convertedLoaderID = 0;
                    break;
                case "2":
                    convertedLoaderID = 1;
                    break;
                case "3":
                    convertedLoaderID = 2;
                    break;
                case "4":
                    convertedLoaderID = 3;
                    break;
                case "5":
                    convertedLoaderID = 4;
                    break;
                default:
                    MelonLogger.Msg($"Error on finding carLoaderID : {carLoaderID}");
                    return -1;
            }

            return convertedLoaderID;
        }
        
        public static IEnumerator InitializePrePatch(CarLoader carLoader, string name, int carLoaderID)
        {
            if(String.IsNullOrEmpty(name))
                yield break;

            
            if (!ClientData.Instance.LoadedCars.Any(s => s.Value.carLoaderID == carLoaderID))
            {
                ModCar newCar = new ModCar(carLoaderID, name, carLoader.ConfigVersion, carLoader.placeNo);
                ClientData.Instance.LoadedCars.Add(carLoaderID, newCar);
                ClientSend.SendModCar(new ModCar(newCar));
            }
          
        }
        
        /*public static IEnumerator InitializePrePatch(CarLoader carLoader, NewCarData carData)
        {
            if(String.IsNullOrEmpty(carData.carToLoad))
                yield break;

            while(GameData.DataInitialzed == false) // DO NOT REMOVE!
                yield return new WaitForSeconds(1);
            
            yield return new WaitForSeconds(1);
            yield return new WaitForEndOfFrame();

            string carLoaderID = carLoader.gameObject.name[10].ToString();
            MelonLogger.Msg($"A car is being loaded! (LoadCarFromFile): {carData.carToLoad}, ID:{carLoaderID}");
            
            int convertedLoaderID = ConvertCarLoaderID(carLoaderID, "Invalid carLoaderID! initializing car Load..");
            
            if (!ClientData.LoadedCars.Any(s => s.Value.carLoaderID == convertedLoaderID))
            {
                ClientSend.SendNewCarData(carData);
            }
          
        }*/

        public static IEnumerator CarIsLoaded(CarLoader loader)
        {
            string carLoaderID = loader.gameObject.name[10].ToString();
            int convertedLoaderID = ConvertCarLoaderID(carLoaderID);

            yield return new WaitForSeconds(1f);
            yield return new WaitForEndOfFrame();
            
            MelonCoroutines.Start(PartsReferences.GetPartsReferences(convertedLoaderID));

            var waitCar = MelonCoroutines.Start(CarManagement.WaitForCarHandle(convertedLoaderID));
            yield return waitCar;
            
            ClientData.Instance.LoadedCars[convertedLoaderID].isCarReady = true;

            yield return new WaitForSeconds(2);
            loader.SaveCarToFile(convertedLoaderID, false);
        }
    }
}