using UnityEditor;
using UnityEngine;

public static class ProjectSetup
{
    [MenuItem("Game/Run First-Time Setup", false, 1000)]
    public static void MenuSetup()
    {
        EditorUtility.DisplayDialog(
            "Grid setup retired",
            "The grid prototype builder is retired. Use Game → Rebuild Platformer Scene.",
            "OK");
    }
}
