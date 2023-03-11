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

        public static List<C_PartsData> PartsHandler = new List<C_PartsData>();
        public static List<C_PartsData> PartsToHandle = new List<C_PartsData>();
        
        public static List<C_carPartsData> CarPartsHandler = new List<C_carPartsData>();
        public static List<C_carPartsData> CarPartsToHandle = new List<C_carPartsData>();

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

            MoveCar();
        }

        private void SendMoneyToServer()
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
        private void SendPositionToServer()
        {
            Vector3 playerPos = MainMod.localPlayer.transform.position;

            if (Vector3.Distance(playerPos, lastPos) > .05f)
            {
                lastPos = playerPos;
                Vector3 newPlayerPos = new Vector3(playerPos.x, playerPos.y - .8f, playerPos.z);
                ClientSend.PlayerMovement(newPlayerPos);
            }
        }
        private void SendRotationToServer()
        {
            Quaternion playerRot = MainMod.localPlayer.transform.rotation;
            if (Quaternion.Angle(lastRot, playerRot) > .05f)
            {
                lastRot = playerRot;
                ClientSend.PlayerRotation(playerRot);
            }
        }
        private void AddItem()
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
        private void RemoveItem()
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


        private void HandleCarSpawning()
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
                    HandleCarParts(carsToHandle[i].carLoaderID);
                    HandleBodyParts(carsToHandle[i].carLoaderID);
                    MelonLogger.Msg("CL: Added to Handler," + "Car ID: " + carsToHandle[i].carID + " Car Loader ID: " + carsToHandle[i].carLoaderID + " Car Status: " + carsToHandle[i].status);
                }
            }
        }

        private void HandleCarRemoving()
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
    
        

        private void MoveCar()
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

        private void HandleCarParts(int carLoaderID)
        {
            PartsToHandle.Clear();
            int indexer = -1;

            foreach (Part _part in MainMod.carLoaders[carLoaderID].Parts)
            {
                indexer++;
                
                C_PartsData partData = new C_PartsData();
                partData.partID = indexer;
                partData.carLoaderID = carLoaderID;
                partData.positionX = _part.p_position.x;
                partData.positionY = _part.p_position.y;
                partData.positionZ = _part.p_position.z;
                
                partData.rotationX = _part.p_rotation.x;
                partData.rotationY = _part.p_rotation.y;
                partData.rotationZ = _part.p_rotation.z;
                
                partData.name = _part.p_name;
                partData.reflection = _part.p_reflection;
                partData.scale = _part.p_scale;

                if (!partIsExistingList(partData, PartsToHandle))
                {
                    PartsToHandle.Add(partData);
                }
            }
            
            foreach (var part in PartsToHandle) 
            {
                if (!partIsExistingList(part, PartsHandler))
                {
                    PartsHandler.Add(part);
                    //MelonLogger.Msg($"add partData to Handler, ID[{part.carLoaderID}]");
                    ClientSend.carParts(part.carLoaderID, part);
                }
                
            }
        }

        public bool partIsExistingList(C_PartsData partToHandle, List<C_PartsData> otherList)
        {
            for (int i = 0; i < otherList.Count; i++)
            {
                if (otherList[i].name == partToHandle.name && otherList[i].carLoaderID == partToHandle.carLoaderID)
                {
                    return true;
                }
            }
            return false;
        }

        IEnumerator HandleAllCarParts(int carLoaderID)
        {
            yield return new WaitForSeconds(2);
            HandleCarParts(carLoaderID);
            HandleBodyParts(carLoaderID);
        }
        
        private void HandleBodyParts(int carLoaderID)
        {
            CarPartsToHandle.Clear();
            
            int indexer = -1;
            foreach (CarPart _part in MainMod.carLoaders[carLoaderID].carParts)
            {
                indexer++;
                C_carPartsData partData = new C_carPartsData();
                partData.carPartID = indexer;
                partData.carLoaderID = carLoaderID;
                partData.name = _part.name;
                partData.switched =  _part.Switched;
                partData.inprogress =  _part.Switched;
                partData.condition =  _part.Condition;
                partData.tunedID =_part.TunedID;
                partData.colors = new C_Color( _part.Color);
                partData.paintType = (int) _part.PaintType;
                partData.conditionStructure =  _part.StructureCondition;
                partData.conditionPaint =  _part.ConditionPaint;
                partData.livery =  _part.Livery;
                partData.liveryStrength =  _part.LiveryStrength;
                partData.outsaidRustEnabled =  _part.OutsideRustEnabled;
                partData.unmounted = _part.Unmounted;

                foreach (String partAttached in  _part.ConnectedParts)
                {
                    partData.mountUnmountWith.Add(partAttached);
                }

                partData.quality = _part.Quality;
                
                if (!carPartIsExistingList(partData, CarPartsToHandle))
                {
                    CarPartsToHandle.Add(partData);
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
                if (otherList[i].name == partToHandle.name && otherList[i].carLoaderID == partToHandle.carLoaderID && otherList[i].quality == partToHandle.quality)
                {
                    return true;
                }
            }
            MelonLogger.Msg($"Rejected CarData: {partToHandle.name}");
            return false;
        }
    }
}