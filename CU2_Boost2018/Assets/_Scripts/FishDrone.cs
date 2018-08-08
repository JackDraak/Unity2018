using UnityEngine;

public class FishDrone : MonoBehaviour {

   private Rigidbody thisRigidbody;
   private Vector3 planes = Vector3.zero;
   private float turnRate, speed;

   private const float SPEED_MAX = 1.2f;
   private const float SPEED_MIN = 0.4f;
   private const float TURN_MAX = 9f;
   private const float TURN_MIN = 3f;

   private bool FiftyFifty()
   {
      if (Mathf.FloorToInt(Random.Range(0, 2)) == 1) return true;
      else return false;
   }

   private void FixedUpdate()
   {
      planes.y = Time.deltaTime * turnRate;
      transform.Rotate(planes, Space.World);
      transform.Translate(Vector3.forward * Time.fixedDeltaTime * speed, Space.Self);
   }

   private void Start()
   {
      turnRate = Random.Range(TURN_MIN, TURN_MAX);
      speed = Random.Range(SPEED_MIN, SPEED_MAX);
      if (FiftyFifty()) turnRate = -turnRate;
   }
}
