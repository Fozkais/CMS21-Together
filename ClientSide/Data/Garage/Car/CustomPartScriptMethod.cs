using System.Collections;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Garage.Car;

public static class CustomPartScriptMethod
{
	public static IEnumerator ShowMounted(PartScript partScript) // avoid gamemode change when mounting piece
	{
		partScript.IsUnmounted = false;
		if (partScript.ShouldUnmountWith())
			foreach (var item in partScript.unmountWith)
				item.MountByGroup(true);
		yield return new WaitForSeconds(0.5f);
		partScript.UnblockBlockParts(false);
		GameObject[] array = partScript.enableOnUnmount;
		for (var i = 0; i < array.Length; i++) array[i].SetActive(false);
		array = partScript.disableOnUnmount;
		foreach (var val in array)
		{
			val.GetComponent<MeshRenderer>().enabled = true;
			if (val.GetComponent<MeshCollider>()) val.GetComponent<MeshCollider>().enabled = true;
		}

		array = partScript.hideWhenUnmontingMounting;
		for (var i = 0; i < array.Length; i++) array[i].GetComponent<Renderer>().enabled = true;
		partScript.OnMountFinished?.Invoke();
	}
}