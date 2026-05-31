using System;
using CMS21_Together_Core.Data.Enum;
using CMS21_Together_Core.Data.GameType;

namespace CMS21_Together_Server.Data
{
    public static class PricingCalculator
    {
        public static int GetPrice(ModItem item, float mod = 1f)
        {
            if (item.ID == "LicensePlate")
            {
                return (int)Math.Round(100f * item.ConditionToShow);
            }

            if (!GameDatabase.ItemsDatabase.TryGetValue(item.ID, out PartProperty prop))
            {
                Log.Logger.Warn($"PricingCalculator: Item {item.ID} is not in the database! Using fallback pricing.");
                float fallbackPrice = 100f * item.ConditionToShow;
                fallbackPrice = GetPriceWithQualityMod(item.Quality, fallbackPrice);
                fallbackPrice *= mod;
                fallbackPrice += (float)item.Quality;
                return Math.Max(1, (int)Math.Round(fallbackPrice));
            }

            int size = item.WheelData?.Size ?? 0;
            int width = item.WheelData?.Width ?? 0;
            int profile = item.WheelData?.Profile ?? 0;
            int et = item.WheelData?.ET ?? 0;

            SpecialGroup specialGroup = prop.SpecialGroup;
            int basePrice = prop.Price;
            float num;

            if (specialGroup != SpecialGroup.Tire)
            {
                if (specialGroup != SpecialGroup.Rim)
                {
                    num = (float)basePrice;
                }
                else
                {
                    num = (float)GetRimPrice(prop.Price, size, et);
                }
            }
            else
            {
                num = (float)GetTirePrice(prop.Price, width, profile, size);
            }

            num *= item.ConditionToShow;
            num = GetPriceWithQualityMod(item.Quality, num);
            num *= mod;
            num += (float)item.Quality;

            int num2 = (int)Math.Round(num);
            if (num2 < 1)
            {
                num2 = 1;
            }
            return num2;
        }

        public static int GetPrice(ModGroupItem groupItem, float mod = 1f)
        {
            int total = 0;
            if (groupItem.ItemList != null)
            {
                foreach (var item in groupItem.ItemList)
                {
                    total += GetPrice(item, 1f); // The game applies the mod only at the end? Actually the game loop calls GetPrice(item, 1f) then sums.
                }
            }
            return total;
        }

        public static int GetRimPrice(int basePrice, int rimSize, int et)
        {
            return (int)((float)basePrice / 100f) * ((Math.Max(rimSize, 12) - 11) * 20) + 5 * et;
        }

        public static int GetTirePrice(int basePrice, int tireWidth, int tireProfile, int tireSize)
        {
            return (int)Math.Round((float)basePrice / 100f * (float)(Math.Max(tireWidth, 135) - 135 + tireProfile + (Math.Max(tireSize, 12) - 12) * 5));
        }

        public static float GetPriceWithQualityMod(int quality, float finalPrice)
        {
            if (quality > 0)
            {
                finalPrice += finalPrice * ((float)(quality * 2) * 0.01f);
            }
            return finalPrice;
        }
    }
}
