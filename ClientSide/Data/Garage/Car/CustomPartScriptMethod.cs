using System.Collections;
using Il2Cpp;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Garage.Car;

public static class CustomPartScriptMethod
{
    public static IEnumerator ShowMounted(PartScript partScript) // avoid gamemode change when mounting piece
    {
        partScript.IsUnmounted = false;
        if ( partScript.ShouldUnmountWith())
        {
            foreach (PartScript item in  partScript.unmountWith)
            {
                item.MountByGroup(instantSet: true);
            }
        }
        yield return new WaitForSeconds(0.5f);
        partScript.UnblockBlockParts(unblock: false);
        GameObject[] array =  partScript.enableOnUnmount;
        for (int i = 0; i < array.Length; i++)
        {
            array[i].SetActive(false);
        }
        array =  partScript.disableOnUnmount;
        foreach (GameObject val in array)
        {
            (val.GetComponent<MeshRenderer>()).enabled = true;
            if (val.GetComponent<MeshCollider>())
            {
                (val.GetComponent<MeshCollider>()).enabled = true;
            }
        }
        array =  partScript.hideWhenUnmontingMounting;
        for (int i = 0; i < array.Length; i++)
        {
            array[i].GetComponent<Renderer>().enabled = true;
        }
        partScript.OnMountFinished?.Invoke();
    }
}