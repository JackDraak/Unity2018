using UnityEngine;

public class GlueCam : MonoBehaviour
{
   [SerializeField] GameObject player;

   private bool paused = false;
   // 1 = infinite glue, 0 = infinite elasticity... 
   // therefore: 0.07f --> correct position by 7%, each frame.
   private float elasticity = 0.07f; 
   private Vector3 initialPosition;
   private Vector3 offset;

   private void LateUpdate()
   {
      if (!paused) transform.position = 
            Vector3.Lerp(transform.position, player.transform.position + offset, elasticity);
   }

   public void Pause() { paused = !paused; }

   public void Restart() { transform.position = initialPosition; }

   private void Start()
   {
      initialPosition = transform.position;
      offset = initialPosition - player.transform.position;
   }
}
