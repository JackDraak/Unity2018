using UnityEngine;

public class UIcontrol : MonoBehaviour {

   // UI elements to hide when the game is: launched | paused | reset 
   [SerializeField] GameObject[] UIobjects;

   private bool deltaStatus, status;

   public bool visible
   {
      get { return status; }
      set { deltaStatus = value; }
   }

   private void SetState()
   {
      status = deltaStatus;
      foreach (GameObject go in UIobjects) go.SetActive(status);
   }

   private void Start()
   {
      // pre-emtively deactivate the controlled HUD elements...
      deltaStatus = false;
      status = false;
      SetState();
   }

   private void Update()
   {
      if (status != deltaStatus) SetState();
   }
}
