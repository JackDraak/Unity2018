using UnityEngine;

public class GlueCam : MonoBehaviour
{
   [SerializeField] GameObject player;

   // Determine how "glued" the cam is to the player. 
   // 1 = infinite glue, 0 = infinite elasticity...
   // 0.01f == correct position by 1%, each frame.
   private const float ELASTICITY_FACTOR = 0.07f; // 06

   private bool paused = false;
   private Vector3 offset;
   private Vector3 startPos;

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
      transform.position = startPos;
   }

   private void Start()
   {
      startPos = transform.position;
      offset = startPos - player.transform.position;
   }
}
