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
        public static bool DataInitialized;
        
        public GameObject localPlayer;
        public GarageLoader garageLoader;
        public FPSInputController playerController;
        public TireChangerLogic tireChanger;
        public WheelBalancerLogic wheelBalancer;
        public CarWashLogic carWash;
        public EngineStandLogic engineStand;
        public CarLoader[] carLoaders;
        public Inventory localInventory;
        public ScreenFader screenFader;
        
        public void Initialize()
        {
            Instance = this;

            playerController = Object.FindObjectOfType<FPSInputController>();
            localPlayer = playerController.gameObject;
            garageLoader = GarageLoader.instance;
            tireChanger = Object.FindObjectOfType<TireChangerLogic>();
            wheelBalancer = Object.FindObjectOfType<WheelBalancerLogic>();
            carWash = Object.FindObjectOfType<CarWashLogic>();
            engineStand = Object.FindObjectOfType<EngineStandLogic>();
            screenFader = Object.FindObjectOfType<ScreenFader>();
           
            carLoaders = new[]
            {
                GameScript.Get().carOnScene[0],
                GameScript.Get().carOnScene[3],
                GameScript.Get().carOnScene[4],
                GameScript.Get().carOnScene[1],
                GameScript.Get().carOnScene[2]
            };

            localInventory = GameScript.Get().GetComponent<Inventory>();

            ClientData.Instance.asGameStarted = true;
            MelonLogger.Msg("Initialized Game Data Successfully!");
            DataInitialized = true;
        }
    }
}