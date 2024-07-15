using System.Collections;
using CMS21Together.ClientSide.Data.Garage.Car;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data;

public class GameData
{
    public static GameData Instance;
    
    public GameObject localPlayer;
    public Inventory localInventory;
    public CarLoader[] carLoaders;
    public static bool isReady;

    public GameData()
    {
        localPlayer = Object.FindObjectOfType<FPSInputController>().gameObject;
        localInventory = GameScript.Get().GetComponent<Inventory>();
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