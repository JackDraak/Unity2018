using UnityEngine;

public class TiltTable : MonoBehaviour
{
   [SerializeField] float rotationSpeed = 10.0f;
   [SerializeField] float minRotation = -10.0f;
   [SerializeField] float maxRotation = 10.0f;

   void Update ()
   {
      tiltTable();
   }

   private void tiltTable()
   {
      SimpleRotation(GetMouseInput()); // this is the 'pre-clamp' rotation (rotation-Z and rotation-X, respectively from X & Y of mouse)
      ClampRotation(transform.rotation.eulerAngles); // half-broken attempt to clamp rotation to min/maxRotation degrees.
      /// moving left and/or up on mouse results in smooth roll into 10 degreee limits...
      /// moving right and/or down on mouse eventually results in a sudden 'reset' to a nominal position... 
      /// but why?
   }

   private void ClampRotation(Vector3 tempEulers)
   {
      tempEulers.x = Mathf.Clamp(tempEulers.x, minRotation, maxRotation);
      tempEulers.y = Mathf.Clamp(tempEulers.y, minRotation, maxRotation);
      tempEulers.z = Mathf.Clamp(tempEulers.z, minRotation, maxRotation);
      transform.rotation = Quaternion.Euler(tempEulers);
   }

   Vector2 GetMouseInput()
   {
      Vector2 mouseXY;
      mouseXY.x = -Input.GetAxis("Mouse X"); // rotZ
      mouseXY.y = Input.GetAxis("Mouse Y"); // rotX
      return mouseXY;
   }

   void SimpleRotation(Vector2 mouseXY)
   {
      Vector3 rotation = Vector3.zero;
      rotation.x = mouseXY.y * Time.deltaTime * rotationSpeed;
      rotation.z = mouseXY.x * Time.deltaTime * rotationSpeed;
      transform.Rotate(rotation, Space.Self); 
   }
}
