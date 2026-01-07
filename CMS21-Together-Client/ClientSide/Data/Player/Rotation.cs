using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.Shared.Data;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Player;

public static class Rotation
{
	private static readonly float minDistance = 0.01f;
	private static Quaternion lastRotation;

	public static void UpdateRotation(int id, QuaternionSerializable rotation)
	{
		if (!ClientData.Instance.connectedClients.ContainsKey(id)) return;
		if (!GameData.isReady) return;

		var player = ClientData.Instance.connectedClients[id];
		if (player.scene != ClientData.UserData.scene) return;

		if (player.userObject != null)
		{
			player.lastUpdateTime = Time.time;
			player.userObject.transform.rotation = rotation.toQuaternion();
		}
		else
		{
			// Business Logic: Store rotation and spawn player if not already spawned
			player.rotation = rotation;
			if (player.userObject == null)
			{
				player.SpawnPlayer();
			}
		}
	}

	public static void SendRotation()
	{
		if (GameData.Instance.localPlayer == null) return;

		var rotation = GameData.Instance.localPlayer.transform.rotation;
		if (Quaternion.Angle(rotation, lastRotation) > minDistance)
		{
			lastRotation = rotation;
			ClientSend.RotationPacket(new QuaternionSerializable(rotation));
		}
	}
}