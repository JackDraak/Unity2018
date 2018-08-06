using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;

public class Player : MonoBehaviour {

   [SerializeField] AudioClip bonusSound;
   [SerializeField] GameObject collisionEffect;
   [SerializeField] AudioClip collisionSound;
   [SerializeField] Text FuelReadout;
   [SerializeField] GameObject thrustLight;
   [SerializeField] AudioClip thrustSound;
   [SerializeField] GameObject tutorialText;

   private const float CLIP_TIME = 0.5f;
   private const float COLLISION_VOLUME = 0.4f;
   private const float DAMAGE_VALUE = 75f;
   private const float DEROTATION_RATE = 0.2f;
   private const float FUEL_PICKUP_VALUE = 420f;
   private const string GUAGE_LABEL = "Gas Reserve: ";
   private const float HIGH_TILT_LIMIT = 359.7f;
   private const string HUD_COLOUR = "\"#FF7070\"";
   private const float KILL_TIMER = 4f;
   private const float LOW_TILT_LIMIT = 0.3f;
   private const float MASTER_VOLUME = 1.0f;
   private const float PICKUP_VOLUME = 0.7f;
   private const float ROTATE_EXPEL_RATE = 0.5f;
   private const float ROTATION_FACTOR = 270f;
   private const float THRUST_EXPEL_RATE = 1f;
   private const float THRUST_FACTOR = 9.81f;
   private const float THRUST_POWER_FACTOR = 0.02f;
   private const float THRUST_VOLUME = 0.22f;

   private bool debugMode, deRotating, invulnerable, tutorialIsVisible;
   private float currentEmissionRate, deRotationTime, thrustAudioLength, thrustAudioTimer;

   private float fuelGenRate = 50f;
   private float fuelLevel = 1000f;
   private float fuelMax = 1000f;
   private float fuelUseRate = 100f;
   private float rotationEmissionRate = 20f;
   private float thrustEmissionRate = 60f;
   private float thrustMax = 0f;
   private float thrustNonEmissionRate = 1.3f;
   private float thrustSliderMax = 60f;
   private float thrustSliderMin = 20f;
   private float thrustSliderValue = 42f;

   private AudioSource[] audioSources;
   private AudioSource xAudio, thrustAudio;
   private ParticleSystem.EmissionModule thrustBubbles;
   private ParticleSystem thrustParticleSystem;
   private Rigidbody thisRigidbody;
   //private RigidbodyConstraints thisRigidbodyConstraints;
   private Timekeeper timeKeeper;

   private Vector3 debugThrustState = Vector3.zero;
   private Vector3 localEulers = Vector3.zero;
   private Vector3 threeControlAxis = Vector3.zero;

   void Start ()
   {
      audioSources = GetComponents<AudioSource>();
      thisRigidbody = GetComponent<Rigidbody>();
      thrustParticleSystem = GetComponent<ParticleSystem>();
      timeKeeper = FindObjectOfType<Timekeeper>();

      thrustAudio = audioSources[0];
      xAudio = audioSources[1];

      debugMode = Debug.isDebugBuild;
      thrustAudioLength = thrustSound.length;
      thrustAudioTimer -= thrustAudioLength;
      thrustBubbles = thrustParticleSystem.emission;

      deRotating = false;
      invulnerable = false;
      tutorialIsVisible = true;

      AdjustEmissionRate(thrustNonEmissionRate);
      RefreshFuelGuage();
      thrustLight.SetActive(false);

      thisRigidbody.constraints = 
         RigidbodyConstraints.FreezeRotationX | 
         RigidbodyConstraints.FreezeRotationY | 
         RigidbodyConstraints.FreezeRotationZ |
         RigidbodyConstraints.FreezePositionZ;
      //thisRigidbodyConstraints = thisRigidbody.constraints;
   }

   void FixedUpdate ()
   {
      if (debugMode) DebugControlPoll();
      GenerateFuel();
      RefreshFuelGuage();
		PlayerControlPoll();
   }

   private void OnGUI()
   {
      int r_x = 35;
      int r_y = 45;
      int r_w = 100;
      int r_h = 30;
      Rect sliderRect = new Rect(r_x, r_y, r_w, r_h);
      Rect labelRect = new Rect(r_x, r_y + r_h, r_w * 3, r_h);
      Rect thrustRect = new Rect(r_x, r_y + r_h * 2, r_w * 3, r_h * 2);

      thrustSliderValue = GUI.HorizontalSlider(sliderRect, thrustSliderValue, thrustSliderMin, thrustSliderMax);
      GUI.Label(labelRect, "<color=" + HUD_COLOUR + "><b><i>Thruster Power Control</i></b> (Up/Down Keys)</color>");
      if (debugThrustState.y > thrustMax) thrustMax = debugThrustState.y; // for debugMode HUD info
      if (debugMode) GUI.Label(thrustRect, 
         "<color=" + HUD_COLOUR + "><b>Live T-Power: current/peak\n" + debugThrustState.y + "\n" + thrustMax + "</b></color>");
   }

   private void OnCollisionEnter(Collision collision)
   {
      switch (collision.gameObject.tag)
      {
         case "BadObject_01":
            if (!invulnerable)
            {
               fuelLevel -= DAMAGE_VALUE;
               if (fuelLevel < 0) fuelLevel = 0;
               GameObject leakDamage = (GameObject)Instantiate(collisionEffect, transform.position, Quaternion.identity);
               xAudio.PlayOneShot(collisionSound, MASTER_VOLUME * COLLISION_VOLUME);
               Destroy(leakDamage, KILL_TIMER);
            }
            else Debug.Log("invulnerable: BO-01");
            break;
         case "BadObject_02":
            break;
         case "GoodObject_01":
            Debug.Log("Pickup Object");
            break;
         case "GoodObject_02":
            break;
         default:
            break;
      }
   }

   private void OnTriggerEnter(Collider other)
   {
      switch (other.gameObject.tag)
      {
         case "BadObject_01":
            break;
         case "BadObject_02":
            break;
         case "GoodObject_01":
            other.gameObject.SetActive(false);
            xAudio.PlayOneShot(bonusSound, MASTER_VOLUME * PICKUP_VOLUME);
            fuelLevel += FUEL_PICKUP_VALUE;
            if (fuelLevel > fuelMax) fuelLevel = fuelMax;
            break;
         case "GoodObject_02":
            break;
         default:
            break;
      }
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

   private void AdjustEmissionRate(float newRate)
   {
      currentEmissionRate = newRate;
      thrustBubbles.rateOverTime = currentEmissionRate;
   }

   private void AutoDeRotate()
   {
      float assertion = Mathf.Abs(Time.time - deRotationTime) * DEROTATION_RATE;
      localEulers = transform.localRotation.eulerAngles;
      float playerTilt = localEulers.z;
      if (playerTilt >= 180 &&  playerTilt < HIGH_TILT_LIMIT)
      {
         transform.Rotate(Vector3.forward * (playerTilt * assertion) * Time.deltaTime);
      }
      else if (playerTilt < 180 && playerTilt > LOW_TILT_LIMIT)
      {
         transform.Rotate(Vector3.back * ((playerTilt + 180) * assertion) * Time.deltaTime);
      }
   }

   private void DebugControlPoll()
   {
      if (Input.GetKeyDown(KeyCode.F)) fuelLevel = fuelMax;
      if (Input.GetKeyDown(KeyCode.I)) invulnerable = !invulnerable;
      if (Input.GetKeyDown(KeyCode.R)) thrustMax = 0;
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
         if (threeControlAxis.z == 0) AdjustEmissionRate(thrustNonEmissionRate);
      }
   }

   private void EndExpulsion()
   {
      thrustAudio.Stop();
      AdjustEmissionRate(thrustNonEmissionRate);
      thrustAudioTimer -= thrustAudioLength;
      thrustLight.SetActive(false);
      debugThrustState = Vector3.zero;
   }

   private bool ExpelGas(float rate)
   {
      float expulsionRate = rate * fuelUseRate * (thrustSliderValue * THRUST_POWER_FACTOR) * Time.fixedDeltaTime;
      if (fuelLevel > expulsionRate)
      {
         fuelLevel -= expulsionRate;
         return true;
      }
      else
      {
         fuelLevel = 0;
         return false;
      }
   }

   private void GenerateFuel()
   {
      fuelLevel += Time.fixedDeltaTime * fuelGenRate;
      if (fuelLevel > fuelMax) fuelLevel = fuelMax;
   }

   private void HideTutorial()
   {
      tutorialIsVisible = false;
      tutorialText.SetActive(false);
      timeKeeper.Begin();
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
      threeControlAxis.z = CrossPlatformInputManager.GetAxis("Jump");
      if (threeControlAxis.z != 0)
      {
         if (ExpelGas(THRUST_EXPEL_RATE))
         {
            Thrust(threeControlAxis.z);
            AdjustEmissionRate(thrustEmissionRate);
            thrustLight.SetActive(true);
         }
         else EndExpulsion();
         if (tutorialIsVisible) HideTutorial();
      }
      else if (thrustAudio.isPlaying)
      {
         EndExpulsion();
      }
   }

   private void PollHorizontal()
   {
      threeControlAxis.x = CrossPlatformInputManager.GetAxis("Horizontal");
      if (threeControlAxis.x != 0)
      {
         if (tutorialIsVisible) HideTutorial();
         if (ExpelGas(ROTATE_EXPEL_RATE))
         {
            Rotate(threeControlAxis.x);
            deRotating = false;
         }
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
         if (tutorialIsVisible) HideTutorial();
         AdjustThrusterPower(threeControlAxis.y);
      }
   }

   private void RefreshFuelGuage()
   {
      int approxLevel = Mathf.FloorToInt(fuelLevel);
      FuelReadout.text = GUAGE_LABEL + approxLevel.ToString();
   }

   private void Rotate(float direction)
   {
      //thisRigidbody.constraints = RigidbodyConstraints.None;
      transform.Rotate(Vector3.back * ROTATION_FACTOR * Time.fixedDeltaTime * direction);
      //thisRigidbody.constraints = thisRigidbodyConstraints;
      if (currentEmissionRate < rotationEmissionRate) AdjustEmissionRate(rotationEmissionRate);
   }

   private void Thrust(float force)
   {
      debugThrustState = Vector3.up * thrustSliderValue * THRUST_FACTOR * Time.deltaTime * force; 
      thisRigidbody.AddRelativeForce(debugThrustState);
      if (thrustAudioTimer + thrustAudioLength - CLIP_TIME < Time.time)
      {
         thrustAudio.Stop();
         thrustAudio.PlayOneShot(thrustSound, MASTER_VOLUME * THRUST_VOLUME);
         thrustAudioTimer = Time.time;
      }
   }
}
