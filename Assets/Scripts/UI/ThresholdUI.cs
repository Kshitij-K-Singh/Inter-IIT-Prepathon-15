using UnityEngine;
using UnityEngine.UI;

public class ThresholdUI : MonoBehaviour
{
    [SerializeField] GameObject overlayRoot;
    [SerializeField] CardView cardA;
    [SerializeField] CardView cardB;
    [SerializeField] CardView cardC;
    [SerializeField] Text footer;
    [SerializeField] Button confirmButton;

    CardView hovered;
    CardView selected;
    bool open;

    public bool IsOpen => open;

    public void Bind(GameObject root, CardView a, CardView b, CardView c, Text footerLabel, Button confirm)
    {
        overlayRoot = root;
        cardA = a;
        cardB = b;
        cardC = c;
        footer = footerLabel;
        confirmButton = confirm;
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveListener(Confirm);
            confirmButton.onClick.AddListener(Confirm);
        }
    }

    void Awake()
    {
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveListener(Confirm);
            confirmButton.onClick.AddListener(Confirm);
        }
    }

    public void Open()
    {
        var triad = GameManager.Instance != null ? GameManager.Instance.Chapters?.Current?.triad : null;
        if (triad == null)
            return;


        cardA?.Setup(this, triad != null ? triad.cardA : null);
        cardB?.Setup(this, triad != null ? triad.cardB : null);
        cardC?.Setup(this, triad != null ? triad.cardC : null);

        hovered = null;
        selected = null;
        RefreshFaces();
        RefreshFooter();

        open = true;
        if (overlayRoot != null) overlayRoot.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Close(bool restoreTime = true)
    {
        open = false;
        hovered = null;
        selected = null;
        if (overlayRoot != null) overlayRoot.SetActive(false);
        if (restoreTime) Time.timeScale = 1f;
    }

    public void OnCardHovered(CardView card)
    {
        hovered = card;
        RefreshFaces();
        RefreshFooter();
    }

    public void OnCardUnhovered(CardView card)
    {
        if (hovered == card) hovered = null;
        RefreshFaces();
        RefreshFooter();
    }

    public void OnCardClicked(CardView card)
    {
        if (card == null) return;
        if (selected == card)
        {
            Confirm();
            return;
        }

        selected = card;
        GameAudio.Instance?.PlaySelect();
        RefreshFaces();
        RefreshFooter();
    }

    void Confirm()
    {
        var chosen = selected != null ? selected : hovered;
        if (chosen == null || chosen.Data == null) return;
        GameManager.Instance?.ApplyChoice(chosen.Data);
    }

    void RefreshFaces()
    {
        var focus = selected != null ? selected : hovered;
        SetFace(cardA, focus);
        SetFace(cardB, focus);
        SetFace(cardC, focus);
    }

    void SetFace(CardView card, CardView focus)
    {
        if (card == null) return;
        if (focus == null || card == focus) card.ShowAmendment();
        else card.ShowErratum();
        card.SetSelected(selected == card);
    }

    void RefreshFooter()
    {
        if (footer == null) return;
        var gain = selected != null ? selected : hovered;
        if (gain == null || gain.Data == null)
        {
            footer.text = "Choose an amendment.";
            return;
        }

        string a = gain.Data.title;
        string b = "";
        string c = "";
        int i = 0;
        foreach (var card in new[] { cardA, cardB, cardC })
        {
            if (card == null || card == gain || card.Data == null) continue;
            if (i == 0) b = card.Data.title;
            else c = card.Data.title;
            i++;
        }

        footer.text = $"You gain {a}. You accept {b} and {c}.";
    }
}
