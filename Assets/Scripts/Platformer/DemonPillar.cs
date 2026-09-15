using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DemonPillar : MonoBehaviour
{
    [SerializeField] int maxHealth = 5;
    [SerializeField] AudioClip hurtClip;

    int health;
    bool used;
    Image fill;
    RectTransform fillRt;
    SpriteRenderer body;
    AudioSource hurtSource;
    static Sprite whiteSprite;

    void Awake()
    {
        body = GetComponent<SpriteRenderer>();
        var col = GetComponent<BoxCollider2D>();
        if (body != null && body.sprite != null && col != null)
        {
            col.size = body.sprite.bounds.size;
            col.offset = body.sprite.bounds.center;
        }
        health = maxHealth;
        hurtSource = gameObject.AddComponent<AudioSource>();
        hurtSource.playOnAwake = false;
        hurtSource.spatialBlend = 0f;
        hurtSource.loop = false;
        hurtSource.volume = 1f;
        hurtSource.mute = false;
        if (hurtClip == null)
        {
#if UNITY_EDITOR
            hurtClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/PillarHurt.mp3");
#endif
        }
        BuildHealthBar();
        RefreshBar();
    }

    public void ResetPillar()
    {
        used = false;
        health = maxHealth;
        if (body != null) body.color = Color.white;
        RefreshBar();
    }

    public void TakeHit(int damage)
    {
        if (used) return;
        health = Mathf.Max(0, health - Mathf.Max(1, damage));
        RefreshBar();
        if (hurtClip != null && hurtSource != null)
            hurtSource.PlayOneShot(hurtClip, 1f);
        GameAudio.Instance?.PlayPillarHurt();
        StopAllCoroutines();
        StartCoroutine(Flash());
        if (health <= 0) Shatter();
    }

    void Shatter()
    {
        if (used) return;
        used = true;
        GameAudio.Instance?.PlayPillar();
        GameManager.Instance?.OnPillarDestroyed();
    }

    void RefreshBar()
    {
        float t = maxHealth <= 0 ? 0f : (float)health / maxHealth;
        if (fill != null) fill.fillAmount = t;
        if (fillRt != null)
        {
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = new Vector2(t, 1f);
            fillRt.offsetMin = new Vector2(2f, 2f);
            fillRt.offsetMax = new Vector2(-2f, -2f);
        }
    }

    System.Collections.IEnumerator Flash()
    {
        if (body != null) body.color = new Color(1f, 0.45f, 0.45f);
        yield return new WaitForSeconds(0.08f);
        if (body != null) body.color = Color.white;
    }

    void BuildHealthBar()
    {
        var canvasGo = new GameObject("HealthBar");
        canvasGo.transform.SetParent(transform, false);
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 40;
        canvas.worldCamera = Camera.main;
        canvasGo.AddComponent<CanvasScaler>();
        var rt = canvasGo.GetComponent<RectTransform>();
        float height = body != null && body.sprite != null ? body.sprite.bounds.size.y : 3.5f;
        rt.localPosition = new Vector3(0f, height + 0.4f, 0f);
        rt.sizeDelta = new Vector2(160f, 22f);
        rt.localScale = new Vector3(0.02f, 0.02f, 0.02f);

        var white = WhiteSprite();

        var bgGo = new GameObject("Bg", typeof(RectTransform), typeof(Image));
        bgGo.transform.SetParent(canvasGo.transform, false);
        var bg = bgGo.GetComponent<Image>();
        bg.sprite = white;
        bg.color = new Color(0.08f, 0.05f, 0.05f, 0.95f);
        bg.raycastTarget = false;
        Stretch(bgGo.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        var fillGo = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillGo.transform.SetParent(canvasGo.transform, false);
        fill = fillGo.GetComponent<Image>();
        fill.sprite = white;
        fill.color = new Color(0.82f, 0.12f, 0.14f, 1f);
        fill.type = Image.Type.Simple;
        fill.raycastTarget = false;
        fillRt = fillGo.GetComponent<RectTransform>();
        RefreshBar();
    }

    static void Stretch(RectTransform rt, Vector2 min, Vector2 max, Vector2 offMin, Vector2 offMax)
    {
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = offMin;
        rt.offsetMax = offMax;
    }

    static Sprite WhiteSprite()
    {
        if (whiteSprite != null) return whiteSprite;
        var tex = Texture2D.whiteTexture;
        whiteSprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 4f);
        return whiteSprite;
    }
}
