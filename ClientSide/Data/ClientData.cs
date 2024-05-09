using System;
using System.Collections;
using System.Collections.Generic;
using CMS21Together.ClientSide.Data.Car;
using CMS21Together.ClientSide.Data.PlayerData;
using CMS21Together.ClientSide.Handle;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using Il2Cpp;
using MelonLoader;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CMS21Together.ClientSide.Data
{
    public static class ClientData
    {
        public static AssetBundle playerBundle;
        public static GameObject playerPrefab;
        public static Dictionary<int, Player> players = new Dictionary<int, Player>();
        public static Dictionary<int, GameObject> PlayersGameObjects = new Dictionary<int, GameObject>();
        
        public static Dictionary<int, ModCar> LoadedCars = new Dictionary<int, ModCar>();
        public static List<(int, string)> tempCarList = new List<(int, string)>();
        
        public static bool isServerAlive = true;
        public static bool needToKeepAlive = false;
        public static bool isKeepingAlive;

        public static List<Item> playerInventory = new List<Item>();
        public static List<GroupItem> playerGroupInventory = new List<GroupItem>();

        public static int playerMoney;
        public static int playerScrap;
        public static int playerExp;

        public static bool asGameStarted;
        
        public static void Init()
        {
            if (GameData.Instance != null)
                GameData.Instance.Initialize();
            else { GameData data = new GameData(); data.Initialize(); }
            
        }
        public static void UpdateClient() // Run Only if player is connected and in scene:"garage" 
        {
            CarManagement.UpdateCars();
            
            Movement.SendPosition();
            Rotation.SendRotation();
            
            ModInventory.UpdateInventory();
            Stats.HandleExp();
            Stats.HandleMoney();
            Stats.HandleScrap();
            
            foreach (var player in ClientData.PlayersGameObjects)
            {
                if (player.Value != null && player.Key != Client.Instance.Id)
                {
                    player.Value.GetComponent<ModCharacterController>().UpdatePlayer();
                }
            }

        }
        public static IEnumerator keepClientAlive()
        {
            isKeepingAlive = true;
            ClientSend.KeepAlive();
            yield return new WaitForSeconds(5);
            isKeepingAlive = false;
        }

        public static IEnumerator isServer_alive()
        {
            if (!Client.Instance.isConnected)
                yield break;

            if (isServerAlive)
            {
                yield return new WaitForSeconds(30);
                isServerAlive = false;
            }
            else
            {
                yield return new WaitForSeconds(35);
                if (!isServerAlive)
                {
                    if (!Client.Instance.isConnected)
                    {
                        if (!ModSceneManager.isInMenu())
                            NotificationCenter.m_instance.StartCoroutine(NotificationCenter.m_instance.SelectSceneToLoad("Menu", SceneType.Menu, true, false));
                        Client.Instance.Disconnect();
                    }
                }
            }
        }
        public static void SpawnPlayer(Player player)
        {
            if (playerPrefab != null)
            {
                GameObject playerObject;
                if (player.id == Client.Instance.Id)
                {
                    playerObject = GameData.localPlayer;
                }
                else
                {
                    if (!ClientData.PlayersGameObjects.ContainsKey(player.id))
                    {
                        playerObject = Object.Instantiate(playerPrefab, player.position.toVector3(),player.rotation.toQuaternion());
                        playerObject.transform.name = player.username;
                        PlayersGameObjects[player.id] = playerObject;
                    }
                    return;
                }
                MelonLogger.Msg($"{player.username} is In-game");
                PlayersGameObjects[player.id] = playerObject;
            }
            else
            {
                MelonLogger.Msg("playerPrefab is not set! aborting...");
                playerPrefabSetup();
                SpawnPlayer(player);
            }
        }


        public static void playerPrefabSetup()
        {
            playerBundle = AssetBundle.LoadFromStream(DataHelper.DeepCopy(DataHelper.LoadContent("CMS21Together.Assets.player.assets")));

            if (playerBundle)
            {

                GameObject player = playerBundle.LoadAsset<GameObject>("playerModel");

                GameObject Nplayer = Object.Instantiate(player);
                Nplayer.AddComponent<ModCharacterController>();
                
                Material material;
                
                Texture baseTexture = playerBundle.LoadAsset<Texture>("tex_base");
                baseTexture.filterMode = FilterMode.Point;
                Texture normalTexture = playerBundle.LoadAsset<Texture>("tex_normal");
                baseTexture.filterMode = FilterMode.Point;
                
                
                material = new Material(Shader.Find("HDRP/Unlit"));
                material.mainTexture = baseTexture;
                material.SetTexture("_BumpMap", normalTexture);

                Nplayer.GetComponentInChildren<SkinnedMeshRenderer>().material = material;
                
                Nplayer.transform.localScale = new Vector3(0.095f, 0.095f, 0.095f);
                Nplayer.transform.position = new Vector3(0, -10, 0);
                Nplayer.transform.rotation = new Quaternion(0, 180, 0, 0);
                    
                playerPrefab = Nplayer; 
                    
                Object.DontDestroyOnLoad(playerPrefab);
                    
                playerBundle.Unload(false);
                MelonLogger.Msg("Loaded player model Succesfully!");
                
            }
            
        }
    }
}