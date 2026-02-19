using Il2CppSystem.IO;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.Logic;

public static class ModGameManager
{
	public static ProfileData CurrentSave { get; private set; }
	public static GameObject PlayerPrefab { get; private set; }
	
	public static void StartGame()
	{
		SaveUtils.ExtendProfileDataSize();
		GameManager manager = Singleton<GameManager>.Instance;

		var writer = new BinaryWriter();
		var save = new ProfileData();

		save.Init();
		save.WriteSaveHeader(writer);
		save.WriteSaveVersion(writer);

		manager.GameDataManager.ProfileData[4] = save;
		manager.ProfileManager.selectedProfile = 4;
		manager.RDGPlayerPrefs.SetInt("selectedProfile", 4);
		Singleton<GameManager>.Instance.ProfileManager.SetNameForCurrentProfile("ClientSave");
		Singleton<GameManager>.Instance.ProfileManager.SetDifficultyForCurrentProfile(DifficultyLevel.Normal);
		manager.ProfileManager.Load();
			
		CurrentSave = manager.ProfileManager.GetSelectedProfileData();
		manager.GameDataManager.LoadProfile();
		manager.StartCoroutine(manager.GameDataManager.Load(true));
		NotificationCenter.m_instance.StartCoroutine(NotificationCenter.m_instance.SelectSceneToLoad("garage", SceneType.Garage, true, false));
	}

	public static void LoadPlayerPrefab()
	{
		AssetBundle bundle = AssetBundle.LoadFromStream
		(DataUtils.ConvertStreamToIL2CPP(DataUtils.LoadContent("CMS21Together.Assets.player.assets")));
		
		if (!bundle) return;
		GameObject prefab = bundle.LoadAsset<GameObject>("playerModel");
		if (!prefab)
		{
			MelonLogger.Warning("Cannot load bundle.");
			return;
		}

		Material material = new Material(Shader.Find("HDRP/Unlit"));
		

		Texture baseTexture = bundle.LoadAsset<Texture>("tex_base");
		if (baseTexture)
		{
			baseTexture.filterMode = FilterMode.Bilinear;
			material.mainTexture = baseTexture;
		}
    
		Texture normalTexture = bundle.LoadAsset<Texture>("tex_normal");
		if (normalTexture) 
		{
			normalTexture.filterMode = FilterMode.Bilinear;
			material.SetTexture("_BumpMap", normalTexture);
		}
		
		SkinnedMeshRenderer renderer = prefab.GetComponentInChildren<SkinnedMeshRenderer>();
		if (renderer) renderer.material = material;

		prefab.transform.position = new Vector3(0, -10, 0);
		prefab.transform.localScale = new Vector3(0.095f, 0.095f, 0.095f);
		prefab.transform.rotation = Quaternion.Euler(0, 180, 0);

		PlayerPrefab = Object.Instantiate(prefab);
		Object.DontDestroyOnLoad(PlayerPrefab);
		bundle.Unload(false);
		PlayerPrefab.SetActive(false);
	}
}