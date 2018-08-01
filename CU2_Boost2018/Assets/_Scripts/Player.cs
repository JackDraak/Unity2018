using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine;

public class Player : MonoBehaviour {

   [SerializeField] float masterVolume = 1.0f;
   [SerializeField] int rotationFactor = 300;
   [SerializeField] AudioClip thrustSound;
   [SerializeField] float thrustVolume = 0.42f;
   public float tSliderValue = 500.0f;

   private AudioSource audioSource;
   private bool derot;
   private float derotTime;
   private float tEmissionRate = 113.0f;
   private float tNonEmissionRate = 13.0f;
   private float tSliderMax = 800.0f;
   private float tSliderMin = 200.0f;
   private ParticleSystem thrustParticles;
   private Rigidbody rigidbody;
   private RigidbodyConstraints constraints;
   private Vector3 controlAxis = Vector3.zero;
   private Vector3 localEulers;

   void Start ()
   {
      audioSource = GetComponent<AudioSource>();
      thrustParticles = GetComponent<ParticleSystem>();
      rigidbody = GetComponent<Rigidbody>();

      derot = false;
      var emission = thrustParticles.emission;
      emission.rateOverTime = tNonEmissionRate;

      rigidbody.constraints = RigidbodyConstraints.FreezePositionZ | 
         RigidbodyConstraints.FreezeRotationX | 
         RigidbodyConstraints.FreezeRotationY |
         RigidbodyConstraints.FreezeRotationZ;

      constraints = rigidbody.constraints;
   }

   void LateUpdate ()
   {
		ProcessInput();
	}

   private void AdjustThrust(float delta)
   {
      if (delta + tSliderValue > tSliderMax)
      {
         tSliderValue = tSliderMax;
      }
      else if (delta + tSliderValue < tSliderMin)
      {
         tSliderValue = tSliderMin;
      }
      else tSliderValue += delta;
   }

   private void CounterRotate()
   {
      float speed = Mathf.Abs(Time.time - derotTime) / 3;
      localEulers = transform.localRotation.eulerAngles;
      var important = localEulers.z;
      if (important >= 180 && important < 359)
      {
         transform.Rotate(Vector3.forward * (important * speed) * Time.deltaTime);
      }
      else if (important < 180 && important > 1)
      {
         transform.Rotate(Vector3.back * ((important + 180) * speed) * Time.deltaTime);
      }
   }

   private void DeRotate()
   {
      if (!derot)
      {
         derotTime = Time.time;
         derot = true;
      }
      else
      {
         CounterRotate();
      }
   }

   private void OnCollisionEnter(Collision collision)
   {
      switch (collision.gameObject.tag)
      {
         case "GoodObject_01":
            //Debug.Log("collision: GO-01");
            break;
         case "GoodObject_02":
            //Debug.Log("collision: GO-02");
            break;
         case "BadObject_01":
            //Debug.Log("collision: BO-01");
            break;
         case "BadObject_02":
            //Debug.Log("collision: BO-02");
            break;
         default:
            //Debug.Log("collision: default");
            break;
      }
   }

   void OnGUI()
   {
      int r_x, r_y, r_w, r_h;
      r_x = 35;
      r_y = 45;
      r_w = 100;
      r_h = 30;
      Rect sliderRect = new Rect(r_x, r_y, r_w, r_h);
      Rect labelRect = new Rect(r_x, r_y + r_h, r_w * 3, r_h);

      tSliderValue = GUI.HorizontalSlider(sliderRect, tSliderValue, tSliderMin, tSliderMax);
      GUI.Label(labelRect, "Thruster Power Control (Up/Down Keys)");
   }

   private void ProcessInput()
   {
      Pro_Horizontal();
      Pro_Vertical();
      Pro_Action();
   }

   private void Pro_Action()
   {
      var emission = thrustParticles.emission;
      controlAxis.z = CrossPlatformInputManager.GetAxis("Jump");
      if (controlAxis.z != 0)
      {
         Thrust(controlAxis.z);
         emission.rateOverTime = tEmissionRate;
      }
      else if (audioSource.isPlaying)
      {
         audioSource.Stop();
         emission.rateOverTime = tNonEmissionRate;
      }
   }

   private void Pro_Horizontal()
   {
      controlAxis.x = CrossPlatformInputManager.GetAxis("Horizontal");
      if (controlAxis.x != 0)
      {
         Rotate(controlAxis.x);
         derot = false;
      }
      else DeRotate();
   }

   private void Pro_Vertical()
   {
      controlAxis.y = CrossPlatformInputManager.GetAxis("Vertical");
      if (controlAxis.y != 0)
      {
         AdjustThrust(controlAxis.y);
      }
   }

   private void Rotate(float direction)
   {
      rigidbody.angularVelocity = Vector3.zero;
      rigidbody.constraints = RigidbodyConstraints.None;
      transform.Rotate(Vector3.back * rotationFactor * Time.fixedDeltaTime * direction);
      rigidbody.constraints = constraints;
   }

   private void Thrust(float force)
   {
      rigidbody.AddRelativeForce(Vector3.up * tSliderValue * Time.deltaTime * force);
      if (!audioSource.isPlaying) audioSource.PlayOneShot(thrustSound, thrustVolume * masterVolume);
   }
}
