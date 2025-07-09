using System.Collections;
using UnityEngine;

namespace ai.aviad.AIBook
{
    public class AudioManager : MonoBehaviour
    {
        [Header("Music")]
        public AudioClip musicClip;
        public AudioSource musicSource;

        [Header("Heartbeat")]
        public AudioSource heartbeatSound;
        public AudioSource finalHeartbeatSound;

        [Header("Atmosphere")]
        public AudioSource maleShriek;
        public AudioSource femaleShriek;
        public AudioSource[] bellChimes;
        public AudioSource[] crows;

        [Header("Book Sounds")]
        public AudioSource bookOpenSound;
        public AudioSource bookCloseSound;
        public AudioSource bookCloseBass;
        public AudioSource pageTurnSound;
        public AudioSource pagesFlippingSound;

        public void StartBackgroundMusic()
        {
            if (musicSource == null)
                musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.clip = musicClip;
            musicSource.loop = true;
            musicSource.playOnAwake = true;
            musicSource.volume = 0.1f;
            musicSource.Play();
        }

        public void StopBackgroundMusic()
        {
            if (musicSource != null)
                musicSource.Stop();
        }

        public void PlayHeartbeat()
        {
            if (heartbeatSound != null)
                heartbeatSound.Play();
        }

        public void PlayFinalHeartbeat()
        {
            if (finalHeartbeatSound != null)
                finalHeartbeatSound.Play();
        }

        public void PlayCrows()
        {
            if (crows != null && crows.Length > 0)
            {
                var index = Random.Range(0, crows.Length);
                crows[index].Play();
            }
        }

        public void PlayShriek(bool isMale)
        {
            if (isMale && maleShriek != null)
                maleShriek.Play();
            else if (!isMale && femaleShriek != null)
                femaleShriek.Play();
        }

        public void PlayBellChimes()
        {
            foreach (var bell in bellChimes)
                bell.Play();
        }

        public void PlayBookOpen()
        {
            if (bookOpenSound != null)
                bookOpenSound.Play();
        }

        public void PlayBookClose()
        {
            if (bookCloseSound != null)
                bookCloseSound.Play();
            if (bookCloseBass != null)
                bookCloseBass.Play();
        }

        public void PlayPageTurn()
        {
            if (pageTurnSound != null)
                pageTurnSound.Play();
        }

        public void PlayPagesFlipping()
        {
            if (pagesFlippingSound != null)
                pagesFlippingSound.Play();
        }

        public void StopPagesFlipping()
        {
            if (pagesFlippingSound != null && pagesFlippingSound.isPlaying)
                pagesFlippingSound.Stop();
        }
    }
}
