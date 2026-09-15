using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
static class PlayModeStartScene
{
    static PlayModeStartScene()
    {
        var game = AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/Game.unity");
        if (game != null)
            EditorSceneManager.playModeStartScene = game;
    }
}
