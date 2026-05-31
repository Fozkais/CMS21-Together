using System.Collections.Generic;
using CMS21_Together_Core.Data.GameType;
using UnityEngine;

namespace CMS21_Together_Client.Logic
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
                IsExamined = item.IsExamined,
                IsPainted = item.IsPainted,
                PaintType = (ModPaintType)(int)item.PaintType,
                PaintData = new ModPaintData
                {
                    metal = item.PaintData.Metal,
                    roughness = item.PaintData.Roughness,
                    clearCoat = item.PaintData.ClearCoat,
                    normalStrenght = item.PaintData.NormalStrength,
                    fresnel = item.PaintData.Fresnel
                },
                Quality = item.Quality,
                Color = new ModColor 
                { 
                    r = item.Color.GetColor().r, 
                    g = item.Color.GetColor().g, 
                    b = item.Color.GetColor().b, 
                    a = item.Color.GetColor().a 
                },
                WashFactor = item.WashFactor,
                UID = item.UID
            };

            modItem.WheelData = new ModWheelData
            {
                Width = item.WheelData.Width,
                Size = item.WheelData.Size,
                Profile = item.WheelData.Profile,
                ET = item.WheelData.ET,
                IsBalanced = item.WheelData.IsBalanced
            };

            if (item.MountObjectData != null)
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

            if (groupItem.ItemList != null)
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
                IsExamined = modItem.IsExamined,
                IsPainted = modItem.IsPainted,
                PaintType = (PaintType)(int)modItem.PaintType,
                PaintData = new PaintData
                {
                    Metal = modItem.PaintData.metal,
                    Roughness = modItem.PaintData.roughness,
                    ClearCoat = modItem.PaintData.clearCoat,
                    NormalStrength = modItem.PaintData.normalStrenght,
                    Fresnel = modItem.PaintData.fresnel
                },
                Quality = modItem.Quality,
                Color = new CustomColor(new Color(modItem.Color.r, modItem.Color.g, modItem.Color.b, modItem.Color.a)),
                WashFactor = modItem.WashFactor,
                UID = modItem.UID
            };

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
