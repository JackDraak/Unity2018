using UnityEngine;

public class Spinner : MonoBehaviour
{
   private const float SPINRATE_MAX = 100f;
   private const float SPINRATE_MIN = 10f;

   private Vector3 spin = Vector3.zero;
   private float spinRate;

   private bool FiftyFifty { get { return (Mathf.FloorToInt(Random.Range(0, 2)) == 1); } }

   private void Start()
   {
      spinRate = Random.Range(SPINRATE_MIN, SPINRATE_MAX);
      if (FiftyFifty) spinRate = -spinRate;
   }

   private void Update()
   {
      spin.y = Time.deltaTime * spinRate;
      transform.Rotate(spin, Space.World);
   }
}
