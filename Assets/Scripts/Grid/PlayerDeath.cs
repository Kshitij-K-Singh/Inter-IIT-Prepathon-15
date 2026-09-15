using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDeath : MonoBehaviour
{
    [SerializeField] Text delayLabel;
    [SerializeField] GameObject deathFlash;
    [SerializeField] float flashSeconds = 0.14f;

    public bool IsResolving { get; private set; }
    public int DeadIn => 0;
    bool delaying;

    public void Bind(Text label, GameObject flash)
    {
        delayLabel = label;
        deathFlash = flash;
    }

    public void ResetAlive()
    {
        StopAllCoroutines();
        IsResolving = false;
        delaying = false;
        if (deathFlash != null) deathFlash.SetActive(false);
        if (delayLabel != null) delayLabel.enabled = false;
    }

    public void TickAfterMove() { }

    public void SetQueue(int count) { }

    public void Kill()
    {
        if (IsResolving || delaying) return;
        float delay = RuleBook.DeathDelaySeconds();
        if (delay <= 0f)
        {
            ResolveDeath();
            return;
        }

        delaying = true;
        StartCoroutine(DelayThenDie(delay));
    }

    IEnumerator DelayThenDie(float seconds)
    {
        float left = seconds;
        while (left > 0f)
        {
            if (delayLabel != null)
            {
                bool hide = RuleBook.DeathHidden();
                delayLabel.enabled = !hide;
                if (!hide) delayLabel.text = Mathf.CeilToInt(left).ToString();
            }
            left -= Time.unscaledDeltaTime;
            yield return null;
        }

        ResolveDeath();
    }

    void ResolveDeath()
    {
        if (IsResolving) return;
        IsResolving = true;
        if (delayLabel != null) delayLabel.enabled = false;
        StartCoroutine(FlashThenRestart());
    }

    IEnumerator FlashThenRestart()
    {
        if (deathFlash != null && !RuleBook.DeathHidden())
            deathFlash.SetActive(true);
        yield return new WaitForSecondsRealtime(flashSeconds);
        if (deathFlash != null) deathFlash.SetActive(false);
        GameManager.Instance?.OnDeathResolved();
    }
}
