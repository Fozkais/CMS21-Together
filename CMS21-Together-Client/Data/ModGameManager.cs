using Il2CppSystem.IO;

namespace CMS21Together.Data;

public static class ModGameManager
{
	public static ProfileData CurrentSave { get; private set; }
	
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
		NotificationCenter.m_instance.StartCoroutine(NotificationCenter.m_instance.SelectSceneToLoad("garage", SceneType.Garage, true, true));
	}
}