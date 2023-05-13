using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CMS21MP.ClientSide.Functionnality
{
    public static class SceneSwaping
    {
        
        public static void UpdatePlayerScene()
        {
            if (PlayerManager.players.Count > 0)
            {
                foreach (KeyValuePair<int, PlayerInfo> player in PlayerManager.players)
                {
                    if (player.Value.activeScene != SceneManager.GetActiveScene().name)
                    {
                        if (GameObject.Find($"{player.Value.username}") != null && player.Value.id != Client.instance.myId)
                        {
                            //Object.Destroy(GameObject.Find($"{player.Value.username}"));
                            //PlayerManager.players.Remove(player.Value.id);
                            GameObject.Find($"{player.Value.username}").gameObject.GetComponent<MeshRenderer>().enabled = false;
                        }
                    }
                    else
                    {
                        if (GameObject.Find($"{player.Value.username}") != null)
                        {
                            if (!GameObject.Find($"{player.Value.username}").gameObject.GetComponent<MeshRenderer>().enabled)
                            {
                                GameObject.Find($"{player.Value.username}").gameObject.GetComponent<MeshRenderer>().enabled = true;
                            }
                        }
                    }
                   
                }
            }
        }
    }
}