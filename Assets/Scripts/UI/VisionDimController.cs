using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(100)]
public class VisionDimController : MonoBehaviour
{
    public static VisionDimController Instance { get; private set; }

    [SerializeField] Image overlay;
    [SerializeField] float holeWorldRadius = 0.55f;
    [SerializeField] float holeSoftWorld = 0.18f;
    [SerializeField] Vector3 holeOffset = new Vector3(0f, 0.8f, 0f);

    Material holeMat;
    static Sprite whiteSprite;
    bool dimmed;

    void Awake()
    {
        Instance = this;
        if (overlay == null) overlay = GetComponent<Image>();
        EnsureMaterial();
        HideOverlay();
        transform.SetAsFirstSibling();
    }

    void Start()
    {
        Apply();
    }

    void OnEnable()
    {
        Instance = this;
        if (GameManager.Instance != null)
            GameManager.Instance.RulesChanged += Apply;
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RulesChanged -= Apply;
        HideOverlay();
        if (Instance == this) Instance = null;
    }

    void OnDestroy()
    {
        if (holeMat != null)
        {
            if (Application.isPlaying) Destroy(holeMat);
            else DestroyImmediate(holeMat);
            holeMat = null;
        }
    }

    public void Bind(Image image)
    {
        overlay = image;
        EnsureMaterial();
    }

    public void Bind(GridWorld world, GridMover mover) => Apply();
    public void Visit(Vector3Int cell) => Apply();

    public void Apply()
    {
        if (RuleBook.VisionDim()) ShowDarkness();
        else HideOverlay();
    }

    void LateUpdate()
    {
        if (dimmed) UpdateHole();
    }

    void ShowDarkness()
    {
        dimmed = true;
        EnsureMaterial();
        if (overlay == null) return;

        overlay.enabled = true;
        overlay.raycastTarget = false;
        overlay.color = Color.black;
        if (overlay.sprite == null)
            overlay.sprite = WhiteSprite();
        if (holeMat != null)
            overlay.material = holeMat;
        UpdateHole();
    }

    void HideOverlay()
    {
        dimmed = false;
        if (overlay == null) return;
        overlay.enabled = false;
        overlay.raycastTarget = false;
    }

    void UpdateHole()
    {
        if (holeMat == null || overlay == null) return;

        var theo = TheoController.Instance;
        var cam = Camera.main;
        if (theo == null || cam == null) return;

        Vector3 world = theo.transform.position + holeOffset;
        var canvas = overlay.canvas;
        Camera eventCam = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera
            : null;
        Vector2 screen = RectTransformUtility.WorldToScreenPoint(cam, world);
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(overlay.rectTransform, screen, eventCam, out var local))
        {
            var rect = overlay.rectTransform.rect;
            float u = Mathf.InverseLerp(rect.xMin, rect.xMax, local.x);
            float v = Mathf.InverseLerp(rect.yMin, rect.yMax, local.y);
            holeMat.SetVector("_Center", new Vector4(u, v, 0f, 0f));
        }

        float viewH = cam.orthographic ? cam.orthographicSize * 2f : 12f;
        holeMat.SetFloat("_Radius", holeWorldRadius / Mathf.Max(viewH, 0.01f));
        holeMat.SetFloat("_Soft", holeSoftWorld / Mathf.Max(viewH, 0.01f));
        holeMat.SetColor("_Color", Color.black);
        holeMat.SetVector("_PillarCenter", new Vector4(-1f, -1f, 0f, 0f));
    }

    void EnsureMaterial()
    {
        if (holeMat != null) return;
        var shared = Resources.Load<Material>("DarknessHole");
        var shader = shared != null ? shared.shader : Shader.Find("UI/DarknessHole");
        if (shader == null) return;
        holeMat = new Material(shader);
        holeMat.SetColor("_Color", Color.black);
    }

    static Sprite WhiteSprite()
    {
        if (whiteSprite != null) return whiteSprite;
        var tex = Texture2D.whiteTexture;
        whiteSprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        return whiteSprite;
    }
}
