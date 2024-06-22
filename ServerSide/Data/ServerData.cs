using System;
using System.Collections;
using System.Collections.Generic;
using CMS21Together.ServerSide.Handle;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using Il2Cpp;
using UnityEngine;

namespace CMS21Together.ServerSide.Data
{
    public static class ServerData
    {
        public static Dictionary<int, DateTime> lastClientActivity = new Dictionary<int, DateTime>();
        public static Dictionary<int, Player> players = new Dictionary<int, Player>(); 
        public static Dictionary<int, ModCar> LoadedCars = new Dictionary<int, ModCar>(); 

        public static List<ModItem> itemInventory = new List<ModItem>();
        public static List<ModGroupItem> groupItemInventory = new List<ModGroupItem>();

        public static int money, scrap, exp;

        public static bool isRunning;
        public static Dictionary<ModIOSpecialType, ModCarPlace> toolsPosition = new Dictionary<ModIOSpecialType, ModCarPlace>();
        public static ModEngineStand engineStand = new ModEngineStand();
        
        
        public static void CheckForInactiveClients()
        {
            // Délai maximum d'inactivité (en secondes)
            int maxInactivityDelay = 60;
            foreach (KeyValuePair<int, DateTime> entry in lastClientActivity)
            {
                int clientId = entry.Key;
                DateTime lastActivity = entry.Value;

                if ((DateTime.Now - lastActivity).TotalSeconds > maxInactivityDelay)
                {
                    // Le client est inactif depuis trop longtemps, le déconnecter
                    ServerSend.DisconnectClient(clientId, "Client Inactive for too long...");
                    Server.clients.Remove(clientId);
                    lastClientActivity.Remove(clientId);
                }
            }
        }

        public static IEnumerator CheckForInactiveClientsRoutine()
        {
            while (isRunning)
            {
                yield return new WaitForSeconds(10); // Vérifier toutes les 10 secondes
                CheckForInactiveClients();
            }
        }

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