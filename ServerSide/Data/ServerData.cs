using System.Collections.Generic;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using Il2Cpp;

namespace CMS21Together.ServerSide.Data
{
    public static class ServerData
    {
        public static Dictionary<int, Player> players = new Dictionary<int, Player>(); 
        public static Dictionary<int, ModCar> LoadedCars = new Dictionary<int, ModCar>(); 

        public static List<ModItem> itemInventory = new List<ModItem>();
        public static List<ModGroupItem> groupItemInventory = new List<ModGroupItem>();

        public static int money, scrap, exp;

        public static bool isRunning;
        public static Dictionary<ModIOSpecialType, ModCarPlace> toolsPosition = new Dictionary<ModIOSpecialType, ModCarPlace>();
        public static ModEngineStand engineStand = new ModEngineStand();

        public static void ResetData()
        {
            players.Clear();
            LoadedCars.Clear();
            
            itemInventory.Clear();
            groupItemInventory.Clear();
            toolsPosition.Clear();
            engineStand = new ModEngineStand();

            money = 0;
            scrap = 0;
            exp = 0;
            
            isRunning = false;
        }
    }
}