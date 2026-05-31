using System.Linq;
using CMS21_Together_Core;
using CMS21_Together_Core.Network;
using CMS21_Together_Core.Network.Packets;
using CMS21_Together_Server.Data;
using CMS21_Together_Server.Log;

namespace CMS21_Together_Server.Network.Handlers
{
    public static class ShopHandlers
    {
        [PacketHandler(PacketTypes.ShopAction)]
        public static void HandleShopAction(long clientId, ShopActionPacket packet)
        {
            var state = GameDataManager.CurrentState;
            
            if (packet.Action == ShopActionType.Buy)
            {
                try
                {
                    int price = 0;
                    if (packet.IsGroupItem)
                    {
                        price = PricingCalculator.GetPrice(packet.GroupItemToBuy);
                        Logger.Debug($"[ShopHandlers] Calculating price for GroupItem: {packet.GroupItemToBuy.ID}. Price={price}");
                    }
                    else
                    {
                        price = PricingCalculator.GetPrice(packet.ItemToBuy);
                        Logger.Debug($"[ShopHandlers] Calculating price for Item: {packet.ItemToBuy.ID}. Price={price}");
                    }

                    if (state.WorldState.Money >= price)
                    {
                        state.WorldState.Money -= price;
                        
                        if (packet.IsGroupItem)
                        {
                            packet.GroupItemToBuy.UID = InventoryHandlers.GenerateNewUID();
                            state.InventoryState.InventoryGroupItems.Add(packet.GroupItemToBuy);
                        }
                        else
                        {
                            packet.ItemToBuy.UID = InventoryHandlers.GenerateNewUID();
                            state.InventoryState.InventoryItems.Add(packet.ItemToBuy);
                        }
                        
                        Logger.Debug($"[ShopHandlers] Buy successful. Client {clientId} bought item. New Money: {state.WorldState.Money}");
                        Server.SendToClients(packet);
                        Server.SendToClients(state.WorldState);
                    }
                    else
                    {
                        Logger.Warn($"Client {clientId} tried to buy item but didn't have enough money. (Has {state.WorldState.Money}, needs {price})");
                    }
                }
                catch (System.Exception ex)
                {
                    Logger.Error($"[ShopHandlers] Error handling Buy action: {ex.Message}\n{ex.StackTrace}");
                }
            }
            else if (packet.Action == ShopActionType.SellSingle)
            {
                // Find item
                var item = state.InventoryState.InventoryItems.FirstOrDefault(i => i.UID == packet.ItemUID);
                if (item != null)
                {
                    int price = PricingCalculator.GetPrice(item);
                    state.InventoryState.InventoryItems.Remove(item);
                    state.WorldState.Money += price;
                    
                    Server.SendToClients(packet);
                    Server.SendToClients(state.WorldState);
                }
                else
                {
                    var groupItem = state.InventoryState.InventoryGroupItems.FirstOrDefault(i => i.UID == packet.ItemUID);
                    if (groupItem != null)
                    {
                        int price = PricingCalculator.GetPrice(groupItem);
                        state.InventoryState.InventoryGroupItems.Remove(groupItem);
                        state.WorldState.Money += price;
                        
                        Server.SendToClients(packet);
                        Server.SendToClients(state.WorldState);
                    }
                }
            }
            else if (packet.Action == ShopActionType.SellCondition)
            {
                int totalEarned = 0;
                
                for (int i = state.InventoryState.InventoryItems.Count - 1; i >= 0; i--)
                {
                    var item = state.InventoryState.InventoryItems[i];
                    if (item.Condition <= packet.SellCondition)
                    {
                        totalEarned += PricingCalculator.GetPrice(item);
                        state.InventoryState.InventoryItems.RemoveAt(i);
                        
                        Server.SendToClients(new InventoryItemActionPacket 
                        { 
                            Action = ItemActionType.Remove, 
                            Item = item 
                        });
                    }
                }
                
                for (int i = state.InventoryState.InventoryGroupItems.Count - 1; i >= 0; i--)
                {
                    var groupItem = state.InventoryState.InventoryGroupItems[i];
                    bool allBelow = true;
                    foreach(var subItem in groupItem.ItemList)
                    {
                        if (subItem.Condition > packet.SellCondition)
                        {
                            allBelow = false;
                            break;
                        }
                    }
                    
                    if (allBelow)
                    {
                        totalEarned += PricingCalculator.GetPrice(groupItem);
                        state.InventoryState.InventoryGroupItems.RemoveAt(i);
                        
                        Server.SendToClients(new InventoryGroupItemActionPacket 
                        { 
                            Action = ItemActionType.Remove, 
                            GroupItem = groupItem 
                        });
                    }
                }

                if (totalEarned > 0)
                {
                    state.WorldState.Money += totalEarned;
                    Server.SendToClients(state.WorldState);
                }
            }
        }

        [PacketHandler(PacketTypes.ItemsExchange)]
        public static void HandleItemsExchange(long clientId, ItemsExchangePacket packet)
        {
            var state = GameDataManager.CurrentState;
            float locationMod = packet.IsJunkyard ? 0.47f : 0.50f;
            
            float discount = 0f;
            if (state.GarageState.PlayerUpgradeLevels.TryGetValue("shop_discount", out bool[] levels))
            {
                int count = 0;
                foreach (bool unlocked in levels)
                {
                    if (unlocked) count++;
                }
                discount = count * 0.05f; // 5% per level
            }

            int totalCost = 0;
            foreach (var item in packet.ItemsToBuy)
            {
                totalCost += PricingCalculator.GetPrice(item, locationMod);
            }
            int discountNum = (int)(totalCost * discount);
            totalCost -= discountNum;

            if (state.WorldState.Money >= totalCost)
            {
                state.WorldState.Money -= totalCost;
                foreach (var item in packet.ItemsToBuy)
                {
                    item.UID = InventoryHandlers.GenerateNewUID();
                    state.InventoryState.InventoryItems.Add(item);
                }
                Server.SendToClients(packet);
                Server.SendToClients(state.WorldState);
                Logger.Debug($"[ShopHandlers] ItemsExchange successful for client {clientId}. Cost={totalCost}. Mod={locationMod}, Discount={discount}");
            }
            else
            {
                Logger.Debug($"[ShopHandlers] ItemsExchange failed for client {clientId}. Not enough money! (Cost={totalCost}, Money={state.WorldState.Money})");
            }
        }

        [PacketHandler(PacketTypes.RegisterModItem)]
        public static void HandleRegisterModItem(long clientId, RegisterModItemPacket packet)
        {
            if (!GameDatabase.ItemsDatabase.ContainsKey(packet.ItemProperty.ID))
            {
                GameDatabase.ItemsDatabase.Add(packet.ItemProperty.ID, packet.ItemProperty);
                Logger.Info($"Registered new Mod Item dynamically: {packet.ItemProperty.ID} with Price {packet.ItemProperty.Price}");
                GameDatabase.SaveModdedItems();
            }
        }
    }
}
