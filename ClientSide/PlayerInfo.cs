using System;
using MelonLoader;
using UnityEngine;

namespace CMS21MP.ClientSide
{
    
    [RegisterTypeInIl2Cpp]
    public class PlayerInfo : MonoBehaviour
    {
        public int id;
        public string username;
    }
}