using UnityEngine;

[System.Serializable]
public class EntitySpawn
{
    public GridEntity prefab;
    public Vector3Int cell;
}

[CreateAssetMenu(menuName = "Game/Chapter Data", fileName = "Chapter")]
public class ChapterData : ScriptableObject
{
    public string id;
    public string displayName;
    public Vector3 spawnPosition = new Vector3(2f, 1.2f, 0f);
    public TriadData triad;
    [TextArea(4, 12)] public string prologue;
    [TextArea] public string deathLine = "And Theo fell. Athena turned the page back.";
    [TextArea] public string tutorialText;
    public Vector3Int spawnCell;
    public bool persistRememberedWalls;
    public Vector3 cameraPosition = new Vector3(0f, 0f, -10f);
    public float cameraOrthoSize = 6f;
    public Vector2Int boundsMin;
    public Vector2Int boundsMax;
    public EntitySpawn[] entities;
}
