using UnhollowerBaseLib;

namespace CMS21Together.Utils;

public static class SaveUtils
{
	public static void ExtendProfileDataSize()
	{
		if (Singleton<GameManager>.Instance.GameDataManager.ProfileData.Length == 5)
			return;

		Il2CppReferenceArray<ProfileData> profileData = new(5);
		profileData[0] = Singleton<GameManager>.Instance.GameDataManager.ProfileData[0];
		profileData[1] = Singleton<GameManager>.Instance.GameDataManager.ProfileData[1];
		profileData[2] = Singleton<GameManager>.Instance.GameDataManager.ProfileData[2];
		profileData[3] = Singleton<GameManager>.Instance.GameDataManager.ProfileData[3];
		
		Singleton<GameManager>.Instance.GameDataManager.ProfileData = profileData;
	}
	
}