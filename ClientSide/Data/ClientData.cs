using System.Collections;
using System.Collections.Generic;
using CMS21Together.ClientSide.Data.Garage.Campaign;
using CMS21Together.ClientSide.Data.Garage.Car;
using CMS21Together.ClientSide.Data.Player;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
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
	public GameObject playerPrefab;
	public int scrap, money;

	public ClientData()
	{
		GameReady = false;
		GameData.Instance = null;

		Player.Inventory.Reset();
		CarSpawnManager.Reset();
		CarSpawnHooks.Reset();
		JobManager.Reset();
		Stats.Reset();
	}

	public void UpdateClient()
	{
		if (GameData.Instance == null)
			MelonCoroutines.Start(InitializeGameData());

		Stats.SendInitialStats();
		Movement.SendPosition();
		Rotation.SendRotation();
		JobManager.UpdateSelectedJob();
		Garage.Tools.ToolsMoveManager.Reset();
	}

	private IEnumerator InitializeGameData()
	{
		GameData.Instance = new GameData();

		yield return new WaitForSeconds(2);
		yield return new WaitForEndOfFrame();

		gamemode = SavesManager.GetGamemodeFromDifficulty(SavesManager.currentSave.Difficulty);
		GameReady = true;
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

	public IEnumerator SpawnPlayer(int exp, int level, Vector3 pos, Quaternion rot , Dictionary<string, List<bool>> skills)
	{
		while (!GameData.isReady)
			yield return new WaitForSeconds(0.1f);
		
		yield return new WaitForEndOfFrame();
		
		GlobalData.PlayerExp = exp;
		GlobalData.PlayerLevel = level;
		UIManager.Get().StatsContainer.CurrentLevel = level;
		UIManager.Get().RefreshAllStats();

		if (pos != Vector3.zero)
			GameData.Instance.localPlayer.transform.position = pos;
		GameData.Instance.localPlayer.transform.rotation = rot;
		
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