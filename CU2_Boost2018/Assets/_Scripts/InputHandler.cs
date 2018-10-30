using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class InputHandler : MonoBehaviour
{
   // 'Keys' In use: 
   // B C Ctrl-E F H I J K L M N Q R Z ESC [ ] 0...9 Vertical Horizontal Jump

   private FishPool        fishPool;
   private MusicPlayer     musicPlayer;
   private PickupTracker   pickupTracker;
   private Pilot           pilot;
   private Pilot_ID_Field  pilot_ID_Field;
   private Player          player;
   private Records         records;

   private const string AXIS_POWER     = "Vertical";
   private const string AXIS_ROTATION  = "Horizontal";
   private const string AXIS_THRUST    = "Jump";

   private void DebugControlPoll()
   {
      // Player: B, F, I.
      if (Input.GetKeyDown(KeyCode.B)) player.BoostMaxPower(0.025f); // 2.5% boost
      if (Input.GetKeyDown(KeyCode.F)) player.TopFuel();
      if (Input.GetKeyDown(KeyCode.I)) player.Invulnerable();

      // PickupTracker: M, N.
      if (Input.GetKeyDown(KeyCode.N)) pickupTracker.TriggerSpawn();
      if (Input.GetKeyDown(KeyCode.M)) pickupTracker.DespawnAll();

      // FishPool: J, K, L.
      if (Input.GetKeyDown(KeyCode.K)) fishPool.ReclaimAllFish();
      if (Input.GetKeyDown(KeyCode.L)) fishPool.Respawn();
      if (Input.GetKeyDown(KeyCode.J)) fishPool.PartialSpawn();

      // Records: Z.
      if (Input.GetKeyDown(KeyCode.Z)) records.AddRecord(Random.Range(0.9f, 11f));
   }

   private void FixedUpdate() { PlayerPhysicsPoll(); }

   private void PlayerPhysicsPoll()
   {
      PollRotation();
      PollThrust();
   }

   private void PollAutoPower()
   {
      // Player: 0, 1, 2... 9.
      if      (Input.GetKeyDown(KeyCode.Alpha0)) player.SetPower(1.0f);
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
      // Misc: C, H, [, ], Ctrl-E.
      if (Input.GetKeyDown(KeyCode.C)) player.CasualMode();
      if (Input.GetKeyDown(KeyCode.H)) ApplyColour.Toggle();
      if (Input.GetKey(KeyCode.LeftBracket)) musicPlayer.VolumeDown();
      else if (Input.GetKey(KeyCode.RightBracket)) musicPlayer.VolumeUp();
      if ((Input.GetKey(KeyCode.RightControl) 
         || Input.GetKey(KeyCode.LeftControl)) 
         && Input.GetKeyDown(KeyCode.E)) pilot_ID_Field.Toggle();

      // Player: R. 
      if (Input.GetKeyDown(KeyCode.R)) player.TriggerRestart();

      //SumPause is Polling: Q & ESC keys. 
      // TODO pull-in ESC & Q monitoring to this class [low priority].
   }

   private void PollPower()
   {
      // Player: Vertical.
      float power = CrossPlatformInputManager.GetAxis(AXIS_POWER);
      if (power != 0) player.AdjustThrusterPower(power);
   }

   private void PollRotation()
   {
      // Player: Horizontal.
      float rotation = CrossPlatformInputManager.GetAxis(AXIS_ROTATION);
      if (rotation != 0) player.ApplyRotation(rotation);
      else player.DeRotate();
   }

   private void PollThrust()
   {
      // Player: Jump.
      float thrust = CrossPlatformInputManager.GetAxis(AXIS_THRUST);
      player.ApplyThrust(thrust);
      if (thrust > 0) player.TriggerThrustAudio();
      else player.CancelThrustAudio();
   }

   private void Start()
   {
      fishPool = FindObjectOfType<FishPool>();
      musicPlayer = FindObjectOfType<MusicPlayer>();
      pickupTracker = FindObjectOfType<PickupTracker>();
      pilot = FindObjectOfType<Pilot>();
      pilot_ID_Field = FindObjectOfType<Pilot_ID_Field>();
      player = FindObjectOfType<Player>();
      records = FindObjectOfType<Records>();
   }

   private void Update()
   {
      if (Debug.isDebugBuild || pilot.MasterPilot) DebugControlPoll();
      PollAutoPower();
      PollPower();
      PollMisc();
   }
}

public static class ApplyColour // RTF helper.
{
   private static int hudIndex = 1;
   private static int nextIndex = -1;

   private static string[] colour = { "#FF7070", "#28B3D1", "#2DE9A8", };

   private static string NextColourString()
   {
      nextIndex++; if (nextIndex >= colour.Length) nextIndex = 0;
      return "<color=" + colour[nextIndex] + ">";
   }

   public static string Blue { get { return "<color=" + colour[1] + ">"; } }
   public static string Close { get { return "</color>"; } }
   public static string Colour { get { return "<color=" + colour[hudIndex] + ">"; } }
   public static string Coral { get { return "<color=" + colour[0] + ">"; } }
   public static string Green { get { return "<color=" + colour[2] + ">"; } }
   public static string NextColour {  get { return NextColourString(); } }
   public static string Open { get { return "<color=" + Colour + ">"; } }

   public static string RTFify(string text)
   {
      string header = "<color=" + Colour + ">";
      string footer = "</color>";
      return header + text + footer;
   }

   public static void Toggle() { hudIndex++; if (hudIndex >= colour.Length) hudIndex = 0; }
}
