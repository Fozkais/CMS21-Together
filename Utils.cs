using UnityEngine;
using MelonLoader;

namespace CMS21MP
{
    public class Utils : MonoBehaviour
    {
        public static Utils instance;
        
        public void Initialize()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                MelonLogger.Msg("Instance already exists, destroying object!");
                Destroy(this);
            }
        }
        
    }
}