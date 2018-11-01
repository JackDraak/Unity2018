using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour
{
   [Tooltip("Music Selection needs two music loops (for regular & casual modes, respectively).")]
   [SerializeField] MusicTrack[] musicSelection;

   AudioSource audioSource;
   bool paused = false;
   int alternateTrack;

   struct MusicTrack
   {
      public AudioClip musicClip;
      public float trackPosition;
   }

   public void AlternateMusic()
   {
      if (musicSelection.Length > 2)
      {
         alternateTrack = (alternateTrack > musicSelection.Length - 1) ? 2 : alternateTrack++;
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
      for (int i = 0; i < musicSelection.Length; i++)
      {
         if (audioSource.clip == musicSelection[i].musicClip) musicSelection[i].trackPosition = audioSource.time;
      }
      audioSource.Stop();

      audioSource.clip = musicSelection[track].musicClip;
      audioSource.time = musicSelection[track].trackPosition;
      audioSource.Play();
   }

   public void Pause()
   {
      paused = !paused;
      if (paused) audioSource.Pause();
      else audioSource.Play();
   }

   void Start() { audioSource = GetComponent<AudioSource>(); }

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
