using System.Collections.Generic;
using System.Collections.ObjectModel;
using CMS21Together.Shared;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;

namespace CMS21Together.ClientSide.Data;

[RegisterTypeInIl2Cpp]
public class ContentManager : MonoBehaviour
{
    public static ContentManager Instance;
    
    public string gameVersion { get; private set; }
    public IReadOnlyDictionary<string, bool> ownedContents { get; private set; }
    
    public void Initialize()
    {
        if(ownedContents != null) return;
            
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

    private void GetGameVersion()
    {
        if(ownedContents != null) return;
            
        gameVersion = GameObject.Find("GameVersion").GetComponent<Text>().text;
    }

    protected void CheckContent()
    {
        if(ownedContents != null) return;
            
        ownedContents = new ReadOnlyDictionary<string, bool>(ApiCalls.API_M3());
    }

}