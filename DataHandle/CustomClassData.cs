using System;
using System.Collections.Generic;
using UnityEngine;

namespace CMS21MP.DataHandle
{
    [Serializable]
    public class C_PartsData
    {
       // public GameObject p_handle;
       public int partID;
       public int carLoaderID;
        public string name;
        public float positionX;
        public float positionY;
        public float positionZ;
        public float rotationX;
        public float rotationY;
        public float rotationZ;
        public float scale;
        public bool reflection;
    }

    [Serializable]
    public class C_carPartsData
    {
        // public GameObject handle;
       public int carPartID;
       public int carLoaderID;
        public string name = string.Empty;
        public bool switched;
        public bool inprogress;
        public float condition = 1f;
        public bool unmounted;
        public string tunedID = string.Empty;
        public C_Color colors;
        public int paintType;
        public float conditionStructure;
        public float conditionPaint;
        public string livery;
        public float liveryStrength;
        public bool outsaidRustEnabled;
        public string adtionalString;
        public List<string> mountUnmountWith = new List<string>();
        public int quality;
    }
    
    [Serializable]
    public class carData
    {
        public int clientID;
        public bool status;
        
        public int carLoaderID;
        public string carID;
        public int carPosition;

        public C_Color carColor;
    }

    [Serializable]
    public class C_Color
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public C_Color(float _r, float _g, float _b, float _a)
        {
            r = _r;
            g = _g;
            b = _b;
            a = _a;
        }
        public C_Color(Color color)
        {
            r = color.r;
            g = color.g;
            b = color.b;
            a = color.a;
        }
    }
}