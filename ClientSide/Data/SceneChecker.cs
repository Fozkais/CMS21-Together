using UnityEngine.SceneManagement;

namespace CMS21MP.ClientSide.Data
{
    public static class SceneChecker
    {
        public static bool isInGarage()
        {
            if(SceneManager.GetActiveScene().name == "garage")
                return true;
            return false;
        }
        public static bool isInJunkyard()
        {
            if(SceneManager.GetActiveScene().name == "Junkyard")
                return true;
            return false;
        }
        public static bool isInDealer()
        {
            if(SceneManager.GetActiveScene().name == "Auto_salon")
                return true;
            return false;
        }
        public static bool isInBarn()
        {
            if(SceneManager.GetActiveScene().name == "Barn")
                return true;
            return false;
        }
    }
}