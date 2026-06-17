using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DawnOfShadow.Core
{
    public class SceneLoader : SingletonBase<SceneLoader>
    {
        public event Action<float> OnLoadingProgressChanged;
        public event Action OnLoadingCompleted;
        public event Action OnLoadingStarted;

        private AsyncOperation _preloadedOperation;
        private string _preloadedSceneName;

        public void PreloadScene(string sceneName)
        {
            if (_preloadedSceneName == sceneName && _preloadedOperation != null)
            {
                return; // Đã hoặc đang preload rồi
            }

            _preloadedSceneName = sceneName;
            StartCoroutine(PreloadSceneCoroutine(sceneName));
        }

        private IEnumerator PreloadSceneCoroutine(string sceneName)
        {
            _preloadedOperation = SceneManager.LoadSceneAsync(sceneName);
            _preloadedOperation.allowSceneActivation = false;

            AsyncOperation operation = _preloadedOperation;
            while (operation != null && operation.progress < 0.9f)
            {
                yield return null;
            }

            if (_preloadedOperation != null)
            {
                Debug.Log($"Scene '{sceneName}' preloaded successfully in background.");
            }
        }

        public void LoadSceneAsync(string sceneName, float minDuration = 0f)
        {
            StartCoroutine(LoadSceneCoroutine(sceneName, minDuration));
        }

        private IEnumerator LoadSceneCoroutine(string sceneName, float minDuration)
        {
            OnLoadingStarted?.Invoke();
            yield return null;

            AsyncOperation operation;

            // Kiểm tra xem scene này đã được preload sẵn chưa
            if (_preloadedSceneName == sceneName && _preloadedOperation != null)
            {
                operation = _preloadedOperation;
                // Reset preload references so they aren't reused
                _preloadedSceneName = null;
                _preloadedOperation = null;
            }
            else
            {
                operation = SceneManager.LoadSceneAsync(sceneName);
                operation.allowSceneActivation = minDuration <= 0f;
            }

            float elapsedTime = 0f;

            while (!operation.isDone)
            {
                elapsedTime += Time.deltaTime;

                // Tránh lỗi chia cho 0 (gây ra NaN) khi minDuration = 0
                float visualProgress = minDuration > 0f ? Mathf.Clamp01(elapsedTime / minDuration) : 1f;
                
                // Keep progress matching visual simulation
                OnLoadingProgressChanged?.Invoke(visualProgress);

                // Check if the scene is loaded in the background and the minimum duration has elapsed
                if (operation.progress >= 0.9f && elapsedTime >= minDuration)
                {
                    operation.allowSceneActivation = true;
                }

                yield return null;
            }

            // Ensure progress is exactly 1.0 at the end
            OnLoadingProgressChanged?.Invoke(1f);
            OnLoadingCompleted?.Invoke();
        }
    }
}
