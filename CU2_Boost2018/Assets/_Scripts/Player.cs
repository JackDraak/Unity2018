using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine;

public class Player : MonoBehaviour {

   [SerializeField] int rotationFactor = 300;
   [SerializeField] int thrustFactor = 500;
   [SerializeField] float thrustVolume = 0.66f;
   [SerializeField] float deR = 0.66f;
   [SerializeField] float deL = 0.66f;
   [SerializeField] float masterVolume = 1.0f;
   [SerializeField] AudioClip thrustSound;

   private Vector2 controlAxis = Vector2.zero;

   ParticleSystem thrustParticles;
   Rigidbody rigidbody;
   RigidbodyConstraints constraints;
   Vector3 localEulers;

   AudioSource audioSource;

   void Start ()
   {
      rigidbody = GetComponent<Rigidbody>();
      thrustParticles = GetComponent<ParticleSystem>();

      rigidbody.constraints = RigidbodyConstraints.FreezePositionZ | 
         RigidbodyConstraints.FreezeRotationX | 
         RigidbodyConstraints.FreezeRotationY |
         RigidbodyConstraints.FreezeRotationZ;
      constraints = rigidbody.constraints;

      audioSource = GetComponent<AudioSource>();
   }
	
	void Update ()
   {
		ProcessInput();
	}

   private void ProcessInput()
   {
      controlAxis.x = CrossPlatformInputManager.GetAxis("Horizontal");
      if (controlAxis.x != 0) Rotate(controlAxis.x);
      else DeRotate();

      controlAxis.y = CrossPlatformInputManager.GetAxis("Jump");
      if (controlAxis.y != 0) Thrust(controlAxis.y);
      else if (audioSource.isPlaying)
      {
         audioSource.Stop();
         thrustParticles.emissionRate = 0;
      }
   }

   private void DeRotate()
   {
      localEulers = transform.localRotation.eulerAngles;
      var important = localEulers.z;
      if (important >= 180 && important < 359)
      {
         transform.Rotate(Vector3.forward * (important * deR) * Time.fixedDeltaTime);
      }
      else if (important < 180 && important > 1)
      {
         transform.Rotate(Vector3.back * ((important + 180) * deL) * Time.fixedDeltaTime);
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
      rigidbody.AddRelativeForce(Vector3.up * thrustFactor * Time.deltaTime * force);
      thrustParticles.emissionRate = 100;
      if (!audioSource.isPlaying) audioSource.PlayOneShot(thrustSound, thrustVolume * masterVolume);
   }
}
