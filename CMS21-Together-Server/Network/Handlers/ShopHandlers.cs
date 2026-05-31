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
                int price = 0;
                if (packet.IsGroupItem)
                {
                    price = PricingCalculator.GetPrice(packet.GroupItemToBuy);
                }
                else
                {
                    price = PricingCalculator.GetPrice(packet.ItemToBuy);
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
                    
                    Server.SendToClients(packet);
                    Server.SendToClients(state.WorldState);
                }
                else
                {
                    Logger.Warn($"Client {clientId} tried to buy item but didn't have enough money.");
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
                
                // Remove all items matching condition
                for (int i = state.InventoryState.InventoryItems.Count - 1; i >= 0; i--)
                {
                    var item = state.InventoryState.InventoryItems[i];
                    if (item.Condition <= packet.SellCondition)
                    {
                        totalEarned += PricingCalculator.GetPrice(item);
                        state.InventoryState.InventoryItems.RemoveAt(i);
                    }
                }
                
                // Group Items usually don't have a single condition in the same way, but let's check
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
                    }
                }

                if (totalEarned > 0)
                {
                    state.WorldState.Money += totalEarned;
                    Server.SendToClients(packet); // All clients will clear their local inventories below condition
                    Server.SendToClients(state.WorldState);
                }
            }
        }

        [PacketHandler(PacketTypes.ItemsExchange)]
        public static void HandleItemsExchange(long clientId, ItemsExchangePacket packet)
        {
            var state = GameDataManager.CurrentState;
            int totalPrice = 0;
            
            foreach(var item in packet.ItemsToBuy)
            {
                totalPrice += PricingCalculator.GetPrice(item);
            }

            if (state.WorldState.Money >= totalPrice)
            {
                state.WorldState.Money -= totalPrice;
                
                foreach(var item in packet.ItemsToBuy)
                {
                    item.UID = InventoryHandlers.GenerateNewUID();
                    state.InventoryState.InventoryItems.Add(item);
                }

                Server.SendToClients(packet);
                Server.SendToClients(state.WorldState);
            }
            else
            {
                Logger.Warn($"Client {clientId} tried to exchange items but didn't have enough money.");
            }
        }

        [PacketHandler(PacketTypes.RegisterModItem)]
        public static void HandleRegisterModItem(long clientId, RegisterModItemPacket packet)
        {
            if (!GameDatabase.ItemsDatabase.ContainsKey(packet.ItemProperty.ID))
            {
                GameDatabase.ItemsDatabase.Add(packet.ItemProperty.ID, packet.ItemProperty);
                Logger.Info($"Registered new Mod Item dynamically: {packet.ItemProperty.ID} with Price {packet.ItemProperty.Price}");
                // Ideally, here we would save it to a JSON file so it persists after server restart
                // GameDatabase.SaveModdedItems();
            }
        }
    }
}
