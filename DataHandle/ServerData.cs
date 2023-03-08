using System;
using System.Collections.Generic;
using System.Linq;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace CMS21MP.DataHandle
{
    public class ServerData
    {
        public static Dictionary<int, List<Item>> AddItemQueue = new Dictionary<int, List<Item>>();
        public static Dictionary<int, List<Item>> RemoveItemQueue = new Dictionary<int, List<Item>>();
        public static Inventory serverInventory = new Inventory();

        public static List<carData> carList = new List<carData>();
        public static List<carData> carListHandle = new List<carData>();

        public static int serverMoney = GlobalData.PlayerMoney;

        public static void UpdateInventory()
        {
            if (RemoveItemQueue.Count > 0)
            {
                foreach (KeyValuePair<int, List<Item>> element in RemoveItemQueue)
                {
                    for (int i = 0; i < element.Value.Count; i++)
                    {
                        serverInventory.items.Remove(element.Value[i]);
                        ServerSend.PlayerInventory(element.Key,element.Value[i], false);
                        RemoveItemQueue[element.Key].Remove(element.Value[i]);
                    }
                }
            }

            if (AddItemQueue.Count > 0)
            {
                foreach (KeyValuePair<int, List<Item>> element in AddItemQueue)
                {
                    for (int i = 0; i < element.Value.Count; i++)
                    {
                        serverInventory.items.Add(element.Value[i]);
                        ServerSend.PlayerInventory(element.Key,element.Value[i], true);
                        AddItemQueue[element.Key].Remove(element.Value[i]);
                    }
                }
            }
        }

public static void UpdateCarList()
        {
            if (carList.Count > 0)
            {
                for (var i = 0; i < carList.Count; i++)
                {
                    var carToHandle = carList[i];
                    if (carToHandle.status && carListHandle.Count > 0)
                    {
                        bool existing = false;
                        
                        for (var x = 0; x < carListHandle.Count; x++)
                        {
                            var car = carListHandle[x];
                            if (car.Exist(carToHandle))
                            {
                                existing = true;
                                break;
                            }
                        }
                        
                        if (existing)
                        {
                            carList.Remove(carToHandle);
                        }
                        else
                        {
                            ServerSend.SpawnCars(carToHandle.clientID, carToHandle);
                            carListHandle.Add(carToHandle);
                            carList.Remove(carToHandle);
                        }
                    }
                    else if(!carToHandle.status && carListHandle.Count > 0)
                    {
                        bool existing = false;
                        
                        for (int x = 0; x < carListHandle.Count; x++)
                        {
                            var car = carListHandle[x];
                            if (car.Exist(carToHandle))
                            {
                                existing = true;
                                break;
                            }
                        }
                        if (existing)
                        {
                            ServerSend.SpawnCars(carToHandle.clientID, carToHandle);
                            carListHandle.Remove(carToHandle);
                            carList.Remove(carToHandle);
                        }
                        else
                        {
                            carList.Remove(carToHandle);
                        }
                    }
                    else
                    {
                        ServerSend.SpawnCars(carToHandle.clientID, carToHandle);
                        carListHandle.Add(carToHandle);
                        carList.Remove(carToHandle);
                    }
                }
            }
        }
    }

    [Serializable]
    public class carData
    {
        public int clientID;
        public bool status;
        
        public int carLoaderID;
        public string carID;
        public int carPosition;

        //public Color carColor;


        public bool Exist(carData car)
        {
            return carID == car.carID && carLoaderID == car.carLoaderID;
        }
    }
}