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
        
        public bool isDifferent(Color color)
        {
            if (Mathf.Abs(this.r - color.r) > 0.01f)
            {
                if (Mathf.Abs(this.g - color.g) > 0.01f)
                {
                    if (Mathf.Abs(this.b - color.b) > 0.01f)
                    {
                        if (Mathf.Abs(this.a - color.a) > 0.01f)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
    }
}