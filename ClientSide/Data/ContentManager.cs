using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CMS21Together.Shared;
using Il2Cpp;
using Il2CppInterop.Runtime;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using Object = Il2CppSystem.Object;

namespace CMS21Together.ClientSide.Data
{
    [RegisterTypeInIl2Cpp]
    public class ContentManager : MonoBehaviour
    {
        public string gameVersion { get; private set; }
        public IReadOnlyDictionary<string, bool> OwnedContents { get; private set; }
        
        public static ContentManager Instance;
        
        public void Initialize()
        {
            if(OwnedContents != null) return;
            
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                MelonLogger.Msg("Instance already exists, destroying object!");
                Destroy(this);
            }
            
            GetGameVersion();
            CheckContent();
        }

        protected void GetGameVersion()
        {
            if(OwnedContents != null) return;
            
            gameVersion = GameObject.Find("GameVersion").GetComponent<Text>().text;
        }
        
        protected void CheckContent()
        {
            if(OwnedContents != null) return;
            
            OwnedContents = new ReadOnlyDictionary<string, bool>(ApiCalls.CallAPIMethod3());
        }

        public void LoadCustomlogo()
        {
            Stream stream = DataHelper.LoadContent("CMS21Together.Assets.cms21TogetherLogo.png");

            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, (int)stream.Length);

            Object[] textures = FindObjectsOfTypeIncludingAssets(Il2CppType.Of<Texture2D>());
            if (textures.Length < 1) { return; }
            
            for (var index = 0; index < textures.Length; index++)
            {
                Texture2D texture = textures[index].TryCast<Texture2D>();

                if (texture != null)
                {
                    if (texture.name == "cms21Logo")
                    {
                        ImageConversion.LoadImage(texture, buffer);
                    }
                }
            }
        }
        
    }
}