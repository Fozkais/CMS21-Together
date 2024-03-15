using System.Collections;
using CMS21Together.ClientSide.Handle;
using CMS21Together.Shared.Data;
using CMS21Together.Shared;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data
{
    public class GameData
    {
        public static GameData Instance;
        public static bool DataInitialzed;
        
        public static GameObject localPlayer;
        public static FPSInputController playerController;
        public TireChangerLogic tireChanger;
        public WheelBalancerLogic wheelBalancer;
        public CarWashLogic carWash;
        public CarLoader[] carLoaders;
        public Inventory localInventory;
        
        public IEnumerator Initialize()
        {
            if (Instance == null)
            {
                Instance = this;

                playerController = Object.FindObjectOfType<FPSInputController>();
                localPlayer = playerController.gameObject;
                tireChanger = Object.FindObjectOfType<TireChangerLogic>();
                wheelBalancer = Object.FindObjectOfType<WheelBalancerLogic>();
                carWash = Object.FindObjectOfType<CarWashLogic>();
            
                carLoaders = new[]
                {
                    GameScript.Get().carOnScene[0],
                    GameScript.Get().carOnScene[3],
                    GameScript.Get().carOnScene[4],
                    GameScript.Get().carOnScene[1],
                    GameScript.Get().carOnScene[2]
                };
                if (ModSceneManager.isInGarage())
                {
                    localInventory = GameScript.Get().GetComponent<Inventory>();
                    if (ClientData.refreshCars)
                    {
                        //ClientSend.SendCarInfo(new ModCar(), true); TODO:Reimplement
                        ClientData.refreshCars = false;
                    }
                }

                ClientData.asGameStarted = true;
                yield return new WaitForSeconds(3);
                MelonLogger.Msg("Initialized Game Data Successfully!");
                DataInitialzed = true;
            }
            else if (Instance != this)
            {
                MelonLogger.Msg("Instance already exists, destroying object!");
            }
            
        }
    }
}