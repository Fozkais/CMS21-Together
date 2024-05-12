using System;
using System.Collections.Generic;
using CMS21Together.ClientSide.Data.PlayerData;
using Il2Cpp;

namespace CMS21Together.Shared.Data
{
    [Serializable]
    public class ModNewInventoryData
    {
        public List<ModItem> Items = new List<ModItem>();
        public List<ModGroupItem> GroupItems = new List<ModGroupItem>();
        public int lastUID;

        public ModNewInventoryData(List<Item> _items, List<GroupItem> _groupItems, int _lastUid)
        {
            foreach (Item item in _items)
            {
                this.Items.Add(new ModItem(item));
            }
            foreach (GroupItem groupItem in _groupItems)
            {
                this.GroupItems.Add(new ModGroupItem(groupItem));
            }

            this.lastUID = _lastUid;
        }
    }
}