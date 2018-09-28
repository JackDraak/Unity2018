﻿//
//    Dev-Notes:
//
//    idea fish bopping bonus level: gain extra %'s of Gas Level.
//    
//    TODO : work on Fog / lighting? work on level 2 ideas?
//

using EZCameraShake;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput; 

public class Player : MonoBehaviour
{
   #region Exposed Variables
   [SerializeField] AudioClip bonusSound; // https://freesound.org/people/reinsamba/sounds/35631/ : https://creativecommons.org/licenses/by/3.0/ 
   [SerializeField] AudioClip collisionSound;
   [SerializeField] AudioClip thrustSound;

   [SerializeField] Color gasHigh = Color.clear;
   [SerializeField] Color gasLow = Color.clear;
   [SerializeField] Color thrustHigh = Color.clear;
   [SerializeField] Color thrustLow = Color.clear;

   [SerializeField] GameObject collisionEffect;
   [SerializeField] GameObject pickupEffect;

   [SerializeField] Image gasFill = null;
   [SerializeField] Image thrustcapFill = null;
   [SerializeField] Image thrustFill = null;

   [SerializeField] TextMeshProUGUI gasSlideText;
   [SerializeField] TextMeshProUGUI goalSlideText;
   [SerializeField] TextMeshProUGUI powerSlideText;
   [SerializeField] TextMeshProUGUI powercapSlideText;
   #endregion

   #region Private Variables
   private const float CLIP_TIME = 0.5f;
   private const float DAMAGE_VALUE = 35f;
   private const float DELAY_GOAL_UPDATE = 0.35f;
   private const float DEROTATION_RATE = 0.2f;
   private const float EMISSION_RATE_INACTIVE = 1.3f;
   private const float EMISSION_RATE_ROTATION = 20f;
   private const float EMISSION_RATE_THRUST = 60f;
   private const float EXPEL_RATE_ROTATE = 0.5f;
   private const float EXPEL_RATE_THRUST = 1f;
   private const float FUEL_GEN_RATE = 40f;
   private const float FUEL_MAX = 1000f;
   private const float FUEL_PICKUP_VALUE = 200f;
   private const float FUEL_POWER_FACTOR = 0.75f;
   private const float FUEL_USE_RATE = 10000f;
   private const float FUEL_WARN_LEVEL = 20f;
   private const float INITIAL_POWER_LEVEL = 0.3f;
   private const float KILL_TIMER = 4f;
   private const float POWER_CONTROLLER_FACTOR = 0.008f;
   private const float ROTATION_FACTOR = 230f;
   private const float TILT_LIMIT_MAX = 357.5f;
   private const float TILT_LIMIT_MIN = 2.5f;
   private const float THRUST_FACTOR = 0.08f;
   private const float THRUST_FADE_FACTOR = 0.03f;
   private const float THRUST_LIGHTRANGE_MAX = 2f;
   private const float THRUST_MAX = 1f;
   private const float THRUST_MIN = 0f;
   private const float THRUST_POWER_BASE = 0.2f;
   private const float THRUST_POWER_FACTOR = 0.02f;
   private const float VOLUME_COLLISION = 0.4f;
   private const float VOLUME_PICKUP = 0.5f;
   private const float VOLUME_THRUST = 0.2f;

   private const int HALF_ARC = 180;

   private const string AXIS_POWER = "Vertical";
   private const string AXIS_ROTATION = "Horizontal";
   private const string AXIS_THRUST = "Jump";
   private const string HUD_COLOUR = "\"#FF7070\""; // coral

   private AudioSource[] audioSources;
   private AudioSource xAudio, thrustAudio;

   private bool debugMode, deRotating, invulnerable, paused, thrustAudioTrack, tutorialIsVisible;

   private FishDrone[] fishDrones;
   private FishPool fishPool;

   private float deRotationTime, thrustAudioLength, thrustAudioTimer;
   private float fuelLevel = FUEL_MAX;
   private float masterVolume = 1.0f;
   private float maxPower = INITIAL_POWER_LEVEL;

   private GameObject cockpit;
   private GameObject thrusterBell;
   private GameObject thrustLight;
   private GameObject tutorialText;

   private GlueCam glueCam;

   private ParticleSystem thrustParticleSystem;

   private ParticleSystem.EmissionModule thrustBubbles;

   private PickupTracker pickupTracker;

   private Quaternion startRotation;

   private Rigidbody thisRigidbody;

   private Slider gasLevelSlider;
   private Slider thrustPowercapSlider;
   private Slider thrustPowerSlider;

   private Timekeeper timeKeeper;

   private UIcontrol uiControl;

   private Vector3 baseThrust = new Vector3(0, 13000, 0);
   private Vector3 localEulers = Vector3.zero;
   private Vector3 startPosition = Vector3.zero;
   private Vector3 threeControlAxis = Vector3.zero;

   // *Sometimes* compiler's aren't smarter than you.
   #pragma warning disable 0414
   private CameraShakeInstance shake = null;
   #pragma warning restore 0414

   // Properties for CameraShake.
   private float shakeMagnitude = 1.0f;
   private float shakeRampDown = 2.0f;
   private float shakeRampUp = 0.2f;
   private float shakeRough = 1.0f;

   // 3D positional & rotational influence of CameraShake, as percentage [1 = 100%]).
   private Vector3 shakePosInf = new Vector3(0.75f, 0.55f, 0.15f);
   private Vector3 shakeRotInf = new Vector3(2f, 3f, 7f);
   #endregion

   private void AdjustEmissionRate(float newRate)
   {
      thrustBubbles.rateOverTime = newRate;
   }

   private void AdjustThrusterPower(float delta)
   {
      delta *= POWER_CONTROLLER_FACTOR;
      float deltaPlus = delta + thrustPowerSlider.value;
      if (deltaPlus > THRUST_MAX) thrustPowerSlider.value = THRUST_MAX;
      else if (deltaPlus < THRUST_MIN + THRUST_POWER_BASE) thrustPowerSlider.value = THRUST_MIN + THRUST_POWER_BASE;
      else thrustPowerSlider.value += delta;
      if (thrustPowerSlider.value > maxPower) thrustPowerSlider.value = maxPower;
      DoPowerUpdate();
   }

   private void AutoDeRotate()
   {
      float assertion = Mathf.Abs(Time.time - deRotationTime) * DEROTATION_RATE;
      localEulers = transform.localRotation.eulerAngles;
      float playerTilt = localEulers.z;
      if (playerTilt >= HALF_ARC && playerTilt < TILT_LIMIT_MAX)
         transform.Rotate(Vector3.forward * (playerTilt * assertion) * Time.deltaTime);
      else if (playerTilt < HALF_ARC && playerTilt > TILT_LIMIT_MIN)
         transform.Rotate(Vector3.back * ((playerTilt + HALF_ARC) * assertion) * Time.deltaTime);
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

   public float BoostMaxPower(float boost)
   {
      maxPower += boost;
      if (maxPower > 1) maxPower = 1;
      thrustPowercapSlider.value = maxPower;
      DoPowercapUpdate();
      return maxPower;
   }

   private void DebugControlPoll()
   {
      if (Input.GetKeyDown(KeyCode.B)) BoostMaxPower(0.025f); // 2.5% boost
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
         if (!Mathf.Approximately(transform.up.x, transform.rotation.x)) AutoDeRotate();
         if (threeControlAxis.z == 0) EndExpulsion();
      }
   }

   private void DoGasUpdate()
   {
      Color colour;
      float ratio = fuelLevel / FUEL_MAX;
      colour = Vector4.Lerp(gasHigh, gasLow, 1 - ratio);
      cockpit.GetComponent<MeshRenderer>().material.color = gasFill.color = colour;
      gasSlideText.text = "Gas Level: " + GasPercent + "%";
   }

   public void DoGoalUpdate()
   {
      goalSlideText.text = "Mini-Goal Progress: " + pickupTracker.PickupPercent() + "%";
   }

   private void DoPowercapUpdate()
   {
      powercapSlideText.text = "Power Cap: " + Mathf.FloorToInt(maxPower * 100) + "%";
      Color colour;
      float ratio = thrustPowercapSlider.value / THRUST_MAX;
      colour = Vector4.Lerp(thrustHigh, thrustLow, 1 - ratio);
      thrustcapFill.color = colour;
   }

   private void DoPowerUpdate()
   {
      thrustLight.GetComponent<Light>().range = THRUST_LIGHTRANGE_MAX;

      Color colour;
      float ratio = thrustPowerSlider.value / THRUST_MAX;
      colour = Vector4.Lerp(thrustHigh, thrustLow, 1 - ratio);

      thrustFill.color = colour;
      if (fuelLevel > 0 && fuelLevel < FUEL_WARN_LEVEL) colour = Color.red;
      thrusterBell.GetComponent<MeshRenderer>().material.color = thrustLight.GetComponent<Light>().color = colour;
      // TODO indicate (in text) when at minimum: BASE_POWER?
      powerSlideText.text = "Power Level: " + Mathf.FloorToInt(100 - (100 * ((THRUST_MAX - thrustPowerSlider.value) / THRUST_MAX))) + "%";
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
         DoPowerUpdate();
         return true;
      }
      else
      {
         gasLevelSlider.value = fuelLevel;
         return false;
      }
   }

   private void FixedUpdate()
   {
      GenerateFuel();
      PlayerControlPoll();
      MaintainAlignment();
      if (debugMode) DebugControlPoll();
   }

   private int FrameRate { get { return (int)(1.0f / Time.smoothDeltaTime); } }

   private int GasPercent
   {
      get { return Mathf.FloorToInt(100 - (100 * ((FUEL_MAX - fuelLevel) / FUEL_MAX))); }
   }

   private void GenerateFuel()
   {
      fuelLevel += Time.fixedDeltaTime * FUEL_GEN_RATE;
      if (fuelLevel > FUEL_MAX) fuelLevel = FUEL_MAX;
      gasLevelSlider.value = fuelLevel;
      DoGasUpdate();
   }

   private void HideTutorial()
   {
      timeKeeper.Begin();
      tutorialIsVisible = false;
      tutorialText.SetActive(false);
      uiControl.Visible = true;
   }

   private void InitVars()
   {
      audioSources = GetComponents<AudioSource>();
      thisRigidbody = GetComponent<Rigidbody>();
      thrustParticleSystem = GetComponent<ParticleSystem>();

      fishDrones = FindObjectsOfType<FishDrone>();
      fishPool = FindObjectOfType<FishPool>();
      glueCam = FindObjectOfType<GlueCam>();
      pickupTracker = FindObjectOfType<PickupTracker>();
      timeKeeper = FindObjectOfType<Timekeeper>();
      uiControl = FindObjectOfType<UIcontrol>();

      cockpit = GameObject.FindGameObjectWithTag("Cockpit");
      thrusterBell = GameObject.FindGameObjectWithTag("Thruster_Bell");
      thrustLight = GameObject.FindGameObjectWithTag("Thruster_Light");
      tutorialText = GameObject.FindGameObjectWithTag("Tutorial_Text");

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

      AdjustEmissionRate(EMISSION_RATE_INACTIVE);
      thrustPowerSlider.maxValue = THRUST_MAX;
      thrustPowerSlider.minValue = THRUST_MIN;
      thrustPowercapSlider.minValue = 0f;
      thrustPowercapSlider.maxValue = 1f;
      thrustPowercapSlider.value = maxPower;
      SetPower(INITIAL_POWER_LEVEL);
      DoPowercapUpdate();

      gasLevelSlider.maxValue = FUEL_MAX;
      gasLevelSlider.minValue = 0;
      gasLevelSlider.value = fuelLevel;
      DoGasUpdate();
   }

   private void MaintainAlignment()
   {
      transform.position = new Vector3(transform.position.x, transform.position.y, 0.0f); // Lock Z position.
      transform.rotation = Quaternion.Euler(0.0f, 0.0f, transform.rotation.eulerAngles.z); // Lock XY rotation.
   }

   private void OnGUI()
   {
      if (debugMode)
      {
         //GUIStyle style = new GUIStyle();
         //style.richText = true;
         string guiString = "<color=\"Red\">" + FrameRate + "</color>";
         GUI.Label(new Rect(0, 0, 100, 100), guiString);
      }
   }

   private void OnCollisionEnter(Collision collision)
   {
      if (collision.gameObject.tag == "BadObject_01")
      {
         if (!invulnerable)
         {
            xAudio.PlayOneShot(collisionSound, masterVolume * VOLUME_COLLISION);
            fuelLevel -= DAMAGE_VALUE;
            if (fuelLevel < 0) fuelLevel = 0;
            GameObject leakDamage = (GameObject)Instantiate(collisionEffect, transform.position, Quaternion.identity);
            shake = CameraShaker.Instance.ShakeOnce(shakeMagnitude, shakeRough, shakeRampUp, shakeRampDown, shakePosInf, shakeRotInf);
            Destroy(leakDamage, KILL_TIMER);
         }
         else Debug.Log("invulnerable: BO-01");
      }
   }

   private void OnTriggerEnter(Collider other)
   {
      if (other.gameObject.tag == "GoodObject_01")
      {
         if (pickupTracker.ClaimPickup(other))
         {
            Destroy(other.gameObject, 0.01f);
            xAudio.PlayOneShot(bonusSound, masterVolume * VOLUME_PICKUP);
            GameObject pickupPop = (GameObject)Instantiate(pickupEffect, other.transform.position, Quaternion.identity);
            Destroy(pickupPop, KILL_TIMER);
            fuelLevel += FUEL_PICKUP_VALUE;
            if (fuelLevel > FUEL_MAX) fuelLevel = FUEL_MAX;
            DoGasUpdate();
         }
      }
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
         uiControl.Visible = false;
      }
      else
      {
         if (!thrustAudioTrack)
         {
            thrustAudio.PlayOneShot(thrustSound, masterVolume * VOLUME_THRUST);
            thrustAudioTimer = Time.time;
            thrustAudioTrack = true;
         }
         if (timeKeeper.Running)
         {
            tutorialText.SetActive(false);
            uiControl.Visible = true;
         }
      }
      return paused;
   }

   private void PlayerControlPoll()
   {
      PollMisc();
      PollPower();
      PollRotation();
      PollThrust();
   }

   private void PollMisc()
   {
      // SumPause is Polling: Q, R & ESC keys.
      // PickupTracker is Polling: M, N only for debug purposes.
      // FishPool is Polling: K, L only for debug purposes.

      // Set power to percentage based on alpha-numeric inputs, 10%-100% 
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

   private void PollPower()
   {
      threeControlAxis.y = CrossPlatformInputManager.GetAxis(AXIS_POWER);
      if (threeControlAxis.y != 0) AdjustThrusterPower(threeControlAxis.y);
   }

   private void PollRotation()
   {
      threeControlAxis.x = CrossPlatformInputManager.GetAxis(AXIS_ROTATION);
      if (threeControlAxis.x != 0)
      {
         if (tutorialIsVisible) HideTutorial();
         if (ExpelGas(EXPEL_RATE_ROTATE))
         {
            Rotate(threeControlAxis.x);
            deRotating = false;
         }
      }
      else DeRotate();
   }

   private void PollThrust()
   {
      threeControlAxis.z = CrossPlatformInputManager.GetAxis(AXIS_THRUST);
      if (threeControlAxis.z != 0)
      {
         if (ExpelGas(EXPEL_RATE_THRUST))
         {
            Thrust(threeControlAxis.z);
            AdjustEmissionRate(EMISSION_RATE_THRUST);
         }
         else EndExpulsion();
         if (tutorialIsVisible) HideTutorial();
      }
      else if (thrustAudio.isPlaying) EndExpulsion();
   }

   public void Restart()
   {
      fishPool.Reset();
      fuelLevel = FUEL_MAX;
      pickupTracker.Restart();
      glueCam.Restart();
      thisRigidbody.velocity = Vector3.zero;
      timeKeeper.Restart();
      transform.position = startPosition;
      transform.rotation = startRotation;
      tutorialIsVisible = true;
      tutorialText.SetActive(true);
      uiControl.Visible = false;
      foreach (FishDrone drone in fishDrones) drone.Reset();
   }

   private void Rotate(float direction)
   {
      float rotationLightLevel = 0.25f; // minimum 25%
      transform.Rotate(Vector3.back * ROTATION_FACTOR * Time.fixedDeltaTime * direction);
      if (thrustBubbles.rateOverTime.constant < EMISSION_RATE_ROTATION) AdjustEmissionRate(EMISSION_RATE_ROTATION);
      if (thrustLight.GetComponent<Light>().range < (THRUST_LIGHTRANGE_MAX * rotationLightLevel))
         thrustLight.GetComponent<Light>().range = THRUST_LIGHTRANGE_MAX * rotationLightLevel;
   }

   private void SetPower(float power)
   {
      if (power > maxPower) power = maxPower;
      thrustPowerSlider.value = power;
      DoPowerUpdate();
   }

   private void Start()
   {
      InitVars();

      // just hanging onto a formula someone posted that might be handy when I get to "level 2":
      // movementFactor = (Mathf.Sin(Time.time * oscillationSpeed)) / 2f + 0.5f;
   }

   private void Thrust(float force)
   {
      Vector3 appliedForce = baseThrust * (thrustPowerSlider.value * THRUST_FACTOR * Time.deltaTime * force); 
      thisRigidbody.AddRelativeForce(appliedForce);
      if (thrustAudioTimer + thrustAudioLength - CLIP_TIME < Time.time)
      {
         thrustAudio.Stop();
         thrustAudio.PlayOneShot(thrustSound, masterVolume * VOLUME_THRUST);
         thrustAudioTimer = Time.time;
      }
   }
}
