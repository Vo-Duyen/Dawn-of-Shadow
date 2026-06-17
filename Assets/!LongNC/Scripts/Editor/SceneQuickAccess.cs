#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

namespace DawnOfShadow.Editor
{
    public static class SceneQuickAccess
    {
        [MenuItem("Dawn of Shadow/Open Scene/1. Loading")]
        public static void OpenLoadingScene()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene("Assets/!LongNC/Scene/Loading.unity");
            }
        }

        [MenuItem("Dawn of Shadow/Open Scene/2. Home")]
        public static void OpenHomeScene()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene("Assets/!LongNC/Scene/Home.unity");
            }
        }

        [MenuItem("Dawn of Shadow/Open Scene/3. Gameplay")]
        public static void OpenGameplayScene()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene("Assets/!LongNC/Scene/Gameplay.unity");
            }
        }
    }
}
#endif
