using System.Collections;
using System.Collections.Generic;
using CMS21Together.ClientSide.Data.Garage.Campaign;
using CMS21Together.ClientSide.Data.Garage.Car;
using CMS21Together.ClientSide.Data.Player;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data;

public class ClientData
{
    public static ClientData Instance;
    public static UserData UserData;
    public static bool GameReady;

    public ClientData()
    {
        GameReady = false;
        GameData.Instance = null;
        
        Inventory.Reset();
        CarSpawnManager.Reset();
        CarSpawnHooks.Reset();
        JobManager.Reset();
        Stats.Reset();

    }
    
    public Dictionary<int, UserData> connectedClients = new Dictionary<int, UserData>();
    public Dictionary<int, ModCar> loadedCars = new Dictionary<int, ModCar>();
    public Dictionary<string, GarageUpgrade> garageUpgrades = new  Dictionary<string, GarageUpgrade>();
    public GameObject playerPrefab;
    public Gamemode gamemode;
    public int scrap, money;

    public void UpdateClient()
    {
        if (GameData.Instance == null)
            MelonCoroutines.Start(InitializeGameData());
        
        Stats.SendInitialStats();
        Movement.SendPosition();
        Rotation.SendRotation();
        JobManager.UpdateSelectedJob();
    }

    private IEnumerator InitializeGameData()
    {
        GameData.Instance = new GameData();
        
        yield return new WaitForSeconds(2);
        yield return new WaitForEndOfFrame();

        gamemode = SavesManager.GetGamemodeFromDifficulty(SavesManager.currentSave.Difficulty);
        GameReady = true;
        MelonLogger.Msg("Game is ready.");
    }
    
    public void LoadPlayerPrefab()
    {
        var playerBundle = AssetBundle.LoadFromStream(DataHelper.DeepCopy(DataHelper.LoadContent("CMS21Together.Assets.player.assets")));

        if (playerBundle)
        {
            GameObject player = playerBundle.LoadAsset<GameObject>("playerModel");
            GameObject playerInstance = Object.Instantiate(player);
            
            Material material;
            Texture baseTexture = playerBundle.LoadAsset<Texture>("tex_base");
            baseTexture.filterMode = FilterMode.Point;
            Texture normalTexture = playerBundle.LoadAsset<Texture>("tex_normal");
            baseTexture.filterMode = FilterMode.Point;
            
            material = new Material(Shader.Find("HDRP/Unlit"));
            material.mainTexture = baseTexture;
            material.SetTexture("_BumpMap", normalTexture);

            playerInstance.GetComponentInChildren<SkinnedMeshRenderer>().material = material;
                
            playerInstance.transform.localScale = new Vector3(0.095f, 0.095f, 0.095f);
            playerInstance.transform.position = new Vector3(0, -10, 0);
            playerInstance.transform.rotation = new Quaternion(0, 180, 0, 0);
                    
            playerPrefab = playerInstance; 
                    
            Object.DontDestroyOnLoad(playerPrefab);
                    
            playerBundle.Unload(false);
            MelonLogger.Msg("[ClientData->LoadPlayerPrefab] Loaded player model Succesfully!");
        }
    }
}