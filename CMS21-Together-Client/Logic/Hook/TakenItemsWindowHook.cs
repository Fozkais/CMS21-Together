using System.Collections.Generic;
using CMS.UI.Windows;
using CMS21_Together_Core.Data.Enum;
using CMS21_Together_Core.Data.GameType;
using CMS21_Together_Core.Network.Packets;
using CMS21Together.Data;
using CMS21Together.Network;
using HarmonyLib;
using MelonLoader;

namespace CMS21Together.Logic.Hook
{
    [HarmonyPatch]
    public static class TakenItemsWindowHook
    {
        [HarmonyPatch(typeof(TakenItemsWindow), "BuyPartsAction")]
        [HarmonyPrefix]
        public static bool BuyPartsActionPrefix(TakenItemsWindow __instance)
        {
            if (Client.Instance.IsConnected)
            {
                int priceAfterDiscount = __instance.priceAfterDiscount;

                if (GlobalData.PlayerMoney < priceAfterDiscount)
                {
                    UIManager.Get().ShowInfoWindow("GUI_BrakKasy");
                    return false;
                }

                var items = Singleton<GameManager>.Instance.TempInventory.GetListOfItems();
                if (items == null || items.Count == 0)
                {
                    MelonLogger.Msg("[DEBUG] TakenItemsWindow: No items to buy.");
                    return false;
                }

                var packet = new ItemsExchangePacket
                {
                    IsJunkyard = GameScript.Get().CurrentSceneType == SceneType.Junkyard,
                    ItemsToBuy = new List<ModItem>()
                };
                MelonLogger.Warning($"[DEBUG] [BuyPartsActionPrefix] List size:{items.Count} , _items size:{items._items.Count}.");
                foreach (var baseItem in items)
                {
                    var item = baseItem.TryCast<Item>();
                    if (item != null)
                    {
                        packet.ItemsToBuy.Add(item.ToModItem());
                    }
                    else
                    {
                        MelonLogger.Warning("[DEBUG] [BuyPartsActionPrefix] Failed to cast baseItem to Item.");
                        var groupItem = baseItem.TryCast<GroupItem>();
                        if (groupItem != null)
                        {
                            // Technically ItemsExchangePacket doesn't have a list for ModGroupItem right now.
                            // We can just add the individual items if needed, or leave it as is if Barn/Junkyard doesn't spawn GroupItems.
                            // Barn/Junkyard parts are always single Items.
                        }
                    }
                }

                // Send the packet to the server to handle the transaction and broadcast the new inventory items
                Client.Instance.Send(packet);

                // We block the local money deduction and let the server handle it.
                // We clear the local TempInventory so GarageLoader doesn't duplicate the items. Actually unneeded since we do total inventory resync on load
                Singleton<GameManager>.Instance.TempInventory.ClearListOfItems();

                // Trigger the scene load back to the garage (simulating the native game logic)
                Singleton<GameManager>.Instance.InputManager.ChangeInput(false, false, false);
                NotificationCenter.Get().SelectSceneToLoad("garage", SceneType.Garage, true);

                return false;
            }

            return true;
        }
    }
}
