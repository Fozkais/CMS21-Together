using System;
using System.Collections.Generic;
using UnityEngine;

namespace CMS21MP.DataHandle
{
    [Serializable]
    public class C_PartScriptData
    {
       // public GameObject p_handle;
       public int partID;
       public int carLoaderID;
       public string carPartName;
       public int s_indexer;

       public string id;
       public string tunedID;
       public bool isExamined;
       public bool isPainted;
       public C_PaintData paintData;
       public int paintType;
       public C_Color color;
       public int quality;
       public float condition;
       public float dust;
       public C_Bolds bolts;
       public bool unmounted;
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
        public float condition;
        public bool unmounted;
        public string tunedID = string.Empty;
        public bool isTinted;
        public C_Color TintColor;
        public C_Color colors;
        public int paintType;
        public C_PaintData paintData;
        public float conditionStructure;
        public float conditionPaint;
        public string livery;
        public float liveryStrength;
        public bool outsaidRustEnabled;
        public float dent;
        public string additionalString;
        public List<string> mountUnmountWith = new List<string>();
        public int quality;
        public float Dust;
        public float washFactor;
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

    [Serializable]
    public class C_PaintData
    {
        
    }

    [Serializable]
    public class C_Bolds
    {
        
    }


    public enum partType
    {
        engine,
        suspensions,
        part
    }
}