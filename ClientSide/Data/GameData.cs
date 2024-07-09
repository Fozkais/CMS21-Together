using Il2Cpp;
using UnityEngine;

namespace CMS21Together.ClientSide.Data;

public class GameData
{
    public static GameData Instance;
    
    public GameObject localPlayer;

    public GameData()
    {
        Instance = this;
        localPlayer = Object.FindObjectOfType<FPSInputController>().gameObject;
    }
}