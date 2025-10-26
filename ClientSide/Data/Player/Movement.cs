using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.Shared.Data;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Player;

public static class Movement
{
	private static readonly float minDistance = 0.01f;
	private static Vector3 lastPosition;

	public static void UpdatePosition(int id, Vector3Serializable position)
	{
		if (!ClientData.Instance.connectedClients.ContainsKey(id)) return;
		if (!GameData.isReady) return;

		var player = ClientData.Instance.connectedClients[id];

		if (player.scene != ClientData.UserData.scene) return;
		if (player.userObject == null) player.SpawnPlayer();

		if (player.userObject)
		{
			Vector3 targetPos = position.toVector3();
			Vector3 curPos = player.userObject.transform.position;

			if (player.lastPosition != null)
			{
				var direction = (targetPos - player.lastPosition.toVector3()).normalized;
				var speed = (targetPos - player.lastPosition.toVector3()).magnitude / Time.deltaTime;

				speed = Mathf.Clamp(speed, 0f, 20f);

				UpdateAnimations(player.userAnimator, direction, speed);
				player.lastUpdateTime = Time.time;

				player.userObject.transform.position = Vector3.Lerp(curPos, targetPos, Time.deltaTime * 15f);
			}
			else
				player.userObject.transform.position = targetPos;
			player.lastPosition = position;
		}
	}

	private static void UpdateAnimations(Animator animator, Vector3 direction, float speed)
	{
		var horizontalSpeed = direction.x * speed;
		var verticalSpeed = direction.z * speed;

		animator.SetFloat("Vertical", Mathf.Lerp(animator.GetFloat("Vertical"), verticalSpeed, Time.deltaTime * 10f));
		animator.SetFloat("Horizontal", Mathf.Lerp(animator.GetFloat("Horizontal"), horizontalSpeed, Time.deltaTime * 10f));
	}
	
	public static void CheckForInactivity()
	{
		foreach (var client in ClientData.Instance.connectedClients.Values)
		{
			if (client == null) continue;
			if (client.userObject == null || client.userAnimator == null) continue;
			
			float elapsedTime = Time.time - client.lastUpdateTime;
			
			if (elapsedTime > 0.15f) 
			{
				client.userAnimator.SetFloat("Vertical", Mathf.Lerp(client.userAnimator.GetFloat("Vertical"), 0, Time.deltaTime * 10f));
				client.userAnimator.SetFloat("Horizontal", Mathf.Lerp(client.userAnimator.GetFloat("Horizontal"), 0, Time.deltaTime * 10f));
				
				var currentRotation = client.userObject.transform.rotation;
				var targetRotation = client.userObject.transform.rotation;
				client.userObject.transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, Time.deltaTime * 5f);
			}
		}
	}

	public static void SendPosition()
	{
		if (GameData.Instance.localPlayer == null) return;

		var position = GameData.Instance.localPlayer.transform.position;
		position.y -= 0.72f; // probably fix player flying
		if (Vector3.Distance(position, lastPosition) > minDistance)
		{
			lastPosition = position;
			ClientSend.PositionPacket(new Vector3Serializable(position));
		}
	}
}