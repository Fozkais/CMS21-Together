using Il2Cpp;
using Inventory = CMS21MP.ClientSide.Functionnality.Inventory;

namespace CMS21MP.DataHandle.CL_Handle
{
    public class InventoryHandle
    {
        public static void ItemReceive(ModItem _Moditem, bool status)
        {

            ModItem item = new ModItem();
            Item _item = new Item();
            item.ToGame(_Moditem, _item);
            
           // MelonLogger.Msg($"Received ItemFromServer, ID:{_itemID}, UID:{_itemUID}, Type:{status}");

            if (status)
            {
                if (!Inventory.ItemsUID.Contains(_item.UID) )
                {
                   // MelonLogger.Msg($"Adding Item with UID:{_itemUID}");
                   Inventory.ItemsHandler.Add(_item);
                   Inventory.ItemsUID.Add(_item.UID);
                    MainMod.localInventory.Add(_item);
                }
            }
            else
            {
                if (Inventory.ItemsUID.Contains(_item.UID))
                {
                   // MelonLogger.Msg($"Removing Item with UID:{_itemUID}");
                   Inventory.ItemsHandler.Remove(_item);
                   Inventory.ItemsUID.Remove(_item.UID);
                    MainMod.localInventory.Delete(_item);
                }
            }
            
        }
        
        public static void GroupItemReceive(ModItemGroup _Moditem, bool status)
        {
            ModItemGroup itemGroup = new ModItemGroup();
            GroupItem _item = new GroupItem(); 
            itemGroup.ToGame(_Moditem, _item);
            
            if (status)
            {
                if (!Inventory.ItemsUID.Contains(_item.UID) )
                {
                    // MelonLogger.Msg($"Adding Item with UID:{_itemUID}");
                    Inventory.GroupItemsHandler.Add(_item);
                    Inventory.ItemsUID.Add(_item.UID);
                    MainMod.localInventory.AddGroup(_item);
                }
            }
            else
            {
                if (Inventory.ItemsUID.Contains(_item.UID))
                {
                    // MelonLogger.Msg($"Removing Item with UID:{_itemUID}");
                    Inventory.GroupItemsHandler.Remove(_item);
                    Inventory.ItemsUID.Remove(_item.UID);
                    MainMod.localInventory.DeleteGroup(_item.UID);
                }
            }
        }
    }
}