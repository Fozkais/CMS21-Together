using System.Collections.Generic;
using CMS21_Together_Core.Data.GameType;
using UnhollowerBaseLib;

namespace CMS21Together.Data
{
    public static class ItemConverter
    {
        public static ModItem ToModItem(this Item item)
        {
            if (item == null) return null;
            
            var modItem = new ModItem
            {
                ID = item.ID,
                NormalID = item.NormalID,
                Condition = item.Condition,
                ConditionToShow = item.ConditionToShow,
                Dent = item.Dent,
                IsExamined = item.IsExamined,
                IsPainted = item.IsPainted,
                PaintType = (ModPaintType)(int)item.PaintType,
                Quality = item.Quality,
                WashFactor = item.WashFactor,
                UID = item.UID
            };

            modItem.PaintData = new ModPaintData
            {
                metal = item.PaintData.Metal,
                roughness = item.PaintData.Roughness,
                clearCoat = item.PaintData.ClearCoat,
                normalStrenght = item.PaintData.NormalStrength,
                fresnel = item.PaintData.Fresnel
            };

            if (!object.ReferenceEquals(item.Color, null) && !object.ReferenceEquals(item.Color.Color, null))
            {
                if (item.Color.Color.Count == 4)
                {
                    modItem.Color = new ModColor 
                    { 
                        r = item.Color.Color[0], 
                        g = item.Color.Color[1], 
                        b = item.Color.Color[2], 
                        a = item.Color.Color[3] 
                    };
                }
            }

            if (!object.ReferenceEquals(item.TintColor, null) && !object.ReferenceEquals(item.TintColor.Color, null))
            {
                if (item.TintColor.Color.Count == 4)
                {
                    modItem.TintColor = new ModColor 
                    { 
                        r = item.TintColor.Color[0], 
                        g = item.TintColor.Color[1], 
                        b = item.TintColor.Color[2], 
                        a = item.TintColor.Color[3] 
                    };
                }
            }

            modItem.WheelData = new ModWheelData
            {
                Width = item.WheelData.Width,
                Size = item.WheelData.Size,
                Profile = item.WheelData.Profile,
                ET = item.WheelData.ET,
                IsBalanced = item.WheelData.IsBalanced
            };

            if (!object.ReferenceEquals(item.MountObjectData, null))
            {
                modItem.MountObjectData = new ModMountObjectData
                {
                    ParentPath = item.MountObjectData.ParentPath,
                    Condition = item.MountObjectData.Condition,
                    IsStuck = item.MountObjectData.IsStuck
                };
            }

            return modItem;
        }

        public static ModGroupItem ToModGroupItem(this GroupItem groupItem)
        {
            if (groupItem == null) return null;

            var modGroupItem = new ModGroupItem
            {
                ID = groupItem.ID,
                IsNormalGroup = groupItem.IsNormalGroup,
                ItemList = new List<ModItem>(),
                UID = groupItem.UID
            };

            if (!object.ReferenceEquals(groupItem.ItemList, null))
            {
                foreach (var item in groupItem.ItemList)
                {
                    modGroupItem.ItemList.Add(item.ToModItem());
                }
            }

            return modGroupItem;
        }

        public static Item ToGameItem(this ModItem modItem)
        {
            if (modItem == null) return null;
            
            var item = new Item(modItem.ID)
            {
                NormalID = modItem.NormalID,
                Condition = modItem.Condition,
                Dent = modItem.Dent,
                IsExamined = modItem.IsExamined,
                IsPainted = modItem.IsPainted,
                PaintType = (PaintType)(int)modItem.PaintType,
                Quality = modItem.Quality,
                WashFactor = modItem.WashFactor,
                UID = modItem.UID
            };

            item.PaintData = new PaintData
            {
                Metal = modItem.PaintData.metal,
                Roughness = modItem.PaintData.roughness,
                ClearCoat = modItem.PaintData.clearCoat,
                NormalStrength = modItem.PaintData.normalStrenght,
                Fresnel = modItem.PaintData.fresnel
            };

            if (modItem.Color != null)
            {
                CustomColor newColor = new CustomColor();
                newColor.Color = new Il2CppStructArray<float>(4);
                newColor.Color[0] = modItem.Color.r;
                newColor.Color[1] = modItem.Color.g;
                newColor.Color[2] = modItem.Color.b;
                newColor.Color[3] = modItem.Color.a;
                item.Color = newColor;
            }
            
            if (modItem.TintColor != null)
            {
                CustomColor newColor = new CustomColor();
                newColor.Color = new Il2CppStructArray<float>(4);
                newColor.Color[0] = modItem.TintColor.r;
                newColor.Color[1] = modItem.TintColor.g;
                newColor.Color[2] = modItem.TintColor.b;
                newColor.Color[3] = modItem.TintColor.a;
                item.TintColor = newColor;
            }

            // WheelData is a struct so we need to get it, modify it, then set it
            var wheelData = item.WheelData;
            if (modItem.WheelData != null)
            {
                wheelData.Width = modItem.WheelData.Width;
                wheelData.Size = modItem.WheelData.Size;
                wheelData.Profile = modItem.WheelData.Profile;
                wheelData.ET = modItem.WheelData.ET;
                wheelData.IsBalanced = modItem.WheelData.IsBalanced;
            }
            item.WheelData = wheelData;

            if (modItem.MountObjectData != null)
            {
                item.MountObjectData = new MountObjectData
                {
                    ParentPath = modItem.MountObjectData.ParentPath,
                    Condition = modItem.MountObjectData.Condition,
                    IsStuck = modItem.MountObjectData.IsStuck
                };
            }

            return item;
        }

        public static GroupItem ToGameGroupItem(this ModGroupItem modGroupItem)
        {
            if (modGroupItem == null) return null;

            var groupItem = new GroupItem(modGroupItem.ID)
            {
                IsNormalGroup = modGroupItem.IsNormalGroup,
                ItemList = new Il2CppSystem.Collections.Generic.List<Item>(),
                UID = modGroupItem.UID
            };

            if (modGroupItem.ItemList != null)
            {
                foreach (var modItem in modGroupItem.ItemList)
                {
                    groupItem.ItemList.Add(modItem.ToGameItem());
                }
            }

            return groupItem;
        }
    }
}
