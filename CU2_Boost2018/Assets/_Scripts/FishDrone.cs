using UnityEngine;

public class FishDrone : MonoBehaviour {

   private Vector3 planes = Vector3.zero;
   private float speed;
   private Rigidbody thisRigidbody;
   private float turnRate;

   private const float SCALE_MAX = 1.5f;
   private const float SCALE_MIN = 0.5f;
   private const float SPEED_MAX = 1.2f;
   private const float SPEED_MIN = 0.4f;
   private const float TURN_MAX = 12f;
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

      Vector3 scale = Vector3.zero;
      scale.x = Random.Range(SCALE_MIN, SCALE_MAX);
      scale.y = Random.Range(SCALE_MIN, SCALE_MAX);
      scale.z = Random.Range(SCALE_MIN, SCALE_MAX);
      transform.localScale = scale;
   }
}
