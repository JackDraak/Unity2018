using UnityEngine;

public class GlueCam : MonoBehaviour
{
   [SerializeField] GameObject player;

   private bool paused = false;
   private const float ELASTICITY_FACTOR = 0.06f;
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
