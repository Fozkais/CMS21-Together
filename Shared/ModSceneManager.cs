using CMS21Together.ClientSide;
using CMS21Together.ClientSide.Data;
using CMS21Together.ClientSide.Handle;
using CMS21Together.Shared.Data;
using MelonLoader;
using UnityEngine.SceneManagement;

namespace CMS21Together.Shared
{
    public class ModSceneManager
    {
         public static bool needToRefreshGarage;

        public static bool isInMenu()
        {
            if (SceneManager.GetActiveScene().name == "Menu")
                return true;
            return false;
        }

        public static GameScene currentScene()
        {
            if (isInBarn())
                return GameScene.barn;
            else if (isInGarage())
                return GameScene.garage;
            else if (isInJunkyard())
                return GameScene.junkyard;
            else if (isInDealer())
                return GameScene.auto_salon;
            else
                return GameScene.garage;
            
            
        }
        public static bool isInGarage(Player player = null)
        {
            if (player == null)
            {
                if (SceneManager.GetActiveScene().name == "garage")
                {
                    return true;
                }
                return false;
            }
            else
            {
                if (player.scene == GameScene.garage)
                {
                    return true;
                }
                return false;
            }
        }
        public static bool isInJunkyard()
        {
            if (SceneManager.GetActiveScene().name == "Junkyard")
            {
                needToRefreshGarage = true;
                return true;
            }
            return false;
        }
        public static bool isInDealer()
        {
            if (SceneManager.GetActiveScene().name == "Auto_salon")
            {
                needToRefreshGarage = true;
                return true;
            }
            return false;
        }
        public static bool isInBarn()
        {
            if (SceneManager.GetActiveScene().name == "Barn")
            {
                needToRefreshGarage = true;
                return true;
            }
            return false;
        }


        public static void UpdatePlayerScene()
        {
            ClientData.players[Client.Instance.Id].scene = currentScene();
            ClientSend.SendSceneChange(currentScene());
        }
    }
}