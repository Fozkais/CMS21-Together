
using System.Collections.Generic;
using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.Shared.Data.Vanilla;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Player;

public static class Movement
{

    private static float minDistance = 0.01f;
    private static Vector3 lastPosition;


    public static void SetSpawnPosition(int id, Vector3Serializable position)
    {
        if(!ClientData.Instance.ConnectedClients.ContainsKey(id)) return;

        var player = ClientData.Instance.ConnectedClients[id];
        
        if(player.scene != ClientData.UserData.scene) return;
        if(player.userObject == null) return;

        player.userObject.transform.position = position.toVector3();
    }
    
    public static void UpdatePosition(int id, Vector3Serializable position)
    {
        if(!ClientData.Instance.ConnectedClients.ContainsKey(id)) return;

        var player = ClientData.Instance.ConnectedClients[id];
        
        if(player.scene != ClientData.UserData.scene) return;
        if (player.userObject == null)
        {
            player.SpawnPlayer();
        }

       // player.userObject.transform.Translate(position.toVector3() * Time.deltaTime); without animation method
       if (player.lastPosition != null)
       {
           Vector3 direction = (position.toVector3() - player.lastPosition.toVector3()).normalized;
           float speed = (position.toVector3() - player.lastPosition.toVector3()).magnitude / Time.deltaTime;

           // Mettre à jour les animations
           UpdateAnimations(player.userAnimator, direction, speed);
       }

       // Mettre à jour la position
       player.userObject.transform.position = position.toVector3();
       player.lastPosition = position;
    }
    
    private static void UpdateAnimations(Animator animator, Vector3 direction, float speed)
    {
        float horizontalSpeed = direction.x * speed;
        float verticalSpeed = direction.z * speed;

        animator.SetFloat("Vertical", Mathf.Lerp(animator.GetFloat("Vertical"), verticalSpeed, Time.deltaTime * 10f));
        animator.SetFloat("Horizontal", Mathf.Lerp(animator.GetFloat("Horizontal"), horizontalSpeed, Time.deltaTime * 10f));
    }

    public static void SendPosition()
    {
        if(GameData.Instance.localPlayer == null) return;

        Vector3 position = GameData.Instance.localPlayer.transform.position;
        position.y -= 0.72f; // probably fix player flying
        if (Vector3.Distance(position, lastPosition) > minDistance)
        {
            lastPosition = position;
            ClientSend.PositionPacket(new Vector3Serializable(position));
        }
    }
}