using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] ChapterController chapterController;
    [SerializeField] ThresholdUI thresholdUI;
    [SerializeField] ConstitutionBar constitutionBar;
    [SerializeField] StoryLog storyLog;
    [SerializeField] GameObject titlePanel;
    [SerializeField] GameObject endingPanel;
    [SerializeField] GameObject deathPanel;
    [SerializeField] Text deathLabel;
    [SerializeField] Button deathRestartButton;
    [SerializeField] Button startButton;
    [SerializeField] Button endingRestartButton;
    [SerializeField] GameObject visionDimOverlay;
    [SerializeField] PlayerDeath playerDeath;

    [SerializeField] List<RuleInstance> rules = new List<RuleInstance>();
    bool awaitingChoice;

    public int CurrentChapterIndex { get; private set; }
    public List<RuleInstance> Rules => rules;
    public ChapterController Chapters => chapterController;
    public bool IsPlaying { get; private set; }
    public bool IsPaused => Time.timeScale == 0f;

    public event Action RulesChanged;

    void Awake()
    {
        Instance = this;
        BindRules();
        Time.timeScale = 1f;
        EnsureDeathPanel();
        if (startButton != null) startButton.onClick.AddListener(StartRun);
        if (endingRestartButton != null) endingRestartButton.onClick.AddListener(RestartRun);
        if (deathRestartButton != null) deathRestartButton.onClick.AddListener(ContinueAfterDeath);
        if (deathPanel != null) deathPanel.SetActive(false);
    }

    void Start()
    {
        ShowTitle();
    }

    public void Bind(
        ChapterController chapters,
        ThresholdUI overlay,
        ConstitutionBar bar,
        StoryLog story,
        GameObject title,
        GameObject ending,
        Button start,
        Button endingRestart,
        GameObject dimmer,
        PlayerDeath death)
    {
        chapterController = chapters;
        thresholdUI = overlay;
        constitutionBar = bar;
        storyLog = story;
        titlePanel = title;
        endingPanel = ending;
        startButton = start;
        endingRestartButton = endingRestart;
        visionDimOverlay = dimmer;
        playerDeath = death;
    }

    public void Bind(
        ChapterController chapters,
        ThresholdUI overlay,
        ConstitutionBar bar,
        GameObject title,
        GameObject ending,
        Button start,
        Button endingRestart)
    {
        Bind(chapters, overlay, bar, null, title, ending, start, endingRestart, null, null);
    }

    public void StartRun()
    {
        rules.Clear();
        CurrentChapterIndex = 0;
        Time.timeScale = 1f;
        if (titlePanel != null) titlePanel.SetActive(false);
        if (endingPanel != null) endingPanel.SetActive(false);
        if (deathPanel != null) deathPanel.SetActive(false);
        GameAudio.Instance?.PlayPage();
        BindRules();
        constitutionBar?.Rebuild();
        RulesChanged?.Invoke();
        StartChapter(0);
    }

    public void StartChapter(int index)
    {
        CurrentChapterIndex = index;
        awaitingChoice = true;
        IsPlaying = false;
        Time.timeScale = 1f;
        if (deathPanel != null) deathPanel.SetActive(false);
        rules.Clear();
        playerDeath?.ResetAlive();
        chapterController?.Load(index);
        TheoController.Instance?.Freeze();
        BindRules();
        ApplyVision();
        constitutionBar?.Rebuild();

        var data = chapterController != null ? chapterController.Current : null;
        if (data != null)
            storyLog?.SetChapter(data.displayName, data.prologue);

        var triad = data != null ? data.triad : null;
        if (triad == null)
            BeginPlay();
        else
            thresholdUI?.Open();
    }

    public void BeginPlay()
    {
        awaitingChoice = false;
        IsPlaying = true;
        Time.timeScale = 1f;
        thresholdUI?.Close(true);
        TheoController.Instance?.Unfreeze();
        ApplyVision();
    }

    public void NextChapter()
    {
        Time.timeScale = 1f;
        thresholdUI?.Close(false);
        rules.RemoveAll(r => r.isErratum || r.expiresEndOfChapter);
        BindRules();
        constitutionBar?.Rebuild();
        RulesChanged?.Invoke();

        int next = CurrentChapterIndex + 1;
        if (chapterController == null || next >= chapterController.Count)
        {
            ShowEnding();
            return;
        }

        StartChapter(next);
    }

    public void RestartChapter()
    {
        Time.timeScale = 1f;
        if (deathPanel != null) deathPanel.SetActive(false);
        thresholdUI?.Close(false);
        playerDeath?.ResetAlive();
        chapterController?.Load(CurrentChapterIndex);
        if (awaitingChoice)
        {
            TheoController.Instance?.Freeze();
            thresholdUI?.Open();
        }
        else
        {
            TheoController.Instance?.Unfreeze();
            IsPlaying = true;
        }
        ApplyVision();
    }

    public void RestartRun()
    {
        StartRun();
    }

    public void KillTheo()
    {
        playerDeath?.Kill();
    }

    public void OnDeathResolved()
    {
        GameAudio.Instance?.PlayDeath();
        ShowDeathScreen();
    }

    public void ContinueAfterDeath()
    {
        StartChapter(CurrentChapterIndex);
    }

    public void RememberCell(Vector3Int cell) { }

    public void OnThresholdReached()
    {
        OnPillarDestroyed();
    }

    public void OnPillarDestroyed()
    {
        storyLog?.Append("Theo struck the demon pillar. The ink around it burned away.");
        NextChapter();
    }

    public void ApplyChoice(CardData chosen)
    {
        var triad = chapterController != null && chapterController.Current != null
            ? chapterController.Current.triad
            : null;

        if (triad != null && chosen != null)
        {
            ApplyCard(triad.cardA, chosen);
            ApplyCard(triad.cardB, chosen);
            ApplyCard(triad.cardC, chosen);
            storyLog?.Append($"Theo chose {chosen.title}. The other two sentences curdled in the margin.");
        }

        BindRules();
        constitutionBar?.Rebuild();
        RulesChanged?.Invoke();
        GameAudio.Instance?.PlayConfirm();
        BeginPlay();
    }

    void ApplyCard(CardData card, CardData chosen)
    {
        if (card == null) return;
        bool gain = card == chosen;
        rules.Add(new RuleInstance
        {
            kind = gain ? card.amendmentKind : card.erratumKind,
            isErratum = !gain,
            expiresEndOfChapter = !gain
        });
    }

    void BindRules()
    {
        RuleBook.Bind(rules, null, false);
    }

    void ApplyVision()
    {
        var dim = VisionDimController.Instance;
        if (dim == null)
        {
            dim = FindFirstObjectByType<VisionDimController>(FindObjectsInactive.Include);
            if (dim != null && !dim.gameObject.activeSelf)
                dim.gameObject.SetActive(true);
        }
        dim?.Apply();
    }

    void ShowTitle()
    {
        IsPlaying = false;
        awaitingChoice = false;
        Time.timeScale = 1f;
        if (titlePanel != null) titlePanel.SetActive(true);
        if (endingPanel != null) endingPanel.SetActive(false);
        if (deathPanel != null) deathPanel.SetActive(false);
        thresholdUI?.Close(false);
        TheoController.Instance?.Freeze();
    }

    void ShowDeathScreen()
    {
        IsPlaying = false;
        Time.timeScale = 0f;
        TheoController.Instance?.Freeze();
        EnsureDeathPanel();
        var data = chapterController != null ? chapterController.Current : null;
        if (deathLabel != null)
            deathLabel.text = data != null && !string.IsNullOrEmpty(data.deathLine)
                ? data.deathLine
                : "Theo fell.";
        if (deathPanel != null) deathPanel.SetActive(true);
        if (endingPanel != null) endingPanel.SetActive(false);
        thresholdUI?.Close(false);
    }

    void ShowEnding()
    {
        IsPlaying = false;
        Time.timeScale = 1f;
        TheoController.Instance?.Freeze();
        if (deathPanel != null) deathPanel.SetActive(false);
        if (endingPanel != null) endingPanel.SetActive(true);
        if (titlePanel != null) titlePanel.SetActive(false);
        thresholdUI?.Close(false);
    }

    void EnsureDeathPanel()
    {
        if (deathPanel != null) return;
        Transform canvas = null;
        if (titlePanel != null) canvas = titlePanel.transform.parent;
        if (canvas == null)
        {
            var found = FindFirstObjectByType<Canvas>();
            if (found != null) canvas = found.transform;
        }
        if (canvas == null) return;

        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        deathPanel = new GameObject("DeathPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        deathPanel.transform.SetParent(canvas.transform, false);
        deathPanel.layer = 5;
        var rt = deathPanel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        var bg = deathPanel.GetComponent<Image>();
        bg.color = new Color(0.04f, 0.03f, 0.03f, 0.97f);
        bg.raycastTarget = true;

        var textGo = new GameObject("DeathText", typeof(RectTransform), typeof(Text));
        textGo.transform.SetParent(deathPanel.transform, false);
        textGo.layer = 5;
        var textRt = textGo.GetComponent<RectTransform>();
        textRt.anchorMin = new Vector2(0.5f, 0.5f);
        textRt.anchorMax = new Vector2(0.5f, 0.5f);
        textRt.sizeDelta = new Vector2(900f, 220f);
        textRt.anchoredPosition = new Vector2(0f, 50f);
        deathLabel = textGo.GetComponent<Text>();
        deathLabel.font = font;
        deathLabel.fontSize = 32;
        deathLabel.alignment = TextAnchor.MiddleCenter;
        deathLabel.color = new Color(0.92f, 0.84f, 0.55f);
        deathLabel.horizontalOverflow = HorizontalWrapMode.Wrap;
        deathLabel.verticalOverflow = VerticalWrapMode.Overflow;
        deathLabel.raycastTarget = false;

        var btnGo = new GameObject("Restart", typeof(RectTransform), typeof(Image), typeof(Button));
        btnGo.transform.SetParent(deathPanel.transform, false);
        btnGo.layer = 5;
        var btnRt = btnGo.GetComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0.5f, 0.5f);
        btnRt.anchorMax = new Vector2(0.5f, 0.5f);
        btnRt.sizeDelta = new Vector2(280f, 56f);
        btnRt.anchoredPosition = new Vector2(0f, -80f);
        var btnImg = btnGo.GetComponent<Image>();
        btnImg.color = new Color(0.92f, 0.84f, 0.55f);
        deathRestartButton = btnGo.GetComponent<Button>();
        deathRestartButton.targetGraphic = btnImg;

        var labelGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
        labelGo.transform.SetParent(btnGo.transform, false);
        var labelRt = labelGo.GetComponent<RectTransform>();
        labelRt.anchorMin = Vector2.zero;
        labelRt.anchorMax = Vector2.one;
        labelRt.offsetMin = Vector2.zero;
        labelRt.offsetMax = Vector2.zero;
        var label = labelGo.GetComponent<Text>();
        label.font = font;
        label.fontSize = 24;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.black;
        label.text = "Restart";
        label.raycastTarget = false;

        deathPanel.SetActive(false);
        deathPanel.transform.SetAsLastSibling();
    }
}
