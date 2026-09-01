using System;
using UnityEngine;

public static class GameEvents
{
    public static Action<Vector2> OnLensBounce;
    public static Action<PhotoResult, int, int> OnPhotoTaken;
    public static Action OnTimeWarning;
    public static Action<bool> OnGameEnd;
}