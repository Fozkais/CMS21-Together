using System.Collections;
using System.Collections.Generic;
using CMS21_Together_Core.Data;
using CMS21_Together_Core.Data.Vanilla;
using CMS21_Together_Core.Data.Vanilla.GarageTool;
using CMS21Together.ClientSide.Data.Garage.Campaign;
using CMS21Together.ClientSide.Data.Garage.Car;
using CMS21Together.ClientSide.Data.Garage.Tools;
using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.ClientSide.Data.Player;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data;

public class ClientData
{
	public static ClientData Instance;
	public static UserData UserData;
	public static bool GameReady;

	private static float lastCarCheckTime;
	private static readonly float carCheckInterval = 0.5f; // Check every 0.5 seconds

	public Dictionary<int, UserData> connectedClients = new();
	public ModEngineStand engineStand;
	public ModEngineStand engineStand2;
	public Gamemode gamemode;
	public Dictionary<string, GarageUpgrade> garageUpgrades = new();
	private bool initRoutine;
	public Dictionary<int, ModCar> loadedCars = new();
	public GameObject playerPrefab;
	public int scrap, money, exp, level;

	public ClientData()
	{
		GameReady = false;
		initRoutine = false;
		GameData.Instance = null;

		Player.Inventory.Reset();
		CarSpawnHooks.Reset();
		JobManager.Reset();
		Stats.Reset();
		GarageUpgradeHooks.Reset();
		Garage.Tools.ToolsMoveManager.Reset();
		Garage.Tools.CarWashLogic.Reset();
		CarPaintLogic.Reset();
		engineStand = new ModEngineStand(null);
		engineStand2 = new ModEngineStand(null);
		garageUpgrades = new Dictionary<string, GarageUpgrade>();
	}

	public void UpdateClient()
	{
		if (!GameData.isReady && !initRoutine)
			MelonCoroutines.Start(InitializeGameData());

		if (GameReady)
		{
			Movement.SendPosition();
			Movement.CheckForInactivity();
			Rotation.SendRotation();

			// Business Logic: Periodically check if local player entered/exited a car
			if (Time.time - lastCarCheckTime >= carCheckInterval)
			{
				lastCarCheckTime = Time.time;
				CarEnterExit.CheckCarState();
			}
		}
	}

	private IEnumerator InitializeGameData()
	{
		initRoutine = true;
		while (SceneManager.CurrentScene() != GameScene.garage)
			yield return new WaitForEndOfFrame();

		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		GameData.Instance = new GameData();
		MelonCoroutines.Start(Stats.SendInitialStats());
		MelonCoroutines.Start(GarageUpgradeHooks.SendInitial());

		// Business Rule: Ensure player prefab is loaded before spawning players
		if (playerPrefab == null)
		{
			LoadPlayerPrefab();
			// Security Rule: Wait multiple frames to ensure prefab is fully loaded and initialized
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();

			// Business Rule: Verify prefab was loaded successfully
			if (playerPrefab == null)
			{
				MelonLogger.Error("[ClientData->InitializeGameData] Failed to load playerPrefab after multiple attempts. Retrying...");
				LoadPlayerPrefab();
				yield return new WaitForEndOfFrame();
				yield return new WaitForEndOfFrame();
			}
		}

		yield return new WaitForSeconds(2);
		yield return new WaitForEndOfFrame();
		gamemode = SavesManager.GetGamemodeFromDifficulty(SavesManager.currentSave.Difficulty);

		// Business Rule: Only set GameReady to true if playerPrefab is loaded
		if (playerPrefab != null)
		{
			GameReady = true;
			initRoutine = false;
			if (SavesManager.currentSaveIndex != MainMod.MAX_SAVE_COUNT)
				SavesManager.SaveModSave(SavesManager.currentSaveIndex);
			foreach (var client in connectedClients)
				if (client.Value.scene == GameScene.garage)
					DoSpawnPlayer(client.Value);
			MelonLogger.Msg("Game is ready.");
		}
		else
		{
			MelonLogger.Error("[ClientData->InitializeGameData] Cannot set GameReady: playerPrefab is still null!");
			initRoutine = false;
		}
	}
	
	public void DoDestroyPlayer(UserData player)
	{
		if (player.userObject == null) return;
		Object.Destroy(player.userObject);
		player.userObject = null;
	}

	public void DoSpawnPlayer(UserData player)
	{
		if (player.userObject != null)
		{
			MelonLogger.Warning($"[UserData->SpawnPlayer] Player {player.username} (ID: {player.playerID}) already spawned. Skipping duplicate spawn.");
			return;
		}

		if (ClientData.Instance.playerPrefab == null)
		{
			MelonLogger.Error("[CMS21-Together] Cannot spawn player: playerPrefab is null.");
			return;
		}
		if (player.playerID == ClientData.UserData.playerID)
		{
			if (GameData.Instance.localPlayer == null)
			{
				MelonLogger.Error("[CMS21-Together] Cannot spawn local player: localPlayer is null.");
				return;
			}
			player.userObject = GameData.Instance.localPlayer;
		}
		else
		{
			// Security Rule: Validate GameData.Instance and localPlayer before accessing
			if (GameData.Instance == null || GameData.Instance.localPlayer == null)
			{
				MelonLogger.Warning($"[UserData->SpawnPlayer] GameData.Instance or localPlayer is null. Cannot spawn player {player.username}. Will retry later.");
				return;
			}

			// Security Rule: Validate localPlayer has Collider component
			var localPlayerCollider = GameData.Instance.localPlayer.GetComponent<Collider>();
			if (localPlayerCollider == null)
			{
				MelonLogger.Warning($"[UserData->SpawnPlayer] Local player has no Collider component. Cannot set up collision ignore for {player.username}.");
			}

			// Business Logic: Instantiate player prefab at correct position and rotation
			player.userObject = Object.Instantiate(ClientData.Instance.playerPrefab, player.position.toVector3(), player.rotation.toQuaternion());
			player.userObject.AddComponent<InfoBillboard>();
			player.userAnimator = player.userObject.GetComponent<Animator>();
			player.userObject.name = player.username;
			
			// Security Rule: Only ignore collision if both colliders exist
			var userObjectCollider = player.userObject.GetComponent<Collider>();
			if (localPlayerCollider != null && userObjectCollider != null)
			{
				Physics.IgnoreCollision(localPlayerCollider, userObjectCollider);
			}
			else
			{
				MelonLogger.Warning($"[UserData->SpawnPlayer] One or both players missing Collider. Collision ignore not set for {player.username}.");
			}
			
			MelonLogger.Msg($"[UserData->SpawnPlayer] Spawned player {player.username} (ID: {player.playerID}) at position {player.position.toVector3()}");
		}
	}

	public void LoadPlayerPrefab()
	{
		var playerBundle = AssetBundle.LoadFromStream(DataHelper.DeepCopy(DataHelper.LoadContent("CMS21Together.Assets.player.assets")));

		if (playerBundle)
		{
			var player = playerBundle.LoadAsset<GameObject>("playerModel");
			if (player == null)
			{
				MelonLogger.Warning("Impossible de charger l'AssetBundle !");
				return;
			}

			var playerInstance = Object.Instantiate(player);

			Material material;
			var baseTexture = playerBundle.LoadAsset<Texture>("tex_base");
			if (baseTexture != null) baseTexture.filterMode = FilterMode.Bilinear;
			var normalTexture = playerBundle.LoadAsset<Texture>("tex_normal");
			if (normalTexture != null) normalTexture.filterMode = FilterMode.Bilinear;

			material = new Material(Shader.Find("HDRP/Unlit"));
			if (baseTexture != null)
				material.mainTexture = baseTexture;
			if (normalTexture != null)
				material.SetTexture("_BumpMap", normalTexture);

			var skinnedMesh = playerInstance.GetComponentInChildren<SkinnedMeshRenderer>();
			if (skinnedMesh != null) skinnedMesh.material = material;

			playerInstance.transform.localScale = new Vector3(0.095f, 0.095f, 0.095f);
			playerInstance.transform.position = new Vector3(0, -10, 0);
			playerInstance.transform.rotation = new Quaternion(0, 180, 0, 0);

			playerPrefab = playerInstance;
			Object.DontDestroyOnLoad(playerInstance);

			playerBundle.Unload(false);
			MelonLogger.Msg("[ClientData->LoadPlayerPrefab] Loaded player prefab successfully!");
		}
	}

	public IEnumerator SpawnPlayer(int _money, int _exp, int _level, Vector3 pos, Quaternion rot, int skillPoints, Dictionary<string,
		List<bool>> skills, long startItemUid, int missionFinished, bool missionInProgress)
	{
		while (!GameReady)
			yield return new WaitForSeconds(0.1f);

		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();

		UIDManager.LastUID = startItemUid;

		MelonLogger.Msg("\nReceived Player info! : \n"
		                + $"MissionFinished : {missionFinished}\n"
		                + $"StoryInProgress : {missionInProgress}\n"
		                + $"StartItemUID : {startItemUid}\n"
		                + $"Exp : {_exp}\n"
		                + $"Money : {_money}\n"
		                + $"Level : {_level}\n"
		                + $"Exp : {_exp}\n"
		                + $"SkillPoints : {skillPoints}\n");
		if (GameManager.Instance.GameDataManager.CurrentProfileData.Difficulty != DifficultyLevel.Sandbox)
		{
			GlobalData.PlayerLevel = _level;
			UIManager.Get().StatsContainer.CurrentLevel = _level;
			UIManager.Get().StatsContainer.Refresh(StatType.Level, true);
			GlobalData.PlayerExp = _exp;
			UIManager.Get().StatsContainer.Refresh(StatType.Experience, true);
			GlobalData.PlayerMoney = _money;
			UIManager.Get().StatsContainer.Refresh(StatType.Money, true);

			Singleton<GameManager>.Instance.UpgradeSystem.availablePoints = skillPoints;
			if (skills != null)
			{
				GameData.Instance.upgradeTools.upgradeSystem.LockUpgradesForPoints();
				foreach (var skill in skills)
				{
					var lvl = 0;
					foreach (var unlocked in skill.Value)
					{
						if (unlocked)
							GameData.Instance.upgradeTools.upgradeSystem.UnlockUpgrade(skill.Key, lvl);
						lvl++;
					}
				}
			}
		}

		GlobalData.MissionsFinished = missionFinished;
		GlobalData.IsStoryMissionInProgress = missionInProgress;

		if (pos != Vector3.zero)
			GameData.Instance.localPlayer.transform.position = pos;
		if (rot != Quaternion.identity)
			GameData.Instance.localPlayer.transform.rotation = rot;

		while (SceneManager.CurrentScene() != GameScene.garage)
			yield return new WaitForSeconds(0.5f);
		while (!NotificationCenter.IsGameReady)
			yield return new WaitForSeconds(0.25f);
		while (!GameData.isReady)
			yield return new WaitForSeconds(0.5f);

		ClientSend.ResyncEngineStandPacket(true);
	}
}