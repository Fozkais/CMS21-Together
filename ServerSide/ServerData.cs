using System.Collections.Generic;
using CMS21Together.CustomData;
using CMS21Together.SharedData;
using Il2Cpp;

namespace CMS21Together.ServerSide
{
    public static class ServerData
    {
        public static Dictionary<int, Player> players = new Dictionary<int, Player>(); // Implemented
        public static Dictionary<int, ModCar> carOnScene = new Dictionary<int, ModCar>(); 

        public static List<ModItem> itemInventory = new List<ModItem>(); // Implemented
        public static List<ModGroupItem> groupItemInventory = new List<ModGroupItem>(); // Implemented

        public static bool isRunning;
        
        public static void ResetData()
        {
            players.Clear();
            carOnScene.Clear();
            
            itemInventory.Clear();
            groupItemInventory.Clear();
        }
    }
}