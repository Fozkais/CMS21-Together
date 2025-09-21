using System.Collections;
using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.Shared.Data.Vanilla.Cars;
using HarmonyLib;
using UnityEngine;
using TM = ToolsMoveManager;

namespace CMS21Together.ClientSide.Data.Garage.Tools;

[HarmonyPatch]
public static class ToolsMoveManager
{
	public static bool listenToMove = true;

	public static void Reset()
	{
		listenToMove = true;
	}

	public static IEnumerator UpdateToolMove(IOSpecialType tool, ModCarPlace place, bool playSound)
	{
		while (!GameData.isReady)
			yield return new WaitForSeconds(0.25f);
		yield return new WaitForEndOfFrame();
		
		listenToMove = false;
		if (place == ModCarPlace.none)
			GameData.Instance.toolsMoveManager.SetOnDefaultPosition(tool);
		else
			GameData.Instance.toolsMoveManager.MoveTo(tool, (CarPlace)place, playSound);
	}

	[HarmonyPatch(typeof(TM), nameof(TM.MoveTo))]
	[HarmonyPrefix]
	public static void MoveToolPatch(IOSpecialType tool, CarPlace place, bool playSound)
	{
		if (!Client.Instance.isConnected || !listenToMove)
		{
			listenToMove = true;
			return;
		}

		ClientSend.ToolPositionPacket(tool, (ModCarPlace)place, playSound);
	}

	[HarmonyPatch(typeof(TM), nameof(TM.SetOnDefaultPosition))]
	[HarmonyPrefix]
	public static void ResetToolPosPatch(IOSpecialType tool)
	{
		if (!Client.Instance.isConnected || !listenToMove)
		{
			listenToMove = true;
			return;
		}

		ClientSend.ToolPositionPacket(tool, ModCarPlace.none);
	}
}