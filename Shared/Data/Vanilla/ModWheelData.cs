using System;
using Il2Cpp;

namespace CMS21Together.Shared.Data
{
    [Serializable]
    public class ModWheelData
    {
        public int ET;
        public bool IsBalanced;
        public int Profile;
        public int Size;
        public int Width;

        public ModWheelData(WheelData itemWheelData)
        {
            this.ET = itemWheelData.ET;
            this.IsBalanced = itemWheelData.IsBalanced;
            this.Profile = itemWheelData.Profile;
            this.Size = itemWheelData.Size;
            this.Width = itemWheelData.Width;
        }

        public ModWheelData()
        {
        }

        public WheelData ToGame(ModWheelData itemWheelData)
        {
            WheelData data = new WheelData();
            data.ET = itemWheelData.ET;
            data.IsBalanced = itemWheelData.IsBalanced;
            data.Profile = itemWheelData.Profile;
            data.Size = itemWheelData.Size;
            data.Width = itemWheelData.Width;
            return data;
        }
    }
}