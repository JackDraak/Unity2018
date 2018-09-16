using UnityEngine;

public class GlueCam : MonoBehaviour
{
   [SerializeField] GameObject player;

   // Determine how "glued" the cam is to the player. 
   private const float ELASTICITY_FACTOR = 0.07f;
   // 1 = infinite glue, 0 = infinite elasticity...
   // therefore: 0.015f --> correct position by 1.5%, each frame.

   private bool paused = false;

   private Vector3 initialPosition;
   private Vector3 offset;

   private void LateUpdate()
   {
      if (!paused) transform.position = Vector3.Lerp(transform.position, player.transform.position + offset, ELASTICITY_FACTOR);
   }

   public void Pause()
   {
      paused = !paused;
   }

   public void Restart()
   {
      transform.position = initialPosition;
   }

   private void Start()
   {
      initialPosition = transform.position;
      offset = initialPosition - player.transform.position;
   }
}
