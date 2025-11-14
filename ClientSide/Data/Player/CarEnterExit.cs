using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.Shared.Data;
using HarmonyLib;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Player;

/// <summary>
/// Handles synchronization of player entering/exiting cars in multiplayer.
/// </summary>
[HarmonyPatch]
public static class CarEnterExit
{
	private static bool wasInCar = false;
	private static int lastCarLoaderID = -1;

	/// <summary>
	/// Checks if the local player is currently inside a car.
	/// This method should be called periodically to detect car enter/exit events.
	/// </summary>
	public static void CheckCarState()
	{
		if (!Client.Instance.isConnected || GameData.Instance == null || GameData.Instance.localPlayer == null)
			return;

		// Business Logic: Check if player is inside a car by checking camera parent or FPSInputController state
		// Observation: When player is in car, the camera is typically a child of the car or the camera position is inside the car
		bool currentlyInCar = false;
		int currentCarLoaderID = -1;

		// Business Logic: Check camera and FPSInputController to determine if player is inside a car
		var mainCamera = Camera.main;
		var fpsController = GameData.Instance.localPlayer?.GetComponent<FPSInputController>();
		
		if (mainCamera != null && mainCamera.transform != null && GameData.Instance.carLoaders != null)
		{
			// Business Logic: Primary check: Check if camera is a child of any car (indicates player is in car)
			var cameraParent = mainCamera.transform.parent;
			if (cameraParent != null)
			{
				for (int i = 0; i < GameData.Instance.carLoaders.Length; i++)
				{
					var carLoader = GameData.Instance.carLoaders[i];
					if (carLoader == null || carLoader.gameObject == null) continue;
					if (string.IsNullOrEmpty(carLoader.carToLoad)) continue;

					// Business Logic: Check if camera is a child of this car
					var currentParent = cameraParent;
					while (currentParent != null)
					{
						if (currentParent == carLoader.transform)
						{
							currentlyInCar = true;
							currentCarLoaderID = i;
							break;
						}
						currentParent = currentParent.parent;
					}
					
					if (currentlyInCar) break;
				}
			}
			
			// Business Logic: Secondary check: Check if FPSInputController is disabled (indicates player is in car)
			// Observation: When player enters car, FPSInputController is typically disabled
			if (!currentlyInCar && fpsController != null && !fpsController.enabled)
			{
				// Business Logic: If FPSInputController is disabled, check which car the player is closest to
				var cameraPos = mainCamera.transform.position;
				float closestDistance = float.MaxValue;
				int closestCarID = -1;
				
				for (int i = 0; i < GameData.Instance.carLoaders.Length; i++)
				{
					var carLoader = GameData.Instance.carLoaders[i];
					if (carLoader == null || carLoader.gameObject == null) continue;
					if (string.IsNullOrEmpty(carLoader.carToLoad)) continue;

					var carPos = carLoader.transform.position;
					var distance = Vector3.Distance(cameraPos, carPos);
					
					if (distance < closestDistance && distance < 3f) // Within 3 meters
					{
						closestDistance = distance;
						closestCarID = i;
					}
				}
				
				if (closestCarID >= 0)
				{
					currentlyInCar = true;
					currentCarLoaderID = closestCarID;
				}
			}
			
			// Business Logic: Fallback: Check distance if other checks didn't work
			if (!currentlyInCar)
			{
				var cameraPos = mainCamera.transform.position;
				
				for (int i = 0; i < GameData.Instance.carLoaders.Length; i++)
				{
					var carLoader = GameData.Instance.carLoaders[i];
					if (carLoader == null || carLoader.gameObject == null) continue;
					if (string.IsNullOrEmpty(carLoader.carToLoad)) continue;

					// Business Logic: Check if camera is very close to car interior (within 1.2 meters)
					// This indicates player is likely inside the car
					var carPos = carLoader.transform.position;
					var distance = Vector3.Distance(cameraPos, carPos);
					
					if (distance < 1.2f)
					{
						currentlyInCar = true;
						currentCarLoaderID = i;
						break;
					}
				}
			}
		}

		// Business Logic: Only send packet if car state changed
		if (currentlyInCar != wasInCar || currentCarLoaderID != lastCarLoaderID)
		{
			wasInCar = currentlyInCar;
			lastCarLoaderID = currentCarLoaderID;
			
			ClientSend.PlayerInCarPacket(currentlyInCar, currentCarLoaderID);
			MelonLogger.Msg($"[CarEnterExit->CheckCarState] Player car state changed: InCar={currentlyInCar}, CarLoaderID={currentCarLoaderID}");
		}

		// Business Logic: If player is in car, check engine sound state
		if (currentlyInCar && currentCarLoaderID >= 0)
		{
			CheckEngineSound(currentCarLoaderID);
		}
	}

	private static bool lastEnginePlaying = false;
	private static float lastEngineCheckTime = 0f;
	private static readonly float engineCheckInterval = 0.1f; // Check every 0.1 seconds for smoother sound sync

	/// <summary>
	/// Checks if the car engine is running and synchronizes the engine sound.
	/// </summary>
	private static void CheckEngineSound(int carLoaderID)
	{
		if (Time.time - lastEngineCheckTime < engineCheckInterval)
			return;

		lastEngineCheckTime = Time.time;

		if (GameData.Instance == null || GameData.Instance.carLoaders == null) return;
		if (carLoaderID < 0 || carLoaderID >= GameData.Instance.carLoaders.Length) return;

		var carLoader = GameData.Instance.carLoaders[carLoaderID];
		if (carLoader == null) return;

		// Business Logic: Check if engine sound is playing by checking AudioSource
		// Observation: Car engine sounds are typically on the car's AudioSource component
		// Note: Temporarily disabled due to AudioModule reference issues - will be re-enabled when AudioModule DLL is available
		// TODO: Re-enable engine sound detection when UnityEngine.AudioModule.dll is added to project references
		bool enginePlaying = false;
		float rpm = 0f;
		
		// Business Logic: For now, we'll skip engine sound detection to avoid compilation errors
		// The engine sound synchronization can be implemented later when the AudioModule reference is available
		// This doesn't affect the core functionality of hiding/showing players when they enter/exit cars

		// Business Logic: Only send packet if engine state changed
		if (enginePlaying != lastEnginePlaying)
		{
			lastEnginePlaying = enginePlaying;
			ClientSend.CarEngineSoundPacket(carLoaderID, enginePlaying, rpm);
			MelonLogger.Msg($"[CarEnterExit->CheckEngineSound] Engine sound state changed: Playing={enginePlaying}, CarLoaderID={carLoaderID}, RPM={rpm}");
		}
	}
}

