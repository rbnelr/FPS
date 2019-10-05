using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using System.Linq;
using System.Collections.Generic;

public class Player : MonoBehaviour {

	public GameObject		Gun;
	public GameObject		FpsCamera;
	public GameObject		TpsCamera;
	public Animator			Animator;
	public CapsuleCollider	CapsuleCollider;
	public Rigidbody		Rigidbody;
	
	float3 vel = 0;

	float3 pos => transform.position;
	quaternion ori => transform.rotation;

	bool IsGrounded = false;
	bool IsWalking => IsGrounded && length(vel) > 0.1f;
	
	public float MaxSpeed = 4f;
	public float SprintMultiplier = 1.8f;

	public float MaxAccel = 20f;

	public float JumpForce = 10f;
	[Range(0f, 1f)]
	public float AirControlFraction = 0.1f;

	public float TerminalVel = 40;

	public float MaxGroundAngle = 45;

	public bool Firstperson = true;

	float Drag (float speed) { // Drag is equal to gravity at TerminalVel to enforce TerminalVel
		float accel = Physics.gravity.magnitude;
		
		float f = speed / TerminalVel;
		f = f * f;
		return f * accel;
	}

	private void FixedUpdate () {
		Rigidbody.useGravity = !IsGrounded;
		
		float control = 1f;
		if (!IsGrounded)
			control *= AirControlFraction;

		float3 accel = (vel - (float3)Rigidbody.velocity) * MaxAccel * control;
		accel = normalizesafe(accel) * min(length(accel), MaxAccel * control);
		Rigidbody.AddForce(accel, ForceMode.Acceleration);
		
		Debug.Log("IsGrounded: "+ IsGrounded +" pos: "+ pos +" vel: "+ (float3)Rigidbody.velocity +" accel: "+ accel);
		
		IsGrounded = false;
	}
	
	List<ContactPoint> _colls = new List<ContactPoint>();
	void Collision (Collision collision) {
		for (int i=0; i<collision.contactCount; ++i) {
			var c = collision.GetContact(i);
			_colls.Add(c);
			
			float ground_ang = dot(c.normal, transform.up);
			ground_ang = acos(saturate(ground_ang)); // saturate to prevent NaN due to float error

			IsGrounded = IsGrounded || ground_ang < radians(MaxGroundAngle);
		}
	}
	private void OnCollisionEnter (Collision collision) => Collision(collision);
	private void OnCollisionStay (Collision collision) => Collision(collision);

	Camera ActiveCamera => (Firstperson ? FpsCamera : TpsCamera).GetComponentInChildren<Camera>();

	void Update () {
		float dt = Time.deltaTime;
		vel = Rigidbody.velocity;
		
		if (Input.GetKeyDown(KeyCode.F))
			Firstperson = !Firstperson;
		FpsCamera.SetActive(Firstperson);
		TpsCamera.SetActive(!Firstperson);

		Mouselook();

		float3 move_dir = 0;
		move_dir.x -= Input.GetKey(KeyCode.A) ? 1f : 0f;
		move_dir.x += Input.GetKey(KeyCode.D) ? 1f : 0f;
		move_dir.z -= Input.GetKey(KeyCode.S) ? 1f : 0f;
		move_dir.z += Input.GetKey(KeyCode.W) ? 1f : 0f;
		bool sprint = Input.GetKey(KeyCode.LeftShift);
		bool jump = Input.GetKeyDown(KeyCode.Space);
		bool crouch = Input.GetKey(KeyCode.LeftControl);
		
		// Planar Movement
		move_dir = normalizesafe(move_dir);
		move_dir = transform.TransformDirection(move_dir);

		float3 target_vel = move_dir * MaxSpeed * (sprint ? SprintMultiplier : 1);
		
		//float control = 1f;
		//if (!IsGrounded)
		//	control *= AirControlFraction;
		//vel = lerp(vel, float3(target_vel.x, vel.y, target_vel.z), control);
		vel = float3(target_vel.x, vel.y, target_vel.z);
		
		//
		float speed = length(vel);
		float3 vel_dir = normalizesafe(vel);

		// Drag
		vel += normalizesafe(vel) * -Drag(length(speed)) * dt;
		
		//Rigidbody.velocity = vel;

		// Jumping
		if (jump && IsGrounded) {
			Rigidbody.AddForce(transform.up * JumpForce, ForceMode.VelocityChange);
		}

		//
		Animator.SetBool("isWalking", IsWalking);
	}

	private void OnDrawGizmos () {
		foreach (var c in _colls) {
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(c.point, 0.05f);
			Gizmos.DrawLine(c.point, c.point + c.normal * 0.1f);
		}
		_colls.Clear();
	}

	#region Mouselook
	public float MouselookSensitiviy = 1f / 5f; // screen radii per mouse input units
		
	public float LookDownLimit = 5f;
	public float LookUpLimit = 5f;

	public bool MouselookActive = true;
	public float2 MouselookAng = float2(0,0);

	void Mouselook () {
		if (Input.GetKeyDown(KeyCode.F2))
			MouselookActive = !MouselookActive;
		
		Cursor.lockState = MouselookActive ? CursorLockMode.Locked : CursorLockMode.None;
		Cursor.visible = !MouselookActive;
		
		if (MouselookActive) {
			float mouseMult = MouselookSensitiviy * ActiveCamera.fieldOfView / 2;
			MouselookAng += mouseMult * float2(Input.GetAxisRaw("Mouse X"), -Input.GetAxisRaw("Mouse Y"));
			MouselookAng.x = fmod(MouselookAng.x, 360f);
			MouselookAng.y = clamp(MouselookAng.y, -90 + LookDownLimit, +90 - LookUpLimit);
		
			Gun		 .transform.localEulerAngles = float3(MouselookAng.y, 0, 0);
			FpsCamera.transform.localEulerAngles = float3(MouselookAng.y, 0, 0);
			transform.localEulerAngles			 = float3(0, MouselookAng.x, 0);
		}
	}
	#endregion
}
