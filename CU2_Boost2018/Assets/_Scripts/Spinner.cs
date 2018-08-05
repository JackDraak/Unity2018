using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spinner : MonoBehaviour {

   [SerializeField] float spinRate = 0.1f;

   private void Update()
   {
      transform.Rotate(0, Time.time * spinRate, 0, Space.World);
   }

}
