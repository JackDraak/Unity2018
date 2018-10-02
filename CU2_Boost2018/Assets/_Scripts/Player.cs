//
//    Dev-Notes:
//
//    idea fish bopping bonus level: gain extra %'s of Gas Level. (bopping fish add fuel in general? bonus-level increases rate? later levels require fish-bopping to survive?)
//    
//    TODO : FIXED - fix audio issue that cropped-up (bubbles not working right: thrust).
//    TODO : FIXED - fix issue where 'R'esetting player while in a countdown gets a bit messy?
//    TODO : COMPLETE - Move miniGoal slider to bottom right 
//    TODO : fix pickups arent despawning all of a sudden... wth?
//    TODO : Casual-mode get's casual music? HUS indicator? both? Something else?
//    TODO : Improve tasklist format/content further?
//    TODO : improve "Records"; make a leaderboard?
//    TODO : design a way to detroy the player.
//    TODO : (change upper-left HUD?) Improve timer aesthetics?
//    TODO : work on Fog / lighting? work on level 2 ideas?
//

using EZCameraShake;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
   #region Exposed Variables
   public bool casualMode, restarting;

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
   [SerializeField] TextMeshProUGUI pilotNameText;
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

   private const string HUD_COLOUR = "\"#FF7070\""; // coral /// #2DA8E9 gemstone blue

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
   #endregion

   private void AdjustEmissionRate(float newRate) { thrustBubbles.rateOverTime = newRate; }

   public void AdjustThrusterPower(float delta)
   {
      delta *= POWER_CONTROLLER_FACTOR;
      float deltaPlus = delta + thrustPowerSlider.value;
      if (deltaPlus > THRUST_MAX) thrustPowerSlider.value = THRUST_MAX;
      else if (deltaPlus < THRUST_MIN + THRUST_POWER_BASE) thrustPowerSlider.value = THRUST_MIN + THRUST_POWER_BASE;
      else thrustPowerSlider.value += delta;
      if (thrustPowerSlider.value > maxPower) thrustPowerSlider.value = maxPower;
      DoPowerUpdate();
   }

   public void ApplyRotation(float rotation)
   {
      if (tutorialIsVisible) HideTutorial();
      if (ExpelGas(EXPEL_RATE_ROTATE))
      {
         Rotate(rotation);
         deRotating = false;
      }
   }

   public void ApplyThrust(float thrust)
   {
      if (thrust != 0)
      {
         if (ExpelGas(EXPEL_RATE_THRUST))
         {
            Thrust(thrust);
            AdjustEmissionRate(EMISSION_RATE_THRUST);
         }
         else EndExpulsion();
         if (tutorialIsVisible) HideTutorial();
      }
      else EndExpulsion();
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
      else deRotating = false;
   }

   public void AutoRestart()
   {
      //if (!casualMode)
      //{
         restarting = true;
         int triggerDelay = 6; // TODO do something with this
         Invoke("Restart", triggerDelay);
         pickupTracker.TriggerCountdown(triggerDelay);
      //}
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

   public void CancelThrustAudio()
   {
      thrustAudio.Stop();
      thrustAudioTimer = Time.time - thrustAudioLength;
   }

   public void CasualMode() { casualMode = !casualMode; }

   public void DeRotate()
   {
      if (!deRotating && (!Mathf.Approximately(transform.up.x, transform.rotation.x)))
      {
         deRotationTime = Time.time;
         deRotating = true;
      }
      else
      {
         if (!Mathf.Approximately(transform.up.x, transform.rotation.x)) AutoDeRotate();
         StopRotationBubbles();
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
      string sliderText;
      thrustLight.GetComponent<Light>().range = THRUST_LIGHTRANGE_MAX;

      Color colour;
      float ratio = thrustPowerSlider.value / THRUST_MAX;
      colour = Vector4.Lerp(thrustHigh, thrustLow, 1 - ratio);

      thrustFill.color = colour;
      if (fuelLevel > 0 && fuelLevel < FUEL_WARN_LEVEL) colour = Color.red;
      thrusterBell.GetComponent<MeshRenderer>().material.color = thrustLight.GetComponent<Light>().color = colour;
      if (Mathf.Approximately(thrustPowerSlider.value, THRUST_MIN)) sliderText = "Power Level (at minimum): ";
      else sliderText = "Power Level: ";
      powerSlideText.text = sliderText + Mathf.FloorToInt(100 - (100 * ((THRUST_MAX - thrustPowerSlider.value) / THRUST_MAX))) + "%";
   }

   private void EndExpulsion()
   {
      thrustAudio.Stop();
      AdjustEmissionRate(EMISSION_RATE_INACTIVE);
      thrustAudioTimer = Time.time - thrustAudioLength; 
      thrustLight.GetComponent<Light>().range = Mathf.Lerp(thrustLight.GetComponent<Light>().range, 0, THRUST_FADE_FACTOR);
      thrusterBell.GetComponent<MeshRenderer>().material.color = 
         Vector4.Lerp(thrusterBell.GetComponent<MeshRenderer>().material.color, Color.black, THRUST_FADE_FACTOR);
   }

   private bool ExpelGas(float rate)
   {
      float expulsionRate = 
         rate * FUEL_USE_RATE * thrustPowerSlider.value * FUEL_POWER_FACTOR * THRUST_POWER_FACTOR * Time.fixedDeltaTime;
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
      lockXYrotation();
      LockZposition();
   }

   private int FrameRate { get { return (int)(1.0f / Time.smoothDeltaTime); } }
   private int GasPercent { get { return Mathf.FloorToInt(100 - (100 * ((FUEL_MAX - fuelLevel) / FUEL_MAX))); } }

   private void GenerateFuel()
   {
      fuelLevel += Time.deltaTime * FUEL_GEN_RATE;
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

   public void ImmediateRestart() { if (!restarting) AutoRestart(); }

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

      casualMode = false;
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

   public void Invulnerable() { invulnerable = !invulnerable; }

   private void LockZposition()
   {
      transform.position = new Vector3(transform.position.x, transform.position.y, 0.0f); // Lock Z position.
   }

   private void lockXYrotation()
   {
      transform.rotation = Quaternion.Euler(0.0f, 0.0f, transform.rotation.eulerAngles.z); // Lock XY rotation.
   }

   private void OnCollisionEnter(Collision collision)
   {
      if (collision.gameObject.tag == "BadObject_01")
      {
         if (!invulnerable)
         {
            float shakeMagnitude = 1.0f;
            float shakeRampDown = 2.0f;
            float shakeRampUp = 0.2f;
            float shakeRough = 1.0f;
            Vector3 shakePosInf = new Vector3(0.75f, 0.55f, 0.15f);
            Vector3 shakeRotInf = new Vector3(2f, 3f, 7f);
   
            xAudio.PlayOneShot(collisionSound, masterVolume * VOLUME_COLLISION);
            fuelLevel -= DAMAGE_VALUE;
            if (fuelLevel < 0) fuelLevel = 0;
            GameObject leakDamage = (GameObject)Instantiate(collisionEffect, transform.position, Quaternion.identity);
            Destroy(leakDamage, KILL_TIMER);
            CameraShakeInstance shake = 
               CameraShaker.Instance.ShakeOnce(shakeMagnitude, shakeRough, shakeRampUp, shakeRampDown, shakePosInf, shakeRotInf);

            // code that wont execute that the compiler beieleves will, in order to supress unused reference warning.
            if (shake == null) Debug.Log(shake.CurrentState); 
         }
         else Debug.Log("invulnerable: BO-01");
      }
   }

   private void OnGUI()
   {
      if (debugMode) GUI.Label(new Rect(0, 0, 100, 100), "<color=\"Red\">" + FrameRate + "</color>");
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
            thrustAudioTimer = Time.time - thrustAudioLength;
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

   private void Restart()
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
      restarting = false;
   }

   private void Rotate(float direction)
   {
      transform.Rotate(Vector3.back * ROTATION_FACTOR * Time.fixedDeltaTime * direction);
      SpewRotationBubbles();
   }

   public void SetPower(float power)
   {
      if (power > maxPower) power = maxPower;
      if (power < THRUST_POWER_BASE) power = THRUST_POWER_BASE;
      thrustPowerSlider.value = power;
      DoPowerUpdate();
   }

   private void SpewRotationBubbles()
   {
      float rotationLightLevel = 0.25f; // minimum 25%
      if (thrustBubbles.rateOverTime.constant < EMISSION_RATE_ROTATION) AdjustEmissionRate(EMISSION_RATE_ROTATION);
      if (thrustLight.GetComponent<Light>().range < (THRUST_LIGHTRANGE_MAX * rotationLightLevel))
         thrustLight.GetComponent<Light>().range = THRUST_LIGHTRANGE_MAX * rotationLightLevel;
   }

   private void StopRotationBubbles()
   {
      if (thrustBubbles.rateOverTime.constant == EMISSION_RATE_ROTATION) AdjustEmissionRate(EMISSION_RATE_INACTIVE);
   }

   private void Start() { InitVars(); }

   private void Thrust(float force)
   {
      Vector3 appliedForce = baseThrust * thrustPowerSlider.value * THRUST_FACTOR * Time.deltaTime * force; 
      thisRigidbody.AddRelativeForce(appliedForce);
   }

   public void TopFuel() { fuelLevel = FUEL_MAX; }

   public void TriggerThrustAudio()
   {
      if (thrustAudioTimer + thrustAudioLength - CLIP_TIME < Time.time)
      {
         thrustAudio.Stop();
         thrustAudio.PlayOneShot(thrustSound, masterVolume * VOLUME_THRUST);
         thrustAudioTimer = Time.time;
      }
   }
}
