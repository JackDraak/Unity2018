using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;

public class Player : MonoBehaviour {

   #region Exposed Variables
   [SerializeField] AudioClip bonusSound; // https://freesound.org/people/reinsamba/sounds/35631/ : https://creativecommons.org/licenses/by/3.0/ 
   [SerializeField] AudioClip collisionSound;
   [SerializeField] AudioClip thrustSound;

   [SerializeField] Color gasHigh = Color.clear;
   [SerializeField] Color gasLow = Color.clear;
   [SerializeField] Color thrustHigh = Color.clear;
   [SerializeField] Color thrustLow = Color.clear;

   [SerializeField] GameObject collisionEffect;

   [SerializeField] Image gasFill = null;
   [SerializeField] Image thrustcapFill = null;
   [SerializeField] Image thrustFill = null;

   [SerializeField] TextMeshProUGUI thrustcapSlideText;
   #endregion

   #region Private Variables
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
   private const float FUEL_POWER_FACTOR = 0.75f;
   private const float FUEL_USE_RATE = 10000f;
   private const float FUEL_WARN_LEVEL = 20f;
   private const float HIGH_TILT_LIMIT = 359.6f;
   private const float INITIAL_POWER_LEVEL = 0.3f;
   private const float KILL_TIMER = 4f;
   private const float LOW_TILT_LIMIT = 0.4f;
   private const float PICKUP_VOLUME = 0.7f;
   private const float ROTATE_EXPEL_RATE = 0.5f;
   private const float ROTATION_FACTOR = 230f;
   private const float THRUST_EXPEL_RATE = 1f;
   private const float THRUST_FACTOR = 0.08f;
   private const float THRUST_FADE_FACTOR = 0.03f;
   private const float THRUST_LIGHTRANGE_MAX = 2f;
   private const float THRUST_MAX = 1f;
   private const float THRUST_MIN = 0f;
   private const float THRUST_POWER_FACTOR = 0.02f;
   private const float THRUST_VOLUME = 0.22f;

   private const int HALF_ARC = 180;

   private const string HUD_COLOUR = "\"#FF7070\"";
   private const string GUAGE_LABEL = "Gas Reserve: ";

   private bool debugMode, deRotating, invulnerable, paused, thrustAudioTrack, tutorialIsVisible;
   private float deRotationTime, thrustAudioLength, thrustAudioTimer;

   private float fuelLevel = FUEL_MAX;
   private float masterVolume = 1.0f;
   private float maxPower = INITIAL_POWER_LEVEL;

   private AudioSource[] audioSources;
   private AudioSource xAudio, thrustAudio;

   private FishDrone[] fishDrones;

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

   private Slider thrustPowercapSlider;
   private Slider thrustPowerSlider;
   private Slider gasLevelSlider;

   private Timekeeper timeKeeper;

   private UIcontrol uiControl;

   private Vector3 baseThrust = new Vector3(0, 13000, 0);
   private Vector3 localEulers = Vector3.zero;
   private Vector3 startPosition = Vector3.zero;
   private Vector3 threeControlAxis = Vector3.zero;
   #endregion

   private void Start ()
   {
      InitVars();

      // just hanging onto a formula someone posted that might be handy when I get to "level 2":
      // movementFactor = (Mathf.Sin(Time.time * oscillationSpeed)) / 2f + 0.5f;
   }

   private void FixedUpdate()
   {
      GenerateFuel();
      PlayerControlPoll();
      MaintainAlignment();
      if (debugMode) DebugControlPoll();
   }

   private void OnApplicationPause(bool pause)
   {
      if (Input.GetKeyDown(KeyCode.Q)) Application.Quit();
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
            //other.gameObject.SetActive(false); // TODO maybe need to revise?
            Destroy(other.gameObject, 0.01f);
            pickupTracker.ClaimPickup();
            xAudio.PlayOneShot(bonusSound, masterVolume * PICKUP_VOLUME);
            fuelLevel += FUEL_PICKUP_VALUE;
            if (fuelLevel > FUEL_MAX) fuelLevel = FUEL_MAX;
            break;
         default:
            break;
      }
   }

   private void Awake()
   {
      // Objects disabled by UIcontrol should be assigned in Awake() to avoid trying to capture them after they've been disabled**.
      // *The other 2 objects under UIcontrol are manually assigned to PickupTracker.cs in the inspector.
      // **Originally UIcontrol.cs was designed to be used from Player.cs through public methods. It still is, but for unknown
      //   reasons, it was failing to enforce that control in the local Start() method (intermittently, no-less), thus the functionality
      //   being moved into UIcontrol.cs:Start()
      gasLevelSlider = GameObject.FindGameObjectWithTag("Slider_Gas").GetComponent<Slider>();
      thrustPowercapSlider = GameObject.FindGameObjectWithTag("Slider_Powercap").GetComponent<Slider>();
      thrustPowerSlider = GameObject.FindGameObjectWithTag("Slider_Power").GetComponent<Slider>();
   }

   private void InitVars()
   {
      audioSources = GetComponents<AudioSource>();
      thisRigidbody = GetComponent<Rigidbody>();
      thrustParticleSystem = GetComponent<ParticleSystem>();

      fishDrones = FindObjectsOfType<FishDrone>();
      pickupTracker = FindObjectOfType<PickupTracker>();
      timeKeeper = FindObjectOfType<Timekeeper>();
      uiControl = FindObjectOfType<UIcontrol>();

      sceneCamera = FindObjectOfType<Camera>().GetComponent<GlueCam>();

      cockpit = GameObject.FindGameObjectWithTag("Cockpit");
      tutorialText = GameObject.FindGameObjectWithTag("Tutorial_Text");
      thrusterBell = GameObject.FindGameObjectWithTag("Thruster_Bell");
      thrustLight = GameObject.FindGameObjectWithTag("Thruster_Light");

      debugMode = Debug.isDebugBuild;
      startPosition = transform.position;
      startRotation = transform.rotation;
      thrustAudioLength = thrustSound.length;
      thrustAudioTimer = 0 - thrustAudioLength;
      thrustBubbles = thrustParticleSystem.emission;

      thrustAudio = audioSources[0];
      xAudio = audioSources[1];

      deRotating = false;
      invulnerable = false;
      paused = false;
      thrustAudioTrack = true;
      tutorialIsVisible = true;
      uiControl.visible = false; // (DEPRECIATED) this seems to fail intermittently, so UIcontrol.cs now handles start-up state directly

      AdjustEmissionRate(EMISSION_RATE_INACTIVE);
      thrustPowerSlider.maxValue = THRUST_MAX;
      thrustPowerSlider.minValue = THRUST_MIN;
      thrustPowercapSlider.minValue = 0f;
      thrustPowercapSlider.maxValue = 1f;
      thrustPowercapSlider.value = maxPower;
      SetPower(INITIAL_POWER_LEVEL);

      gasLevelSlider.maxValue = FUEL_MAX;
      gasLevelSlider.minValue = 0;
      gasLevelSlider.value = fuelLevel;
      DoColourForGasLevel();
      DoColourForThrustcap();
   }

   private void AdjustEmissionRate(float newRate)
   {
      thrustBubbles.rateOverTime = newRate;
   }

   private void AdjustThrusterPower(float delta)
   {
      delta *= 0.01f;
      float deltaPlus = delta + thrustPowerSlider.value;
      if (deltaPlus > THRUST_MAX) thrustPowerSlider.value = THRUST_MAX;
      else if (deltaPlus < THRUST_MIN) thrustPowerSlider.value = THRUST_MIN;
      else thrustPowerSlider.value += delta;
      if (thrustPowerSlider.value > maxPower) thrustPowerSlider.value = maxPower;
      DoColourForThrustPower();
   }

   private void AutoDeRotate()
   {
      float assertion = Mathf.Abs(Time.time - deRotationTime) * DEROTATION_RATE;
      localEulers = transform.localRotation.eulerAngles;
      float playerTilt = localEulers.z;
      if (playerTilt >= HALF_ARC &&  playerTilt < HIGH_TILT_LIMIT) transform.Rotate(Vector3.forward * (playerTilt * assertion) * Time.deltaTime);
      else if (playerTilt < HALF_ARC && playerTilt > LOW_TILT_LIMIT) transform.Rotate(Vector3.back * ((playerTilt + HALF_ARC) * assertion) * Time.deltaTime);
   }

   public float BoostMaxPower()
   {
      maxPower += 0.1f;
      if (maxPower > 1) maxPower = 1;
      thrustPowercapSlider.value = maxPower;
      DoColourForThrustcap();
      return maxPower;
   }

   private void DoColourForGasLevel()
   {
      Color colour;
      float ratio = fuelLevel / FUEL_MAX;
      colour = Vector4.Lerp(gasHigh, gasLow, 1 - ratio);
      cockpit.GetComponent<MeshRenderer>().material.color = gasFill.color = colour;
   }

   private void DoColourForThrustcap()
   {
      thrustcapSlideText.text = "Thrust Cap: " + Mathf.FloorToInt(maxPower * 100) + "%";
      Color colour;
      float ratio = thrustPowercapSlider.value / THRUST_MAX;
      colour = Vector4.Lerp(thrustHigh, thrustLow, 1 - ratio);
      thrustcapFill.color = colour;
   }

   private void DoColourForThrustPower()
   {
      thrustLight.GetComponent<Light>().range = THRUST_LIGHTRANGE_MAX;

      Color colour;
      float ratio = thrustPowerSlider.value / THRUST_MAX;
      colour = Vector4.Lerp(thrustHigh, thrustLow, 1 - ratio);

      thrustFill.color = colour;
      if (fuelLevel > 0 && fuelLevel < FUEL_WARN_LEVEL) colour = Color.red;
      thrusterBell.GetComponent<MeshRenderer>().material.color = thrustLight.GetComponent<Light>().color = colour;
   }

   private void DebugControlPoll()
   {
      if (Input.GetKeyDown(KeyCode.B)) BoostMaxPower();
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
      thrustLight.GetComponent<Light>().range = Mathf.Lerp(thrustLight.GetComponent<Light>().range, 0, THRUST_FADE_FACTOR);
      thrusterBell.GetComponent<MeshRenderer>().material.color = 
         Vector4.Lerp(thrusterBell.GetComponent<MeshRenderer>().material.color, Color.black, THRUST_FADE_FACTOR);
   }

   private bool ExpelGas(float rate)
   {
      float expulsionRate = 
         rate * FUEL_USE_RATE * ((thrustPowerSlider.value * FUEL_POWER_FACTOR) * THRUST_POWER_FACTOR) * Time.fixedDeltaTime;
      if (fuelLevel > expulsionRate)
      {
         fuelLevel -= expulsionRate;
         gasLevelSlider.value = fuelLevel;
         DoColourForThrustPower();
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
      uiControl.visible = true;
      timeKeeper.Begin();
   }

   private void MaintainAlignment()
   {
      transform.position = new Vector3(transform.position.x, transform.position.y, 0.0f); // Lock Z position.
      transform.rotation = Quaternion.Euler(0.0f, 0.0f, transform.rotation.eulerAngles.z); // Lock XY rotation.
   }

   public bool Pause()
   {
      paused = !paused;
      if (paused)
      {
         if (thrustAudio.isPlaying)
         {
            thrustAudioTrack = false;
            thrustAudio.Stop();
         }
         if (xAudio.isPlaying) xAudio.Stop();
         tutorialText.SetActive(true);
         uiControl.visible = false;
      }
      else
      {
         if (!thrustAudioTrack)
         {
            thrustAudio.PlayOneShot(thrustSound, masterVolume * THRUST_VOLUME);
            thrustAudioTimer = Time.time;
            thrustAudioTrack = true;
         }
         if (timeKeeper.started)
         {
            tutorialText.SetActive(false);
            uiControl.visible = true;
         }
      }
      return paused;
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
         }
         else EndExpulsion();
         if (tutorialIsVisible) HideTutorial();
      }
      else if (thrustAudio.isPlaying) EndExpulsion();
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
      // SumPause.cs is Polling: Q, R & ESC keys.
      if (Input.GetKeyDown(KeyCode.Alpha0)) SetPower(1.0f);
      else if (Input.GetKeyDown(KeyCode.Alpha9)) SetPower(0.9f);
      else if (Input.GetKeyDown(KeyCode.Alpha8)) SetPower(0.8f);
      else if (Input.GetKeyDown(KeyCode.Alpha7)) SetPower(0.7f);
      else if (Input.GetKeyDown(KeyCode.Alpha6)) SetPower(0.6f);
      else if (Input.GetKeyDown(KeyCode.Alpha5)) SetPower(0.5f);
      else if (Input.GetKeyDown(KeyCode.Alpha4)) SetPower(0.4f);
      else if (Input.GetKeyDown(KeyCode.Alpha3)) SetPower(0.3f);
      else if (Input.GetKeyDown(KeyCode.Alpha2)) SetPower(0.2f);
      else if (Input.GetKeyDown(KeyCode.Alpha1)) SetPower(0.1f);
   }

   private void PollVertical()
   {
      threeControlAxis.y = CrossPlatformInputManager.GetAxis("Vertical");
      if (threeControlAxis.y != 0) AdjustThrusterPower(threeControlAxis.y);
   }

   public void Restart()
   {
      fuelLevel = FUEL_MAX;
      pickupTracker.Restart();
      sceneCamera.Restart();
      timeKeeper.Restart();
      tutorialIsVisible = true;
      tutorialText.SetActive(true);
      uiControl.visible = false;
      transform.position = startPosition;
      transform.rotation = startRotation;
      thisRigidbody.velocity = Vector3.zero;
      foreach (FishDrone drone in fishDrones) drone.Reset();
   }

   private void Rotate(float direction)
   {
      transform.Rotate(Vector3.back * ROTATION_FACTOR * Time.fixedDeltaTime * direction);
      if (thrustBubbles.rateOverTime.constant < EMISSION_RATE_ROTATION) AdjustEmissionRate(EMISSION_RATE_ROTATION);
      if (thrustLight.GetComponent<Light>().range < (THRUST_LIGHTRANGE_MAX / 4)) thrustLight.GetComponent<Light>().range = THRUST_LIGHTRANGE_MAX / 4;
   }

   private void SetPower(float power)
   {
      if (power > maxPower) power = maxPower;
      thrustPowerSlider.value = power;
      DoColourForThrustPower();
   }

   private void Thrust(float force)
   {
      Vector3 appliedForce = baseThrust * (thrustPowerSlider.value * THRUST_FACTOR * Time.deltaTime * force); 
      thisRigidbody.AddRelativeForce(appliedForce);
      if (thrustAudioTimer + thrustAudioLength - CLIP_TIME < Time.time)
      {
         thrustAudio.Stop();
         thrustAudio.PlayOneShot(thrustSound, masterVolume * THRUST_VOLUME);
         thrustAudioTimer = Time.time;
      }
   }
}
