using UnityEngine;
using System.Collections;
using DawnOfShadow.Core;

namespace DawnOfShadow.UI
{
    public class LoadingBootstrapper : MonoBehaviour
    {
        [SerializeField] private float delayBeforeStart = 1.0f;
        [SerializeField] private float loadingDuration = 4.0f;
        [SerializeField] private string targetSceneName = "Home";

        private IEnumerator Start()
        {
            // Optional delay to let the screen initialize and present the initial creative UI animation
            yield return new WaitForSeconds(delayBeforeStart);

            // Trigger the async loading of the home scene with minimum loading duration
            SceneLoader.Instance.LoadSceneAsync(targetSceneName, loadingDuration);
        }
    }
}
