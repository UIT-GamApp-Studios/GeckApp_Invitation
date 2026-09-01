using UnityEngine;

public class Mascot : MonoBehaviour
{
    [SerializeField] private MediaGameConfig config;

    public bool IsCaptured { get; private set; } = false;

    private float destroyYThreshold;

    public void Initialize(MediaGameConfig gameConfig, float topBoundaryY)
    {
        config = gameConfig;
        destroyYThreshold = topBoundaryY + 1.0f;
    }

    private void Update()
    {
        transform.Translate(Vector3.up * config.mascotSpeed * Time.deltaTime);

        if (transform.position.y > destroyYThreshold)
        {
            Destroy(gameObject);
        }
    }

    public PhotoResult EvaluatePhotoQuality(Vector2 lensCenter, float lensRadius)
    {
        float distance = Vector2.Distance(transform.position, lensCenter);
        
        float perfectMaxDistance = (lensRadius - config.mascotRadius) + config.perfectTolerance;
        
        float goodMaxDistance = lensRadius + config.mascotRadius;

        if (distance <= perfectMaxDistance)
        {
            return PhotoResult.Perfect;
        }
        else if (distance < goodMaxDistance)
        {
            return PhotoResult.Good;
        }

        return PhotoResult.Miss;
    }

    public void MarkAsCaptured()
    {
        IsCaptured = true;
    }
}