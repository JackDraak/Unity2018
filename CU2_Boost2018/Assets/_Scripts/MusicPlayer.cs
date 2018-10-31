using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour
{
   [Tooltip("Needs two music loops for regular & casual modes, respectively.")]
   [SerializeField] AudioClip[] musicSelection;

   AudioSource audioSource;
   bool paused = false;
   float[] trackPosition;

   public void CasualMode(bool casual)
   {
      if (casual) SetTrack(1);
      else SetTrack(0);
   }

   void SetTrack(int track)
   {
      // Not the most elegant solution; syncing individual track positions.
      if (audioSource.clip == musicSelection[0]) trackPosition[0] = audioSource.time;
      else if (audioSource.clip == musicSelection[1]) trackPosition[1] = audioSource.time;
      audioSource.Stop();

      audioSource.clip = musicSelection[track];
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
      trackPosition = new float[musicSelection.Length];
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
