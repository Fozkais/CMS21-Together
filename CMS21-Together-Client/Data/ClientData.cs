using System.Collections.Generic;
using CMS21_Together_Core.Network.Packets;
using CMS21Together.Logic;
using CMS21Together.Logic.Player;
using CMS21Together.Managers;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.Data;

public static class ClientData
{
	public static Dictionary<int, PlayerInstance> Players = new Dictionary<int, PlayerInstance>();
	
	// Global Game States
	public static bool IsWorldStateSynced { get; set; }
	public static bool IsGarageStateSynced { get; set; }
	public static bool IsInventorySynced { get; set; }
	public static bool IsInitialSyncFinished { get; set; }
	public static bool IsServerUpdating { get; set; }

	public static void Reset()
	{
		IsWorldStateSynced = false;
		IsGarageStateSynced = false;
		IsInventorySynced = false;
		IsInitialSyncFinished = false;
		IsServerUpdating = false;
		
		Players.Clear();
	}

	public static void Update()
	{
		if (!IsInitialSyncFinished) return;
		
		Movement.UpdateMovement();
	}

	public static void SpawnPlayer(MovementPacket packet)
	{
		if (!ModGameManager.PlayerPrefab)
		{
			MelonLogger.Warning("Cannot Spawn player, Player prefab is null.");
			return;
		}
		Vector3 pos = new Vector3(packet.Position.X, packet.Position.Y, packet.Position.Z);
		Vector3 vel = new Vector3(packet.Velocity.X, packet.Velocity.Y, packet.Velocity.Z);
		Quaternion rot = new Quaternion(packet.Rotation.X, packet.Rotation.Y, packet.Rotation.Z, packet.Rotation.W);

		GameObject player = Object.Instantiate(ModGameManager.PlayerPrefab, pos, rot);
		player.gameObject.SetActive(true);
		player.name = $"Player[{packet.SenderId}]";
		PlayerInstance instance = player.AddComponent<PlayerInstance>();
		instance.UpdateNetworkState(pos, rot, vel, packet.CameraPitch, packet.IsGrounded, packet.IsCrouching, packet.IsRunning);
		Players[packet.SenderId] = instance;
	}
}