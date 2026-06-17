using UnityEngine;
using System.Collections.Generic;

namespace DawnOfShadow.Core
{
    public class AudioManager : SingletonBase<AudioManager>
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Audio Clips")]
        [SerializeField] private AudioClip defaultBGM;
        // Mở rộng bằng Dictionary hoặc cấu hình riêng trong tương lai nếu cần thiết

        private void Start()
        {
            // Lắng nghe sự kiện để phát âm thanh thông qua GameEventSystem
            GameEventSystem.Subscribe("PlaySFX", PlaySFX);
            GameEventSystem.Subscribe("PlayBGM", PlayBGM);

            if (defaultBGM != null)
            {
                PlayBGM(defaultBGM.name);
            }
        }

        private void OnDestroy()
        {
            GameEventSystem.Unsubscribe("PlaySFX", PlaySFX);
            GameEventSystem.Unsubscribe("PlayBGM", PlayBGM);
        }

        public void PlayBGM(object data)
        {
            string clipName = data as string;
            // TODO: Load AudioClip từ Resources hoặc Addressables dựa trên tên
            // Tạm thời bỏ qua phần logic load clip nếu chưa có hệ thống Asset Management
            if (bgmSource != null)
            {
                if (!bgmSource.isPlaying)
                {
                    bgmSource.Play();
                }
            }
        }

        public void PlaySFX(object data)
        {
            string clipName = data as string;
            // TODO: Tìm và phát clip tương ứng
            if (sfxSource != null)
            {
                // sfxSource.PlayOneShot(clip);
            }
        }

        public void SetVolume(float volume)
        {
            AudioListener.volume = Mathf.Clamp01(volume);
        }
    }
}
