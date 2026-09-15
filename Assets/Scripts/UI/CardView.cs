using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] Text titleLabel;
    [SerializeField] Text bodyLabel;
    [SerializeField] Image background;

    public CardData Data { get; private set; }
    ThresholdUI owner;
    Vector3 baseScale = Vector3.one;

    public void Bind(Text title, Text body, Image image)
    {
        titleLabel = title;
        bodyLabel = body;
        background = image;
    }

    void Awake()
    {
        baseScale = transform.localScale;
        if (background != null)
        {
            background.preserveAspect = true;
            background.color = Color.white;
        }
    }

    public void Setup(ThresholdUI ui, CardData data)
    {
        owner = ui;
        Data = data;
        ShowAmendment();
        SetSelected(false);
    }

    public void ShowAmendment()
    {
        if (Data == null) return;
        if (titleLabel != null)
        {
            titleLabel.text = Data.title;
            titleLabel.color = new Color(0.12f, 0.08f, 0.08f);
        }
        if (bodyLabel != null)
        {
            bodyLabel.text = Data.amendmentText;
            bodyLabel.color = new Color(0.18f, 0.12f, 0.10f);
        }
        ApplyArt(Data.amendmentArt);
    }

    public void ShowErratum()
    {
        if (Data == null) return;
        if (titleLabel != null)
        {
            titleLabel.text = Data.title;
            titleLabel.color = new Color(0.55f, 1f, 0.92f);
        }
        if (bodyLabel != null)
        {
            bodyLabel.text = Data.erratumText;
            bodyLabel.color = Color.white;
        }
        ApplyArt(Data.erratumArt != null ? Data.erratumArt : Data.amendmentArt);
    }

    public void SetSelected(bool selected)
    {
        transform.localScale = baseScale * (selected ? 1.06f : 1f);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        GameAudio.Instance?.PlayHover();
        owner?.OnCardHovered(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        owner?.OnCardUnhovered(this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        owner?.OnCardClicked(this);
    }

    void ApplyArt(Sprite sprite)
    {
        if (background == null) return;
        background.color = Color.white;
        background.preserveAspect = true;
        if (sprite != null) background.sprite = sprite;
    }
}
