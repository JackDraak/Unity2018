using UnityEngine;

public class Keyboard : MonoBehaviour
{
   [SerializeField] Terminal connectedToTerminal;
   [SerializeField] AudioClip[] keyStrokeSounds;

   AudioSource audioSource;

   public void PlayRandomSound()
   {
      int randomIndex = UnityEngine.Random.Range(0, keyStrokeSounds.Length);
      audioSource.clip = keyStrokeSounds[randomIndex];
      audioSource.Play();
   }

   private void Start()
   {
      audioSource = GetComponent<AudioSource>();
      QualitySettings.vSyncCount = 0;      // No V-Sync so Update() not held back by render
      Application.targetFrameRate = 1000;  // To minimise delay playing key sounds
      WarnIfTerminalNotConneced();
   }

   private void Update()
   {
      bool isValidKey = Input.inputString.Length > 0;
      if (isValidKey)
      {
         PlayRandomSound();
      }
      if (connectedToTerminal)
      {
         connectedToTerminal.ReceiveFrameInput(Input.inputString);
      }
   }

   private void WarnIfTerminalNotConneced()
   {
      if (!connectedToTerminal)
      {
         Debug.LogWarning("Keyboard not connected to a terminal");
      }
   }
}
