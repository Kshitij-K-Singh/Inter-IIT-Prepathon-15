using UnityEngine;

public enum HazardKind
{
    Pit,
    Spike,
    Red
}

public class DeathZone : MonoBehaviour
{
    [SerializeField] HazardKind kind = HazardKind.Pit;

    public void SetKind(HazardKind hazard) => kind = hazard;

    void Awake()
    {
        var col = GetComponent<Collider2D>();
        if (col == null)
        {
            var box = gameObject.AddComponent<BoxCollider2D>();
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                box.size = sr.sprite.bounds.size;
                box.offset = sr.sprite.bounds.center;
            }
            col = box;
        }
        col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other) => TryKill(other);

    void OnTriggerStay2D(Collider2D other) => TryKill(other);

    void TryKill(Collider2D other)
    {
        if (!IsPlayer(other)) return;
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;
        if (GameManager.Instance.IsPaused) return;

        bool blessed = RuleBook.LightFoot();
        if ((kind == HazardKind.Spike || kind == HazardKind.Red) && blessed)
            return;

        GameManager.Instance.KillTheo();
    }

    static bool IsPlayer(Collider2D other)
    {
        if (other.CompareTag("Player")) return true;
        var body = other.attachedRigidbody;
        return body != null && body.CompareTag("Player");
    }
}
