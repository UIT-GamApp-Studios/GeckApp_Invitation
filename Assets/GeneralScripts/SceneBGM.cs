using UnityEngine;
using System;

public class SceneBGM : MonoBehaviour
{
    [System.Serializable]
    public class BGMTrack
    {
        public string trackName;
        public AudioClip clip;
        
        [Range(0f, 2f)] 
        public float baseVolume = 1f;
    }

    [Header("BGM List for this Scene")]
    public BGMTrack[] bgmList;
}