using UnityEngine;

public class TiltTable : MonoBehaviour
{
   [SerializeField] float rotationSpeed = 10.0f;
   [SerializeField] float minRotation = -10.0f;
   [SerializeField] float maxRotation = 10.0f;

   Vector3 priorRotation = Vector3.zero;

	void Update ()
   {
      var mouseX = -Input.GetAxis("Mouse X"); // rotZ
      var mouseY = Input.GetAxis("Mouse Y"); // rotX

      Vector3 rotation = new Vector3(mouseY, 0.0f, mouseX);
      rotation.x = mouseY * Time.deltaTime * rotationSpeed;
      rotation.z = mouseX * Time.deltaTime * rotationSpeed;

      rotation.x += priorRotation.x;
      rotation.z += priorRotation.z;

      rotation.x = Mathf.Clamp(rotation.x, minRotation, maxRotation);
      rotation.z = Mathf.Clamp(rotation.z, minRotation, maxRotation);

      transform.Rotate(rotation, Space.Self);
   }
}
