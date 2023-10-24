using System.Collections.Generic;
using CMS21MP.ServerSide;
using CMS21MP.SharedData;
using Il2Cpp;
using MelonLoader;
using Steamworks;
using UnityEngine;

namespace CMS21MP.ClientSide.Data
{
    public class ClientData : MonoBehaviour
    {
        public static GameObject playerPrefab;
        public static Dictionary<int, Player> serverPlayers = new Dictionary<int, Player>();
        public static Dictionary<int, GameObject> serverPlayerInstances = new Dictionary<int, GameObject>();
        
        public static List<ModCar> carOnScene = new List<ModCar>();
        public static CarLoader[] carLoaders;

        public static List<Item> playerInventory = new List<Item>();
        public static List<GroupItem> playerGroupInventory = new List<GroupItem>();
        public static Inventory localInventory;

        public static int playerMoney;
        public static int playerScrap;
        public static int playerExp;

        public static void Init()
        {
            carLoaders = new[] // TODO: Reorganise order to match other scene loader organisation
            {
                GameScript.Get().carOnScene[0],
                GameScript.Get().carOnScene[3],
                GameScript.Get().carOnScene[4],
                GameScript.Get().carOnScene[1],
                GameScript.Get().carOnScene[2]
            };
            if (SceneChecker.isInGarage())
            {
                localInventory = GameScript.Get().GetComponent<Inventory>();
            }

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
                    playerObject = Instantiate(playerPrefab, player.position.toVector3(),player.rotation.toQuaternion());
                    playerObject.transform.name = player.username;
                    
                }
                MelonLogger.Msg($"Instance set for player: {player.username}");
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
            AssetBundle playerModel = AssetBundle.LoadFromFile(@"Mods\togetherMod\playermodel");
            AssetBundle playerTexture = AssetBundle.LoadFromFile(@"Mods\togetherMod\playertexture");

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
                    
                    DontDestroyOnLoad(playerPrefab);
                    
                    playerModel.Unload(false);
                    playerTexture.Unload(false);
                    MelonLogger.Msg("Loaded player model Succesfully!");
                }
                
            }
            
        }
        
        public static void ReceivePacket()
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
        }
    }
}