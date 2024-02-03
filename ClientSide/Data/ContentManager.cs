using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using CMS21Together.Shared;
using Il2Cpp;
using Il2CppInterop.Runtime;
using MelonLoader;
using UnityEngine;
using Color = UnityEngine.Color;
using Object = Il2CppSystem.Object;

namespace CMS21Together.ClientSide.Data
{
    [Obfuscation][RegisterTypeInIl2Cpp]
    public class ContentManager : MonoBehaviour
    {
        public Dictionary<string, bool> Contents = new Dictionary<string, bool>();
        protected bool ContentSet;
        
        public static ContentManager Instance;
        
        public void Initialize()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                MelonLogger.Msg("Instance already exists, destroying object!");
                Destroy(this);
            }

            CheckContent();
        }

        [Obfuscation]
        protected async void CheckContent()
        {
            if(ContentSet) return;
            
            Contents.Clear();
            await Task.Delay(2000);
            foreach (DLC content in Singleton<GameManager>.Instance.PlatformManager.GetDLCSystem().DLCs)
            {
                if(!Contents.ContainsKey(content.Name))
                    Contents.Add(content.Name, content.Owned);
            }

            ContentSet = true;
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