using UnityEngine;

public class FishDrone : MonoBehaviour
{
   [SerializeField] float RAYCAST_MAX_DISTANCE = 1.0f;

   private Animator animator;
   private bool avoiding = false;
   private float changeDelay, changeTime;
   private float correctedSpeed, newSpeed, speed;
   private float correctedTurnRate, turnRate;
   private float roughScale, scaleFactor;
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

   Vector3 ahead, port, starbord;
   // Bit shift the index of the layer (8) to get a bit mask
   int layerMask = 1 << 8;

   private void AvoidCollisions()
   {
      avoiding = false;
      ahead = transform.TransformDirection(Vector3.forward);
      port = ahead;
      starbord = ahead;
      port.z -= 0.5f;
      starbord.z += 0.5f;

      RaycastHit hit;
      // Look ahead.
      if (Physics.Raycast(transform.position, ahead, out hit, RAYCAST_MAX_DISTANCE, layerMask))
      {
         Debug.DrawRay(transform.position, ahead, Color.red, .1f);
         correctedSpeed = Mathf.Lerp(speed * 0.2f, speed, 1 / correctedSpeed); // TODO tweak this
         avoiding = true;
      }

      // Look left.
      if (Physics.Raycast(transform.position, port, out hit, RAYCAST_MAX_DISTANCE, layerMask))
      {
         Debug.DrawRay(transform.position, port, Color.blue, .1f);
         if (turnRate < 0) correctedTurnRate = turnRate * 3; // TODO tweak this
         else correctedTurnRate = turnRate * -3;
         avoiding = true;
      }

      // Look right.
      if (Physics.Raycast(transform.position, starbord, out hit, RAYCAST_MAX_DISTANCE, layerMask))
      {
         Debug.DrawRay(transform.position, starbord, Color.yellow, .1f);
         if (turnRate > 0) correctedTurnRate = turnRate * 3; // TODO tweak this
         else correctedTurnRate = turnRate * -3;
         avoiding = true;
      }

      // turn
      dimensions.y = Time.deltaTime * correctedTurnRate;
      transform.Rotate(dimensions, Space.World);

      // propel
      transform.Translate(Vector3.forward * Time.fixedDeltaTime * correctedSpeed, Space.Self);
      if (changeTime + changeDelay < Time.time) SetSpeed();

      // LERP speed
      if (!(Mathf.Approximately(speed, newSpeed)))
      {
         speed = Mathf.Lerp(speed, newSpeed, LERP_FACTOR_FOR_SPEED);
         animator.SetFloat("stateSpeed", speed * scaleFactor * ANIMATION_SPEED_FACTOR);
      }
   }

   // On average, return 'True' ~half the time, and 'False' ~half the time.
   private bool FiftyFifty()
   {
      if (Mathf.FloorToInt(Random.Range(0, 2)) == 1) return true;
      else return false;
   }

   private void FixedUpdate()
   {
      //TODO 
      AvoidCollisions();

      // turn
      //dimensions.y = Time.deltaTime * correctedTurnRate;
      //transform.Rotate(dimensions, Space.World);

      // propel
      //transform.Translate(Vector3.forward * Time.fixedDeltaTime * correctedSpeed, Space.Self);
      //if (changeTime + changeDelay < Time.time) SetSpeed();
      //LerpSpeeds();
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
      //if (!avoiding) // TODO make specific to advancing and not turning?
      //{
         // If speed and newSpeed are not ~=, lerp them by LERP_FACTOR_FOR_SPEED.
         if (!(Mathf.Approximately(speed, newSpeed)))
         {
            speed = Mathf.Lerp(speed, newSpeed, LERP_FACTOR_FOR_SPEED);
            animator.SetFloat("stateSpeed", speed * scaleFactor * ANIMATION_SPEED_FACTOR);
         }
      //}
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
      // This would cast rays only against colliders in layer 8.
      // But instead we want to collide against everything except layer 8. The ~ operator does this, it inverts a bitmask.
      layerMask = ~layerMask;

      animator = GetComponent<Animator>();
      startPos = transform.position;
      startQuat = transform.rotation;
      Init();
   }
}
