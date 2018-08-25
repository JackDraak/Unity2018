using UnityEngine;

public class UIcontrol : MonoBehaviour {

   [SerializeField] GameObject TPC_slider;
   [SerializeField] GameObject TPL_slider;
   [SerializeField] GameObject GL_slider;
   [SerializeField] GameObject MGP_slider;
   [SerializeField] GameObject tasklist;

   private bool status, deltaStatus;
   private GameObject[] UIobjects = new GameObject[5];

   public bool HUD_vis
   {
      get { return status; }
      set { deltaStatus = value; }
   }

   private void SetState()
   {
      foreach (GameObject go in UIobjects)
      {
         go.SetActive(status);
      }
   }

   private void Start()
   {
      UIobjects[0] = TPC_slider;
      UIobjects[1] = TPL_slider;
      UIobjects[2] = GL_slider;
      UIobjects[3] = MGP_slider;
      UIobjects[4] = tasklist;

      // pre-emtively deactivate the controlled HUD elements...
      deltaStatus = false;
      status = false;
      SetState();
   }

   private void Update()
   {
      if (status != deltaStatus)
      {
         status = deltaStatus;
         SetState();
      }
   }
}
