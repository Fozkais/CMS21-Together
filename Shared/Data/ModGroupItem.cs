using System;
using System.Collections.Generic;
using Il2Cpp;
using MelonLoader;

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
            if (item != null)
            {
                this.IsNormalGroup = item.IsNormalGroup;
                this.ItemList = new List<ModItem>();
                if (item.ItemList != null)
                {
                    foreach (Item _item in item.ItemList)
                    {
                        if (_item != null)
                        {
                            this.ItemList.Add(new ModItem(_item));
                        }
                    }
                }

                this.Size = item.Size;
                this.ID = item.ID;
                this.UID = item.UID;
            }
            else
            {
                MelonLogger.Msg("Error: GroupItem is null in ModGroupItem constructor.");
            }
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
        
        public GroupItem ToGame()
        {
            GroupItem original = new GroupItem();
            
            original.IsNormalGroup = this.IsNormalGroup;
            original.ItemList = Convert(this.ItemList);
            original.Size = this.Size;
            original.ID = this.ID;
            original.UID = this.UID;

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