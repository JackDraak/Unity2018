using UnityEngine;

public class FishDrone : MonoBehaviour {


   private float speed;
   private float turnRate;
   private float changeDelay, changeTime;
   private Rigidbody thisRigidbody;
   private Vector3 planes = Vector3.zero;
   //private UnityEditor.Animations.AnimatorState animatorState;
   private Animator animator;

   private const float ANIMATION_SPEED_FACTOR = 1.8f;
   private const float CHANGE_MAX = 5f;
   private const float CHANGE_MIN = 1f;
   private const float SCALE_MAX = 1.6f;
   private const float SCALE_MIN = 0.4f;
   private const float SPEED_MAX = 1.1f;
   private const float SPEED_MIN = 0.3f;
   private const float TURN_MAX = 10f;
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
      if (changeTime + changeDelay > Time.time) SetSpeed();
   }

   private void SetSpeed()
   {
      changeTime = Time.time;
      speed = Random.Range(SPEED_MIN, SPEED_MAX);
      changeDelay = Random.Range(CHANGE_MIN, CHANGE_MAX);
      animator.SetFloat("stateSpeed", speed * ANIMATION_SPEED_FACTOR);
   }

   private void Start()
   {
      animator = GetComponent<Animator>();
      //animatorState = GetComponent<UnityEditor.Animations.AnimatorState>();

      turnRate = Random.Range(TURN_MIN, TURN_MAX);
      if (FiftyFifty()) turnRate = -turnRate;

      Vector3 scale = Vector3.zero;
      scale.x = Random.Range(SCALE_MIN, SCALE_MAX);
      scale.y = Random.Range(SCALE_MIN, SCALE_MAX);
      scale.z = Random.Range(SCALE_MIN, SCALE_MAX);
      transform.localScale = scale;

      SetSpeed();
   }
}
