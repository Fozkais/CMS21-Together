using System;
using Il2CppInterop.Runtime.Injection;
using MelonLoader;
using UnityEngine;

namespace CMS21MP
{
    
    [RegisterTypeInIl2Cpp]
    public class PlayerManager : MonoBehaviour
    {
        public int id;
        public string username;
    }
}