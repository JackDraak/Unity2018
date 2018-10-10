using UnityEngine;

public class FishDrone : MonoBehaviour
{
   private bool enhancedLogging = true;

   private Animator animator;
   private bool caution;
   private float changeDelay, changeTime;
   private float newSpeed, speed; 
   private float correctedTurnRate, lerpTurnRate, turnRate;
   private float lastContact;
   private float raycastSleepTime;
   private float revTime;
   private float roughScale, scaleFactor;
   private float speedScaleFactor;
   private int layerMask;
   private Quaternion startQuat;
   private Rigidbody thisRigidbody;
   private Vector3 fore, port, starbord;
   private Vector3 scale;
   private Vector3 startPos;

   private const float ANIMATION_SCALING_LARGE = 0.4f;
   private const float ANIMATION_SCALING_MED = 0.7f;
   private const float ANIMATION_SCALING_SMALL = 1.7f;
   private const float ANIMATION_SPEED_FACTOR = 1.8f;
   private const float CHANGE_TIME_MAX = 16.0f;
   private const float CHANGE_TIME_MIN = 4.0f;
   private const float CHANGE_TURN_DELAY = 5.0f;
   private const float LERP_FACTOR_FOR_SPEED = 0.09f;
   private const float LERP_FACTOR_FOR_TURN = 0.18f;
   private const float MOTIVATION_SCALING_LARGE = 1.0f;
   private const float MOTIVATION_SCALING_MED = 0.9f;
   private const float MOTIVATION_SCALING_SMALL = 0.8f;
   private const float RAYCAST_CORRECTION_FACTOR = 9.0f;
   private const float RAYCAST_DRAWTIME = 3.0f;
   private const float RAYCAST_DETECTION_ANGLE = 23.0f;
   private const float RAYCAST_VERTICAL_OFFSET = 0.08f;
   private const float RAYCAST_MAX_DISTANCE = 1.33f;
   private const float RAYCAST_SLEEP_DELAY = 0.4f;
   private const float REVERSE_DELAY = 6f;
   private const float SCALE_MAX = 1.8f;
   private const float SCALE_MIN = 0.3f;
   private const float SIZE_LARGE_BREAK = 3.0f;
   private const float SIZE_MID_BREAK = 2.0f;
   private const float SPEED_MAX = 1.2f;
   private const float SPEED_MIN = 0.2f;
   private const float TURNRATE_MAX = 10.0f;
   private const float TURNRATE_MIN = 5.0f;

   private void BeFishy()
   {
      OrientView();
      PlanPath();
      Motivate();
   }

   private void DetectNeighbors(out RaycastHit hitPort, out RaycastHit hitPortLow, out RaycastHit hitStarbord, out RaycastHit hitStarbordLow)
   {
      Vector3 lowPos = transform.position;
      Vector3 highPos = transform.position;
      lowPos.y -= RAYCAST_VERTICAL_OFFSET * roughScale;
      highPos.y += RAYCAST_VERTICAL_OFFSET * roughScale;

      // Look ahead.
      var fl = Physics.Raycast(lowPos, fore, RAYCAST_MAX_DISTANCE, layerMask);
      var fh = Physics.Raycast(highPos, fore, RAYCAST_MAX_DISTANCE, layerMask);
      if (fl || fh)
      {
         caution = true;
         newSpeed = Mathf.Lerp(speed, speed * 0.2f, 1 / newSpeed);
         raycastSleepTime = Time.time + RAYCAST_SLEEP_DELAY;
         lastContact = Time.time;
         if (enhancedLogging)
         {
            Debug.DrawRay(lowPos, fore, Color.yellow, RAYCAST_DRAWTIME);
            Debug.DrawRay(highPos, fore, Color.yellow, RAYCAST_DRAWTIME);
         }
      }
      else if (caution)
      {
         caution = false;
         SetNewRandomSpeed();
      }

      // Look left.
      var pl = Physics.Raycast(lowPos, port, out hitPortLow, RAYCAST_MAX_DISTANCE, layerMask);
      var ph = Physics.Raycast(highPos, port, out hitPort, RAYCAST_MAX_DISTANCE, layerMask);
      if (pl || ph)
      {
         raycastSleepTime = Time.time + RAYCAST_SLEEP_DELAY;
         if (enhancedLogging)
         {
            Debug.DrawRay(lowPos, port, Color.blue, RAYCAST_DRAWTIME);
            Debug.DrawRay(highPos, port, Color.blue, RAYCAST_DRAWTIME);
         }
      }

      // Look right.
      var sl = Physics.Raycast(lowPos, starbord, out hitStarbordLow, RAYCAST_MAX_DISTANCE, layerMask);
      var sh = Physics.Raycast(highPos, starbord, out hitStarbord, RAYCAST_MAX_DISTANCE, layerMask);
      if (sl || sh)
      {
         raycastSleepTime = Time.time + RAYCAST_SLEEP_DELAY;
         if (enhancedLogging)
         {
            Debug.DrawRay(lowPos, starbord, Color.red, RAYCAST_DRAWTIME);
            Debug.DrawRay(highPos, starbord, Color.red, RAYCAST_DRAWTIME);
         }
      }
   }

   private void DetermineTurn(RaycastHit hitPort, RaycastHit hitPortLow, RaycastHit hitStarbordLow, RaycastHit hitStarbord)
   {
      // Turn more sharply when a neighbour is detected.
      if (hitPort.distance > 0 || hitStarbord.distance > 0)
      {
         lastContact = Time.time;
         correctedTurnRate = turnRate * RAYCAST_CORRECTION_FACTOR;
         if (hitPort.distance > 0 && hitStarbord.distance > 0)
         {
            // When there are neighbors on both sides, if course is toward closer target then invert course (but not too often).
            if (((hitPort.distance > hitStarbord.distance && correctedTurnRate > 0)
               || (hitPort.distance < hitStarbord.distance && correctedTurnRate > 0))
               && revTime < Time.time)
            {
               correctedTurnRate = -correctedTurnRate;
               revTime = Time.time + REVERSE_DELAY;
            }
         }
      }
      else correctedTurnRate = turnRate;
   }

   private void DirectionChanger()
   {
      if (Time.time > lastContact + CHANGE_TURN_DELAY)
      {
         lastContact = Time.time;
         if (FiftyFifty) SetNewTurnRate();
      }
   }

   private bool FiftyFifty { get { return (Mathf.FloorToInt(Random.Range(0, 2)) == 1); } }

   private void FixedUpdate() { BeFishy(); }

   private Vector3 GetNewScale { get { return new Vector3(Scale, Scale, Scale); } }

   private void Init()
   {
      scale = GetNewScale;
      SetLocalScale(scale);
      TuneAnimationSpeed(scale);

      SetNewTurnRate();
      SetNewOrientation();
      SetNewRandomSpeed();
      speed = newSpeed;
      if (animator.isActiveAndEnabled) animator.SetFloat("stateSpeed", speed * scaleFactor * ANIMATION_SPEED_FACTOR);

      lastContact = 0;
      revTime = 0;
      caution = false;
   }

   private void LerpSpeed()
   {
      if (!(Mathf.Approximately(speed, newSpeed)))
      {
         speed = Mathf.Lerp(speed, newSpeed, LERP_FACTOR_FOR_SPEED);
         animator.SetFloat("stateSpeed", speed * scaleFactor * ANIMATION_SPEED_FACTOR);
         ///if (enhancedLogging) Debug.Log(speed + " :speed | newSpeed: " + newSpeed);
      }
   }

   private void LerpTurn()
   {
      if (!(Mathf.Approximately(correctedTurnRate, lerpTurnRate)))
      {
         lerpTurnRate = Mathf.Lerp(lerpTurnRate, correctedTurnRate, LERP_FACTOR_FOR_TURN);
         ///if (enhancedLogging) Debug.Log(lerpTurnRate + " :lerpTurnRate | correctedTurnRate: " + correctedTurnRate);
      }
   }

   private void Motivate()
   {
      LerpTurn();
      LerpSpeed();
      Vector3 dimensions = Vector3.zero;
      dimensions.y = Time.deltaTime * lerpTurnRate;
      transform.Rotate(dimensions, Space.Self); // Turn.
      transform.Translate(Vector3.forward * Time.fixedDeltaTime * speed * speedScaleFactor, Space.Self); // Propel.
      if (changeTime + changeDelay < Time.time) SetNewRandomSpeed();
   }

   private void OnCollisionEnter(Collision collision)
   {
      if (collision.gameObject.tag == "Player")
      {
         lastContact = Time.time;
         speed += SPEED_MAX;
         newSpeed = speed * 0.75f;
      }
   }

   private void OrientView()
   {
      fore = transform.forward;
      port = Quaternion.Euler(0, -RAYCAST_DETECTION_ANGLE, 0) * transform.forward;
      starbord = Quaternion.Euler(0, RAYCAST_DETECTION_ANGLE, 0) * transform.forward;

      Vector3 lowPos = transform.position;
      Vector3 highPos = transform.position;
      lowPos.y -= RAYCAST_VERTICAL_OFFSET * roughScale;
      highPos.y += RAYCAST_VERTICAL_OFFSET * roughScale;

      /// Enable these rays to visualize the wiskers in the scene view (or with gizmos enabled).
      //Debug.DrawRay(highPos, fore, Color.green, 0);
      //Debug.DrawRay(highPos, port, Color.cyan, 0);
      //Debug.DrawRay(highPos, starbord, Color.magenta, 0);
      //Debug.DrawRay(lowPos, fore, Color.green, 0);
      //Debug.DrawRay(lowPos, port, Color.cyan, 0);
      //Debug.DrawRay(lowPos, starbord, Color.magenta, 0);
   }

   private void PlanPath()
   {
      DirectionChanger();
      if (Time.time > raycastSleepTime)
      {
         raycastSleepTime = Time.time + RAYCAST_SLEEP_DELAY;
         RaycastHit hitPort, hitPortLow, hitStarbord, hitStarbordLow;
         DetectNeighbors(out hitPort, out hitPortLow, out hitStarbord, out hitStarbordLow);
         DetermineTurn(hitPort, hitPortLow, hitStarbord, hitStarbordLow);
      }
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

   private float Scale { get { return Random.Range(SCALE_MIN, SCALE_MAX); } }

   private void SetNewOrientation()
   {
      Vector3 thisRotation = Vector3.zero;
      thisRotation.y = Random.Range(0f, 360f);
      transform.Rotate(thisRotation, Space.Self);
   }

   private void SetLocalScale(Vector3 scale) { transform.localScale = scale; }

   private void SetNewRandomSpeed()
   {
      changeTime = Time.time;
      changeDelay = Random.Range(CHANGE_TIME_MIN, CHANGE_TIME_MAX);
      newSpeed = Random.Range(SPEED_MIN, SPEED_MAX);
   }

   private void SetNewTurnRate()
   {
      turnRate = Random.Range(TURNRATE_MIN, TURNRATE_MAX);
      if (FiftyFifty) turnRate = -turnRate;
      // TODO -- why force this? //correctedTurnRate = turnRate;
   }

   private void Start()
   {
      animator = GetComponent<Animator>();
      startPos = transform.position;
      startQuat = transform.rotation;    
      int defaultLayer = 0;
      layerMask = 1 << defaultLayer; // Apply a bitshift to create a mask.
      Init();
   }

   private void TuneAnimationSpeed(Vector3 scale) // Set dynamic animation speed (~slower for larger fish).
   {
      roughScale = scale.x + scale.y + scale.z / 3.0f; // Average the scales of the 3 planes.
      if (roughScale < SIZE_MID_BREAK)
      {
         scaleFactor = ANIMATION_SCALING_SMALL;
         speedScaleFactor = MOTIVATION_SCALING_SMALL;
      }
      else if (roughScale < SIZE_LARGE_BREAK)
      {
         scaleFactor = ANIMATION_SCALING_MED;
         speedScaleFactor = MOTIVATION_SCALING_MED;
      }
      else
      {
         scaleFactor = ANIMATION_SCALING_LARGE;
         speedScaleFactor = MOTIVATION_SCALING_LARGE;
      }
   }
}
