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

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (kind == HazardKind.Red && RuleBook.RedKills())
        {
            GameManager.Instance?.KillTheo();
            return;
        }
        if ((kind == HazardKind.Spike || kind == HazardKind.Red) && RuleBook.LightFoot())
            return;
        GameManager.Instance?.KillTheo();
    }
}
