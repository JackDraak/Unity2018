using UnityEngine;

public class FishDrone : MonoBehaviour
{
   private Animator animator;
   private float changeDelay, changeTime;
   private float roughScale, scaleFactor;
   private float speed, newSpeed;
   private float turnRate;
   private Quaternion startQuat;
   private Rigidbody thisRigidbody;
   private Vector3 dimensions = Vector3.zero;
   private Vector3 startPos;

   private const float ANIMATION_SCALING_LARGE = 0.4f;
   private const float ANIMATION_SCALING_MED = 0.7f;
   private const float ANIMATION_SCALING_SMALL = 1.7f;
   private const float ANIMATION_SPEED_FACTOR = 1.8f;
   private const float CHANGE_TIME_MAX = 10f;
   private const float CHANGE_TIME_MIN = 4f;
   private const float LERP_FACTOR_FOR_SPEED = 0.003f;
   private const float SCALE_MAX = 1.6f;
   private const float SCALE_MIN = 0.4f;
   private const float SIZE_LARGE_BREAK = 3f;
   private const float SIZE_MID_BREAK = 2f;
   private const float SPEED_MAX = 1.3f;
   private const float SPEED_MIN = 0.2f;
   private const float TURNRATE_MAX = 10f;
   private const float TURNRATE_MIN = 3f;

   // On average, return 'True' ~half the time, and 'False' ~half the time.
   private bool FiftyFifty()
   {
      if (Mathf.FloorToInt(Random.Range(0, 2)) == 1) return true;
      else return false;
   }

   private void FixedUpdate()
   {
      // turn
      dimensions.y = Time.deltaTime * turnRate;
      transform.Rotate(dimensions, Space.World);

      // propel
      transform.Translate(Vector3.forward * Time.fixedDeltaTime * speed, Space.Self);
      if (changeTime + changeDelay < Time.time) SetSpeed();
      LerpSpeeds();

      //TODO AvoidCollisions();
   }

   private void Init()
   {
      // Set dynamic turnrate and direction.
      turnRate = Random.Range(TURNRATE_MIN, TURNRATE_MAX);
      if (FiftyFifty()) turnRate = -turnRate;

      // Set dynamic rotation in Y dimension.
      Vector3 thisRotation = Vector3.zero;
      thisRotation.y = Random.Range(0f,360f);
      transform.Rotate(thisRotation, Space.World);

      // Set dynamic scale.
      Vector3 scale = Vector3.zero;
      scale.x = Random.Range(SCALE_MIN, SCALE_MAX);
      scale.y = Random.Range(SCALE_MIN, SCALE_MAX);
      scale.z = Random.Range(SCALE_MIN, SCALE_MAX);
      transform.localScale = scale;

      // Set dynamic animation speed (~slower for larger fish).
      roughScale = scale.x + scale.y + scale.z / 3.0f; // Average the scales of the 3 planes.
      if (roughScale < SIZE_MID_BREAK) scaleFactor = ANIMATION_SCALING_SMALL;
      else if (roughScale < SIZE_LARGE_BREAK) scaleFactor = ANIMATION_SCALING_MED;
      else scaleFactor = ANIMATION_SCALING_LARGE;
      SetSpeed();
   }

   private void LerpSpeeds()
   {
      // If speed and newSpeed are not ~=, lerp them by LERP_FACTOR_FOR_SPEED.
      if (!(Mathf.Approximately(speed, newSpeed)))
      {
         speed = Mathf.Lerp(speed, newSpeed, LERP_FACTOR_FOR_SPEED);
         animator.SetFloat("stateSpeed", speed * scaleFactor * ANIMATION_SPEED_FACTOR);
      }
   }

   private void OnCollisionEnter(Collision collision)
   {
      if (collision.gameObject.tag == "Player") speed++;
   }

   public void Reset()
   {
      if (transform != null)
      {
         transform.position = startPos;
         transform.rotation = startQuat;
         Init();
      }
   }

   private void SetSpeed()
   {
      changeTime = Time.time;
      changeDelay = Random.Range(CHANGE_TIME_MIN, CHANGE_TIME_MAX);
      newSpeed = Random.Range(SPEED_MIN, SPEED_MAX);
   }

   private void Start()
   {
      animator = GetComponent<Animator>();
      startPos = transform.position;
      startQuat = transform.rotation;
      Init();
   }
}
