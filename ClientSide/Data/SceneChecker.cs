using CMS21MP.ClientSide.DataHandle;
using CMS21MP.SharedData;
using MelonLoader;
using UnityEngine.SceneManagement;

namespace CMS21MP.ClientSide.Data
{
    public static class SceneChecker
    {
        public static bool needToRefreshGarage;

        public static bool isNotInMenu()
        {
            if (SceneManager.GetActiveScene().name != "Menu")
                return true;
            return false;
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
                if (player.scene == "garage")
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


        public static void UpdatePlayerScene(string scene)
        {
            MelonLogger.Msg("Trigger UpdateScene : " + scene);
            if (GameData.DataInitialzed)
            {
                ClientData.serverPlayers[Client.Instance.Id].scene = scene;
                ClientSend.SendSceneChange(scene);
                MelonLogger.Msg("Sended Scene Update :" + scene);
            }
        }
        
        public static GameScene wichScene(string sceneName)
        {
            GameScene scene = GameScene.unknow;
            switch (sceneName)
            {
                case "garage":
                    scene = GameScene.garage;
                    break;
                case "Junkyard":
                    scene = GameScene.junkyard;
                    break;
                case "Auto_salon":
                    scene = GameScene.auto_salon;
                    break;
                case "Barn":
                    scene = GameScene.barn;
                    break;
            }
            return scene;
        }
    }
}