using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MelonLoader;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

namespace CMS21Together.Shared.Steam
{
    [RegisterTypeInIl2Cpp]
    public class ModSteamManager : MonoBehaviour
    {
        public static ModSteamManager Instance;
        public SteamClientData clientData;
        
        public void Initialize()
        {
            Instance = this;
            SteamClient.Init(1190000);
            SteamNetworkingUtils.InitRelayNetworkAccess();

            clientData = new SteamClientData();
        }
    }

    public struct SteamClientData
    {
        public string PlayerName { get; set; }
        public SteamId PlayerSteamId { get; set; }
        public string playerSteamIdString;

        public SteamClientData()
        {
            PlayerName = SteamClient.Name;
            PlayerSteamId = SteamClient.SteamId;
            playerSteamIdString = PlayerSteamId.ToString();
        }
    }
}