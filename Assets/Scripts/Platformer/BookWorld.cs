using UnityEngine;

[DefaultExecutionOrder(100)]
public class BookWorld : MonoBehaviour
{
    [SerializeField] SpriteRenderer page;
    [SerializeField] Camera worldCamera;
    [SerializeField] Color deskColor = new Color(0.23f, 0.23f, 0.24f);
    [SerializeField] bool lockToCamera = true;
    [SerializeField] bool keepEditorScale = true;
    [SerializeField] float cover = 1.02f;
    [SerializeField] float zDistance = 12f;

    void Awake()
    {
        if (page == null) page = GetComponent<SpriteRenderer>();
        if (worldCamera == null) worldCamera = Camera.main;
        if (page != null) page.sortingOrder = -40;
        if (worldCamera != null)
        {
            worldCamera.backgroundColor = deskColor;
            worldCamera.clearFlags = CameraClearFlags.SolidColor;
        }
    }

    public void BindAndFit(SpriteRenderer renderer, Camera cam)
    {
        page = renderer;
        worldCamera = cam;
    }

    void LateUpdate()
    {
        if (!lockToCamera) return;
        PinToCamera();
    }

    void PinToCamera()
    {
        if (worldCamera == null) worldCamera = Camera.main;
        if (worldCamera == null || page == null || page.sprite == null) return;

        var camPos = worldCamera.transform.position;
        transform.position = new Vector3(camPos.x, camPos.y, camPos.z + zDistance);
        transform.rotation = Quaternion.identity;

        if (keepEditorScale) return;

        float worldH = 2f * worldCamera.orthographicSize * cover;
        float worldW = worldH * worldCamera.aspect;
        float spriteW = page.sprite.rect.width / page.sprite.pixelsPerUnit;
        float spriteH = page.sprite.rect.height / page.sprite.pixelsPerUnit;
        if (spriteW < 0.01f || spriteH < 0.01f) return;

        float scale = Mathf.Max(worldW / spriteW, worldH / spriteH);
        transform.localScale = new Vector3(scale, scale, 1f);
    }
}
