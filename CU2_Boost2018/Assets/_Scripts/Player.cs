using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;

public class Player : MonoBehaviour {

   [SerializeField] AudioClip bonusSound;
   [SerializeField] AudioClip collisionSound;
   [SerializeField] AudioClip thrustSound;
   [SerializeField] Color gasHigh, gasLow, thrustHigh, thrustLow;
   [SerializeField] GameObject collisionEffect;
   [SerializeField] Image gasFill, thrustFill;

   private const float CLIP_TIME = 0.5f;
   private const float COLLISION_VOLUME = 0.4f;
   private const float DAMAGE_VALUE = 35f;
   private const float DEROTATION_RATE = 0.2f;
   private const float EMISSION_RATE_INACTIVE = 1.3f;
   private const float EMISSION_RATE_ROTATION = 20f;
   private const float EMISSION_RATE_THRUST = 60f;
   private const float FUEL_GEN_RATE = 40f;
   private const float FUEL_MAX = 1000f;
   private const float FUEL_PICKUP_VALUE = 200f;
   private const float FUEL_USE_RATE = 100f;
   private const float HIGH_TILT_LIMIT = 359.6f;
   private const float KILL_TIMER = 4f;
   private const float LOW_TILT_LIMIT = 0.4f;
   private const float PICKUP_VOLUME = 0.7f;
   private const float ROTATE_EXPEL_RATE = 0.5f;
   private const float ROTATION_FACTOR = 270f;
   private const float THRUST_EXPEL_RATE = 1f;
   private const float THRUST_FACTOR = 9.81f;
   private const float THRUST_MAX = 60f;
   private const float THRUST_MIN = 20f;
   private const float THRUST_POWER_FACTOR = 0.02f;
   private const float THRUST_VOLUME = 0.22f;
   private const string HUD_COLOUR = "\"#FF7070\"";
   private const string GUAGE_LABEL = "Gas Reserve: ";

   private bool debugMode, deRotating, invulnerable, tutorialIsVisible;
   private float deRotationTime, thrustAudioLength, thrustAudioTimer;

   private float fuelLevel = 1000f;
   private float masterVolume = 1.0f;
   private float thrustSliderValue = 40f;

   private AudioSource[] audioSources;
   private AudioSource xAudio, thrustAudio;
   private GameObject cockpit;
   private GameObject thrusterBell;
   private GameObject thrustLight;
   private GameObject tutorialText;
   private GlueCam sceneCamera;
   private ParticleSystem.EmissionModule thrustBubbles;
   private ParticleSystem thrustParticleSystem;
   private PickupTracker pickupTracker;
   private Quaternion startRotation;
   private Rigidbody thisRigidbody;
   private Slider thrustPowerSlider;
   private Slider gasLevelSlider;
   private Timekeeper timeKeeper;
   private Vector3 debugThrustState = Vector3.zero;
   private Vector3 localEulers = Vector3.zero;
   private Vector3 startPosition = Vector3.zero;
   private Vector3 threeControlAxis = Vector3.zero;

   void Start ()
   {
      audioSources = GetComponents<AudioSource>();
      pickupTracker = FindObjectOfType<PickupTracker>();
      sceneCamera = FindObjectOfType<Camera>().GetComponent<GlueCam>();
      thisRigidbody = GetComponent<Rigidbody>();
      thrustParticleSystem = GetComponent<ParticleSystem>();
      timeKeeper = FindObjectOfType<Timekeeper>();

      cockpit = GameObject.FindGameObjectWithTag("Cockpit");
      tutorialText = GameObject.FindGameObjectWithTag("Tutorial_Text");
      thrusterBell = GameObject.FindGameObjectWithTag("Thruster_Bell");
      thrustLight = GameObject.FindGameObjectWithTag("Thruster_Light");

      gasLevelSlider = GameObject.FindGameObjectWithTag("Slider_Gas").GetComponent<Slider>();
      thrustPowerSlider = GameObject.FindGameObjectWithTag("Slider_Power").GetComponent<Slider>();

      debugMode = Debug.isDebugBuild;
      startPosition = transform.position;
      startRotation = transform.rotation;
      thrustAudioLength = thrustSound.length;
      thrustAudioTimer -= thrustAudioLength;
      thrustBubbles = thrustParticleSystem.emission;

      thrustAudio = audioSources[0];
      xAudio = audioSources[1];

      deRotating = false;
      invulnerable = false;
      tutorialIsVisible = true;

      AdjustEmissionRate(EMISSION_RATE_INACTIVE);
      thrustPowerSlider.maxValue = THRUST_MAX;
      thrustPowerSlider.minValue = THRUST_MIN;
      thrustPowerSlider.value = thrustSliderValue;
      DoColourThrustPower();
      thrustLight.SetActive(false);

      gasLevelSlider.maxValue = FUEL_MAX;
      gasLevelSlider.minValue = 0;
      gasLevelSlider.value = fuelLevel;
      DoColourForGasLevel();

      thisRigidbody.constraints = 
         RigidbodyConstraints.FreezeRotationX | 
         RigidbodyConstraints.FreezeRotationY | 
         RigidbodyConstraints.FreezeRotationZ |
         RigidbodyConstraints.FreezePositionZ;
   }

   void FixedUpdate ()
   {
      GenerateFuel();
		PlayerControlPoll();
      if (debugMode) DebugControlPoll();
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
               xAudio.PlayOneShot(collisionSound, masterVolume * COLLISION_VOLUME);
               Destroy(leakDamage, KILL_TIMER);
            }
            else Debug.Log("invulnerable: BO-01");
            break;
         default:
            break;
      }
   }

   private void OnTriggerEnter(Collider other)
   {
      switch (other.gameObject.tag)
      {
         case "GoodObject_01":
            other.gameObject.SetActive(false);
            xAudio.PlayOneShot(bonusSound, masterVolume * PICKUP_VOLUME);
            fuelLevel += FUEL_PICKUP_VALUE;
            if (fuelLevel > FUEL_MAX) fuelLevel = FUEL_MAX;
            break;
         default:
            break;
      }
   }

   private void AdjustEmissionRate(float newRate)
   {
      thrustBubbles.rateOverTime = newRate;
   }

   private void AdjustThrusterPower(float delta)
   {
      if (delta + thrustSliderValue > THRUST_MAX)
      {
         thrustSliderValue = THRUST_MAX;
      }
      else if (delta + thrustSliderValue < THRUST_MIN)
      {
         thrustSliderValue = THRUST_MIN;
      }
      else thrustSliderValue += delta;

      thrustPowerSlider.value = thrustSliderValue;
      DoColourThrustPower();
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

   private void DoColourThrustPower()
   {
      Color colour;
      float ratio = thrustSliderValue / THRUST_MAX;
      colour = Vector4.Lerp(thrustHigh, thrustLow, 1 - ratio);
      thrustFill.color = colour;
      thrustLight.GetComponent<Light>().color = colour;
      thrusterBell.GetComponent<MeshRenderer>().material.color = colour;
   }

   private void DoColourForGasLevel()
   {
      Color colour;
      float ratio = fuelLevel / FUEL_MAX;
      colour = Vector4.Lerp(gasHigh, gasLow, 1 - ratio);
      gasFill.color = colour;
      cockpit.GetComponent<MeshRenderer>().material.color = colour;
   }

   private void DebugControlPoll()
   {
      if (Input.GetKeyDown(KeyCode.F)) fuelLevel = FUEL_MAX;
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
         if (threeControlAxis.z == 0) EndExpulsion();
      }
   }

   private void EndExpulsion()
   {
      thrustAudio.Stop();
      AdjustEmissionRate(EMISSION_RATE_INACTIVE);
      thrustAudioTimer -= thrustAudioLength;
      thrustLight.SetActive(false);
      debugThrustState = Vector3.zero;
   }

   private bool ExpelGas(float rate)
   {
      float expulsionRate = rate * FUEL_USE_RATE * (thrustSliderValue * THRUST_POWER_FACTOR) * Time.fixedDeltaTime;
      if (fuelLevel > expulsionRate)
      {
         fuelLevel -= expulsionRate;
         gasLevelSlider.value = fuelLevel;
         return true;
      }
      else
      {
         gasLevelSlider.value = fuelLevel;
         return false;
      }
   }

   private void GenerateFuel()
   {
      fuelLevel += Time.fixedDeltaTime * FUEL_GEN_RATE;
      if (fuelLevel > FUEL_MAX) fuelLevel = FUEL_MAX;
      gasLevelSlider.value = fuelLevel;
      DoColourForGasLevel();
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
            AdjustEmissionRate(EMISSION_RATE_THRUST);
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
      if (Input.GetKeyDown(KeyCode.R)) Restart();

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

   private void Restart()
   {
      sceneCamera.Restart();
      fuelLevel = FUEL_MAX;
      timeKeeper.Init();
      tutorialIsVisible = true;
      tutorialText.SetActive(true);
      transform.position = startPosition;
      transform.rotation = startRotation;
      pickupTracker.Restart();
   }
 

   private void Rotate(float direction)
   {
      transform.Rotate(Vector3.back * ROTATION_FACTOR * Time.fixedDeltaTime * direction);
      if (thrustBubbles.rateOverTime.constant < EMISSION_RATE_ROTATION) AdjustEmissionRate(EMISSION_RATE_ROTATION);
   }

   private void Thrust(float force)
   {
      debugThrustState = Vector3.up * thrustSliderValue * THRUST_FACTOR * Time.deltaTime * force; 
      thisRigidbody.AddRelativeForce(debugThrustState);
      if (thrustAudioTimer + thrustAudioLength - CLIP_TIME < Time.time)
      {
         thrustAudio.Stop();
         thrustAudio.PlayOneShot(thrustSound, masterVolume * THRUST_VOLUME);
         thrustAudioTimer = Time.time;
      }
   }
}
