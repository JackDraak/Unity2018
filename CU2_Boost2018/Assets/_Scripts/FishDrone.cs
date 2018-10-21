using UnityEngine;

public class FishDrone : MonoBehaviour
{
   private Animator animator;

   private bool caution;
   private bool enhancedLogging = true;

   private class WhiskerSet
   {
      public Vector3 fore;
      public Vector3 port;
      public Vector3 starboard;
      public Vector3 lower;
      public Vector3 upper;
   }

   private float speedChageDelay, speedChangeTime;
   private float newSpeed, speed; 
   private float correctedTurnRate, lerpTurnRate, turnRate;
   private float lastContact;
   private float raycastSleepTime;
   private float revTime;
   private float roughScale, scaleFactor;
   private float speedScaleFactor;

   private int group;
   private int layerMask;

   private Quaternion startQuat;

   private Vector3 startPos;

   private WhiskerSet whiskerSet;

   private const float ANIMATION_SPEED_FACTOR = 1.8f;
   private const float RAYCAST_SLEEP_DELAY = 0.333f / GROUP_MAX;
   private const float SPEED_MAX = 1.2f;
   private const float SPEED_MIN = 0.2f;
   private const int GROUP_MAX = 7;

   private static int rayGroup = 0;

   private void BeFishy()
   {
      if (Time.time > raycastSleepTime) CycleRaycastGroups();
      Motivate();
   }

   private bool CheckFlanks(RaycastHit hitPort, RaycastHit hitStarboard)
   {
      const float RAYCAST_CORRECTION_FACTOR = 9.0f;
      const float REVERSE_DELAY = 6f;

      // Turn more sharply when a neighbour is detected.
      if (hitPort.distance > 0 || hitStarboard.distance > 0)
      {
         lastContact = Time.time;
         correctedTurnRate = turnRate * RAYCAST_CORRECTION_FACTOR;
         if (hitPort.distance > 0 && hitStarboard.distance > 0)
         {
            // When there are neighbors on both sides, if course is toward closer target then invert course (but not too often).
            if (((hitPort.distance > hitStarboard.distance && correctedTurnRate > 0)
               || (hitPort.distance < hitStarboard.distance && correctedTurnRate > 0))
               && revTime < Time.time)
            {
               correctedTurnRate = -correctedTurnRate;
               revTime = Time.time + REVERSE_DELAY;
            }
         }
         return true;
      }
      return false;
   }

   private void CycleRaycastGroups()
   {
      group++;
      if (group > GROUP_MAX)
      {
         group = 0;
         //Vector3 fore, port, starbord, lower, upper;
         SetWhiskerVectors(); // (out fore, out port, out starbord, out lower, out upper);
         PlanPath(); //  (fore, port, starbord, lower, upper);
      }
   }

   private void DetectNeighbors(out RaycastHit hitPort, out RaycastHit hitPortLow, out RaycastHit hitStarbord, out RaycastHit hitStarbordLow)
   {
      const float RAYCAST_DRAWTIME = 3.0f;
      const float RAYCAST_MAX_DISTANCE = 1.33f;

      //Vector3 fore = whiskerVectorSet[0];
      //Vector3 port = whiskerVectorSet[1];
      //Vector3 starboard = whiskerVectorSet[2];
      //Vector3 lower = whiskerVectorSet[3];
      //Vector3 upper = whiskerVectorSet[4];

      // Look ahead.
      bool fl = Physics.Raycast(whiskerSet.lower, whiskerSet.fore, RAYCAST_MAX_DISTANCE, layerMask);
      bool fh = Physics.Raycast(whiskerSet.upper, whiskerSet.fore, RAYCAST_MAX_DISTANCE, layerMask);
      if (fl || fh)
      {
         caution = true;
         newSpeed = Mathf.Lerp(speed, speed * 0.2f, 1 / newSpeed);
         raycastSleepTime = Time.time + RAYCAST_SLEEP_DELAY;
         lastContact = Time.time;
         if (enhancedLogging)
         {
            Debug.DrawRay(whiskerSet.lower, whiskerSet.fore, Color.yellow, RAYCAST_DRAWTIME);
            Debug.DrawRay(whiskerSet.upper, whiskerSet.fore, Color.yellow, RAYCAST_DRAWTIME);
         }
      }
      else if (caution)
      {
         caution = false;
         SetNewRandomSpeed();
      }

      // Look to port.
      bool pl = Physics.Raycast(whiskerSet.lower, whiskerSet.port, out hitPortLow, RAYCAST_MAX_DISTANCE, layerMask);
      bool ph = Physics.Raycast(whiskerSet.upper, whiskerSet.port, out hitPort, RAYCAST_MAX_DISTANCE, layerMask);
      if (pl || ph)
      {
         raycastSleepTime = Time.time + RAYCAST_SLEEP_DELAY;
         if (enhancedLogging)
         {
            Debug.DrawRay(whiskerSet.lower, whiskerSet.port, Color.blue, RAYCAST_DRAWTIME);
            Debug.DrawRay(whiskerSet.upper, whiskerSet.port, Color.blue, RAYCAST_DRAWTIME);
         }
      }

      // Look to starboard.
      bool sl = Physics.Raycast(whiskerSet.lower, whiskerSet.starboard, out hitStarbordLow, RAYCAST_MAX_DISTANCE, layerMask);
      bool sh = Physics.Raycast(whiskerSet.upper, whiskerSet.starboard, out hitStarbord, RAYCAST_MAX_DISTANCE, layerMask);
      if (sl || sh)
      {
         raycastSleepTime = Time.time + RAYCAST_SLEEP_DELAY;
         if (enhancedLogging)
         {
            Debug.DrawRay(whiskerSet.lower, whiskerSet.starboard, Color.red, RAYCAST_DRAWTIME);
            Debug.DrawRay(whiskerSet.upper, whiskerSet.starboard, Color.red, RAYCAST_DRAWTIME);
         }
      }
   }

   private void DetermineTurnRate(RaycastHit hitPortHigh, RaycastHit hitPortLow, RaycastHit hitStarboardHigh, RaycastHit hitStarboardLow)
   {
      if (CheckFlanks(hitPortHigh, hitStarboardHigh)) { } // Respond to upper raycasts,
      else if (CheckFlanks(hitPortLow, hitStarboardLow)) { } // or check lower raycasts.
      // If there are no hits, revert to normal turnRate.
      else correctedTurnRate = turnRate;

   }

   private void DirectionChanger()
   {
      const float CHANGE_TURN_DELAY = 5.0f;

      if (Time.time > lastContact + CHANGE_TURN_DELAY)
      {
         lastContact = Time.time;
         if (FiftyFifty) SetNewTurnRate();
      }
   }

   private bool FiftyFifty { get { return (Mathf.FloorToInt(Random.Range(0, 2)) == 1); } }

   private void FixedUpdate() { BeFishy(); }

   private static int GetGroupID()
   {
      rayGroup++;
      if (rayGroup > GROUP_MAX) rayGroup = 0;
      return rayGroup;
   }

   private Vector3 GetNewScale { get { return new Vector3(Scale, Scale, Scale); } }

   private void Init()
   {
      Vector3 scale = GetNewScale;
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

      group = GetGroupID();
      whiskerSet = new WhiskerSet();
   }

   private void LerpSpeed()
   {
      const float LERP_FACTOR_FOR_SPEED = 0.09f;

      if (!(Mathf.Approximately(speed, newSpeed)))
      {
         speed = Mathf.Lerp(speed, newSpeed, LERP_FACTOR_FOR_SPEED);
         animator.SetFloat("stateSpeed", speed * scaleFactor * ANIMATION_SPEED_FACTOR);
         ///if (enhancedLogging) Debug.Log(speed + " :speed | newSpeed: " + newSpeed);
      }
   }

   private void LerpTurn()
   {
      const float LERP_FACTOR_FOR_TURN = 0.18f;

      if (!(Mathf.Approximately(correctedTurnRate, lerpTurnRate)))
      {
         lerpTurnRate = Mathf.Lerp(lerpTurnRate, correctedTurnRate, LERP_FACTOR_FOR_TURN);
         ///if (enhancedLogging) Debug.Log(lerpTurnRate + " :lerpTurnRate | correctedTurnRate: " + correctedTurnRate);
      }
   }

   private void Motivate()
   {
      Vector3 vector3 = Vector3.zero;
      LerpTurn();
      LerpSpeed();
      vector3.y = Time.deltaTime * lerpTurnRate;
      transform.Rotate(vector3, Space.Self); // Turn.
      transform.Translate(Vector3.forward * Time.fixedDeltaTime * speed * speedScaleFactor, Space.Self); // Propel.
      if (speedChangeTime + speedChageDelay < Time.time) SetNewRandomSpeed();
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

   private void PlanPath() // (Vector3 fore, Vector3 port, Vector3 starbord, Vector3 lower, Vector3 upper)
   {
      //Vector3[] whiskerVectorSet = new Vector3[5];
      //whiskerVectorSet[0] = fore;
      //whiskerVectorSet[1] = port;
      //whiskerVectorSet[2] = starbord;
      //whiskerVectorSet[3] = lower;
      //whiskerVectorSet[4] = upper;

      DirectionChanger();
      if (Time.time > raycastSleepTime)
      {
         raycastSleepTime = Time.time + RAYCAST_SLEEP_DELAY;
         RaycastHit hitPortHigh, hitPortLow, hitStarbordHigh, hitStarbordLow;
         DetectNeighbors(out hitPortHigh, out hitPortLow, out hitStarbordHigh, out hitStarbordLow);
         DetermineTurnRate(hitPortHigh, hitPortLow, hitStarbordHigh, hitStarbordLow);
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

   private float Scale
   { get
      { 
      const float SCALE_MAX = 1.8f;
      const float SCALE_MIN = 0.3f;
      return Random.Range(SCALE_MIN, SCALE_MAX);
      }
   }

   private void SetLocalScale(Vector3 scale) { transform.localScale = scale; }

   private void SetNewOrientation()
   {
      Vector3 thisRotation = Vector3.zero;
      thisRotation.y = Random.Range(0f, 360f);
      transform.Rotate(thisRotation, Space.Self);
   }

   private void SetNewRandomSpeed()
   {
      const float CHANGE_TIME_MAX = 16.0f;
      const float CHANGE_TIME_MIN = 4.0f;

      speedChangeTime = Time.time;
      speedChageDelay = Random.Range(CHANGE_TIME_MIN, CHANGE_TIME_MAX);
      newSpeed = Random.Range(SPEED_MIN, SPEED_MAX);
   }

   private void SetNewTurnRate()
   {
      const float TURNRATE_MAX = 10.0f;
      const float TURNRATE_MIN = 5.0f;

      turnRate = Random.Range(TURNRATE_MIN, TURNRATE_MAX);
      if (FiftyFifty) turnRate = -turnRate;
   }

   private void SetWhiskerVectors() // (out Vector3 fore, out Vector3 port, out Vector3 starbord, out Vector3 lower, out Vector3 upper)
   {
      const float RAYCAST_DETECTION_ANGLE = 28.0f;
      const float RAYCAST_VERTICAL_OFFSET = 0.07f;

      whiskerSet.fore = transform.forward;
      whiskerSet.port = Quaternion.Euler(0, -RAYCAST_DETECTION_ANGLE, 0) * transform.forward;
      whiskerSet.starboard = Quaternion.Euler(0, RAYCAST_DETECTION_ANGLE, 0) * transform.forward;

      whiskerSet.lower = transform.position;
      whiskerSet.upper = transform.position;
      whiskerSet.lower.y -= RAYCAST_VERTICAL_OFFSET * roughScale;
      whiskerSet.upper.y += RAYCAST_VERTICAL_OFFSET * roughScale;

      // Enable these rays to visualize the wiskers in the scene view(or with gizmos enabled).
      if (group == 0)
      {
         float lineLife = 0.2f;
         Debug.DrawRay(whiskerSet.upper, whiskerSet.fore, Color.green, lineLife);
         Debug.DrawRay(whiskerSet.upper, whiskerSet.port, Color.cyan, lineLife);
         Debug.DrawRay(whiskerSet.upper, whiskerSet.starboard, Color.magenta, lineLife);
         Debug.DrawRay(whiskerSet.lower, whiskerSet.fore, Color.green, lineLife);
         Debug.DrawRay(whiskerSet.lower, whiskerSet.port, Color.cyan, lineLife);
         Debug.DrawRay(whiskerSet.lower, whiskerSet.starboard, Color.magenta, lineLife);
      }
   }

   private void Start()
   {
      animator = GetComponent<Animator>();
      startPos = transform.position;
      startQuat = transform.rotation;    
      int defaultLayer = 0;
      layerMask = 1 << defaultLayer; // Applying a bitshift to create a 'mask'.
      Init();
   }

   // Set dynamic animation speed (~slower for larger fish, ~faster for smaller fish).
   private void TuneAnimationSpeed(Vector3 scale) 
   {
      const float ANIMATION_SCALING_LARGE = 0.4f;
      const float ANIMATION_SCALING_MED = 0.7f;
      const float ANIMATION_SCALING_SMALL = 1.7f;
      const float MOTIVATION_SCALING_LARGE = 1.0f;
      const float MOTIVATION_SCALING_MED = 0.9f;
      const float MOTIVATION_SCALING_SMALL = 0.8f;
      const float SIZE_LARGE_BREAK = 3.0f;
      const float SIZE_MID_BREAK = 2.0f;

      // Average the scales of the 3 planes; called 'rough' for a reason.
      roughScale = scale.x + scale.y + scale.z / 3.0f; 
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
