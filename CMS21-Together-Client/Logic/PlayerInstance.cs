using System;
using MelonLoader;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace CMS21Together.Logic;

[RegisterTypeInIl2Cpp]
public class PlayerInstance : MonoBehaviour
{
	public PlayerInstance(IntPtr ptr) : base(ptr) {}
	public PlayerInstance() : base(ClassInjector.DerivedConstructorPointer<PlayerInstance>()) => ClassInjector.DerivedConstructorBody(this);
	
	private Vector3 targetPosition;
	private Quaternion targetRotation;
	private Vector3 currentVelocity;
	private float targetCameraPitch;
	private bool isGrounded;
	private bool isCrouching;
	private bool isRunning;
	
	private Vector3 lastPosition;
	private Quaternion lastRotation;
	private float timeSinceLastUpdate;
	private float timeBetweenUpdates = 0.05f;
	private float lastPacketTime;
	
	public float teleportThreshold = 3.0f;
	
	private Animator animator;
	private Transform spineBone;
	private Transform headBone;
	private float currentPitch;

	private void Awake()
	{
		animator = GetComponent<Animator>();
		targetPosition = transform.position;
		targetRotation = transform.rotation;
		
		if (animator != null)
		{
			spineBone = FindBone(transform, "Spine1");
			if (spineBone == null) spineBone = FindBone(transform, "Spine");
			if (spineBone == null) spineBone = FindBone(transform, "Chest");

			headBone = FindBone(transform, "Head");
			if (headBone == null) headBone = FindBone(transform, "Neck");
		}
	}

	private Transform FindBone(Transform current, string namePart)
	{
		if (current.name.Contains(namePart))
			return current;
			
		for (int i = 0; i < current.childCount; i++)
		{
			Transform found = FindBone(current.GetChild(i), namePart);
			if (found != null) return found;
		}
		return null;
	}

	public void UpdateNetworkState(Vector3 pos, Quaternion rot, Vector3 vel, float pitch, bool grounded, bool crouching, bool running)
	{
		lastPosition = transform.position;
		lastRotation = transform.rotation;
		targetPosition = pos;
		targetRotation = rot;
		
		timeBetweenUpdates = Mathf.Max(0.01f, Time.time - lastPacketTime);
		lastPacketTime = Time.time;
		timeSinceLastUpdate = 0f;
		
		currentVelocity = vel;
		targetCameraPitch = pitch;
		isGrounded = grounded;
		isCrouching = crouching;
		isRunning = running;
	}
	
	private void Update()
	{
		timeSinceLastUpdate += Time.deltaTime;
		float t = Mathf.Clamp01(timeSinceLastUpdate / timeBetweenUpdates);
		
		HandleMovement(t);
		HandleRotation(t);
		HandleAnimation();
	}

	private void LateUpdate()
	{
		float pitch = targetCameraPitch;
		if (pitch > 180f) pitch -= 360f;
		
		currentPitch = Mathf.Lerp(currentPitch, pitch, Time.deltaTime * 15f);

		if (spineBone != null)
		{
			float spinePitch = Mathf.Clamp(currentPitch, -20f, 20f);
			spineBone.Rotate(transform.right, spinePitch, Space.World);
		}

		if (headBone != null)
		{
			float headPitch = Mathf.Clamp(currentPitch, -45f, 45f);
			headBone.Rotate(transform.right, headPitch, Space.World);
		}
	}

	private void HandleMovement(float t)
	{
		float distance = Vector3.Distance(transform.position, targetPosition);
		
		if (distance > teleportThreshold)
		{
			transform.position = targetPosition;
			lastPosition = targetPosition;
		}
		else
		{
			transform.position = Vector3.Lerp(lastPosition, targetPosition, t);
		}
	}

	private void HandleRotation(float t)
	{
		transform.rotation = Quaternion.Slerp(lastRotation, targetRotation, t);
	}

	private void HandleAnimation()
	{
		if (animator == null) return;
		
		Vector3 localVelocity = transform.InverseTransformDirection(currentVelocity);
		float speed = new Vector2(localVelocity.x, localVelocity.z).magnitude;
		
		float h = 0f;
		float v = 0f;
		
		if (speed > 0.1f)
		{
			Vector2 normalizedDir = new Vector2(localVelocity.x, localVelocity.z).normalized;
			
			if (isCrouching) 
			{
				h = normalizedDir.x * 1.0f;
				v = normalizedDir.y * 1.0f;
			}
			else
			{
				float magnitude = isRunning ? 1.0f : 0.5f;
				h = normalizedDir.x * magnitude;
				v = normalizedDir.y * magnitude;
			}
		}
		
		animator.SetFloat("Horizontal", h, 0.1f, Time.deltaTime);
		animator.SetFloat("Vertical", v, 0.1f, Time.deltaTime);
		animator.SetBool("IsGrounded", isGrounded);
		animator.SetBool("IsCrouching", isCrouching);
		
		// Setup future parameter for camera pitch if the model has a spine bone setup
		// animator.SetFloat("Pitch", targetCameraPitch);
	}
}