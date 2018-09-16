using UnityEngine;

public class FishDrone : MonoBehaviour
{

   private Animator animator;
   private float changeDelay, changeTime;
   private float correctedSpeed, newSpeed, speed;
   private float correctedTurnRate, turnRate;
   private float roughScale, scaleFactor;
   private int layerMask; // = 1 << 8; // Bit shift the index of the layer (8) to get a bit mask
   private Quaternion startQuat;
   private Rigidbody thisRigidbody;
   private Vector3 dimensions = Vector3.zero;
   private Vector3 fore, port, starbord;
   private Vector3 startPos;
   
   private const float ANIMATION_SCALING_LARGE = 0.4f;
   private const float ANIMATION_SCALING_MED = 0.7f;
   private const float ANIMATION_SCALING_SMALL = 1.7f;
   private const float ANIMATION_SPEED_FACTOR = 1.8f;
   private const float CHANGE_TIME_MAX = 10f;
   private const float CHANGE_TIME_MIN = 4f;
   private const float LERP_FACTOR_FOR_SPEED = 0.003f;
   private const float RAYCAST_CORRECTION_FACTOR = 3.3f;
   private const float RAYCAST_DRAWTIME = 0;
   private const float RAYCAST_MAX_DISTANCE = 1.0f;
   private const float SCALE_MAX = 1.6f;
   private const float SCALE_MIN = 0.4f;
   private const float SIZE_LARGE_BREAK = 3f;
   private const float SIZE_MID_BREAK = 2f;
   private const float SPEED_MAX = 1.3f;
   private const float SPEED_MIN = 0.2f;
   private const float TURNRATE_MAX = 10f;
   private const float TURNRATE_MIN = 3f;

   private void BeFishy()
   {
      OrientView();
      PlanPath();
      Motivate();
      LerpSpeed();
   }

   private bool FiftyFifty() // On average, return 'True' ~half the time, and 'False' ~half the time.
   {
      if (Mathf.FloorToInt(Random.Range(0, 2)) == 1) return true;
      else return false;
   }

   private void FixedUpdate()
   {
      BeFishy();
   }

   private void Init()
   {
      // Set dynamic turnrate/direction.
      turnRate = Random.Range(TURNRATE_MIN, TURNRATE_MAX);
      if (FiftyFifty()) turnRate = -turnRate;

      // Set dynamic starting orientation in Y dimension.
      Vector3 thisRotation = Vector3.zero;
      thisRotation.y = Random.Range(0f, 360f);
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

   private void LerpSpeed()
   {
      if (!(Mathf.Approximately(speed, newSpeed)))
      {
         speed = Mathf.Lerp(speed, newSpeed, LERP_FACTOR_FOR_SPEED);
         animator.SetFloat("stateSpeed", speed * scaleFactor * ANIMATION_SPEED_FACTOR);
      }
   }

   private void Motivate()
   {
      // Turn.
      dimensions.y = Time.deltaTime * correctedTurnRate;
      transform.Rotate(dimensions, Space.World);

      // Propel.
      transform.Translate(Vector3.forward * Time.fixedDeltaTime * correctedSpeed, Space.Self);
      if (changeTime + changeDelay < Time.time) SetSpeed();
   }

   private void OrientView()
   {
      fore = transform.forward;
      port = starbord = fore;
      port.z -= 0.6f;
      starbord.z += 0.6f;
   }

   private void PlanPath()
   {
      RaycastHit hitPort, hitStarbord;

      // Look ahead.
      if (Physics.Raycast(transform.position, fore, RAYCAST_MAX_DISTANCE, layerMask))
      {
         Debug.DrawRay(transform.position, fore, Color.red, RAYCAST_DRAWTIME);
         correctedSpeed = Mathf.Lerp(speed, speed * 0.2f, 1 / correctedSpeed);
      }
      else correctedSpeed = speed;

      // Look left.
      if (Physics.Raycast(transform.position, port, out hitPort, RAYCAST_MAX_DISTANCE, layerMask))
      {
         Debug.DrawRay(transform.position, port, Color.blue, RAYCAST_DRAWTIME);
      }

      // Look right.
      if (Physics.Raycast(transform.position, starbord, out hitStarbord, RAYCAST_MAX_DISTANCE, layerMask))
      {
         Debug.DrawRay(transform.position, starbord, Color.yellow, RAYCAST_DRAWTIME);
      }

      // Select a direction based on path or obstacle detection.
      if (hitPort.distance > 0 && hitStarbord.distance > 0)
      {
         if (hitPort.distance < hitStarbord.distance) correctedTurnRate = turnRate * RAYCAST_CORRECTION_FACTOR;
         else correctedTurnRate = -turnRate * RAYCAST_CORRECTION_FACTOR;
      }
      else if (hitPort.distance > 0) correctedTurnRate = turnRate * RAYCAST_CORRECTION_FACTOR; // TODO finish
      else if (hitStarbord.distance > 0) correctedTurnRate = -turnRate * RAYCAST_CORRECTION_FACTOR;
      else correctedTurnRate = turnRate;
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
      // Setup for collision-avoidance: layerMask
      // Bit shift the index of the layer (8) to get a bit mask
      layerMask = 1 << 8; // This would cast rays only against colliders in layer 8.
      // But we want to collide against everything except layer 8. 
      layerMask = ~layerMask; // The ~ operator inverts the bitmask.

      animator = GetComponent<Animator>();
      startPos = transform.position;
      startQuat = transform.rotation;
      Init();
   }
}
