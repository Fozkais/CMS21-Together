using CMS.UI;
using CMS.UI.Windows;
using CMS21_Together_Core;
using CMS21_Together_Core.Logging;
using CMS21_Together_Core.Network;
using CMS21_Together_Core.Network.Packets;
using CMS21Together.Data;

namespace CMS21Together.Network.Handlers
{
    public static class InventoryHandlers
    {
        private static System.Collections.Generic.Queue<InventorySyncPacket> syncQueue = new System.Collections.Generic.Queue<InventorySyncPacket>();
        private static bool isProcessingSync = false;
        
        public static bool IgnoreInventoryHooks = false;

        [PacketHandler(PacketTypes.InventoryData)]
        public static void HandleInventorySync(long clientId, InventorySyncPacket packet)
        {
            syncQueue.Enqueue(packet);
            if (!isProcessingSync)
            {
                isProcessingSync = true;
                MelonLoader.MelonCoroutines.Start(ProcessSyncQueue());
            }
        }

        private static System.Collections.IEnumerator ProcessSyncQueue()
        {
            // Wait for the garage state to be fully synced and ready
            while (!ClientData.IsGarageStateSynced)
            {
                yield return new UnityEngine.WaitForSeconds(0.1f);
            }

            while (syncQueue.Count > 0)
            {
                var packet = syncQueue.Dequeue();

                IgnoreInventoryHooks = true;

                if (packet.IsFirstBatch)
                {
                    // Clear all local inventories first to prevent duplication
                    Singleton<GameManager>.Instance.Inventory.DeleteAllInventory();
                    
                    var warehouse = Singleton<GameManager>.Instance.Warehouse;
                    if (warehouse != null)
                    {
                        var allWhItems = warehouse.SortItemsForCategory(global::SortType.ByAlphabetAsc, CMS.UI.Logic.InventoryCategories.All);
                        if (allWhItems != null)
                        {
                            foreach (var baseItem in allWhItems)
                            {
                                var i = baseItem.TryCast<Item>();
                                if (i != null) warehouse.Delete(i);
                                else
                                {
                                    var gi = baseItem.TryCast<GroupItem>();
                                    if (gi != null) warehouse.Delete(gi);
                                }
                            }
                        }
                    }

                    Log.Debug("[InventoryHandlers] Cleared local inventory and warehouse for full sync.");
                }

                int count = 0;
                if (packet.InventoryItems != null)
                {
                    foreach (var item in packet.InventoryItems)
                    {
                        Singleton<GameManager>.Instance.Inventory.Add(item.ToGameItem());
                        count++;
                    }
                }
                if (packet.InventoryGroupItems != null)
                {
                    foreach (var group in packet.InventoryGroupItems)
                    {
                        Singleton<GameManager>.Instance.Inventory.AddGroup(group.ToGameGroupItem());
                        count++;
                    }
                }
                if (packet.WarehouseItems != null)
                {
                    foreach (var item in packet.WarehouseItems)
                    {
                        Singleton<GameManager>.Instance.Warehouse.Add(item.ToGameItem());
                        count++;
                    }
                }
                if (packet.WarehouseGroupItems != null)
                {
                    foreach (var group in packet.WarehouseGroupItems)
                    {
                        Singleton<GameManager>.Instance.Warehouse.Add(group.ToGameGroupItem());
                        count++;
                    }
                }
                
                IgnoreInventoryHooks = false;
                
                Log.Debug($"[InventoryHandlers] Received batch containing {count} items. LastBatch={packet.IsLastBatch}");

                if (packet.IsLastBatch)
                {
                    Log.Success("[InventoryHandlers] Inventory sync complete!");
                    ClientData.IsInventorySynced = true;
                    RefreshInventoryWindow();
                    RefreshWarehouseWindow();
                }

                yield return null; // wait a frame between batches
            }

            isProcessingSync = false;
        }

        [PacketHandler(PacketTypes.InventoryItemAction)]
        public static void HandleInventoryItemAction(long clientId, InventoryItemActionPacket packet)
        {
            IgnoreInventoryHooks = true;
            try
            {
                if (packet.Action == ItemActionType.Add)
                {
                    Singleton<GameManager>.Instance.Inventory.Add(packet.Item.ToGameItem());
                }
                else if (packet.Action == ItemActionType.Remove)
                {
                    var item = Singleton<GameManager>.Instance.Inventory.GetItem(packet.Item.UID);
                    if (item != null)
                        Singleton<GameManager>.Instance.Inventory.Delete(item);
                }
            }
            finally
            {
                IgnoreInventoryHooks = false;
            }
            
            RefreshInventoryWindow();
        }

        [PacketHandler(PacketTypes.InventoryGroupItemAction)]
        public static void HandleInventoryGroupItemAction(long clientId, InventoryGroupItemActionPacket packet)
        {
            IgnoreInventoryHooks = true;
            try
            {
                if (packet.Action == ItemActionType.Add)
                {
                    Singleton<GameManager>.Instance.Inventory.AddGroup(packet.GroupItem.ToGameGroupItem());
                }
                else if (packet.Action == ItemActionType.Remove)
                {
                    Singleton<GameManager>.Instance.Inventory.DeleteGroup(packet.GroupItem.UID);
                }
            }
            finally
            {
                IgnoreInventoryHooks = false;
            }
            
            RefreshInventoryWindow();
        }

        [PacketHandler(PacketTypes.WarehouseAction)]
        public static void HandleWarehouseAction(long clientId, WarehouseActionPacket packet)
        {
            IgnoreInventoryHooks = true;
            try
            {
                if (packet.ToWarehouse)
                {
                    // Remove from Inventory, Add to Warehouse
                    if (packet.IsGroupItem)
                    {
                        Singleton<GameManager>.Instance.Inventory.DeleteGroup(packet.GroupItem.UID); 
                        Singleton<GameManager>.Instance.Warehouse.Add(packet.GroupItem.ToGameGroupItem());
                    }
                    else
                    {
                        var item = Singleton<GameManager>.Instance.Inventory.GetItem(packet.Item.UID);
                        if (item != null)
                            Singleton<GameManager>.Instance.Inventory.Delete(item);
                        Singleton<GameManager>.Instance.Warehouse.Add(packet.Item.ToGameItem());
                    }
                }
                else
                {
                    // From Warehouse to Inventory
                    if (packet.IsGroupItem)
                    {
                        var grp = packet.GroupItem.ToGameGroupItem();
                        Singleton<GameManager>.Instance.Warehouse.Delete(grp);
                        Singleton<GameManager>.Instance.Inventory.AddGroup(grp);
                    }
                    else
                    {
                        var item = packet.Item.ToGameItem();
                        Singleton<GameManager>.Instance.Warehouse.Delete(item);
                        Singleton<GameManager>.Instance.Inventory.Add(item);
                    }
                }
            }
            finally
            {
                IgnoreInventoryHooks = false;
            }
            
            RefreshInventoryWindow();
            RefreshWarehouseWindow();
        }

        public static void RefreshInventoryWindow()
        {
            try
            {
                if (WindowManager.Instance != null)
                {
                    var invWindow = WindowManager.Instance.GetWindowByID<InventoryWindow>(WindowID.Inventory);
                    if (invWindow != null && invWindow.isActive)
                    {
                        invWindow.Refresh(true);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.Error($"[InventoryHandlers] Error refreshing InventoryWindow: {ex.Message}");
            }
        }
        
        public static void RefreshWarehouseWindow()
        {
            try
            {
                if (WindowManager.Instance != null)
                {
                    var whWindow = WindowManager.Instance.GetWindowByID<WarehouseWindow>(WindowID.Warehouse);
                    if (whWindow != null && whWindow.isActive)
                    {
                        whWindow.Refresh(true);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.Error($"[InventoryHandlers] Error refreshing WarehouseWindow: {ex.Message}");
            }
        }
    }
}
