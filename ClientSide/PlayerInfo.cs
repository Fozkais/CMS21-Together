using System;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CMS21MP.ClientSide
{
    
    [RegisterTypeInIl2Cpp]
    public class PlayerInfo : MonoBehaviour
    {
        public int id;
        public string username;
        public string activeScene = "garage";
    }
}