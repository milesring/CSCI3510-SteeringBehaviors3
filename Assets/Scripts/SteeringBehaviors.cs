using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringBehaviors : MonoBehaviour {
	

	void Start(){
		
	}

	// Update is called once per frame
	void Update () {
		SteeringOutput steering = getSteering();
		this.transform.Translate(steering.linear*Time.deltaTime);
		this.transform.Rotate (new Vector3 (0, steering.angular * Time.deltaTime, 0));
	}

	SteeringOutput getSteering(){
		SteeringOutput steering = new SteeringOutput ();

		/*/
		 * Seek 
		/*/

		//steering.linear = target - transform.position;

		/*/
		 * Face
		/*/
		if (steering.linear.magnitude == 0) {
			//not moving
		}

		steering.linear = steering.linear.normalized;
		//steering.linear *= maxAcceleration;
		steering.angular = 0.0f;
		return steering;
	}

	struct SteeringOutput{
		public Vector3 linear;
		public float angular;
	}

}
