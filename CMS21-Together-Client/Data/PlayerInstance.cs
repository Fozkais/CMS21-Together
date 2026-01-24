using System;
using CMS21_Together_Core.Data.GameType;
using MelonLoader;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace CMS21Together.Data;

[RegisterTypeInIl2Cpp]
public class PlayerInstance : MonoBehaviour
{
	public PlayerInstance(IntPtr ptr) : base(ptr) {}
	public PlayerInstance() : base(ClassInjector.DerivedConstructorPointer<PlayerInstance>()) => ClassInjector.DerivedConstructorBody(this);
	
	private Vector3 targetPosition;
	private Quaternion targetRotation;
	private Vector3 currentVelocity;
	
	public float positionLerpSpeed = 10f;
	public float rotationLerpSpeed = 15f;
	public float teleportThreshold = 3.0f;
	
	private Animator animator;

	private void Awake()
	{
		animator = GetComponent<Animator>();
		targetPosition = transform.position;
		targetRotation = transform.rotation;
	}

	public void UpdateNetworkState(Vector3 pos, Quaternion rot, Vector3 vel)
	{
		targetPosition = pos;
		targetRotation = rot;
		currentVelocity = vel;
	}
	
	private void Update()
	{
		HandleMovement();
		HandleRotation();
		HandleAnimation();
	}

	private void HandleMovement()
	{
		float distance = Vector3.Distance(transform.position, targetPosition);
		
		if (distance > teleportThreshold)
			transform.position = targetPosition;
		else
			transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * positionLerpSpeed);
	}

	private void HandleRotation()
	{
		transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationLerpSpeed);
	}

	private void HandleAnimation()
	{
		if (animator == null) return;
		
		Vector3 localVelocity = transform.InverseTransformDirection(currentVelocity);
		
		float horizontal = localVelocity.x;
		float vertical = localVelocity.z;
		
		animator.SetFloat("Horizontal", horizontal, 0.1f, Time.deltaTime);
		animator.SetFloat("Vertical", vertical, 0.1f, Time.deltaTime);
	}
}