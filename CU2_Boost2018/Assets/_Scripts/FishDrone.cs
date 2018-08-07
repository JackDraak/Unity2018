using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishDrone : MonoBehaviour {

   private Vector3 mySpin = Vector3.zero;
   private float spinRate, speed;
   private Rigidbody thisRigidbody;

   private bool FiftyFifty()
   {
      if (Mathf.FloorToInt(Random.Range(0, 2)) == 1) return true;
      else return false;
   }

   private void Start()
   {
      spinRate = Random.Range(1f, 6f);
      speed = Random.Range(0.66f, 1.33f);
      if (FiftyFifty()) spinRate = -spinRate;
   }

   private void FixedUpdate()
   {
      mySpin.y = Time.deltaTime * spinRate;
      transform.Rotate(mySpin, Space.World);
      transform.Translate(Vector3.forward * Time.fixedDeltaTime * speed, Space.Self);
   }
}
