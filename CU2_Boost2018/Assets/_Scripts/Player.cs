using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine;

public class Player : MonoBehaviour {

   [SerializeField] float masterVolume = 1.0f;  // TODO move this to a more apropriate place (i.e. file).
   [SerializeField] float thrustVolume = 0.42f; // TODO move this to a more apropriate place (i.e. file).
   [SerializeField] int rotationFactor = 300;
   [SerializeField] int thrustFactor = 10;
   [SerializeField] AudioClip thrustSound;
   [SerializeField] GameObject tutorialText;

   private bool debugMode, deRotating, invulnerable, tutorialVisible;
   private float deRotationTime;
   private float thrustAudioLength;
   private float thrustAudioTimer;
   private float thrustEmissionRate = 113f;
   private float thrustNonEmissionRate = 13f;
   private float thrustSliderMax = 120f;
   private float thrustSliderMin = 25f;
   private float thrustSliderValue = 45f;
   private Vector3 threeControlAxis = Vector3.zero;
   private Vector3 localEulers = Vector3.zero;
   private AudioSource audioSource;
   private ParticleSystem thrustParticleSystem;
   private Rigidbody thisRigidbody;
   private RigidbodyConstraints rigidbodyConstraints;

   void Start ()
   {
      debugMode = Debug.isDebugBuild;

      audioSource = GetComponent<AudioSource>();
      thisRigidbody = GetComponent<Rigidbody>();
      thrustParticleSystem = GetComponent<ParticleSystem>();

      var emission = thrustParticleSystem.emission;
      emission.rateOverTime = thrustNonEmissionRate;

      thisRigidbody.constraints = 
         RigidbodyConstraints.FreezeRotationX | 
         RigidbodyConstraints.FreezeRotationY | 
         RigidbodyConstraints.FreezeRotationZ |
         RigidbodyConstraints.FreezePositionZ;
      rigidbodyConstraints = thisRigidbody.constraints;

      thrustAudioLength = thrustSound.length;
      thrustAudioTimer -= thrustAudioLength;
      deRotating = false;
      invulnerable = false;
      tutorialVisible = true;
   }

   void LateUpdate ()
   {
      if (debugMode) DebugControlPoll();
		PlayerControlPoll();
   }

   private void AdjustThrusterPower(float delta)
   {
      if (delta + thrustSliderValue > thrustSliderMax)
      {
         thrustSliderValue = thrustSliderMax;
      }
      else if (delta + thrustSliderValue < thrustSliderMin)
      {
         thrustSliderValue = thrustSliderMin;
      }
      else thrustSliderValue += delta;
   }

   private void AutoDeRotate()
   {
      float speed = Mathf.Abs(Time.time - deRotationTime) / 3;
      localEulers = transform.localRotation.eulerAngles;
      float playerTilt = localEulers.z;
      if (playerTilt >= 180 && playerTilt < 359)
      {
         transform.Rotate(Vector3.forward * (playerTilt * speed) * Time.deltaTime);
      }
      else if (playerTilt < 180 && playerTilt > 1)
      {
         transform.Rotate(Vector3.back * ((playerTilt + 180) * speed) * Time.deltaTime);
      }
   }

   private void DebugControlPoll()
   {
      if (Input.GetKeyDown(KeyCode.I)) invulnerable = !invulnerable;
   }

   private void DeRotate()
   {
      if (!deRotating)
      {
         deRotationTime = Time.time;
         deRotating = true;
      }
      else
      {
         AutoDeRotate();
      }
   }

   private void HideTutorial()
   {
      tutorialVisible = false;
      tutorialText.SetActive(false);
   }

   private void OnCollisionEnter(Collision collision)
   {
      switch (collision.gameObject.tag)
      {
         case "BadObject_01":
            if (!invulnerable) Debug.Log("collision: BO-01");
            else Debug.Log("invulnerable: BO-01");
            break;
         case "BadObject_02":
            //Debug.Log("collision: BO-02");
            break;
         case "GoodObject_01":
            //Debug.Log("collision: GO-01");
            break;
         case "GoodObject_02":
            //Debug.Log("collision: GO-02");
            break;
         default:
            //Debug.Log("collision: default");
            break;
      }
   }

   private void OnGUI()
   {
      int r_x, r_y, r_w, r_h;
      r_x = 35;
      r_y = 45;
      r_w = 100;
      r_h = 30;
      Rect sliderRect = new Rect(r_x, r_y, r_w, r_h);
      Rect labelRect = new Rect(r_x, r_y + r_h, r_w * 3, r_h);

      thrustSliderValue = GUI.HorizontalSlider(sliderRect, thrustSliderValue, thrustSliderMin, thrustSliderMax);
      GUI.Label(labelRect, "<color=\"black\"><b><i>Thruster Power Control</i></b> (Up/Down Keys)</color>");
   }

   private void PlayerControlPoll()
   {
      PollAction();
      PollHorizontal();
      PollVertical();
      PollMisc();
   }

   private void PollAction()
   {
      var emission = thrustParticleSystem.emission;
      threeControlAxis.z = CrossPlatformInputManager.GetAxis("Jump");
      if (threeControlAxis.z != 0)
      {
         if (tutorialVisible) HideTutorial();
         Thrust(threeControlAxis.z);
         emission.rateOverTime = thrustEmissionRate;
      }
      else if (audioSource.isPlaying)
      {
         audioSource.Stop();
         thrustAudioTimer -= thrustAudioLength;
         emission.rateOverTime = thrustNonEmissionRate;
      }
   }

   private void PollHorizontal()
   {
      threeControlAxis.x = CrossPlatformInputManager.GetAxis("Horizontal");
      if (threeControlAxis.x != 0)
      {
         if (tutorialVisible) HideTutorial();
         Rotate(threeControlAxis.x);
         deRotating = false;
      }
      else DeRotate();
   }

   private void PollMisc()
   {
      if (Input.GetKeyDown(KeyCode.Q)) Application.Quit();
   }

   private void PollVertical()
   {
      threeControlAxis.y = CrossPlatformInputManager.GetAxis("Vertical");
      if (threeControlAxis.y != 0)
      {
         if (tutorialVisible) HideTutorial();
         AdjustThrusterPower(threeControlAxis.y);
      }
   }

   private void Rotate(float direction)
   {
      thisRigidbody.angularVelocity = Vector3.zero;
      thisRigidbody.constraints = RigidbodyConstraints.None;
      transform.Rotate(Vector3.back * rotationFactor * Time.fixedDeltaTime * direction);
      thisRigidbody.constraints = rigidbodyConstraints;
   }

   private void Thrust(float force)
   {
      thisRigidbody.AddRelativeForce(Vector3.up * thrustSliderValue * thrustFactor * Time.deltaTime * force);
      // if the audio clip is in the final half second, re-que it
      if (thrustAudioTimer + thrustAudioLength - 0.5f < Time.time)
      {
         audioSource.Stop();
         audioSource.PlayOneShot(thrustSound, thrustVolume * masterVolume);
         thrustAudioTimer = Time.time;
      }
   }
}
