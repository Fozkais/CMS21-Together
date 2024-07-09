using System.Collections.Generic;
using CMS21Together.ClientSide.Data.Player;
using CMS21Together.Shared;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data;

public class ClientData
{
    public static ClientData Instance;
    public static UserData UserData;

    public ClientData()
    {
        UserData = TogetherModManager.LoadUserData();
        LoadPlayerPrefab();
    }

    public Dictionary<int, UserData> ConnectedClients = new Dictionary<int, UserData>();
    public GameObject playerPrefab;


    public void UpdateClient()
    {
        Movement.SendPosition();
        Rotation.SendRotation();
        
    }
    private void LoadPlayerPrefab()
    {
        var playerBundle = AssetBundle.LoadFromStream(DataHelper.DeepCopy(DataHelper.LoadContent("CMS21Together.Assets.player.assets")));

        if (playerBundle)
        {

            GameObject player = playerBundle.LoadAsset<GameObject>("playerModel");

            GameObject Nplayer = Object.Instantiate(player);
           // Nplayer.AddComponent<ModCharacterController>(); TODO: reimplement
                
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
            MelonLogger.Msg("[ClientData->LoadPlayerPrefab] Loaded player model Succesfully!");
        }
    }
}