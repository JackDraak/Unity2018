using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour
{
   [Tooltip("Music Clips needs a minimum of two music clips (for regular & casual modes, respectively)." +
      "Additional clips will be added to the Alternate queue.")]
   [SerializeField] AudioClip[] musicClips;

   AudioSource audioSource;
   bool paused = false;
   float[] trackPosition;
   int alternateTrack;

   public void AlternateMusic()
   {
      if (musicClips.Length > 2)
      {
         alternateTrack++;
         if (alternateTrack < 2) alternateTrack = 2;
         if (alternateTrack > musicClips.Length - 1) alternateTrack = 2;
         SetTrack(alternateTrack);
      }
   }

   public void CasualMode(bool casual)
   {
      if (casual) SetTrack(1);
      else SetTrack(0);
   }

   void InitTrackPositions()
   {
      trackPosition = new float[musicClips.Length];
      for (int n = 0; n < musicClips.Length; n++)
      {
         trackPosition[n] = 0f;
      }
   }

   public void Pause()
   {
      paused = !paused;
      if (paused) audioSource.Pause();
      else audioSource.Play();
   }

   void RememberTrackPosition()
   {
      for (int i = 0; i < musicClips.Length; i++)
      {
         if (audioSource.clip == musicClips[i]) trackPosition[i] = audioSource.time;
      }
   }

   void SetTrack(int track)
   {
      RememberTrackPosition();
      audioSource.Stop();
      audioSource.clip = musicClips[track];
      audioSource.time = trackPosition[track];
      audioSource.Play();

      Debug.Log("MusicPlayer:SetTrack(" + track + ", " + audioSource.clip.name + ")");
   }

   void Start()
   {
      audioSource = GetComponent<AudioSource>();
      Debug.Log(musicClips.Length);
      InitTrackPositions();
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
