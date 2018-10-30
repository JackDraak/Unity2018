//    Dev-Notes:
//
//    idea fish bopping bonus level: gain extra %'s of Gas Level. 
//           (bopping fish add fuel in general? bonus-level increases rate?
//           later levels require fish-bopping to survive?)
//    
//    TODO : setup a NavMesh and use it to help define a "spawn area" (for FishPool) along with the AI boundary 
//           colliders and scenery colliders?

//    TODO : Casual-mode get's casual music? HUD indicator(DONE) Something else?
//    TODO : Improve tasklist format/content further?
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

   [SerializeField] AudioClip       bonusSound; 
   [SerializeField] AudioClip       collisionSound;
   [SerializeField] AudioClip       thrustSound;

   [SerializeField] Color           gasHigh = Color.clear;
   [SerializeField] Color           gasLow = Color.clear;
   [SerializeField] Color           thrustHigh = Color.clear;
   [SerializeField] Color           thrustLow = Color.clear;

   [SerializeField] GameObject      collisionEffect;
   [SerializeField] GameObject      pickupEffect;

   [SerializeField] Image           gasFill = null;
   [SerializeField] Image           thrustcapFill = null;
   [SerializeField] Image           thrustFill = null;

   [SerializeField] TextMeshProUGUI casualModeIndicatorText;
   [SerializeField] TextMeshProUGUI gasSlideText;
   [SerializeField] TextMeshProUGUI goalSlideText;
   [SerializeField] TextMeshProUGUI powerSlideText;
   [SerializeField] TextMeshProUGUI powercapSlideText;
   [SerializeField] TextMeshProUGUI pilotNameText;
   #endregion

   #region Private Variables
   const float CLIP_TIME = 0.5f;
   const float DAMAGE_VALUE = 35f;
   const float DELAY_GOAL_UPDATE = 0.35f;
   const float DEROTATION_RATE = 0.2f;
   const float EMISSION_RATE_INACTIVE = 1.3f;
   const float EMISSION_RATE_ROTATION = 20f;
   const float EMISSION_RATE_THRUST = 60f;
   const float EXPEL_RATE_ROTATE = 0.5f;
   const float EXPEL_RATE_THRUST = 1f;
   const float FUEL_GEN_RATE = 25f; // need to find a balance where sinking is inevitable at any power level, based on gen alone
   const float FUEL_MAX = 1000f;
   const float FUEL_PICKUP_VALUE = 200f;
   const float FUEL_POWER_FACTOR = 0.75f;
   const float FUEL_USE_RATE = 10000f;
   const float FUEL_WARN_LEVEL = 20f;
   const float INITIAL_POWER_LEVEL = 0.3f;
   const float KILL_TIMER = 4f;
   const float POWER_CONTROLLER_FACTOR = 0.008f;
   const float ROTATION_FACTOR = 230f;
   const float TILT_LIMIT_MAX = 357.5f;
   const float TILT_LIMIT_MIN = 2.5f;
   const float THRUST_FACTOR = 0.08f;
   const float THRUST_FADE_FACTOR = 0.03f;
   const float THRUST_LIGHTRANGE_MAX = 2f;
   const float THRUST_MAX = 1f;
   const float THRUST_MIN = 0f;
   const float THRUST_POWER_BASE = 0.15f; 
   const float THRUST_POWER_FACTOR = 0.02f;
   const float VOLUME_COLLISION = 0.4f;
   const float VOLUME_PICKUP = 0.5f;
   const float VOLUME_THRUST = 0.2f;

   const int DELAY_PROGRESS = 6;
   const int HALF_ARC = 180;

   AudioSource[] audioSources;
   AudioSource xAudio, thrustAudio;

   bool debugMode, deRotating, invulnerable, paused, thrustAudioTrack, tutorialIsVisible;

   FishDrone[] fishDrones;

   FishPool fishPool;

   float deRotationTime, thrustAudioLength, thrustAudioTimer;
   float fuelLevel = FUEL_MAX;
   float masterVolume = 1.0f;
   float maxPower = INITIAL_POWER_LEVEL;

   GameObject cockpit;
   GameObject thrusterBell;
   GameObject thrustLight;
   GameObject tutorialText;

   GlueCam glueCam;

   ParticleSystem thrustParticleSystem;

   ParticleSystem.EmissionModule thrustBubbles;

   PickupTracker pickupTracker;

   Pilot pilot;

   Quaternion startRotation;

   Rigidbody thisRigidbody;

   Slider gasLevelSlider;
   Slider thrustPowercapSlider;
   Slider thrustPowerSlider;

   Timekeeper timeKeeper;

   UIcontrol uiControl;

   Vector3 baseThrust = new Vector3(0, 13000, 0);
   Vector3 localEulers = Vector3.zero;
   Vector3 startPosition = Vector3.zero;
   #endregion

   void AdjustEmissionRate(float newRate) { thrustBubbles.rateOverTime = newRate; }

   public void AdjustThrusterPower(float delta)
   {
      delta *= POWER_CONTROLLER_FACTOR;
      float deltaPlus = delta + thrustPowerSlider.value;
      if (deltaPlus > THRUST_MAX) thrustPowerSlider.value = THRUST_MAX;
      else if (deltaPlus < THRUST_MIN + THRUST_POWER_BASE)
         thrustPowerSlider.value = THRUST_MIN + THRUST_POWER_BASE;
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

   void AutoDeRotate()
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
      restarting = true;
      Invoke("Restart", DELAY_PROGRESS);
      pickupTracker.TriggerCountdown(DELAY_PROGRESS);
   }

   void Awake()
   {
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
      if (thrustAudio.isPlaying) thrustAudio.Stop();
      thrustAudioTimer = Time.time - thrustAudioLength;
   }

   public void CasualMode()
   {
      casualMode = !casualMode;
      IndicateMode();
   }

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

   void DoGasUpdate()
   {
      Color colour;
      float ratio = fuelLevel / FUEL_MAX;
      colour = Vector4.Lerp(gasHigh, gasLow, 1 - ratio);
      cockpit.GetComponent<MeshRenderer>().material.color = gasFill.color = colour;
      gasSlideText.text = "Gas Level: " + ApplyColour.Green + GasPercent + "%" + ApplyColour.Close;
   }

   public void DoGoalUpdate()
   {
      goalSlideText.text = "Mini-Goal Progress: " + ApplyColour.Green + pickupTracker.PickupPercent + "%" + ApplyColour.Close;
   }

   void DoPowercapUpdate()
   {
      powercapSlideText.text = "Power Cap: " + ApplyColour.Green + Mathf.FloorToInt(maxPower * 100) + "%" + ApplyColour.Close;
      Color colour;
      float ratio = thrustPowercapSlider.value / THRUST_MAX;
      colour = Vector4.Lerp(thrustHigh, thrustLow, 1 - ratio);
      thrustcapFill.color = colour;
   }

   void DoPowerUpdate()
   {
      string sliderText;
      thrustLight.GetComponent<Light>().range = THRUST_LIGHTRANGE_MAX;

      Color colour;
      float ratio = thrustPowerSlider.value / THRUST_MAX;
      colour = Vector4.Lerp(thrustHigh, thrustLow, 1 - ratio);

      thrustFill.color = colour;
      if (fuelLevel > 0 && fuelLevel < FUEL_WARN_LEVEL) colour = Color.red;
      thrusterBell.GetComponent<MeshRenderer>().material.color = thrustLight.GetComponent<Light>().color = colour;
      if (thrustPowerSlider.value == THRUST_POWER_BASE)
         sliderText = "Power Level"+ ApplyColour.Blue + "(at minimum)" + ApplyColour.Close + ": ";
      else sliderText = "Power Level: ";
      int currentPercent = Mathf.FloorToInt(100 - (100 * ((THRUST_MAX - thrustPowerSlider.value) / THRUST_MAX)));
      powerSlideText.text = sliderText + ApplyColour.Green + currentPercent + "%" + ApplyColour.Close;
   }

   void EndExpulsion()
   {
      thrustAudio.Stop();
      AdjustEmissionRate(EMISSION_RATE_INACTIVE);
      thrustAudioTimer = Time.time - thrustAudioLength; 
      thrustLight.GetComponent<Light>().range = Mathf.Lerp(thrustLight.GetComponent<Light>().range, 0, THRUST_FADE_FACTOR);
      thrusterBell.GetComponent<MeshRenderer>().material.color = 
         Vector4.Lerp(thrusterBell.GetComponent<MeshRenderer>().material.color, Color.black, THRUST_FADE_FACTOR);
   }

   bool ExpelGas(float rate)
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

   void FixedUpdate()
   {
      GenerateFuel();
      lockXYrotation();
      LockZposition();
   }

   int FrameRate { get { return (int)(1.0f / Time.smoothDeltaTime); } }

   int GasPercent { get { return Mathf.FloorToInt(100 - (100 * ((FUEL_MAX - fuelLevel) / FUEL_MAX))); } }

   void GenerateFuel()
   {
      fuelLevel += Time.deltaTime * FUEL_GEN_RATE;
      if (fuelLevel > FUEL_MAX) fuelLevel = FUEL_MAX;
      gasLevelSlider.value = fuelLevel;
      DoGasUpdate();
   }

   void HideTutorial()
   {
      timeKeeper.Begin();
      tutorialIsVisible = false;
      tutorialText.SetActive(false);
      uiControl.Visible = true;
   }

   void IndicateMode()
   {
      if (!casualMode)
      {
         casualModeIndicatorText.text = "";
         if (pickupTracker.Count == 0 && !restarting && Time.time > 1) AutoRestart();
      }
      else casualModeIndicatorText.text = 
            ApplyColour.Green + "~" + 
            ApplyColour.Close + " Casual Mode " + 
            ApplyColour.Green + "~" + 
            ApplyColour.Close;
   }

   void Init()
   {
      audioSources            = GetComponents<AudioSource>();
      thisRigidbody           = GetComponent<Rigidbody>();
      thrustParticleSystem    = GetComponent<ParticleSystem>();

      fishDrones              = FindObjectsOfType<FishDrone>();
      fishPool                = FindObjectOfType<FishPool>();
      glueCam                 = FindObjectOfType<GlueCam>();
      pickupTracker           = FindObjectOfType<PickupTracker>();
      pilot                   = FindObjectOfType<Pilot>();
      timeKeeper              = FindObjectOfType<Timekeeper>();
      uiControl               = FindObjectOfType<UIcontrol>();

      cockpit                 = GameObject.FindGameObjectWithTag("Cockpit");
      thrusterBell            = GameObject.FindGameObjectWithTag("Thruster_Bell");
      thrustLight             = GameObject.FindGameObjectWithTag("Thruster_Light");
      tutorialText            = GameObject.FindGameObjectWithTag("Tutorial_Text");

      debugMode               = Debug.isDebugBuild;
      startPosition           = transform.position;
      startRotation           = transform.rotation;
      thrustAudioLength       = thrustSound.length;
      thrustAudioTimer        = 0 - thrustAudioLength;
      thrustBubbles           = thrustParticleSystem.emission;

      thrustAudio             = audioSources[0];
      xAudio                  = audioSources[1];

      casualMode              = false;
      deRotating              = false;
      invulnerable            = false;
      paused                  = false;
      thrustAudioTrack        = true;
      tutorialIsVisible       = true;

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

      pilotNameText.text = pilot.ID;
      IndicateMode();
   }

   public void Invulnerable() { invulnerable = !invulnerable; }

   void LockZposition()
   {
      transform.position = new Vector3(transform.position.x, transform.position.y, 0.0f); // Lock Z position.
   }

   void lockXYrotation()
   {
      transform.rotation = Quaternion.Euler(0.0f, 0.0f, transform.rotation.eulerAngles.z); // Lock XY rotation.
   }

   void OnCollisionEnter(Collision collision)
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

   void OnGUI()
   {
      if (debugMode) GUI.Label(new Rect(0, 0, 100, 100), "<color=\"Red\">" + FrameRate + "</color>");
   }

   void OnTriggerEnter(Collider other)
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

   void Restart()
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

   void Rotate(float direction)
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

   void SpewRotationBubbles()
   {
      float rotationLightLevel = 0.25f; // minimum 25%
      if (thrustBubbles.rateOverTime.constant < EMISSION_RATE_ROTATION) AdjustEmissionRate(EMISSION_RATE_ROTATION);
      if (thrustLight.GetComponent<Light>().range < (THRUST_LIGHTRANGE_MAX * rotationLightLevel))
         thrustLight.GetComponent<Light>().range = THRUST_LIGHTRANGE_MAX * rotationLightLevel;
   }

   void StopRotationBubbles()
   {
      if (thrustBubbles.rateOverTime.constant == EMISSION_RATE_ROTATION) AdjustEmissionRate(EMISSION_RATE_INACTIVE);
   }

   void Start() { Init(); }

   void Thrust(float force)
   {
      Vector3 appliedForce = baseThrust * thrustPowerSlider.value * THRUST_FACTOR * Time.deltaTime * force; 
      thisRigidbody.AddRelativeForce(appliedForce);
   }

   public void TopFuel() { fuelLevel = FUEL_MAX; }

   public void TriggerRestart() { if (!restarting && casualMode) AutoRestart(); }

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

/// Other license terms:
// https://freesound.org/people/reinsamba/sounds/35631/ : https://creativecommons.org/licenses/by/3.0/ : bonusSound
