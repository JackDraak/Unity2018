using UnityEngine;

public class FishDrone : MonoBehaviour {

   private bool on;
   private float changeDelay, changeTime;
   private float speed, newSpeed;
   private float turnRate;
   private float roughScale, scaleFactor;
   private Rigidbody thisRigidbody;
   private Vector3 planes = Vector3.zero;
   private Animator animator;

   private const float ANIMATION_SPEED_FACTOR = 1.8f;
   private const float CHANGE_MAX = 3f; // TODO change
   private const float CHANGE_MIN = 2f; // TODO change
   private const float SCALE_MAX = 1.6f;
   private const float SCALE_MIN = 0.4f;
   private const float SPEED_MAX = 1.3f;
   private const float SPEED_MIN = 0.2f;
   private const float TURN_MAX = 10f;
   private const float TURN_MIN = 3f;

   private void DoLerp()
   {
      if (!(Mathf.Approximately(speed, newSpeed)))
      {
         speed = Mathf.Lerp(speed, newSpeed, 0.003f);
         animator.SetFloat("stateSpeed", speed * scaleFactor * ANIMATION_SPEED_FACTOR);
      }
   }

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
      if (changeTime + changeDelay < Time.time)
      {
         SetSpeed();
         //Debug.Log(transform + "speed: " + speed);
      }
      DoLerp();
   }

   private void OnCollisionEnter(Collision collision)
   {
      if (collision.gameObject.tag == "Player") speed = speed + 1;
   }

   private void OnTriggerEnter(Collider other)
   {
      if (other.gameObject.tag == "BadObject_01") transform.Rotate(0, 180, 0);
   }

   private void SetSpeed()
   {
      changeTime = Time.time;
      changeDelay = Random.Range(CHANGE_MIN, CHANGE_MAX);
      newSpeed = Random.Range(SPEED_MIN, SPEED_MAX);
   }

   private void Start()
   {
      turnRate = Random.Range(TURN_MIN, TURN_MAX);
      if (FiftyFifty()) turnRate = -turnRate;

      Vector3 scale = Vector3.zero;
      scale.x = Random.Range(SCALE_MIN, SCALE_MAX);
      scale.y = Random.Range(SCALE_MIN, SCALE_MAX);
      scale.z = Random.Range(SCALE_MIN, SCALE_MAX);
      transform.localScale = scale;

      roughScale = scale.x + scale.y + scale.z / 3.0f;
      if (roughScale < 2)
      {
         scaleFactor = 1.7f;
      }
      else if (roughScale < 3)
      {
         scaleFactor = 0.7f; // TODO
      }
      else scaleFactor = 0.4f; // TODO
      Debug.Log(roughScale);

      animator = GetComponent<Animator>();
      SetSpeed();
   }
}
