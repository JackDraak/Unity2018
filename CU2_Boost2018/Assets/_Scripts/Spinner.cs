using UnityEngine;

public class Spinner : MonoBehaviour {

   private float spinRate;

   private void Start()
   {
      spinRate = Random.Range(10f, 100f);
   }

   private void Update()
   {
      Vector3 mySpin = Vector3.zero;
      mySpin.y = Time.deltaTime * spinRate;
      transform.Rotate(mySpin, Space.World);
   }
}
