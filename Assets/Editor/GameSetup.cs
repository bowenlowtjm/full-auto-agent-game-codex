using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class GameSetup
{
    [MenuItem("Pully/Setup Scenes")]
    public static void SetupScenes()
    {
        Directory.CreateDirectory("Assets/_Game/Scenes");

        CreateMenuScene();
        CreateGameScene();
        CreateGameOverScene();

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene("Assets/_Game/Scenes/MenuScene.unity", true),
            new EditorBuildSettingsScene("Assets/_Game/Scenes/GameScene.unity", true),
            new EditorBuildSettingsScene("Assets/_Game/Scenes/GameOverScene.unity", true)
        };

        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        Debug.Log("[GameSetup] Scenes and build settings configured.");
        AssetDatabase.SaveAssets();
    }

    private static void EnsureCamera()
    {
        if (Camera.main != null) return;
        var go = new GameObject("Main Camera");
        var cam = go.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.08f, 0.08f, 0.12f, 1f);
        go.tag = "MainCamera";
        go.transform.position = new Vector3(0, 0, -10);
    }

    private static void CreateMenuScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        EnsureCamera();
        new GameObject("MenuController").AddComponent<Pully.Game.MenuController>();
        EditorSceneManager.SaveScene(scene, "Assets/_Game/Scenes/MenuScene.unity");
    }

    private static void CreateGameScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        EnsureCamera();
        new GameObject("GameBootstrap").AddComponent<Pully.Game.GameBootstrap>();
        EditorSceneManager.SaveScene(scene, "Assets/_Game/Scenes/GameScene.unity");
    }

    private static void CreateGameOverScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        EnsureCamera();
        new GameObject("GameOverController").AddComponent<Pully.Game.GameOverController>();
        EditorSceneManager.SaveScene(scene, "Assets/_Game/Scenes/GameOverScene.unity");
    }
}
