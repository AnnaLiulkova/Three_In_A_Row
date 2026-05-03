using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public class SaveOnPlay 
{
    static SaveOnPlay() 
    {
        EditorApplication.playModeStateChanged += SaveWhenEnteringPlayMode;
    }

    private static void SaveWhenEnteringPlayMode(PlayModeStateChange state) 
    {
        if (state == PlayModeStateChange.ExitingEditMode) 
        {
            Debug.Log("Автозбереження перед Play Mode...");
            EditorSceneManager.SaveOpenScenes(); 
            AssetDatabase.SaveAssets();          
        }
    }
}