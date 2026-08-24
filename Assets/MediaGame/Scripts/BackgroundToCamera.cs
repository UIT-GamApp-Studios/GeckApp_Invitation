using UnityEngine;

[ExecuteAlways]
public class FitBackgroundToCamera : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        Fit();
    }

    private void Start()
    {
        Fit();
    }

    public void Fit()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null || spriteRenderer.sprite == null || mainCamera == null) return;

        transform.localScale = Vector3.one;

        float spriteWidth = spriteRenderer.sprite.bounds.size.x;
        float spriteHeight = spriteRenderer.sprite.bounds.size.y;

        float worldScreenHeight = mainCamera.orthographicSize * 2.0f;
        float worldScreenWidth = worldScreenHeight * mainCamera.aspect;

        Vector3 scale = transform.localScale;
        scale.x = worldScreenWidth / spriteWidth;
        scale.y = worldScreenHeight / spriteHeight;

        transform.localScale = scale;
        transform.position = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y, transform.position.z);
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (!Application.isPlaying)
        {
            Fit();
        }
    }
#endif
}