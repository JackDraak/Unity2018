using UnityEngine;

public class Gravity : MonoBehaviour {

   public float G = -9.81f; // m/s/s

   public void Attract(Transform body)
   {
      Vector3 targetDirection = (body.position - transform.position).normalized;
      Vector3 bodyUp = body.up;

      body.rotation = Quaternion.FromToRotation(bodyUp, targetDirection) * body.rotation;
      body.GetComponent<Rigidbody>().AddForce(targetDirection * G);
   }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
