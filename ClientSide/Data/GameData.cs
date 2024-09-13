using System.Collections;
using CMS21Together.ClientSide.Data.Garage.Campaign;
using CMS21Together.ClientSide.Data.Garage.Car;
using Il2Cpp;
using Il2CppCMS.UI.Logic.Upgrades;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data;

public class GameData
{
    public static GameData Instance;
    public static bool isReady;
    
    public GameObject localPlayer;
    public Inventory localInventory;
    public CarLoader[] carLoaders;
    public GarageAndToolsTab upgradeTools;
    public OrderGenerator orderGenerator;
    public SpringClampLogic springClampLogic;
    public TireChangerLogic tireChanger;
    public WheelBalancerLogic wheelBalancer;

    public GameData()
    {
        localPlayer = Object.FindObjectOfType<FPSInputController>().gameObject;
        localInventory = GameScript.Get().GetComponent<Inventory>();
        upgradeTools = Object.FindObjectOfType<GarageLevelManager>().garageAndToolsTab;
        orderGenerator = Object.FindObjectOfType<OrderGenerator>();
        springClampLogic = Object.FindObjectOfType<SpringClampLogic>();
        tireChanger = Object.FindObjectOfType<TireChangerLogic>();
        wheelBalancer = Object.FindObjectOfType<WheelBalancerLogic>();
        carLoaders = new[]
        {
            GameScript.Get().carOnScene[0],
            GameScript.Get().carOnScene[3],
            GameScript.Get().carOnScene[4],
            GameScript.Get().carOnScene[1],
            GameScript.Get().carOnScene[2]
        };
        
        isReady = true;
    }

    public static IEnumerator GameReady()
    {
        while (!isReady)
            yield return new WaitForSeconds(0.2f);
    }
}