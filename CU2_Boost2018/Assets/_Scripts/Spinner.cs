using UnityEngine;

public class Spinner : MonoBehaviour {

   private const float SPINRATE_HIGH = 100f;
   private const float SPINRATE_LOW = 10f;

   private Vector3 mySpin = Vector3.zero;
   private float spinRate;

   // On average, return 'True' ~half the time, and 'False' ~half the time.
   private bool FiftyFifty()
   {
      if (Mathf.FloorToInt(Random.Range(0, 2)) == 1) return true;
      else return false;
   }

   private void Start()
   {
      spinRate = Random.Range(SPINRATE_LOW, SPINRATE_HIGH);
      if (FiftyFifty()) spinRate = -spinRate;
   }

   private void Update()
   {
      mySpin.y = Time.deltaTime * spinRate;
      transform.Rotate(mySpin, Space.World);
   }
}
