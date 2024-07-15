using System;
using Il2CppCMS.Containers;

namespace CMS21Together.Shared.Data.Vanilla
{
    [Serializable]
    public class ModTuningData
    {
        public bool IsTuned;
        public short[] Values;
        public float TuningValue;

        public ModTuningData(TuningData data)
        {
            if (data != null)
            {
                IsTuned = data.IsTuned;
                Values = data.Values;
                TuningValue = data.TuningValue;
            }
        }
        
        public TuningData ToGame()
        {
            TuningData data = new TuningData();
            data.IsTuned = this.IsTuned;
            data.Values = this.Values;
            data.TuningValue = this.TuningValue;
            return data;
        }
    }
}