using CMS21_Together_Core.Data.GameType;
using CMS21_Together_Core.Network.Packets;
using CMS21Together.Network;
using HarmonyLib;
using UnityEngine;
using System.Reflection;
using CMS21Together.Data;

namespace CMS21Together.Logic.Player;

public static class Movement
{
	private const float SendRate = 0.05f; // Increased to 20Hz
	private const float MinMoveDistanceSqr = 0.01f * 0.01f;
	private const float MinRotAngle = 1.0f;

	private static Vector3 lastSentPosition;
	private static Quaternion lastSentRotation;
	private static Vector3 lastSentVelocity;
	private static float lastSentPitch;
	private static bool lastSentCrouching;
	private static bool lastSentRunning;
	private static float nextSendTime;

	private static FieldInfo isCrouchingField;
	private static PropertyInfo isCrouchingProperty;
	private static System.Func<bool> GetIsCrouching;

	public static void UpdateMovement()
	{
		if (GetIsCrouching == null)
		{
			isCrouchingProperty = AccessTools.Property(typeof(FPSCamera), "isCrouching");
			if (isCrouchingProperty != null)
			{
				GetIsCrouching = () => (bool)isCrouchingProperty.GetValue(null);
			}
			else
			{
				isCrouchingField = AccessTools.Field(typeof(FPSCamera), "isCrouching");
				if (isCrouchingField != null)
				{
					GetIsCrouching = () => (bool)isCrouchingField.GetValue(null);
				}
				else
				{
					GetIsCrouching = () => false;
				}
			}
		}

		if (GameData.Instance == null || !GameData.Instance.LocalPlayer) return;
		if (Time.time < nextSendTime) return;

		var transform = GameData.Instance.LocalPlayer.transform;
		Vector3 currentPos = transform.position;
		if (Physics.Raycast(currentPos, Vector3.down, out RaycastHit hit, 3f, -4194305))
		{
			currentPos.y = hit.point.y;
		}
		else
		{
			currentPos.y -= 0.72f;
		}
		Quaternion currentRot = transform.rotation;
		
		bool positionChanged = (currentPos - lastSentPosition).sqrMagnitude > MinMoveDistanceSqr;
		bool rotationChanged = Quaternion.Angle(currentRot, lastSentRotation) > MinRotAngle;
		Vector3 velocity = GameData.Instance.LocalPlayer.movement.velocity;
		bool velocityChanged = (velocity - lastSentVelocity).sqrMagnitude > 0.01f;
		
		bool isCrouching = GetIsCrouching();
		bool crouchChanged = isCrouching != lastSentCrouching;

		bool isRunning = Singleton<GameManager>.Instance.InputManager.GameplayRun() && velocity.sqrMagnitude > 0.1f;
		bool runChanged = isRunning != lastSentRunning;
		
		float pitch = Camera.main != null ? Camera.main.transform.eulerAngles.x : 0f;
		bool pitchChanged = Mathf.Abs(Mathf.DeltaAngle(pitch, lastSentPitch)) > MinRotAngle;
		
		if (!positionChanged && !rotationChanged && !velocityChanged && !crouchChanged && !runChanged && !pitchChanged) return;
		
		bool grounded = GameData.Instance.LocalPlayer.grounded;
		
		Client.Instance.Send(new MovementPacket()
		{
			Position = new Vector3Serializable(currentPos.x, currentPos.y, currentPos.z),
			Velocity = new Vector3Serializable(velocity.x, velocity.y, velocity.z),
			Rotation = new QuaternionSerializable(currentRot.x, currentRot.y, currentRot.z, currentRot.w),
			CameraPitch = pitch,
			IsGrounded = grounded,
			IsCrouching = isCrouching,
			IsRunning = isRunning
		});
		
		lastSentPosition = currentPos;
		lastSentRotation = currentRot;
		lastSentVelocity = velocity;
		lastSentPitch = pitch;
		lastSentCrouching = isCrouching;
		lastSentRunning = isRunning;
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
			instance.UpdateNetworkState(pos, rot, vel, packet.CameraPitch, packet.IsGrounded, packet.IsCrouching, packet.IsRunning);
		}
	}
}