using UnityEngine;
using DG.Tweening;
using DawnOfShadow.Core;

namespace DawnOfShadow.Gameplay
{
    public class CameraShaker : SingletonBase<CameraShaker>
    {
        [Header("Camera Configs")]
        [SerializeField] private Transform cameraTransform;

        private Vector3 _originalLocalPosition;
        private bool _isShaking = false;

        protected override void Awake()
        {
            base.Awake();
            if (cameraTransform == null)
            {
                cameraTransform = Camera.main != null ? Camera.main.transform : transform;
            }
        }

        public void Shake(float duration = 0.2f, float strength = 0.4f, int vibrato = 10, float randomness = 90f)
        {
            if (cameraTransform == null) return;

            if (!_isShaking)
            {
                _originalLocalPosition = cameraTransform.localPosition;
            }

            _isShaking = true;
            cameraTransform.DOComplete();

            cameraTransform.DOShakePosition(duration, strength, vibrato, randomness, false, true)
                .OnComplete(() =>
                {
                    cameraTransform.localPosition = _originalLocalPosition;
                    _isShaking = false;
                });
        }
    }
}
