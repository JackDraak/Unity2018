using UnityEngine;

public class UIcontrol : MonoBehaviour
{
   // UI elements to hide when the game is: launched | paused | reset 
   [SerializeField] GameObject[] UIobjects;

   bool deltaStatus, status;

   void SetState()
   {
      status = deltaStatus;
      foreach (GameObject go in UIobjects) go.SetActive(status);
   }

   void Start()
   {
      // pre-emtively deactivate the controlled HUD elements...
      deltaStatus = false;
      status = false;
      SetState();
   }

   void Update() { if (status != deltaStatus) SetState(); }

   public bool Visible
   {
      get { return status; }
      set { deltaStatus = value; }
   }
}
