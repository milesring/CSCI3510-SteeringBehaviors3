using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPath : SteeringBehaviors{
	public Material normal;
	public Material actual;
	public Material selected;
	public Material predicted;

	[Header("Seek")]
	public float maxAcceleration;
	public float maxSpeed;
	public Vector3 target;
	public float timeToTarget = 0.1f;
	private Vector3 velocity;
	private Vector3 position;
	[Header("Face")]
	//Holds the max angular acceleration and rotation
	public float maxAngularAcceleration;
	public float maxRotation;
	private float orientation;


	private Path path;	
	// Holds distance along path to generate target
	// Can be negative if the character is to move
	// along the reverse direction
	[Header("Follow Path Vanilla")]
	public int pathOffset;
	public bool useParameterTracking;
	public int buffer;


	[Header("Follow Path Predictive")]
	public bool predictivePathing;
	public float predictTime = 0.1f;

	//holds target position on path
	private int targetParam = 0;

	//holds current position on path
	private int currentParam;

	//tracks actual position on path for predictive
	private int actualParam;

	// Use this for initialization
	void Start () {
		path.setPath ();
		path.selected = selected;
		path.normal = normal;
		path.predicted = predicted;
		path.actual = actual;
		path.buffer = buffer;
		position = this.transform.position;
	}

	// Update is called once per frame
	void Update () {
		if (predictivePathing) {
			followPathPredict ();
		} else {
			followPath ();
		}
		SteeringOutput steering = getSteering();

		//update pos and orientation
		position += velocity * Time.deltaTime;

		//update veloc and rotation
		velocity += steering.linear * Time.deltaTime;

		if (velocity.sqrMagnitude > maxSpeed * maxSpeed) {
			velocity.Normalize ();
			velocity *= maxSpeed;
		}

		//this.transform.Translate(steering.linear*Time.deltaTime);
		this.transform.Translate(velocity*Time.deltaTime);


		//Debug.Log (steering.angular);
		//this.transform.Rotate (new Vector3 (0, steering.angular * Time.deltaTime, 0));
	}

	void followPath(){


		//Find current position on path
		path.setNormal(currentParam);

		if (useParameterTracking) {
			currentParam = path.getParamTracking (transform.position, currentParam);
		} else {
			currentParam = path.getParam (transform.position, currentParam);
		}
		path.setActual (currentParam);
		Debug.Log ("Current node: " + currentParam);

		//Offset target
		path.setNormal(targetParam);
		targetParam = currentParam + pathOffset;
		if (targetParam > path.size () - 1) {
			targetParam = 0;
		}
		Debug.Log("Target node: " + targetParam);
		path.setTargeted (targetParam);

		//Get target position
		target = path.getPosition(targetParam);
	}

	void followPathPredict(){

		//Find the predicted future location
		Vector3 futurePos = transform.position + velocity * predictTime;

		//Find the current position on the path
		path.setNormal(actualParam);

		actualParam = path.getParam(transform.position, actualParam);

		path.setActual (actualParam);
		Debug.Log ("Actual node: " + actualParam);

		path.setNormal (currentParam);

		if (useParameterTracking) {
			currentParam = path.getParamTracking (futurePos, currentParam);
		} else {
			currentParam = path.getParam (futurePos, currentParam);
		}

		path.setPredicted (currentParam);
		Debug.Log ("Predicted node: " + currentParam);

		//Offset it
		path.setNormal(targetParam);

		targetParam = currentParam + pathOffset;

		path.setTargeted (targetParam);
		Debug.Log("Target node: " + targetParam);

		//Get target position
		target = path.getPosition(targetParam);

	}

	SteeringOutput getSteering(){
		SteeringOutput steering = new SteeringOutput ();

		/*/
		 * Seek 
		/*/

		Vector3 direction = target - transform.position;
		float distance = direction.magnitude;

		Vector3 targetVelocity = direction;
		orientation = transform.rotation.eulerAngles.y;
		targetVelocity.Normalize ();
		targetVelocity *= maxSpeed;

		steering.linear = targetVelocity - velocity;
		steering.linear /= timeToTarget;

		if (steering.linear.magnitude > maxAcceleration) {
			steering.linear.Normalize ();
			steering.linear *= maxAcceleration;
		}

		/*/
		 * Face
		/*/
		float targetOrientation = Mathf.Rad2Deg * Mathf.Atan2 (-velocity.x, velocity.z);
		//Debug.Log("Direction to face: "+targetOrientation);
		if(targetOrientation-orientation < 0){
			targetOrientation = -targetOrientation;
		}
		steering.angular = targetOrientation;

		/*
		if (Mathf.Abs (steering.angular) > maxAngularAcceleration) {
			steering.angular /= Mathf.Abs (steering.angular);
			steering.angular *= maxAngularAcceleration;
		}
		*/
		return steering;
	}

	struct SteeringOutput{
		public Vector3 linear;
		public float angular;
	}

	struct Path{
		Vector3[] path2;
		GameObject[] nodes;
		public Material selected;
		public Material normal;
		public Material predicted;
		public Material actual;
		Renderer rend;
		public int buffer;

		public int size(){
			return path2.Length;
		}

		public void setPath(){
			nodes = GameObject.FindGameObjectsWithTag ("Node");
			path2 = new Vector3[nodes.Length];
			for (int i = 0; i < path2.Length; ++i) {
				GameObject temp = GameObject.Find ("Pathnode (" + i + ")");
				path2 [i] = temp.transform.position;
				nodes [i] = temp;
			}
				
		}

		public int getParam(Vector3 position, int lastParam){
			int param = path2.Length;
			float closest = 1000.0f;

			for (int i = lastParam; i < path2.Length; ++i) {
				if((path2[i]-position).magnitude<closest){
					closest = (path2 [i] - position).magnitude;
					param = i;
				}


			}

			return param; 
		}

		public int getParamTracking(Vector3 position, int lastParam){
			int param = path2.Length;
			float closest = 1000.0f;

			for (int i = lastParam; i < lastParam+buffer; ++i) {
				if((path2[i]-position).magnitude<closest){
					closest = (path2 [i] - position).magnitude;
					param = i;
				}


			}

			return param; 
		}

		public Vector3 getPosition(int param){
			return path2 [param];
		}

		public void setTargeted(int param){
			rend = nodes [param].GetComponent<Renderer> ();
			rend.sharedMaterial = selected;
		}

		public void setNormal(int param){
			rend = nodes [param].GetComponent<Renderer> ();
			rend.sharedMaterial = normal;
		}

		public void setPredicted(int param){
			rend = nodes [param].GetComponent<Renderer> ();
			rend.sharedMaterial = predicted;
		}

		public void setActual(int param){
			rend = nodes [param].GetComponent<Renderer> ();
			rend.sharedMaterial = actual;
		}
	}
		
}
