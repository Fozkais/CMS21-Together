using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CMS21MP.DataHandle;
using Il2Cpp;
using Il2CppCMS.Tutorial;
using MelonLoader;
using UnityEngine;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

namespace CMS21MP.ClientSide
{
    [RegisterTypeInIl2Cpp]
    public class playerManagement : MonoBehaviour
    {
        // Pos & Rot
        private Vector3 lastPos = new Vector3(0,0,0);
        private Quaternion lastRot = new Quaternion(0, 0, 0, 0);
        
        // Inventory
        public List<Item> currentInventory = new List<Item>();
        public static List<Item> InventoryHandler = new List<Item>();
        public List<Item> currentInventoryHandler = new List<Item>();
        public static List<long> ItemsUID = new List<long>();
        
        public static List<carData> carHandler = new List<carData>();
        public static List<carData> carsToHandle = new List<carData>();

        public static List<List<C_PartScriptData>> PartsHandler = new List<List<C_PartScriptData>>();
        public static List<List<C_PartScriptData>> PartsToHandle = new List<List<C_PartScriptData>>();
        
        public static List<C_carPartsData> CarPartsHandler = new List<C_carPartsData>();
        public static List<C_carPartsData> CarPartsToHandle = new List<C_carPartsData>();

        public static bool hasFinishedUpdatingCar = true;
        
        // Money
        public static int moneyHandler;
        public static int serverMoney;

        public void playerInfoUpdate()
        {
            SendPositionToServer();
            SendRotationToServer();
            AddItem();
            RemoveItem();
            SendMoneyToServer();

            HandleCarSpawning();
            carHandling();
            HandleCarRemoving();

            if(hasFinishedUpdatingCar)
                MelonCoroutines.Start(delayCarUpdating());
            
            MoveCar();
            
        }

        public IEnumerator delayCarUpdating()
        {
            hasFinishedUpdatingCar = false;
            yield return new WaitForSeconds(2);
            if (MainMod.carLoaders.Length > 0)
            {
                HandleCarParts();
                HandleParts();
                HandleEngineParts();
                HandleSuspensionParts();
            }
            hasFinishedUpdatingCar = true;
        }

        public void SendMoneyToServer()
        {
            int playerMoney = GlobalData.PlayerMoney;
            if (playerMoney > moneyHandler)
            {
                int diff = playerMoney - moneyHandler;
                moneyHandler += diff;
                ClientSend.PlayerMoney(diff, true);
                MelonLogger.Msg($"Sending money!! +{diff}");
            }
            else if (playerMoney < moneyHandler)
            {
                int diff = playerMoney - moneyHandler;
                moneyHandler += diff;
                ClientSend.PlayerMoney(diff, false);
                MelonLogger.Msg($"Sending money!! {diff}");
            }
        }
        public void SendPositionToServer()
        {
            Vector3 playerPos = MainMod.localPlayer.transform.position;

            if (Vector3.Distance(playerPos, lastPos) > .05f)
            {
                lastPos = playerPos;
                Vector3 newPlayerPos = new Vector3(playerPos.x, playerPos.y - .8f, playerPos.z);
                ClientSend.PlayerMovement(newPlayerPos);
            }
        }
        public void SendRotationToServer()
        {
            Quaternion playerRot = MainMod.localPlayer.transform.rotation;
            if (Quaternion.Angle(lastRot, playerRot) > .05f)
            {
                lastRot = playerRot;
                ClientSend.PlayerRotation(playerRot);
            }
        }
        public void AddItem()
        {
            if (ItemsUID.Count == 0)
                ItemsUID.Add(1);
            
            var localInventory = MainMod.localInventory.items;

            currentInventory.Clear();
            foreach (Item item in localInventory)
            {
                currentInventory.Add(item);
            }

            for (int i = 0; i < currentInventory.Count; i++) 
            {
                if (!ItemsUID.Contains(currentInventory[i].UID)) 
                {
                   // MelonLogger.Msg($"Founded a new Item ! ID:{currentInventory[i].ID}, UID:{currentInventory[i].UID}");
                    ItemsUID.Add(currentInventory[i].UID);
                    InventoryHandler.Add(currentInventory[i]);
                    ClientSend.PlayerInventory(currentInventory[i], true); // Add new Item
                }
            }
        }
        public void RemoveItem()
        {
            currentInventoryHandler.Clear();
            foreach (Item item in InventoryHandler)
            {
                currentInventoryHandler.Add(item);
            }

            for (int i = 0; i < currentInventoryHandler.Count; i++)
            {
                if (!MainMod.localInventory.items.Contains(currentInventoryHandler[i]))
                {
                   // MelonLogger.Msg($"Founded a Removed Item ! ID:{currentInventoryHandler[i].ID}, UID:{currentInventoryHandler[i].UID}");
                    ItemsUID.Remove(currentInventoryHandler[i].UID);
                    InventoryHandler.Remove(currentInventoryHandler[i]);
                    ClientSend.PlayerInventory(currentInventoryHandler[i], false);
                }
            }
        }


        public void HandleCarSpawning()
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
                        car.carColor = new C_Color(carLoader.color.r, carLoader.color.g, carLoader.color.b, carLoader.color.a);
                        carsToHandle.Add(car);
                        //MelonLogger.Msg("CL: Added to ToHandle," + "Car ID: " + car.carID + " Car Loader ID: " +
                                         // car.carLoaderID + " Car Position: " + car.carPosition + " Car Status: " +
                                         // car.status);
                    }
                }
            }
        }
        public void carHandling()
        {
            for (int i = 0; i < carsToHandle.Count; i++)
            {
                if(!isExistingList(carsToHandle[i], carHandler))
                {
                    carHandler.Add(carsToHandle[i]);
                    ClientSend.SpawnCars(carsToHandle[i]);
                   // HandleParts(carsToHandle[i].carLoaderID);
                   // HandleCarParts(carsToHandle[i].carLoaderID);
                    //HandleEngineParts(carsToHandle[i].carLoaderID);
                   // HandleSuspensionParts(carsToHandle[i].carLoaderID);
                    MelonLogger.Msg("CL: Added to Handler," + "Car ID: " + carsToHandle[i].carID + " Car Loader ID: " + carsToHandle[i].carLoaderID + " Car Status: " + carsToHandle[i].status);
                }
            }
        }

        public void HandleCarRemoving()
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
        public bool isExistingList(carData carToHandle, List<carData> otherList)
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
        
        public void MoveCar()
        {
            foreach (carData carData in carHandler)
            {
                if (carData.carPosition != MainMod.carLoaders[carData.carLoaderID].placeNo)
                {
                    carData.carPosition = MainMod.carLoaders[carData.carLoaderID].placeNo;
                    ClientSend.MoveCar(carData.carPosition, carData.carLoaderID);
                } 
            }
        }

        public void HandleParts()
        {
            PartsToHandle.Clear();
            int indexer = -1;

            foreach (carData car in carHandler)
            {
                foreach (Part _part in MainMod.carLoaders[car.carLoaderID].Parts)
                {
                    indexer++;
                    List<C_PartScriptData> tempList = new List<C_PartScriptData>();
                    var partObject = _part.p_handle;
                    List<PartScript> parts = partObject.GetComponentsInChildren<PartScript>().ToList();
                    foreach (PartScript part in parts)
                    {
                        C_PartScriptData carPart = new C_PartScriptData();
                        carPart.carLoaderID = car.carLoaderID;
                        carPart.partID = indexer;
                        carPart.carPartName = _part.p_name;

                        carPart.id = part.id;
                        carPart.tunedID = part.tunedID;
                        carPart.isExamined = part.IsExamined;
                        carPart.unmounted = part.IsUnmounted;
                        carPart.isPainted = part.IsPainted;
                        carPart.color = new C_Color(part.currentColor);
                        carPart.paintType = (int)part.CurrentPaintType;
                        carPart.quality = part.Quality;
                        carPart.condition = part.Condition;
                        carPart.dust = part.Dust;
                        //carPart.bolts = part.Bolts;
                        
                        tempList.Add(carPart);
                    }
                    MelonLogger.Msg($"CL: Adding carPart:{tempList[0].carPartName} to PartsToHandle!, with {tempList.Count} parts.");
                    PartsToHandle.Add(tempList);
                }
            }

            
            for (int i = 0; i < PartsToHandle.Count; i++)
            {
                if (!ListIsExistingList(PartsToHandle[i], PartsHandler))
                {
                    //MelonLogger.Msg($"CL: Sending carPart to server!, with {PartsToHandle[i].Count} parts");
                    PartsHandler.Add(PartsToHandle[i]);
                    ClientSend.carParts(PartsToHandle[i][0].carLoaderID, PartsToHandle[i], partType.part);
                }
            }
        }
        
        public void HandleEngineParts()
        {
            PartsToHandle.Clear();
            int indexer = -1;
            indexer++;

            foreach (carData car in carHandler)
            {
                List<C_PartScriptData> tempList = new List<C_PartScriptData>();
                var partObject = MainMod.carLoaders[car.carLoaderID].e_engine_h;
                List<PartScript> parts = partObject.GetComponentsInChildren<PartScript>().ToList();
                foreach (PartScript part in parts)
                {
                    C_PartScriptData carPart = new C_PartScriptData();
                    carPart.carLoaderID = car.carLoaderID;
                    carPart.partID = indexer;

                    carPart.id = part.id;
                    carPart.tunedID = part.tunedID;
                    carPart.isExamined = part.IsExamined;
                    carPart.unmounted = part.IsUnmounted;
                    carPart.isPainted = part.IsPainted;
                    carPart.color = new C_Color(part.currentColor);
                    carPart.paintType = (int)part.CurrentPaintType;
                    carPart.quality = part.Quality;
                    carPart.condition = part.Condition;
                    carPart.dust = part.Dust;
                    //carPart.bolts = part.mountObject;
                    
                    tempList.Add(carPart);
                }
                MelonLogger.Msg($"CL: Adding carPart:{tempList[0].carPartName} to PartsToHandle!, with {tempList.Count} parts.");
                PartsToHandle.Add(tempList);
            }
            
            for (int i = 0; i < PartsToHandle.Count; i++)
            {
                if (!ListIsExistingList(PartsToHandle[i], PartsHandler))
                {
                    //MelonLogger.Msg($"CL: Sending carPart to server!, with {PartsToHandle[i].Count} parts");
                    PartsHandler.Add(PartsToHandle[i]);
                    ClientSend.carParts(PartsToHandle[i][0].carLoaderID, PartsToHandle[i], partType.engine);
                }
            }
        }
         public void HandleSuspensionParts()
         {
             PartsToHandle.Clear();
             int indexer = -1;
             int s_indexer = -1;
             foreach (carData car in carHandler)
             {
                 foreach (CarLoader carLoader in MainMod.carLoaders)
                 {
                     List<GameObject> partObjects = new List<GameObject>();
                     partObjects.Add(MainMod.carLoaders[car.carLoaderID].s_frontCenter_h);
                     partObjects.Add(MainMod.carLoaders[car.carLoaderID].s_frontLeft_h);
                     partObjects.Add(MainMod.carLoaders[car.carLoaderID].s_frontRight_h);
                     partObjects.Add(MainMod.carLoaders[car.carLoaderID].s_rearCenter_h);
                     partObjects.Add(MainMod.carLoaders[car.carLoaderID].s_rearLeft_h);
                     partObjects.Add(MainMod.carLoaders[car.carLoaderID].s_rearRight_h);

                     foreach (GameObject partObject in partObjects)
                     {
                         s_indexer++;
                         List<C_PartScriptData> tempList = new List<C_PartScriptData>();
                         
                         List<PartScript> parts = partObject.GetComponentsInChildren<PartScript>().ToList();
                         foreach (PartScript part in parts)
                         {
                            indexer++;
                             C_PartScriptData carPart = new C_PartScriptData();
                             carPart.carLoaderID = car.carLoaderID;
                             carPart.partID = indexer;
                             carPart.carPartName = partObject.name;
                             carPart.s_indexer = s_indexer;

                             carPart.id = part.id;
                             carPart.tunedID = part.tunedID;
                             carPart.isExamined = part.IsExamined;
                             carPart.unmounted = part.IsUnmounted;
                             carPart.isPainted = part.IsPainted;
                             carPart.color = new C_Color(part.currentColor);
                             carPart.paintType = (int)part.CurrentPaintType;
                             carPart.quality = part.Quality;
                             carPart.condition = part.Condition;
                             carPart.dust = part.Dust;
                             //carPart.bolts = part.mountObject;
                            
                             tempList.Add(carPart);
                         }
                         MelonLogger.Msg($"CL: Adding carPart:{tempList[0].carPartName} to PartsToHandle!, with {tempList.Count} parts.");
                         PartsToHandle.Add(tempList);
                     }
                 }

             }


             
             for (int i = 0; i < PartsToHandle.Count; i++)
             {
                 if (!ListIsExistingList(PartsToHandle[i], PartsHandler))
                 {
                     //MelonLogger.Msg($"CL: Sending carPart to server!, with {PartsToHandle[i].Count} parts");
                     PartsHandler.Add(PartsToHandle[i]);
                     ClientSend.carParts(PartsToHandle[i][0].carLoaderID, PartsToHandle[i], partType.suspensions);
                 }
             }
         }
        public bool ListIsExistingList(List<C_PartScriptData> listToHandle, List<List<C_PartScriptData>> otherList)
        { 
            return otherList.Any(other => listToHandle.All(item => 
                other.Any(otherItem => item.unmounted == otherItem.unmounted && item.id == otherItem.id)));
        }

        public void HandleCarParts()
        {
            CarPartsToHandle.Clear();
            
            int indexer = -1;
            foreach (carData car in carHandler)
            {
                foreach (CarPart _part in MainMod.carLoaders[car.carLoaderID].carParts)
                {
                    indexer++;
                    C_carPartsData partData = new C_carPartsData();
                    partData.carPartID = indexer;
                    partData.carLoaderID = car.carLoaderID;
                    partData.name = _part.name;
                    partData.switched = _part.Switched;
                    partData.inprogress = _part.Switched;
                    partData.condition = _part.Condition;
                    partData.unmounted = _part.Unmounted;
                    partData.tunedID = _part.TunedID;
                    partData.isTinted = _part.IsTinted;
                    partData.TintColor = new C_Color(_part.TintColor);
                    partData.colors = new C_Color(_part.Color);
                    partData.paintType = (int)_part.PaintType;
                    //partData.paintData = _part.PaintData;
                    partData.conditionStructure = _part.StructureCondition;
                    partData.conditionPaint = _part.ConditionPaint;
                    partData.livery = _part.Livery;
                    partData.liveryStrength = _part.LiveryStrength;
                    partData.outsaidRustEnabled = _part.OutsideRustEnabled;
                    partData.dent = _part.Dent;
                    partData.additionalString = _part.AdditionalString;
                    partData.Dust = _part.Dust;
                    partData.washFactor = _part.WashFactor;

                    foreach (String partAttached in _part.ConnectedParts)
                    {
                        partData.mountUnmountWith.Add(partAttached);
                    }

                    partData.quality = _part.Quality;

                    if (!carPartIsExistingList(partData, CarPartsToHandle))
                    {
                        CarPartsToHandle.Add(partData);
                    }
                }
            }


            for (int i = 0; i < PartsToHandle.Count; i++)
            {
                if (!carPartIsExistingList(CarPartsToHandle[i], CarPartsHandler))
                {
                    //MelonLogger.Msg($"CL: Sending carPart to server!");
                    CarPartsHandler.Add(CarPartsToHandle[i]);
                    ClientSend.bodyParts(CarPartsToHandle[i].carLoaderID, CarPartsToHandle[i]);
                }
            }
        }
        
        public bool carPartIsExistingList(C_carPartsData partToHandle, List<C_carPartsData> otherList)
        {
            for (int i = 0; i < otherList.Count; i++)
            {
                if (otherList[i].name == partToHandle.name && otherList[i].carLoaderID == partToHandle.carLoaderID && otherList[i].quality == partToHandle.quality && partToHandle.unmounted == otherList[i].unmounted)
                {
                    //MelonLogger.Msg($"Rejected CarData: {partToHandle.name}");
                    return true;
                }
            }
            return false;
        }
    }
}