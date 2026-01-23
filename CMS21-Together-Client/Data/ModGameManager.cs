using CMS21_Together_Core.Data;
using CMS21Together.Network;
using Il2CppSystem.IO;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.Data;

public static class ModGameManager
{
	public static ProfileData CurrentSave { get; private set; }
	public static GameObject PlayerPrefab { get; private set; }
	
	public static void StartGame()
	{
		if (!LoadPlayerPrefab())
		{
			Client.Instance.Disconnect();
			return;
		}
		
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

	private static bool LoadPlayerPrefab()
	{
		AssetBundle bundle = AssetBundle.LoadFromStream
		(DataUtils.ConvertStreamToIL2CPP(DataHelper.LoadContent("CMS21Together.Assets.player.assets")));
		
		if (!bundle) return false;

		GameObject prefab = bundle.LoadAsset<GameObject>("playerModel");
		if (!prefab)
		{
			MelonLogger.Warning("Cannot load bundle.");
			return false;
		}

		Material material = new Material(Shader.Find("HDRP/Lit"));
		if (!material)
		{
			MelonLogger.Warning("Cannot create material.");
			return false;
		}
		
		Texture baseTexture = bundle.LoadAsset<Texture>("tex_base");
		if (!baseTexture)
		{
			MelonLogger.Warning("Cannot create base texture.");
			return false;
		}
		
		Texture normalTexture = bundle.LoadAsset<Texture>("tex_normal");
		if (!normalTexture)
		{
			MelonLogger.Warning("Cannot create normal texture.");
			return false;
		}

		baseTexture.filterMode = FilterMode.Bilinear;
		normalTexture.filterMode = FilterMode.Bilinear;
		material.mainTexture = baseTexture;
		material.SetTexture("_BumpMap", normalTexture);
		SkinnedMeshRenderer renderer = prefab.GetComponentInChildren<SkinnedMeshRenderer>();
		if (renderer) renderer.material = material;

		prefab.transform.localScale = new Vector3(0.095f, 0.095f, 0.095f);
		prefab.transform.rotation = new Quaternion(0, 180, 0, 0);

		PlayerPrefab = prefab;
		Object.DontDestroyOnLoad(PlayerPrefab);
		bundle.Unload(false);
		return true;
	}
}