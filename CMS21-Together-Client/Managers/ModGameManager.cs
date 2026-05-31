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
		(DataUtils.ConvertStreamToIL2CPP(DataUtils.LoadContent("CMS21Together.Assets.playermodel.bundle")));
		
		if (!bundle) return;
		GameObject prefab = bundle.LoadAsset<GameObject>("model_rigged");
		if (!prefab)
		{
			MelonLogger.Warning("Cannot load model_rigged from bundle.");
			return;
		}

		RuntimeAnimatorController animatorController = bundle.LoadAsset<RuntimeAnimatorController>("model_ac");
		Material material = new Material(Shader.Find("HDRP/Unlit"));
		Texture baseTexture = bundle.LoadAsset<Texture>("texture_shaded");
		Texture normalTexture = bundle.LoadAsset<Texture>("texture_normal");
		Texture maskTexture = bundle.LoadAsset<Texture>("texture_mask");

		if (material)
		{
			Shader litShader = Shader.Find("HDRP/Unlit");
			if (litShader != null) material.shader = litShader;
			
			// Force Opaque instead of Transparent
			material.SetFloat("_SurfaceType", 0.0f);
			material.SetInt("_ZWrite", 1);
			material.renderQueue = 2000;
			material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
			material.DisableKeyword("_ALPHATEST_ON");
			material.DisableKeyword("_ALPHABLEND_ON");

			if (baseTexture)
			{
				material.SetTexture("_BaseColorMap", baseTexture);
				material.mainTexture = baseTexture;
			}

			if (normalTexture)
			{
				material.SetTexture("_NormalMap", normalTexture);
				material.SetTexture("_BumpMap", normalTexture);
			}
			if (maskTexture) material.SetTexture("_MaskMap", maskTexture);
			
		}

		SkinnedMeshRenderer renderer = prefab.GetComponentInChildren<SkinnedMeshRenderer>();
		if (renderer && material) renderer.material = material;

		Animator animator = prefab.GetComponent<Animator>();
		if (animator && animatorController)
		{
			animator.runtimeAnimatorController = animatorController;
		}

		prefab.transform.position = new Vector3(0, -10, 0);
		prefab.transform.localScale = new Vector3(1f, 1f, 1f);
		prefab.transform.rotation = Quaternion.Euler(0, 180, 0);

		PlayerPrefab = Object.Instantiate(prefab);
		Object.DontDestroyOnLoad(PlayerPrefab);
		bundle.Unload(false);
		PlayerPrefab.SetActive(false);
	}
}