using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Il2CppCMS.MainMenu.Logic;
using MelonLoader;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

namespace CMS21MP.ClientSide.Functionnality
{
    [RegisterTypeInIl2Cpp]
    public class DLCSupport : MonoBehaviour
    {

        protected Dictionary<string, bool> DlcTempDictionary = new Dictionary<string, bool>();
        public ReadOnlyDictionary<string, bool> hasDLC;
        protected bool DlcSet;
        public bool DLCListSet  { get { return DlcSet; } set { if (!DlcSet) { DlcSet = value; } } }

        public static DLCSupport instance;
        
        public void Initialize()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                MelonLogger.Msg("Instance already exists, destroying object!");
                Destroy(this);
            }
        }

        public  void CheckDLC()
        {
            if (!DLCListSet)
            {
                SteamAPI.Init();
                if (SteamAPI.Init())
                {
                    DlcTempDictionary.Add("Nissan DLC", SteamApps.BIsDlcInstalled(new AppId_t(1685720)));
                    DlcTempDictionary.Add("Ford DLC", SteamApps.BIsDlcInstalled(new AppId_t(2282030)));
                    DlcTempDictionary.Add("Mercedes DLC", SteamApps.BIsDlcInstalled(new AppId_t(2112232)));
                    DlcTempDictionary.Add("Drag Racing DLC", SteamApps.BIsDlcInstalled(new AppId_t(2112231)));
                    DlcTempDictionary.Add("Aston Martin DLC", SteamApps.BIsDlcInstalled(new AppId_t(2112230)));
                    DlcTempDictionary.Add("Mazda Remastered DLC", SteamApps.BIsDlcInstalled(new AppId_t(2085260)));
                    DlcTempDictionary.Add("Lotus Remastered DLC", SteamApps.BIsDlcInstalled(new AppId_t(1981550)));
                    DlcTempDictionary.Add("Hot Rod Remastered DLC", SteamApps.BIsDlcInstalled(new AppId_t(1931780)));
                    DlcTempDictionary.Add("Land Rover DLC", SteamApps.BIsDlcInstalled(new AppId_t(1748991)));
                    DlcTempDictionary.Add("Pagani Remastered DLC", SteamApps.BIsDlcInstalled(new AppId_t(1773800)));
                    DlcTempDictionary.Add("Porsche Remastered DLC", SteamApps.BIsDlcInstalled(new AppId_t(1773801)));
                    DlcTempDictionary.Add("Jaguar DLC", SteamApps.BIsDlcInstalled(new AppId_t(1748990)));
                    DlcTempDictionary.Add("Electric Car DLC", SteamApps.BIsDlcInstalled(new AppId_t(1685721)));
                    hasDLC = new ReadOnlyDictionary<string, bool>(DlcTempDictionary);
                    DlcTempDictionary.Clear();
                    DLCListSet = true;
                }
                else
                {
                    MelonLogger.Msg("Need to Initialize Steam to get DLC with Multiplayer Mod");
                }
            }

            foreach (KeyValuePair<string, bool> dlc in hasDLC)
            {
                MelonLogger.Msg($"DLC: {dlc.Key} | Owned: {dlc.Value}");
            }
        }
    }
}