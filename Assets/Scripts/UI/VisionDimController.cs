using UnityEngine;
using UnityEngine.UI;

public class VisionDimController : MonoBehaviour
{
    public static VisionDimController Instance { get; private set; }

    [SerializeField] Image overlay;
    [SerializeField] Color dim = new Color(0f, 0f, 0f, 1f);

    void Awake()
    {
        Instance = this;
    }

    public void Bind(Image image)
    {
        overlay = image;
    }

    public void Bind(GridWorld world, GridMover mover)
    {
        Apply();
    }

    public void Visit(Vector3Int cell)
    {
        Apply();
    }

    public void Apply()
    {
        if (overlay == null) return;
        bool on = RuleBook.VisionDim();
        overlay.enabled = on;
        overlay.color = dim;
        overlay.raycastTarget = false;
    }
}
