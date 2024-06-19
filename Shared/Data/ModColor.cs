using System;
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
            r = color.r;
            g = color.g;
            b = color.b;
            a = color.a;
        }
        
        public static Color ToColor(ModColor color)
        {
            return new Color(color.r, color.g, color.b, color.a);
        }
        
        public bool IsDifferent(Color color, float tolerance = 0.00f)
        {
            return Mathf.Abs(this.r - color.r) > tolerance ||
                   Mathf.Abs(this.g - color.g) > tolerance ||
                   Mathf.Abs(this.b - color.b) > tolerance ||
                   Mathf.Abs(this.a - color.a) > tolerance;
        }

    }
}