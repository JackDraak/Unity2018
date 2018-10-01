using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class InputHandler : MonoBehaviour
{
   private FishPool fishPool;
   private MusicPlayer musicPlayer;
   private PickupTracker pickupTracker;
   private Player player;
   private Records records;

   private const string AXIS_POWER = "Vertical";
   private const string AXIS_ROTATION = "Horizontal";
   private const string AXIS_THRUST = "Jump";

   private void Start()
   {
      fishPool = FindObjectOfType<FishPool>();
      musicPlayer = FindObjectOfType<MusicPlayer>();
      pickupTracker = FindObjectOfType<PickupTracker>();
      player = FindObjectOfType<Player>();
      records = FindObjectOfType<Records>();
   }
   private void FixedUpdate()
   {
      PlayerControlPoll();
      PollMisc();
      if (Debug.isDebugBuild) DebugControlPoll();
   }

   private void DebugControlPoll()
   {
      // Player: B, I, F
      if (Input.GetKeyDown(KeyCode.B)) player.BoostMaxPower(0.025f); // 2.5% boost
      if (Input.GetKeyDown(KeyCode.F)) player.TopFuel(); 
      if (Input.GetKeyDown(KeyCode.I)) player.Invulnerable();

      // PickupTracker: M, N
      if (Input.GetKeyDown(KeyCode.N)) pickupTracker.TriggerSpawn();
      if (Input.GetKeyDown(KeyCode.M)) pickupTracker.DespawnAll();

      // FishPool: J, K, L
      if (Input.GetKeyDown(KeyCode.K)) fishPool.ReclaimAllFish();
      if (Input.GetKeyDown(KeyCode.L)) fishPool.Respawn();
      if (Input.GetKeyDown(KeyCode.J)) fishPool.PartialSpawn();

      // Records: Z
      if (Input.GetKeyDown(KeyCode.Z)) records.AddRecord(Random.Range(0.9f, 11f));
   }

   private void PlayerControlPoll()
   {
      PollAutoPower();
      PollPower();
      PollRotation();
      PollThrust();

      // SumPause is Polling: Q, R & ESC keys. // ESC, Q Only // TODO pull-in ESC & Q monitoring tho this class [low priority].
      if (Input.GetKeyDown(KeyCode.R)) player.ImmediateRestart();
   }

   private void PollAutoPower()
   {
      if (Input.GetKeyDown(KeyCode.Alpha0)) player.SetPower(1.0f);
      else if (Input.GetKeyDown(KeyCode.Alpha9)) player.SetPower(0.9f);
      else if (Input.GetKeyDown(KeyCode.Alpha8)) player.SetPower(0.8f);
      else if (Input.GetKeyDown(KeyCode.Alpha7)) player.SetPower(0.7f);
      else if (Input.GetKeyDown(KeyCode.Alpha6)) player.SetPower(0.6f);
      else if (Input.GetKeyDown(KeyCode.Alpha5)) player.SetPower(0.5f);
      else if (Input.GetKeyDown(KeyCode.Alpha4)) player.SetPower(0.4f);
      else if (Input.GetKeyDown(KeyCode.Alpha3)) player.SetPower(0.3f);
      else if (Input.GetKeyDown(KeyCode.Alpha2)) player.SetPower(0.2f);
      else if (Input.GetKeyDown(KeyCode.Alpha1)) player.SetPower(0.1f);
   }

   private void PollMisc()
   {
      if (Input.GetKeyDown(KeyCode.C)) player.CasualMode();
      if (Input.GetKey(KeyCode.LeftBracket)) musicPlayer.VolumeDown();
      else if (Input.GetKey(KeyCode.RightBracket)) musicPlayer.VolumeUp();
   }

   private void PollPower()
   {
      float power = CrossPlatformInputManager.GetAxis(AXIS_POWER);
      if (power != 0) player.AdjustThrusterPower(power);
   }

   private void PollRotation()
   {
      float rotation = CrossPlatformInputManager.GetAxis(AXIS_ROTATION);
      if (rotation != 0) player.ApplyRotation(rotation);
      else player.DeRotate();
   }

   private void PollThrust()
   {
      float thrust = CrossPlatformInputManager.GetAxis(AXIS_THRUST);
      player.ApplyThrust(thrust);
      if (thrust > 0) player.TriggerThrustAudio();
      else player.CancelThrustAudio();
   }
}
