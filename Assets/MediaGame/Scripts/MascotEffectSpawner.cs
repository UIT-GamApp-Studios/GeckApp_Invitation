using UnityEngine;

public class MascotEffectSpawner : MonoBehaviour
{
    [SerializeField] private ParticleSystem perfectParticlePrefab;

    private void OnEnable()
    {
        GameEvents.OnPhotoTaken += HandlePhotoTaken;
    }

    private void OnDisable()
    {
        GameEvents.OnPhotoTaken -= HandlePhotoTaken;
    }

    private void HandlePhotoTaken(PhotoResult result, int scoreAdded, int currentScore)
    {
        if (result == PhotoResult.Perfect && perfectParticlePrefab != null)
        {
            ParticleSystem effectInstance = Instantiate(perfectParticlePrefab, transform.position, Quaternion.identity);
            Destroy(effectInstance.gameObject, effectInstance.main.duration + effectInstance.main.startLifetime.constantMax);
        }
    }
}