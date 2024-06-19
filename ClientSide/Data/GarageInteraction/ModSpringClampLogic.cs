using CMS21Together.Shared.Data;
using HarmonyLib;
using Il2Cpp;

namespace CMS21Together.ClientSide.Data.GarageInteraction
{
    [HarmonyPatch]
    public static class ModSpringClampLogic
    {
        public static bool listenToSpringClampLogic = true;
        
        [HarmonyPatch(typeof(SpringClampLogic), nameof(SpringClampLogic.SetGroupOnSpringClamp))]
        [HarmonyPrefix]
        public static void SetEngineGroupPatch(GroupItem groupItem, bool instant, bool mount)
        {
            if(!Client.Instance.isConnected) return;

            if (listenToSpringClampLogic)
            {
                ModGroupItem item = new ModGroupItem(groupItem);
                ClientSend.SendGroupOnSpringClamp(item, instant, mount);
            }
        }
        
        [HarmonyPatch(typeof(SpringClampLogic), nameof(SpringClampLogic.ClearSpringClamp))]
        [HarmonyPrefix]
        public static void ClearSpringPatch()
        {
            if(!Client.Instance.isConnected) return;

            if (listenToSpringClampLogic)
            {
                ClientSend.SendClearSpring();
            }
        }
        
    }
}