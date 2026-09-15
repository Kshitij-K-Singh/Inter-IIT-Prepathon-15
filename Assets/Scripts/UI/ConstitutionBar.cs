using UnityEngine;
using UnityEngine.UI;

public class ConstitutionBar : MonoBehaviour
{
    [SerializeField] Transform chipRoot;
    [SerializeField] Font font;
    [SerializeField] Color amendmentColor = new Color(0.85f, 0.72f, 0.22f);
    [SerializeField] Color erratumColor = new Color(0.75f, 0.18f, 0.18f);

    public void Bind(Transform root, Font uiFont)
    {
        chipRoot = root;
        font = uiFont;
    }

    void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RulesChanged += Rebuild;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RulesChanged -= Rebuild;
    }

    public void Rebuild()
    {
        if (chipRoot == null) return;

        for (int i = chipRoot.childCount - 1; i >= 0; i--)
            Destroy(chipRoot.GetChild(i).gameObject);

        var rules = GameManager.Instance != null ? GameManager.Instance.Rules : null;
        if (rules == null) return;

        var chipFont = font != null ? font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (chipFont == null) chipFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

        foreach (var rule in rules)
        {
            var go = new GameObject(rule.kind.ToString(), typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            go.transform.SetParent(chipRoot, false);
            var image = go.GetComponent<Image>();
            image.color = rule.isErratum ? erratumColor : amendmentColor;
            var layout = go.GetComponent<LayoutElement>();
            layout.minHeight = 28f;
            layout.minWidth = 88f;
            layout.preferredHeight = 28f;
            layout.preferredWidth = 110f;

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
            labelGo.transform.SetParent(go.transform, false);
            var labelRt = labelGo.GetComponent<RectTransform>();
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = new Vector2(6f, 2f);
            labelRt.offsetMax = new Vector2(-6f, -2f);
            var text = labelGo.GetComponent<Text>();
            text.font = chipFont;
            text.fontSize = 14;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.black;
            text.text = rule.kind.ToString();
            text.raycastTarget = false;
        }
    }
}
