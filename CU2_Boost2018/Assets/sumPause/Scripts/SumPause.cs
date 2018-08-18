/*
Code and project: SumPause
The MIT License (MIT)

Copyright (c) 2016 Jerry Denton

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

######################################################

Icons and Audio assets are from the awesome CCO asset creator Kenney - https://kenney.itch.io/
License (Creative Commons Zero, CC0) - http://creativecommons.org/publicdomain/zero/1.0/
*/

/// devenote: I have taken many liberties with this sourcefile... go find the original, is my advice.

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class SumPause : MonoBehaviour {

   [SerializeField] bool useEvent = false, detectEscapeKey = true;
   [SerializeField] Sprite pausedSprite, playingSprite;

   public delegate void PauseAction(bool paused);
   public static event PauseAction pauseEvent;

   static bool status = false;

   Image image;
   private Player player;
   private GlueCam glueCam;

   /// Sets/Returns current pause state (true for paused, false for normal)
   public static bool Status
   {
      get { return status; }
      set {
         status = value;
         //Debug.Log("Pause status set to " + status.ToString());
         OnChange();

         // Change image to the proper sprite if everything is set
         if (CheckLinks()) instance.image.sprite = status ? instance.pausedSprite : instance.playingSprite;
         else Debug.LogError("Links missing on SumPause component. Please check the sumPauseButton object for missing references.");

         // Notify other objects of change
         if (instance.useEvent && pauseEvent != null) pauseEvent(status);
      }
   }

   public static SumPause instance;

   void Awake ()
   {
      image = GetComponent<Image>();
   }

   void Start ()
   {
      if (SumPause.instance == null) SumPause.instance = this;
      else Destroy(this);

      player = FindObjectOfType<Player>();
      glueCam = FindObjectOfType<GlueCam>();
   }

   void Update() 
   {
      if (detectEscapeKey && Input.GetKeyDown(KeyCode.Escape)) TogglePause();
   }

   public void TogglePause ()
   {
      Status = !Status; 
      player.Pause();
      glueCam.Pause();
   }

   static bool CheckLinks ()
   {
      return (instance.image != null && instance.playingSprite != null && instance.pausedSprite != null);
   }

   static void OnChange()
   {
      if(status) Time.timeScale = 0;
      else Time.timeScale = 1;
   }
}
