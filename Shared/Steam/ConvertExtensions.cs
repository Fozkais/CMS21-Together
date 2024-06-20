using System.Text;

namespace CMS21Together.Shared.Steam
{
    public static class ConvertExtensions
    {
        public static string ToBase36(this ulong value)
        {
            string symbols = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            StringBuilder result = new StringBuilder();

            while (value > 0)
            {
                result.Insert(0, symbols[(int)(value % 36)]); // Cast to int for remainder
                value /= 36;
            }

            return result.ToString();
        }
    }
}