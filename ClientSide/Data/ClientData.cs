using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CMS21Together.ClientSide.DataHandle;
using CMS21Together.CustomData;
using CMS21Together.SharedData;
using Il2Cpp;
using MelonLoader;
//using Steamworks;
using UnityEngine;

namespace CMS21Together.ClientSide.Data
{
    public static class ClientData 
    {
        public static GameObject playerPrefab;
        public static Dictionary<int, Player> players = new Dictionary<int, Player>();
        public static Dictionary<int, GameObject> serverPlayerInstances = new Dictionary<int, GameObject>();
        
        public static Dictionary<int, ModCar> carOnScene = new Dictionary<int, ModCar>();
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
        public static bool refreshCars;

        public static void Init()
        {
            MelonCoroutines.Start(GameData.InitializeGameData());
            
        }
        
        public static void UpdateClientInfo()
        {
            Movement.SendMovement();
            Movement.SendRotation();
            
            Car.UpdateCars();
            ModInventory.UpdateInventory();
            Stats.HandleStats();

        }

        public static IEnumerator keepClientAlive()
        {
            if (Client.Instance.isConnected)
            {
                isKeepingAlive = true;
                ClientSend.KeepAlive();
               // MelonLogger.Msg("KeepinClientAlive!");
                yield return new WaitForSeconds(5);
                isKeepingAlive = false;
            }
            isKeepingAlive = false;
        }

        public static IEnumerator isServer_alive()
        {
            if (!Client.Instance.isConnected)
                yield break;


            if (isServerAlive)
            {
               // MelonLogger.Msg("Server is Alive!");
                yield return new WaitForSeconds(5);
                isServerAlive = false;
            }
            else
            {
                yield return new WaitForSeconds(8);
                if (!isServerAlive)
                {
                    if (!Client.Instance.isConnected)
                    {
                        if(SceneChecker.isNotInMenu())
                            NotificationCenter.m_instance.StartCoroutine(NotificationCenter.m_instance.SelectSceneToLoad("Menu", SceneType.Menu, true, false));
                        Client.Instance.Disconnect();


                    }
                    else
                    {
                        MelonLogger.Msg($"CL: Server no longer alive! Disconnecting...");
                        if(SceneChecker.isNotInMenu())
                            NotificationCenter.m_instance.StartCoroutine(NotificationCenter.m_instance.SelectSceneToLoad("Menu", SceneType.Menu, true, false));
                        Client.Instance.Disconnect();

                    }
                    
                }
            }
        }



        public static void SpawnPlayer(Player player, int id)
        {
            if (playerPrefab != null)
            {
                GameObject playerObject;
                if (id == Client.Instance.Id)
                {
                    playerObject = GameData.localPlayer;
                }
                else
                {
                    playerObject = Object.Instantiate(playerPrefab, player.position.toVector3(),player.rotation.toQuaternion());
                    playerObject.transform.name = player.username;
                    
                }
                MelonLogger.Msg($"{player.username} is In-game");
                serverPlayerInstances[id] = playerObject;
            }
            else
            {
                MelonLogger.Msg("playerPrefab is not set! aborting...");
                playerInit();
                SpawnPlayer(player, id);
            }
        }

        public static void playerInit()
        {
            AssetBundle playerModel;
            AssetBundle playerTexture;
            
            playerModel = AssetBundle.LoadFromStream(DataHelper.DeepCopy(LoadContent("CMS21Together.Assets.playermodel.asset"))); 
            playerTexture = AssetBundle.LoadFromStream(DataHelper.DeepCopy(LoadContent("CMS21Together.Assets.playertexture.asset")));
            
            if (playerModel)
            {
                Mesh mesh = playerModel.LoadAsset<GameObject>("playermodel").GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh;
                Material material;

                if (playerTexture)
                {
                    Texture modelTexture = playerTexture.LoadAsset<Texture>("rp_nathan_animated_003_dif");
                    modelTexture.filterMode = FilterMode.Point;
                    Material mat = new Material(Shader.Find("HDRP/Unlit"));
                    mat.mainTexture = modelTexture;
                    material = mat;
                    
                    
                    GameObject model = new GameObject();
                    model.AddComponent<MeshFilter>();
                    model.AddComponent<MeshRenderer>();
                    model.GetComponent<MeshFilter>().mesh = mesh;
                    model.GetComponent<MeshRenderer>().material = material;
                    
                    model.name = "playerModel";
                    model.transform.localScale = new Vector3(0.90f, 0.90f, 0.90f);
                    model.transform.position = new Vector3(0, -10, 0);
                    model.transform.rotation = new Quaternion(0, 180, 0, 0);
                    
                    playerPrefab = model;
                    
                    Object.DontDestroyOnLoad(playerPrefab);
                    
                    playerModel.Unload(false);
                    playerTexture.Unload(false);
                    MelonLogger.Msg("Loaded player model Succesfully!");
                }
                
            }
            
        }
        
        private static Stream LoadContent(string assemblyPath)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream(assemblyPath);
            
            return stream;
        }
        
        /*public static void ReceivePacket()
        {
            while (SteamNetworking.IsP2PPacketAvailable())
            {
                MelonLogger.Msg("Packet received");
                var packet = SteamNetworking.ReadP2PPacket();
                if (packet.HasValue)
                {
                    PacketHandling.HandlePacket(packet.Value.Data);
                }
            }
        }*/
    }
}