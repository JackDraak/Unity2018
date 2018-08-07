using UnityEngine;

public class Spinner : MonoBehaviour {

   private float spinRate;
   private Vector3 mySpin = Vector3.zero;

   private void Start()
   {
      spinRate = Random.Range(10f, 100f);
      if (FiftyFifty()) spinRate = -spinRate;
   }

   private bool FiftyFifty()
   {
      if (Mathf.FloorToInt(Random.Range(0, 2)) == 1) return true;
      else return false;
   }

   private void Update()
   {
      mySpin.y = Time.deltaTime * spinRate;
      transform.Rotate(mySpin, Space.World);
   }
}
