using System;
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
    }
}