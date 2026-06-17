using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using DawnOfShadow.Core;

namespace DawnOfShadow.UI
{
    public class LoadingScreenView : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image progressBarFill;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private TextMeshProUGUI loadingTipText;
        [SerializeField] private RectTransform spinnerIcon;

        [Header("Creative Design Configs")]
        [SerializeField] private string[] tips = {
            "Shadows hide the truth. Step carefully.",
            "Use Dodge (Skill 1) to gain temporary invincibility frames.",
            "Collect special elements in gameplay to boost character base stats.",
            "Bosses attack with AOE spells at the end of each stage."
        };

        private Tween spinnerTween;
        private Tween bgPatternTween;

        private static LoadingScreenView _instance;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (_instance != this) return;

            // Subscribe to SceneLoader events
            SceneLoader.Instance.OnLoadingStarted += ShowLoading;
            SceneLoader.Instance.OnLoadingProgressChanged += UpdateProgress;
            SceneLoader.Instance.OnLoadingCompleted += HideLoading;

            // Hide initially
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                if (SceneLoader.Instance != null)
                {
                    SceneLoader.Instance.OnLoadingStarted -= ShowLoading;
                    SceneLoader.Instance.OnLoadingProgressChanged -= UpdateProgress;
                    SceneLoader.Instance.OnLoadingCompleted -= HideLoading;
                }
                KillTweens();
            }
        }

        private void ShowLoading()
        {
            KillTweens();
            
            // Randomize tip text
            if (loadingTipText != null && tips.Length > 0)
            {
                loadingTipText.text = tips[Random.Range(0, tips.Length)];
                loadingTipText.transform.localScale = Vector3.one * 0.8f;
                loadingTipText.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack);
            }

            progressBarFill.fillAmount = 0f;
            progressText.text = "0%";

            // Show CanvasGroup instantly
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;

            // Spin loader icon creatively
            if (spinnerIcon != null)
            {
                spinnerTween = spinnerIcon.DORotate(new Vector3(0, 0, -360), 2f, RotateMode.FastBeyond360)
                    .SetLoops(-1, LoopType.Incremental)
                    .SetEase(Ease.Linear)
                    .SetUpdate(true);
            }
        }

        private void UpdateProgress(float progress)
        {
            // Smoothly animate the progress bar fill using DOTween
            progressBarFill.DOFillAmount(progress, 0.25f).SetEase(Ease.OutQuad).SetUpdate(true);
            
            // Text scale punch on progress update
            progressText.text = $"{(progress * 100f):0}%";
            progressText.transform.DOScale(1.2f, 0.1f)
                .OnComplete(() => progressText.transform.DOScale(1f, 0.15f))
                .SetUpdate(true);
        }

        private void HideLoading()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            KillTweens();
        }

        private void KillTweens()
        {
            spinnerTween?.Kill();
            bgPatternTween?.Kill();
        }
    }
}
