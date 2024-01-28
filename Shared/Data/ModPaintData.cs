using System;
using Il2Cpp;

namespace CMS21Together.Shared.Data
{
    [Serializable]
    public class ModPaintData
    {
        public float metal;
        public float roughness;
        public float clearCoat;
        public float normalStrenght;
        public float fresnel;
        public float p_metal;
        public float p_roughness;
        public float p_clearCoat;
        public float p_normalStrenght;
        public float p_fresnel;

        public ModPaintData() { }

        public ModPaintData(PaintData data)
        {
            this.metal = data.metal;
            this.roughness = data.roughness;
            this.clearCoat = data.clearCoat;
            this.normalStrenght = data.normalStrength;
            this.fresnel = data.fresnel;

            this.p_metal = data.Metal;
            this.p_roughness = data.Roughness;
            this.p_clearCoat = data.ClearCoat;
            this.p_normalStrenght = data.NormalStrength;
            this.p_fresnel = data.Fresnel;
        }
        
        public PaintData ToGame(ModPaintData data)
        {
            PaintData paintData = new PaintData();
            paintData.metal = data.metal;
            paintData.roughness = data.roughness;
            paintData.clearCoat = data.clearCoat;
            paintData.normalStrength = data.normalStrenght;
            paintData.fresnel = data.fresnel;

            paintData.Metal = data.p_metal;
            paintData.Roughness = data.p_roughness;
            paintData.ClearCoat = data.p_clearCoat;
            paintData.NormalStrength = data.p_normalStrenght;
            paintData.Fresnel = data.p_fresnel;
            return paintData;
        }
    }
}