using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIcontrol : MonoBehaviour {

   [SerializeField] GameObject TPC_slider;
   [SerializeField] GameObject TPL_slider;
   [SerializeField] GameObject GL_slider;
   [SerializeField] GameObject MGP_slider;
   [SerializeField] GameObject tasklist;

   private bool status, deltaStatus;
   private GameObject[] UIobjects = new GameObject[5];

   private void Start()
   {
      UIobjects[0] = TPC_slider;
      UIobjects[1] = TPL_slider;
      UIobjects[2] = GL_slider;
      UIobjects[3] = MGP_slider;
      UIobjects[4] = tasklist;
      deltaStatus = true;
      status = true;
   }

   private void Update()
   {
      if (status != deltaStatus)
      {
         status = deltaStatus;
         foreach (GameObject go in UIobjects)
         {
            go.SetActive(status);
         }
      }
   }

   public void HUD_view()
   {
      deltaStatus = true;
   }

   public void HUD_hide()
   {
      deltaStatus = false;
   }
   public bool HUD_vis
   {
      get { return status; }
      set { deltaStatus = value; }
   }
}
