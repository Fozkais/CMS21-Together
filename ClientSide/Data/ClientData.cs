using System.Collections;
using System.Collections.Generic;
using CMS21Together.ClientSide.Data.Garage.Campaign;
using CMS21Together.ClientSide.Data.Garage.Car;
using CMS21Together.ClientSide.Data.Player;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using CMS21Together.Shared.Data.Vanilla.GarageTool;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data;

public class ClientData
{
	public static ClientData Instance;
	public static UserData UserData;
	public static bool GameReady;

	public Dictionary<int, UserData> connectedClients = new();
	public Gamemode gamemode;
	public Dictionary<string, GarageUpgrade> garageUpgrades = new();
	public Dictionary<int, ModCar> loadedCars = new();
	public ModEngineStand engineStand;
	public GameObject playerPrefab;
	public int scrap, money ,exp, level;

	public ClientData()
	{
		GameReady = false;
		GameData.Instance = null;

		Player.Inventory.Reset();
		CarSpawnHooks.Reset();
		JobManager.Reset();
		Stats.Reset();
		GarageUpgradeHooks.Reset();
		Garage.Tools.ToolsMoveManager.Reset();
		engineStand = new();
	}

	public void UpdateClient()
	{
		if (GameData.isReady == false)
			MelonCoroutines.Start(InitializeGameData());

		if (GameReady)
		{
			Movement.SendPosition();
			Movement.CheckForInactivity();
			Rotation.SendRotation();
		}
	}

	private IEnumerator InitializeGameData()
	{
		while (SceneManager.CurrentScene() != GameScene.garage)
			yield return new WaitForEndOfFrame();
		
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		GameData.Instance = new GameData();
		MelonCoroutines.Start(Stats.SendInitialStats());
		MelonCoroutines.Start(GarageUpgradeHooks.SendInitial());

		yield return new WaitForSeconds(2);
		yield return new WaitForEndOfFrame();
		gamemode = SavesManager.GetGamemodeFromDifficulty(SavesManager.currentSave.Difficulty);
		GameReady = true;
		SavesManager.SaveModSave(SavesManager.currentSaveIndex);
		MelonLogger.Msg("Game is ready.");
	}

	public void LoadPlayerPrefab()
	{
		var playerBundle = AssetBundle.LoadFromStream(DataHelper.DeepCopy(DataHelper.LoadContent("CMS21Together.Assets.player.assets")));

		if (playerBundle)
		{
			GameObject player = playerBundle.LoadAsset<GameObject>("playerModel");
			var playerInstance = Object.Instantiate(player);

			Material material;
			Texture baseTexture = playerBundle.LoadAsset<Texture>("tex_base");
			baseTexture.filterMode = FilterMode.Bilinear;
			Texture normalTexture = playerBundle.LoadAsset<Texture>("tex_normal");
			baseTexture.filterMode = FilterMode.Bilinear;

			material = new Material(Shader.Find("HDRP/Unlit"));
			material.mainTexture = baseTexture;
			material.SetTexture("_BumpMap", normalTexture);

			playerInstance.GetComponentInChildren<SkinnedMeshRenderer>().material = material;

			playerInstance.transform.localScale = new Vector3(0.095f, 0.095f, 0.095f);
			playerInstance.transform.position = new Vector3(0, -10, 0);
			playerInstance.transform.rotation = new Quaternion(0, 180, 0, 0);

			playerPrefab = playerInstance;

			Object.DontDestroyOnLoad(playerPrefab);

			playerBundle.Unload(false);
			MelonLogger.Msg("[ClientData->LoadPlayerPrefab] Loaded player model Succesfully!");
		}
	}

	public IEnumerator SpawnPlayer(int _exp, int _level, Vector3 pos, Quaternion rot, int skillPoints, Dictionary<string, List<bool>> skills, long startItemUid)
	{
		while (!ClientData.GameReady)
			yield return new WaitForSeconds(0.1f);
		
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();

		UIDManager.LastUID = startItemUid;

		if (SavesManager.currentSave.Difficulty != DifficultyLevel.Sandbox)
		{
			GlobalData.PlayerLevel = _level;
			UIManager.Get().StatsContainer.CurrentLevel = _level;
			UIManager.Get().StatsContainer.Refresh(StatType.Level, true);
			GlobalData.PlayerExp = _exp;
			UIManager.Get().StatsContainer.Refresh(StatType.Experience, true);
			
			Singleton<GameManager>.Instance.UpgradeSystem.availablePoints = skillPoints;
		}
		
		if (pos != Vector3.zero)
			GameData.Instance.localPlayer.transform.position = pos;
		if (rot != Quaternion.identity)
			GameData.Instance.localPlayer.transform.rotation = rot;
		
		if (skills != null)
		{
			GameData.Instance.upgradeTools.upgradeSystem.LockUpgradesForPoints();
			foreach (KeyValuePair<string, List<bool>> skill in skills)
			{
				int lvl = 0;
				foreach (bool unlocked in skill.Value)
				{
					if(unlocked)
						GameData.Instance.upgradeTools.upgradeSystem.UnlockUpgrade(skill.Key, lvl);
					lvl++;
				}
			}
		}
	}
}