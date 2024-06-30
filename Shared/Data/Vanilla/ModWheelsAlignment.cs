using System;
using Il2Cpp;

namespace CMS21Together.Shared.Data
{
    [Serializable]
    public struct ModWheelsAlignment
    {
        public float FL;
        public float FR;
        public float RL;
        public float RR;

        public ModWheelsAlignment(WheelsAlignmentData data)
        {
            FL = data.FL;
            FR = data.FR;
            RL = data.RL;
            RR = data.RR;
        }
        
        public WheelsAlignmentData ToGame(ModWheelsAlignment _data)
        {
            WheelsAlignmentData data = new WheelsAlignmentData();
            data.FL = _data.FL;
            data.FR = _data.FR;
            data.RL = _data.RL;
            data.RR = _data.RR;
            return data;
        }
    }
}