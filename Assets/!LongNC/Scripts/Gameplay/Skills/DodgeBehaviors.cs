using System.Collections;
using UnityEngine;
using DG.Tweening;

namespace DawnOfShadow.Gameplay.Skills
{
    public interface IDodgeBehavior
    {
        void PerformDodge(MonoBehaviour runner, Transform subject, Vector3 inputDir, float speed, System.Action onComplete);
    }

    public class DashDodge : IDodgeBehavior
    {
        private float _dashDistance = 4f;
        private float _dashDuration = 0.2f;

        public void PerformDodge(MonoBehaviour runner, Transform subject, Vector3 inputDir, float speed, System.Action onComplete)
        {
            Vector3 targetDir = inputDir.normalized;
            if (targetDir == Vector3.zero) targetDir = subject.forward;

            Vector3 targetPos = subject.position + targetDir * _dashDistance;
            
            // Sweep test or collision offset can be added, simple DOTween movement for dash
            subject.DOMove(targetPos, _dashDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => onComplete?.Invoke());
        }
    }

    public class TeleportDodge : IDodgeBehavior
    {
        private float _teleportDistance = 5f;

        public void PerformDodge(MonoBehaviour runner, Transform subject, Vector3 inputDir, float speed, System.Action onComplete)
        {
            runner.StartCoroutine(TeleportRoutine(subject, inputDir, onComplete));
        }

        private IEnumerator TeleportRoutine(Transform subject, Vector3 inputDir, System.Action onComplete)
        {
            Vector3 targetDir = inputDir.normalized;
            if (targetDir == Vector3.zero) targetDir = subject.forward;

            Vector3 targetPos = subject.position + targetDir * _teleportDistance;

            // Fade out
            yield return subject.DOScale(0f, 0.1f).SetEase(Ease.InBack).WaitForCompletion();
            
            // Move position
            subject.position = targetPos;

            // Fade in
            yield return subject.DOScale(1f, 0.1f).SetEase(Ease.OutBack).WaitForCompletion();

            onComplete?.Invoke();
        }
    }

    public class RollDodge : IDodgeBehavior
    {
        private float _rollDistance = 3.5f;
        private float _rollDuration = 0.35f;

        public void PerformDodge(MonoBehaviour runner, Transform subject, Vector3 inputDir, float speed, System.Action onComplete)
        {
            Vector3 targetDir = inputDir.normalized;
            if (targetDir == Vector3.zero) targetDir = subject.forward;

            Vector3 targetPos = subject.position + targetDir * _rollDistance;

            // Perform rolling visual rotation with DOTween alongside movement
            subject.DOMove(targetPos, _rollDuration).SetEase(Ease.OutQuad);
            subject.DORotate(subject.eulerAngles + new Vector3(0, 360, 0), _rollDuration, RotateMode.FastBeyond360)
                .OnComplete(() => onComplete?.Invoke());
        }
    }
}
