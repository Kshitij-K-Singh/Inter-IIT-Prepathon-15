using UnityEngine;

[DisallowMultipleComponent]
public class GridEntity : MonoBehaviour
{
    public Vector3Int cell;
    public bool pushable;
    public bool red;
    public bool pit;
    public bool win;
    public bool threshold;
    public bool wall;

    public bool IsPushable => pushable || HasUnityTag("Pushable");
    public bool IsRed => red || HasUnityTag("Red");
    public bool IsPit => pit || HasUnityTag("Pit");
    public bool IsWin => win || HasUnityTag("Win");
    public bool IsThreshold => threshold || HasUnityTag("Threshold");
    public bool IsWall => wall || HasUnityTag("Wall");

    public void SnapToGrid(Grid grid)
    {
        if (grid == null) return;
        transform.position = grid.GetCellCenterWorld(cell);
    }

    bool HasUnityTag(string tagName)
    {
        return !string.IsNullOrEmpty(tag) && CompareTag(tagName);
    }
}
