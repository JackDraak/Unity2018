using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour
{
   private AudioSource audioSource;
   private bool paused = false;

   public void Pause()
   {
      paused = !paused;
      if (paused) audioSource.Pause();
      else audioSource.Play();
   }

   private void Start()
   {
      audioSource = GetComponent<AudioSource>();
	}

   private void Update()
   {
      if (Input.GetKey(KeyCode.LeftBracket)) VolumeDown();
      else if (Input.GetKey(KeyCode.RightBracket)) VolumeUp();
   }

   private void VolumeDown()
   {
      if (audioSource.volume > .1f) audioSource.volume -= .1f * Time.deltaTime;
      else audioSource.volume = 0f;
   }

   private void VolumeUp()
   {
      if (audioSource.volume < .9f) audioSource.volume += .1f * Time.deltaTime;
      else audioSource.volume = 1f;
   }
}
