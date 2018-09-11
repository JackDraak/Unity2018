using System.Collections;
using UnityEngine;

public class GlueCam : MonoBehaviour
{
   [SerializeField] GameObject player;

   // Determine how "glued" the cam is to the player. 
   // 1 = infinite glue, 0 = infinite elasticity...
   // 0.01f == correct position by 1%, each frame.
   private const float ELASTICITY_FACTOR = 0.07f;

   private bool wobbling = false;
   private bool dampening = true;
   private bool paused = false;
   private Vector3 offset;
   private Vector3 initialPosition;

   private void LateUpdate()
   {
      if (!paused && !wobbling) transform.position = Vector3.Lerp(transform.position, player.transform.position + offset, ELASTICITY_FACTOR);
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

   public IEnumerator Shake(float duration, float magnitude)
   {
      if (!paused && !wobbling)
      {
         wobbling = true;
         Vector3 startPosition = transform.position;
         float elapsed = 0.0f;
         while (elapsed < duration)
         {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            x = Mathf.Lerp(x, 0.0f, 0.1f);
            y = Mathf.Lerp(y, 0.0f, 0.1f);
            transform.position = new Vector3(transform.position.x + x, transform.position.y + y, transform.position.z);
            if (dampening) magnitude = Mathf.Lerp(magnitude, 0.0f, 0.001f);
            elapsed += Time.deltaTime;
            yield return null;
         }
         wobbling = false;
         transform.position = startPosition;
      }
   }
}
