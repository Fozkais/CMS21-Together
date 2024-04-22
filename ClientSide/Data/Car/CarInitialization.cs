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

        public  static int ConvertCarLoaderID(string carLoaderID, string errorMsg)
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
                    MelonLogger.Msg(errorMsg);
                    return -1;
            }

            return convertedLoaderID;
        }
        
        public static IEnumerator InitializePrePatch(CarLoader carLoader, string name)
        {
            if(String.IsNullOrEmpty(name))
                yield break;

            while(GameData.DataInitialized == false) // DO NOT REMOVE!
                yield return new WaitForSeconds(1);
            
            yield return new WaitForSeconds(1);
            yield return new WaitForEndOfFrame();

            string carLoaderID = carLoader.gameObject.name[10].ToString();
            MelonLogger.Msg($"A car is being loaded! (LoadCar): {name}, ID:{carLoaderID}");
            
            int convertedLoaderID = ConvertCarLoaderID(carLoaderID, "Invalid carLoaderID! initializing car Load..");
            
            if (!ClientData.LoadedCars.Any(s => s.Value.carLoaderID == convertedLoaderID))
            {
                ModCar newCar = new ModCar(convertedLoaderID, carLoader.ConfigVersion, carLoader.placeNo);
                ClientData.LoadedCars.Add(convertedLoaderID, newCar);
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

        public static IEnumerator LoadCar(CarLoader loader)
        {
            
            int count0 = 0;
            while (GameData.DataInitialized == false && count0 < 20)
            {
                count0 += 1;
                yield return new WaitForSeconds(0.3f);
            }
            
            string carLoaderID = loader.gameObject.name[10].ToString();
            int convertedLoaderID = ConvertCarLoaderID(carLoaderID, "Invalid carLoaderID! aborting car Load..");
            
            int count = 0;
            while (!ClientData.LoadedCars.ContainsKey(convertedLoaderID) && count < 15)
            {
                count += 1;
                yield return new WaitForSeconds(0.1f);
            }
            
            yield return new WaitForEndOfFrame();
            
            ClientData.LoadedCars[convertedLoaderID].isCarLoaded = true;
            MelonCoroutines.Start(PartsReferences.GetPartsReferences(convertedLoaderID));
        }
    }
}