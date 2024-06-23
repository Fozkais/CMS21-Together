using System;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.Shared.Data
{
    [Serializable]
    public class ModColor
    {
        public float r;
        public float g;
        public float b;
        public float a;
        
        public ModColor(float r, float g, float b, float a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }
        
        public ModColor(Color color)
        {
            r = Mathf.Round(color.r * 1000f) / 1000f;
            g = Mathf.Round(color.g * 1000f) / 1000f;
            b = Mathf.Round(color.b * 1000f) / 1000f;
            a = Mathf.Round(color.a * 1000f) / 1000f;
        }
        
        public static Color ToColor(ModColor color)
        {
            return new Color(color.r, color.g, color.b, color.a);
        }
        
        public bool IsDifferent(Color color, float tolerance = 0.03f)
        {
            return Mathf.Abs(this.r - color.r) > tolerance ||
                   Mathf.Abs(this.g - color.g) > tolerance ||
                   Mathf.Abs(this.b - color.b) > tolerance ||
                   Mathf.Abs(this.a - color.a) > tolerance;
        }

    }
}