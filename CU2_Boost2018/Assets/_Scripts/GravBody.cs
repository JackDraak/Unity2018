using UnityEngine;

public class GravBody : MonoBehaviour {

   Gravity planet;

	// Use this for initialization
	void Start () {
      planet = GameObject.FindGameObjectWithTag("Planet").GetComponent<Gravity>();

      var myBody = GetComponent<Rigidbody>();
      myBody.useGravity = false;
      myBody.constraints = RigidbodyConstraints.FreezeRotation;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

   private void FixedUpdate()
   {
      planet.Attract(transform);
   }
}
