using UnityEngine;

public class Spinner : MonoBehaviour {

   private const float SPINRATE_FAST = 100f;
   private const float SPINRATE_SLOW = 10f;

   private Vector3 spin = Vector3.zero;
   private float spinRate;

   // On average, return 'True' ~half the time, and 'False' ~half the time.
   private bool FiftyFifty()
   {
      if (Mathf.FloorToInt(Random.Range(0, 2)) == 1) return true;
      else return false;
   }

   private void Start()
   {
      spinRate = Random.Range(SPINRATE_SLOW, SPINRATE_FAST);
      if (FiftyFifty()) spinRate = -spinRate;
   }

   private void Update()
   {
      spin.y = Time.deltaTime * spinRate;
      transform.Rotate(spin, Space.World);
   }
}
