using System;
using System.Collections.Generic;
using Il2Cpp;

namespace CMS21Together.Shared.Data
{
    [Serializable]
    public class ModGroupItem
    {
        public int Index;
        
        public bool IsNormalGroup;
        public List<ModItem> ItemList = new List<ModItem>();
        public float Size;
        public string ID;
        public long UID;

        public ModGroupItem()
        {
        }

        public ModGroupItem(GroupItem item)
        {
            this.IsNormalGroup = item.IsNormalGroup;
            foreach (Item _item in item.ItemList)
            {
                this.ItemList.Add(new ModItem(_item));
            }

            this.Size = item.Size;
            this.ID = item.ID;
            this.UID = item.UID;
        }

        public GroupItem ToGame(ModGroupItem ModGroupItem)
        {
            GroupItem original = new GroupItem();
            
            original.IsNormalGroup = ModGroupItem.IsNormalGroup;
            original.ItemList = Convert(ModGroupItem.ItemList);
            original.Size = ModGroupItem.Size;
            original.ID = ModGroupItem.ID;
            original.UID = ModGroupItem.UID;

            return original;
        }

        public Il2CppSystem.Collections.Generic.List<Item> Convert(List<ModItem> items)
        {
            Il2CppSystem.Collections.Generic.List<Item> FinalItem = new Il2CppSystem.Collections.Generic.List<Item>();
            foreach (var item in items)
            {
                FinalItem.Add(item.ToGame(item));
            }

            return FinalItem;
        }
    }
}