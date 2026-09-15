using UnityEngine;

[CreateAssetMenu(menuName = "Game/Card Data", fileName = "Card")]
public class CardData : ScriptableObject
{
    public string title;
    public RuleKind amendmentKind;
    [TextArea] public string amendmentText;
    public RuleKind erratumKind;
    [TextArea] public string erratumText;
    public Sprite amendmentArt;
    public Sprite erratumArt;
}
