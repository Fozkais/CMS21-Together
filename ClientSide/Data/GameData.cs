using System.Collections;
using Il2Cpp;
using UnityEngine;

namespace CMS21Together.ClientSide.Data;

public class GameData
{
    public static GameData Instance;
    
    public GameObject localPlayer;
    public Inventory localInventory;
    public bool isReady;

    public GameData()
    {
        Instance = this;
        localPlayer = Object.FindObjectOfType<FPSInputController>().gameObject;
        localInventory = GameScript.Get().GetComponent<Inventory>();

        isReady = true;
    }

    public IEnumerator GameReady()
    {
        while (!isReady)
            yield return new WaitForSeconds(0.2f);
    }
}