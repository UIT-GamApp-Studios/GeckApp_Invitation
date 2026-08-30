using UnityEngine;
using System;

public class TokenManager : MonoBehaviour
{
    public static TokenManager Instance { get; private set; }

    [Tooltip("Số token khởi đầu nếu chưa có dữ liệu lưu trước đó")]
    [SerializeField] private int startingTokens = 0;

    private const string SAVE_KEY = "PlayerTokenCount";

    public int CurrentTokens { get; private set; }

    public static event Action<int> OnTokenChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        CurrentTokens = PlayerPrefs.GetInt(SAVE_KEY, startingTokens);
    }

    private void Start()
    {
        OnTokenChanged?.Invoke(CurrentTokens);
    }

    public void AddTokens(int amount)
    {
        if (amount <= 0) return;
        CurrentTokens += amount;
        SaveAndNotify();
    }

    public bool SpendTokens(int amount)
    {
        if (amount <= 0) return true;
        if (CurrentTokens < amount) return false;

        CurrentTokens -= amount;
        SaveAndNotify();
        return true;
    }

    public void SetTokens(int amount)
    {
        CurrentTokens = Mathf.Max(0, amount);
        SaveAndNotify();
    }

    private void SaveAndNotify()
    {
        PlayerPrefs.SetInt(SAVE_KEY, CurrentTokens);
        PlayerPrefs.Save();
        OnTokenChanged?.Invoke(CurrentTokens);
    }
}