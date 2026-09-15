using UnityEngine;
using UnityEngine.UI;

public class StoryLog : MonoBehaviour
{
    public static StoryLog Instance { get; private set; }

    [SerializeField] Text page;
    [SerializeField] int maxChars = 1400;

    string body = "";

    void Awake()
    {
        Instance = this;
    }

    public void Bind(Text text)
    {
        page = text;
    }

    public void SetChapter(string title, string prologue)
    {
        body = $"{title}\n\n{prologue}\n";
        Paint();
    }

    public void Append(string line)
    {
        if (string.IsNullOrEmpty(line)) return;
        body += "\n" + line;
        if (body.Length > maxChars)
            body = body.Substring(body.Length - maxChars);
        Paint();
    }

    void Paint()
    {
        if (page != null) page.text = body;
    }
}
