using System.Collections.Generic;
using UnityEngine;

public class GachaRewardManager : MonoBehaviour
{
    public static GachaRewardManager Instance { get; private set; }

    private const string TOKEN_KEY = "Gacha_Token_Count";
    private const string REWARDED_SCENES_KEY = "Gacha_Rewarded_Scenes";
    private const string SAVED_GACHA_CARDS_KEY = "Gacha_Saved_Cards";

    private int currentTokens;
    private HashSet<string> rewardedScenes = new HashSet<string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public int GetTokenCount() => currentTokens;

    // Kiểm tra xem đã đủ 3 Token để MỞ KHÓA VĨNH VIỄN Gacha chưa
    public bool IsGachaUnlocked() => currentTokens >= 3;

    public bool EarnTokenForScene(string sceneName)
    {
        if (rewardedScenes.Contains(sceneName)) return false;

        rewardedScenes.Add(sceneName);
        currentTokens++;
        SaveData();
        return true;
    }

    // --- LƯU & ĐỌC KẾT QUẢ GACHA ---
    public void SaveGachaResult(List<string> cardNames)
    {
        string data = string.Join(",", cardNames);
        PlayerPrefs.SetString(SAVED_GACHA_CARDS_KEY, data);
        PlayerPrefs.Save();
    }

    public List<string> GetSavedGachaResult()
    {
        string data = PlayerPrefs.GetString(SAVED_GACHA_CARDS_KEY, "");
        if (string.IsNullOrEmpty(data)) return null;

        return new List<string>(data.Split(','));
    }

    public bool HasSavedGachaResult()
    {
        return PlayerPrefs.HasKey(SAVED_GACHA_CARDS_KEY) && !string.IsNullOrEmpty(PlayerPrefs.GetString(SAVED_GACHA_CARDS_KEY));
    }

    #region Save & Load
    private void SaveData()
    {
        PlayerPrefs.SetInt(TOKEN_KEY, currentTokens);
        string scenesData = string.Join(",", rewardedScenes);
        PlayerPrefs.SetString(REWARDED_SCENES_KEY, scenesData);
        PlayerPrefs.Save();
    }

    private void LoadData()
    {
        currentTokens = PlayerPrefs.GetInt(TOKEN_KEY, 0);
        rewardedScenes.Clear();
        string scenesData = PlayerPrefs.GetString(REWARDED_SCENES_KEY, "");
        if (!string.IsNullOrEmpty(scenesData))
        {
            string[] splitScenes = scenesData.Split(',');
            foreach (var scene in splitScenes)
            {
                if (!string.IsNullOrEmpty(scene)) rewardedScenes.Add(scene);
            }
        }
    }
    #endregion
}