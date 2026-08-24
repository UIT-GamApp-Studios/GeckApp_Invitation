using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Minigame/GameConfig")]
public class MediaGameConfig : ScriptableObject
{
    [Header("Game Loop Settings")]
    public int targetScore = 10;
    public float timeLimit = 30f;
    public float timeWarningThreshold = 5f;

    [Header("Lens Settings")]
    public float lensSpeed = 5f;
    public float lensRadius = 1.2f;
    public float lensAcceleration = 10f;
    public float lensDeceleration = 5f;
    public float bounceMultiplier = 1.5f;

    [Header("Mascot Settings")]
    public float mascotSpeed = 3f;
    public float mascotRadius = 0.5f;
    public float mascotSpawnInterval = 1.5f;
    public int maxActiveMascots = 4;
    public float spawnOverlapRadius = 1.0f;

    [Header("Photo Precision")]
    [Tooltip("Độ dung sai cho phép lệch một chút vẫn tính Perfect")]
    public float perfectTolerance = 0.2f; 

    [Header("Photo Feedback UI Settings")]
    public string perfectText = "PERFECT!";
    public Color perfectColor = Color.green;

    public string goodText = "GOOD!";
    public Color goodColor = Color.yellow;

    public string missText = "MISS!";
    public Color missColor = Color.red;
}