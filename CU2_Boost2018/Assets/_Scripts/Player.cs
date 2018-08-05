using UnityStandardAssets.CrossPlatformInput;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour {

   [SerializeField] float masterVolume = 1.0f;  // TODO move this to a more apropriate place (i.e. file).
   [SerializeField] float thrustVolume = 0.22f; // TODO move this to a more apropriate place (i.e. file).
   [SerializeField] int rotationFactor = 300;
   [SerializeField] int thrustFactor = 10;
   [SerializeField] AudioClip thrustSound;
   [SerializeField] AudioClip bonusSound;
   [SerializeField] GameObject thrustLight;
   [SerializeField] GameObject tutorialText;
   [SerializeField] Text FuelReadout;
   [SerializeField] GameObject collisionEffect;

   private bool debugMode, deRotating, invulnerable, tutorialVisible;
   private float deRotationTime;
   private float currentEmissionRate;
   private float fuelGenRate = 50;
   private float fuelLevel = 1000f;
   private float fuelMax = 1000f;
   private float fuelUseRate = 75;
   private float thrustAudioLength;
   private float thrustAudioTimer;
   private float thrustEmissionRate = 60f;
   private float rotationEmissionRate = 20f;
   private float thrustNonEmissionRate = 1.3f;
   private float thrustSliderMax = 120f;
   private float thrustSliderMin = 25f;
   private float thrustSliderValue = 45f;
   private float thrustMax = 0f;
   private Vector3 threeControlAxis = Vector3.zero;
   private Vector3 thrustState = Vector3.zero;
   private Vector3 localEulers = Vector3.zero;
   private AudioSource audioSource;
   private ParticleSystem thrustParticleSystem;
   private ParticleSystem.EmissionModule emission;
   private ParticleSystem collisionParticleSystem;
   private ParticleSystem.EmissionModule colmission;
   private Rigidbody thisRigidbody;
   private RigidbodyConstraints rigidbodyConstraints;

   void Start ()
   {
      thrustLight.SetActive(false);
      debugMode = Debug.isDebugBuild;
      audioSource = GetComponent<AudioSource>();
      thisRigidbody = GetComponent<Rigidbody>();
      thrustParticleSystem = GetComponent<ParticleSystem>();
      collisionParticleSystem = collisionEffect.GetComponent<ParticleSystem>();
      colmission = collisionParticleSystem.emission;

      emission = thrustParticleSystem.emission;
      currentEmissionRate = thrustNonEmissionRate;
      emission.rateOverTime = currentEmissionRate;
      thrustAudioLength = thrustSound.length;
      thrustAudioTimer -= thrustAudioLength;

      FuelReadout.text = fuelLevel.ToString();
      deRotating = false;
      invulnerable = false;
      tutorialVisible = true;

      thisRigidbody.constraints = 
         RigidbodyConstraints.FreezeRotationX | 
         RigidbodyConstraints.FreezeRotationY | 
         RigidbodyConstraints.FreezeRotationZ |
         RigidbodyConstraints.FreezePositionZ;
      rigidbodyConstraints = thisRigidbody.constraints;
   }

   void FixedUpdate () // Previously, FixedUpdate didnt seem to address surigng issue; Now it's definately required for smooth 'flight'.
   {
      GenerateFuel();
      FuelReadout.text = fuelLevel.ToString();
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

   private bool BurnGas()
   {
      float burnRate = fuelUseRate * Time.fixedDeltaTime;
      if (fuelLevel > burnRate)
      {
         fuelLevel -= burnRate;
         return true;
      }
      else
      {
         fuelLevel = 0;
         return false;
      }
   }

   private void DebugControlPoll()
   {
      if (Input.GetKeyDown(KeyCode.I)) invulnerable = !invulnerable;
      if (Input.GetKeyDown(KeyCode.R)) thrustMax = 0;
   }

   private void EndBurn()
   {
      audioSource.Stop();
      thrustAudioTimer -= thrustAudioLength;
      currentEmissionRate = thrustNonEmissionRate;
      emission.rateOverTime = currentEmissionRate;
      thrustState = Vector3.zero;
      thrustLight.SetActive(false);
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
         currentEmissionRate = thrustNonEmissionRate;
         emission.rateOverTime = currentEmissionRate;
         AutoDeRotate();
      }
   }

   private void GenerateFuel()
   {
      fuelLevel += Time.fixedDeltaTime * fuelGenRate;
      if (fuelLevel > fuelMax) fuelLevel = fuelMax;
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
            if (!invulnerable)
            {
               //Debug.Log("collision: BO-01");
               fuelLevel -= 100;
               if (fuelLevel < 0) fuelLevel = 0;
               GameObject newDamage = (GameObject)Instantiate(collisionEffect, transform.position, Quaternion.identity);
               Destroy(newDamage, 1f);
            }
            else Debug.Log("invulnerable: BO-01");
            break;
         case "BadObject_02":
            //Debug.Log("collision: BO-02");
            break;
         case "GoodObject_01":
            Debug.Log("Pickup Object");
            break;
         case "GoodObject_02":
            //Debug.Log("collision: GO-02");
            break;
         default:
            //Debug.Log("collision: default");
            break;
      }
   }

   private void OnTriggerEnter(Collider other)
   {
      switch (other.gameObject.tag)
      {
         case "BadObject_01":
            Debug.Log("trigger: BO-01");
            break;
         case "BadObject_02":
            //Debug.Log("collision: BO-02");
            break;
         case "GoodObject_01":
            other.gameObject.SetActive(false);
            audioSource.PlayOneShot(bonusSound, 0.5f * masterVolume);
            fuelLevel += 500;
            if (fuelLevel > fuelMax) fuelLevel = fuelMax;
            //Debug.Log("Pickup Object Trigger");
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
      Rect thrustRect = new Rect(r_x, r_y + r_h * 2 , r_w * 3, r_h *2);

      thrustSliderValue = GUI.HorizontalSlider(sliderRect, thrustSliderValue, thrustSliderMin, thrustSliderMax);
      if (thrustState.y > thrustMax) thrustMax = thrustState.y;
      GUI.Label(labelRect, "<color=\"#FF7070\"><b><i>Thruster Power Control</i></b> (Up/Down Keys)</color>");
      if (debugMode) GUI.Label(thrustRect, "<color=\"#FF7070\"><b>T-Power: current/peak\n" + thrustState.y + "\n" + thrustMax + "</b></color>");
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
         if (tutorialVisible) HideTutorial();
         if (BurnGas())
         {
            Thrust(threeControlAxis.z);
            currentEmissionRate = thrustEmissionRate;
            emission.rateOverTime = currentEmissionRate;
            thrustLight.SetActive(true);
         }
         else EndBurn();
      }
      else if (audioSource.isPlaying)
      {
         EndBurn();
      }
   }

   private void PollHorizontal()
   {
      threeControlAxis.x = CrossPlatformInputManager.GetAxis("Horizontal");
      if (threeControlAxis.x != 0)
      {
         if (tutorialVisible) HideTutorial();
         if (BurnGas())
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
         if (tutorialVisible) HideTutorial();
         AdjustThrusterPower(threeControlAxis.y);
      }
   }

   private void Rotate(float direction)
   {
      //thisRigidbody.angularVelocity = Vector3.zero;
      thisRigidbody.constraints = RigidbodyConstraints.None;
      transform.Rotate(Vector3.back * rotationFactor * Time.fixedDeltaTime * direction);
      thisRigidbody.constraints = rigidbodyConstraints;
      if (currentEmissionRate < rotationEmissionRate)
      {
         currentEmissionRate = rotationEmissionRate;
         emission.rateOverTime = currentEmissionRate;
      }
   }

   private void Thrust(float force)
   {
      thrustState = Vector3.up * thrustSliderValue * thrustFactor * Time.deltaTime * force; // weird.. smoothDeltaTime seemed to fix surging, but now it doesnt seem needed!?!?
      //if (thrustState.y > 4 * thrustSliderValue) thrustState.y = 4 * thrustSliderValue;

      thisRigidbody.AddRelativeForce(thrustState);
      // if the audio clip is in the final half second, re-que it
      if (thrustAudioTimer + thrustAudioLength - 0.5f < Time.time)
      {
         audioSource.Stop();
         audioSource.PlayOneShot(thrustSound, thrustVolume * masterVolume);
         thrustAudioTimer = Time.time;
      }
   }
}
