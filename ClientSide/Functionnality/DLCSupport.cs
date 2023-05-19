using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Il2Cpp;
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

        public Dictionary<string, bool> hasDLC = new Dictionary<string, bool>();
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

        public async void CheckDLC()
        {
            hasDLC.Clear();
            DLCListSet = true;
            await Task.Delay(2000);
            foreach (DLC dlc in Singleton<GameManager>.Instance.PlatformManager.GetDLCSystem().DLCs)
            {
                hasDLC.Add(dlc.Name, dlc.Owned);
            }


            foreach (KeyValuePair<string, bool> dlc in hasDLC)
            {
                MelonLogger.Msg($"DLC: {dlc.Key} | Owned: {dlc.Value}");
            }
        }
    }
}