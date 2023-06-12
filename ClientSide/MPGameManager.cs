using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CMS21MP.ClientSide.Functionnality;
using CMS21MP.DataHandle;
using Il2Cpp;
using MelonLoader;
using UnityEngine;
using CarPart = CMS21MP.ClientSide.Functionnality.CarPart;
using Inventory = CMS21MP.ClientSide.Functionnality.Inventory;

namespace CMS21MP.ClientSide
{
    [RegisterTypeInIl2Cpp]
    public class MPGameManager : MonoBehaviour
    {
        
        public static Dictionary<int,Dictionary<int, List<ModPartScript_Info>>> OriginalParts = new Dictionary<int,Dictionary<int, List<ModPartScript_Info>>>();
        public static Dictionary<int, Dictionary<int, List<PartScriptInfo>>> PartsHandle = new Dictionary<int, Dictionary<int, List<PartScriptInfo>>>();
        
        public static Dictionary<int, Dictionary<int, ModPartScript_Info>> OriginalEngineParts = new Dictionary<int, Dictionary<int, ModPartScript_Info>>();
        public static Dictionary<int, Dictionary<int, PartScriptInfo>> EnginePartsHandle = new Dictionary<int, Dictionary<int, PartScriptInfo>>();
        
        public static Dictionary<int,Dictionary<int, List<ModPartScript_Info>>> OriginalSuspensionParts = new Dictionary<int,Dictionary<int, List<ModPartScript_Info>>>();
        public static Dictionary<int,Dictionary<int, List<PartScriptInfo>>> SuspensionPartsHandle = new Dictionary<int,Dictionary<int, List<PartScriptInfo>>>();

        public static bool isUpdateRunning;

        public async Task InfoUpdate()
        {
            isUpdateRunning = true;
            Movement.HandleMovement();
            Inventory.HandleInventory();
            SceneSwaping.UpdatePlayerScene();
            Stats.HandleStats();
            MoveCar();
            await CarSpawn.HandleCarSpawn();
            await CarPart.HandleAllParts();
            isUpdateRunning = false;
        }
        public void MoveCar()
        {
            foreach (KeyValuePair<int , carData> carData in  CarSpawn.CarHandle)
            {
                if (carData.Value.carPosition != MainMod.carLoaders[carData.Value.carLoaderID].placeNo)
                {
                    carData.Value.carPosition = MainMod.carLoaders[carData.Value.carLoaderID].placeNo;
                    ClientSend.MoveCar(carData.Value.carPosition, carData.Value.carLoaderID);
                } 
            }
        }

        public static void HandleServerReset()
        {
            CarSpawn.CarHandle.Clear();
            ServerData.serverInventory.items.Clear();
            ServerData.ItemAddQueue.Clear();
            ServerData.ItemRemoveQueue.Clear();
            ServerData.serverMoney = 0;
        }
        
    }
}