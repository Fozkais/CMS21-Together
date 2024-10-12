using MelonLoader;
using Steamworks;
using UnityEngine;

namespace CMS21Together.Shared
{
    [RegisterTypeInIl2Cpp]
    public class SteamManager : MonoBehaviour
    {
        public static SteamManager Instance;
        public SteamClientData clientData;
        
        public SteamManager()
        {
            //SteamClient.Init(1190000);
            //SteamNetworkingUtils.InitRelayNetworkAccess();

           // clientData = new SteamClientData();
        }
    }

    public struct SteamClientData
    {
      /*  public string PlayerName { get; set; }
        public SteamId PlayerSteamId { get; set; }
        public string playerSteamIdString;

        public SteamClientData()
        {
            PlayerName = SteamClient.Name;
            PlayerSteamId = SteamClient.SteamId;
            playerSteamIdString = PlayerSteamId.ToString();
        }*/
    }
}