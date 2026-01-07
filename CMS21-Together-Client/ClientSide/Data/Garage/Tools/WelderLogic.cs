using System.Collections;
using CMS;
using CMS21Together.ClientSide.Data.Handle;
using HarmonyLib;
using UnityEngine;
using WelderL = WelderLogic;

namespace CMS21Together.ClientSide.Data.Garage.Tools;

[HarmonyPatch]
public static class WelderLogic
{
	public static bool listen = true;

	[HarmonyPatch(typeof(WelderL), nameof(WelderL.DoWorkAnim))]
	[HarmonyPrefix]
	public static void DoWorkAnimFix(CarLoader carLoader, WelderL __instance)
	{
		if (!Client.Instance.isConnected || !listen)
		{
			listen = true;
			return;
		}

		var carLoaderID = carLoader.gameObject.name[10] - '0' - 1;

		ClientSend.WelderPacket(carLoaderID);
	}

	public static IEnumerator UseWelder(int carLoaderID)
	{
		while (!ClientData.GameReady)
			yield return new WaitForSeconds(0.25f);
		yield return new WaitForEndOfFrame();

		listen = false;
		var l = GameData.Instance.welderLogic;
		var carLoader = GameData.Instance.carLoaders[carLoaderID];

		StartAnimFix(carLoader, l);
		carLoader.TweenCondition("body", 1f, l.effectTime);
		carLoader.SetDent(carLoader.GetCarPart("body"), 1f);
		carLoader.SetDent(carLoader.GetCarPart("details"), 1f);
		yield return FinishAnimFix(carLoader, l);
		l.EnableInteractiveObjects();
	}

	private static void StartAnimFix(CarLoader carLoader, WelderL l)
	{
		l.interactiveObject.enabled = false;
		carLoader.CloseCar();
		carLoader.EnableIO(false);
		var connectedLifter = carLoader.GetConnectedLifter();
		if (connectedLifter != null)
		{
			connectedLifter.ButtonDown.enabled = false;
			connectedLifter.ButtonUp.enabled = false;
		}

		l.carCollider = carLoader.GetModel().transform.Find("body");
		var main = l.particles.main;
		main.duration = l.effectTime;
		var num = 1f / l.carCollider.lossyScale.x;
		main.startSize = 0.02f * num;
		l.particles.limitVelocityOverLifetime.limit = 3f * num;
		var velocityOverLifetime = l.particles.velocityOverLifetime;
		velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(num, -num);
		velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(1.5f * num, 0f);
		velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(num, -num);
		l.shapeModule = l.particles.shape;
		if (l.carCollider)
		{
			l.shapeModule.meshRenderer = l.carCollider.GetComponent<MeshRenderer>();
			SoundManager.Get().PlaySFX(l.sfx, l.carCollider.transform.position);
			l.particles.Play();
		}
	}

	private static IEnumerator FinishAnimFix(CarLoader carLoader, WelderL l)
	{
		while (l.particles.IsAlive()) yield return YieldInstructions.WaitForEndOfFrame;
		l.shapeModule.meshRenderer = null;
		yield return YieldInstructions.WaitForEndOfFrame;
		carLoader.EnableIO(true);
		var connectedLifter = carLoader.GetConnectedLifter();
		if (connectedLifter != null)
		{
			connectedLifter.ButtonDown.enabled = true;
			connectedLifter.ButtonUp.enabled = true;
		}

		l.interactiveObject.enabled = true;
		yield break;
	}
}