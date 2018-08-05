using UnityStandardAssets.CrossPlatformInput;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour {

   [SerializeField] AudioClip bonusSound;
   [SerializeField] AudioClip collisionSound;
   [SerializeField] AudioClip thrustSound;
   [SerializeField] GameObject collisionEffect;
   [SerializeField] GameObject thrustLight;
   [SerializeField] GameObject tutorialText;
   [SerializeField] Text FuelReadout;

   private const float CLIP_TIME = 0.5f;
   private const float DAMAGE_VALUE = 100f;
   private const float FUEL_PICKUP_VALUE = 500f;
   private const string GUAGE_LABEL = "Gas Reserve: ";
   private const string HUD_COLOUR = "\"#FF7070\"";
   private const float KILL_TIMER = 4f;
   private const float MASTER_VOLUME = 1.0f;
   private const int ROTATION_FACTOR = 300;
   private const int THRUST_FACTOR = 10;
   private const float THRUST_VOLUME = 0.22f;

   private bool debugMode, deRotating, invulnerable, tutorialIsVisible;
   private float currentEmissionRate;
   private float deRotationTime;
   private float fuelGenRate = 50f;
   private float fuelLevel = 1000f;
   private float fuelMax = 1000f;
   private float fuelUseRate = 70f;
   private float rotationEmissionRate = 20f;
   private float thrustAudioLength;
   private float thrustAudioTimer;
   private float thrustEmissionRate = 60f;
   private float thrustNonEmissionRate = 1.3f;
   private float thrustSliderMax = 120f;
   private float thrustSliderMin = 25f;
   private float thrustSliderValue = 45f;
   private float thrustMax = 0f;
   private AudioSource audioSource;
   private ParticleSystem thrustParticleSystem;
   private ParticleSystem.EmissionModule thrustBubbles;
   private Rigidbody thisRigidbody;
   private RigidbodyConstraints rigidbodyConstraints;
   private Vector3 threeControlAxis = Vector3.zero;
   private Vector3 thrustState = Vector3.zero;
   private Vector3 localEulers = Vector3.zero;

   void Start ()
   {
      debugMode = Debug.isDebugBuild;
      audioSource = GetComponent<AudioSource>();
      thisRigidbody = GetComponent<Rigidbody>();
      thrustParticleSystem = GetComponent<ParticleSystem>();

      thrustBubbles = thrustParticleSystem.emission;
      thrustAudioLength = thrustSound.length;
      thrustAudioTimer -= thrustAudioLength;

      deRotating = false;
      invulnerable = false;
      tutorialIsVisible = true;

      AdjustEmissionRate(thrustNonEmissionRate);
      thrustLight.SetActive(false);
      RefreshFuelGuage();

      thisRigidbody.constraints = 
         RigidbodyConstraints.FreezeRotationX | 
         RigidbodyConstraints.FreezeRotationY | 
         RigidbodyConstraints.FreezeRotationZ |
         RigidbodyConstraints.FreezePositionZ;
      rigidbodyConstraints = thisRigidbody.constraints;
   }

   void FixedUpdate ()
   {
      GenerateFuel();
      RefreshFuelGuage();
		PlayerControlPoll();
      if (debugMode) DebugControlPoll();
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
      if (thrustState.y > thrustMax) thrustMax = thrustState.y; // for debug HUD info
      if (debugMode) GUI.Label(thrustRect, "<color=" + HUD_COLOUR + "><b>Live T-Power: current/peak\n" + thrustState.y + "\n" + thrustMax + "</b></color>");
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
               GameObject newDamage = (GameObject)Instantiate(collisionEffect, transform.position, Quaternion.identity);
               audioSource.PlayOneShot(collisionSound, MASTER_VOLUME);
               Destroy(newDamage, KILL_TIMER);
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
            audioSource.PlayOneShot(bonusSound, 0.7f * MASTER_VOLUME);
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
      float speed = Mathf.Abs(Time.time - deRotationTime) * 0.2f;
      localEulers = transform.localRotation.eulerAngles;
      float playerTilt = localEulers.z;
      if (playerTilt >= 180 &&  playerTilt < 359.7f)
      {
         transform.Rotate(Vector3.forward * (playerTilt * speed) * Time.deltaTime);
      }
      else if (playerTilt < 180 && playerTilt > 0.3f)
      {
         transform.Rotate(Vector3.back * ((playerTilt + 180) * speed) * Time.deltaTime);
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
      audioSource.Stop();
      AdjustEmissionRate(thrustNonEmissionRate);
      thrustAudioTimer -= thrustAudioLength;
      thrustLight.SetActive(false);
      thrustState = Vector3.zero;
   }

   private bool ExpelGas()
   {
      float expulsionRate = fuelUseRate * Time.fixedDeltaTime;
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
         if (ExpelGas())
         {
            Thrust(threeControlAxis.z);
            AdjustEmissionRate(thrustEmissionRate);
            thrustLight.SetActive(true);
         }
         else EndExpulsion();
         if (tutorialIsVisible) HideTutorial();
      }
      else if (audioSource.isPlaying)
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
         if (ExpelGas())
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
      int meanLevel = Mathf.FloorToInt(fuelLevel);
      FuelReadout.text = GUAGE_LABEL + meanLevel.ToString();
   }

   private void Rotate(float direction)
   {
      thisRigidbody.constraints = RigidbodyConstraints.None;
      transform.Rotate(Vector3.back * ROTATION_FACTOR * Time.fixedDeltaTime * direction);
      thisRigidbody.constraints = rigidbodyConstraints;
      if (currentEmissionRate < rotationEmissionRate) AdjustEmissionRate(rotationEmissionRate);
   }

   private void Thrust(float force)
   {
      thrustState = Vector3.up * thrustSliderValue * THRUST_FACTOR * Time.deltaTime * force; 
      thisRigidbody.AddRelativeForce(thrustState);
      if (thrustAudioTimer + thrustAudioLength - CLIP_TIME < Time.time)
      {
         audioSource.Stop();
         audioSource.PlayOneShot(thrustSound, THRUST_VOLUME * MASTER_VOLUME);
         thrustAudioTimer = Time.time;
      }
   }
}
