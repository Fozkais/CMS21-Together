using System;
using CMS21Together.ClientSide;
using CMS21Together.ServerSide;
using CMS21Together.Shared;
using MelonLoader;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable All

namespace CMS21Together
{
    public class MainMod : MelonMod
    {
        public const int MAX_SAVE_COUNT = 22;
        public const int MAX_PLAYER = 4;
        public const int PORT = 7777;
        public const string ASSEMBLY_MOD_VERSION = "0.3.5";
        public const string MOD_VERSION = "Together " + ASSEMBLY_MOD_VERSION;
        public bool isModInitialized;

        public override void OnEarlyInitializeMelon()
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(ApiCalls.CurrentDomain_AssemblyResolve);
        }

        public override void OnLateInitializeMelon()
        {
            GameObject modObject = new GameObject("TogetherMod");
            Object.DontDestroyOnLoad(modObject);

            var cl = modObject.AddComponent<Client>();
            var sv = modObject.AddComponent<Server>();
            cl.Initialize();
           
            isModInitialized = true;
            LoggerInstance.Msg("Together Mod Initialized!");
        }
        

        public override void OnGUI()
        {
            if(!isModInitialized) {return;}
        }
        public override void OnSceneWasLoaded(int buildindex, string sceneName) // Runs when a Scene has Loaded and is passed the Scene's Build Index and Name.
        {
            if(!isModInitialized) {return;}
            
        }

        public override void OnUpdate()
        {
            if(!isModInitialized) {return;}
          
            ThreadManager.UpdateThread();
        }

        public override void OnLateUpdate()
        {
            if(!isModInitialized) {return;}
        }
        
        public override void OnApplicationQuit() // Runs when the Game is told to Close.ca
        {
           
        }
    }
}