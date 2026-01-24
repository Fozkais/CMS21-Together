using CMS21_Together_Core.Data;
using CMS21_Together_Core.Data.GameType;
using CMS21_Together_Core.Network.Packets;
using CMS21Together.Network;
using UnityEngine;

namespace CMS21Together.Data.Player;

public static class Movement
{
	private const float SendRate = 0.1f;
	private const float MinMoveDistanceSqr = 0.01f * 0.01f;
	private const float MinRotAngle = 1.0f;

	private static Vector3 lastSentPosition;
	private static Quaternion lastSentRotation;
	private static float nextSendTime;

	public static void UpdateMovement()
	{
		if (GameData.Instance == null || !GameData.Instance.LocalPlayer) return;
		if (Time.time < nextSendTime) return;

		var transform = GameData.Instance.LocalPlayer.transform;
		Vector3 currentPos = transform.position;
		currentPos.y -= 0.72f;
		Quaternion currentRot = transform.rotation;
		
		bool positionChanged = (currentPos - lastSentPosition).sqrMagnitude > MinMoveDistanceSqr;
		bool rotationChanged = Quaternion.Angle(currentRot, lastSentRotation) > MinRotAngle;
		
		if (!positionChanged && !rotationChanged) return;
		
		Vector3 velocity = GameData.Instance.LocalPlayer.movement.velocity;
		
		Client.Instance.Send(new MovementPacket()
		{
			Position = new Vector3Serializable(currentPos.x, currentPos.y, currentPos.z),
			Velocity = new Vector3Serializable(velocity.x, velocity.y, velocity.z),
			Rotation = new QuaternionSerializable(currentRot.x, currentRot.y, currentRot.z, currentRot.w)
		});
		
		lastSentPosition = currentPos;
		lastSentRotation = currentRot;
		nextSendTime = Time.time + SendRate;
	}

	public static void UpdateRemotePlayer(MovementPacket packet)
	{
		if (!ClientData.Players.TryGetValue(packet.SenderId, out PlayerInstance instance))
			ClientData.SpawnPlayer(packet);
		else
		{
			Vector3 pos = new Vector3(packet.Position.X, packet.Position.Y, packet.Position.Z);
			Vector3 vel = new Vector3(packet.Velocity.X, packet.Velocity.Y, packet.Velocity.Z);
			Quaternion rot = new Quaternion(packet.Rotation.X, packet.Rotation.Y, packet.Rotation.Z, packet.Rotation.W);
			instance.UpdateNetworkState(pos, rot, vel);
		}
	}
}