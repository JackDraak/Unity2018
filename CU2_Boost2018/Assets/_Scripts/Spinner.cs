using UnityEngine;

public class Spinner : MonoBehaviour
{
   const float SPINRATE_MAX = 100f;
   const float SPINRATE_MIN = 10f;

   Vector3 spin = Vector3.zero;
   float spinRate;

   bool FiftyFifty { get { return (Mathf.FloorToInt(Random.Range(0, 2)) == 1); } }

   void Start()
   {
      spinRate = Random.Range(SPINRATE_MIN, SPINRATE_MAX);
      if (FiftyFifty) spinRate = -spinRate;
   }

   void Update()
   {
      spin.y = Time.deltaTime * spinRate;
      transform.Rotate(spin, Space.World);
   }
}
