using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour
{
   [Tooltip("Music Selection needs two music loops (for regular & casual modes, respectively).")]
   [SerializeField] AudioClip[] musicClips;

   AudioSource audioSource;
   bool paused = false;
   float[] trackPosition;
   int alternateTrack;

   public void AlternateMusic()
   {
      if (musicClips.Length > 2)
      {
         alternateTrack = (alternateTrack > musicClips.Length - 1) ? 2 : alternateTrack++;
         SetTrack(alternateTrack);
      }
   }

   public void CasualMode(bool casual)
   {
      if (casual) SetTrack(1);
      else SetTrack(0);
   }

   void SetTrack(int track)
   {
      // Remember what the current track position is.
      for (int i = 0; i < musicClips.Length; i++)
      {
         if (audioSource.clip == musicClips[i]) trackPosition[i] = audioSource.time;
      }
      audioSource.Stop();

      // Set, seek and play 'track'.
      audioSource.clip = musicClips[track];
      audioSource.time = trackPosition[track];
      audioSource.Play();
   }

   public void Pause()
   {
      paused = !paused;
      if (paused) audioSource.Pause();
      else audioSource.Play();
   }

   void Start()
   {
      audioSource = GetComponent<AudioSource>();
      trackPosition = new float[musicClips.Length];
   }

   public void VolumeDown()
   {
      if (audioSource.volume > .1f) audioSource.volume -= .1f * Time.deltaTime;
      else audioSource.volume = 0f;
   }

   public void VolumeUp()
   {
      if (audioSource.volume < .9f) audioSource.volume += .1f * Time.deltaTime;
      else audioSource.volume = 1f;
   }
}
