using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;

public class Player : MonoBehaviour {

	public GameObject		Gun;
	public GameObject		FpsCamera;
	public GameObject		TpsCamera;
	public Animator			Animator;
	public CapsuleCollider	CapsuleCollider;
	public Rigidbody		Rigidbody;
	
	float3 pos => transform.position;
	quaternion ori => transform.rotation;

	bool IsGrounded = false;
	float3 GroundNormal;
	bool IsWalking => IsGrounded && length(target_vel) > 0.1f;
	
	public float MaxSpeed = 10f;
	public float WalkMultiplier = 0.4f;

	public float BaseAccel = 200f;
	public float AirControlAccel = 15f;
	public AnimationCurve AccelCurve;

	public float JumpVel = 10f;

	public float TerminalVel = 40;

	public float MaxGroundAngle = 45;
	public AnimationCurve GroundAngleWalkSpeed;

	public bool Firstperson = true;

	float Drag (float speed) { // Drag is equal to gravity at TerminalVel to enforce TerminalVel
		float accel = Physics.gravity.magnitude;
		
		float f = speed / TerminalVel;
		f = f * f;
		return f * accel;
	}

	float2 move_dir;
	bool walk = false;
	bool jump = false;
	int jumped = int.MaxValue; // frames after jump
	
	float3 target_vel = 0;
	float3 avgGroundNormal = 0;
	
	float3 _accel = 0;

	private void FixedUpdate () {
		// NOTE: using IsGrounded from Collision checks of prev frame, ie. 1 frane lag, solution would be to use LateFixedUpdate but that does not exist
		Rigidbody.useGravity = !IsGrounded;

		GroundNormal = IsGrounded ? normalizesafe(avgGroundNormal) : 0;

		//
		float3 move_dir3 = transform.TransformDirection(float3(move_dir.x, 0, move_dir.y));

		float walk_mult = walk ? WalkMultiplier : 1;

		float maxSpeed = MaxSpeed * walk_mult;
		target_vel = move_dir3 * maxSpeed;
		
		// frame0:
		//   jump =  true   vel.y = 0   IsGrounded = true
		// frame1:
		//   jump = false   vel.y = 5   IsGrounded = true <- fix this, or else we slow down our jump velocity
		// frame2:
		//   jump = false   vel.y = 5   IsGrounded = false
		if (jumped == 1)
			IsGrounded = false; 

		if (IsGrounded) {
			target_vel -= GroundNormal * dot(target_vel, GroundNormal);

			float incl = clamp(dot(normalizesafe(target_vel), normalizesafe(Physics.gravity)), -1,+1); // ground ang in cos(deg)
			incl = acos(incl) / ((float)PI/2) - 1f; // ground inc in deg/90 ( [-1, +1] for [-90deg, +90deg] )
			incl *= 90f / MaxGroundAngle; // in [-1, +1] of MaxGroundAngle
			incl = incl * 0.5f + 0.5f;

			float walk_speed = GroundAngleWalkSpeed.Evaluate(incl);

			target_vel = normalizesafe(target_vel) * length(target_vel) * walk_speed;
		}

		float3 cur_vel = Rigidbody.velocity;
		float3 delta_v = target_vel - cur_vel; // Velocity change needed to achieve target velocity
		
		float max_accel = IsGrounded ? BaseAccel * walk_mult : AirControlAccel;
		
		float3 accel = delta_v / Time.fixedDeltaTime; // Acceleration needed to achieve target velocity in 1 FixedUpdate
		
		if (!IsGrounded) {
			float3 grav_dir = normalizesafe(Physics.gravity);
			accel -= grav_dir * dot(grav_dir, accel);
		}

		accel = normalizesafe(accel) * min(length(accel), max_accel); // Clamp Acceleration to a max which causes our velocity to not be equal to target_vel in 1 FixedUpdate, but be smoothed
		
		accel += normalizesafe(cur_vel) * -Drag(length(cur_vel)) * Time.fixedDeltaTime;
		
		Rigidbody.AddForce(accel, ForceMode.Acceleration);

		if (IsGrounded && jump) {
			Rigidbody.AddForce(transform.up * JumpVel, ForceMode.VelocityChange);
			jumped = 0;
		}
		
		jumped++;

		//Debug.Log(
		//	"pos: "+ pos +
		//	"  vel: "+ ((float3)Rigidbody.velocity) +
		//	"  target_vel: "+ target_vel +
		//	"  accel: "+ accel +
		//	"  jump: "+ jump +
		//	"  IsGrounded: "+ IsGrounded
		//	);

		IsGrounded = false;
		avgGroundNormal = 0;
	}
	
	List<ContactPoint> _colls = new List<ContactPoint>();
	void Collision (Collision collision) {
		for (int i=0; i<collision.contactCount; ++i) {
			var c = collision.GetContact(i);
			_colls.Add(c);
			
			float ground_ang = dot(c.normal, transform.up);
			ground_ang = acos(saturate(ground_ang)); // saturate to prevent NaN due to float error

			if (ground_ang < radians(MaxGroundAngle)) {
				avgGroundNormal += (float3)c.normal;
				IsGrounded = true;
			}
		}
	}
	private void OnCollisionEnter (Collision collision) => Collision(collision);
	private void OnCollisionStay (Collision collision) => Collision(collision);

	Camera ActiveCamera => (Firstperson ? FpsCamera : TpsCamera).GetComponentInChildren<Camera>();
	
	void LateUpdate () {
		if (Input.GetKeyDown(KeyCode.F))
			Firstperson = !Firstperson;
		FpsCamera.SetActive(Firstperson);
		TpsCamera.SetActive(!Firstperson);

		Mouselook();

		move_dir = 0;
		move_dir.x -= Input.GetKey(KeyCode.A) ? 1f : 0f;
		move_dir.x += Input.GetKey(KeyCode.D) ? 1f : 0f;
		move_dir.y -= Input.GetKey(KeyCode.S) ? 1f : 0f;
		move_dir.y += Input.GetKey(KeyCode.W) ? 1f : 0f;
		walk = Input.GetKey(KeyCode.LeftShift);
		jump = Input.GetKeyDown(KeyCode.Space);
		bool crouch = Input.GetKey(KeyCode.LeftControl);
		
		move_dir = normalizesafe(move_dir);

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

		float3 a = pos;
		float3 b = pos + 0.05f;

		Gizmos.color = Color.cyan;
		Gizmos.DrawLine(a, a + target_vel);
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(b, b + (float3)Rigidbody.velocity);
		//Gizmos.color = Color.yellow;
		//Gizmos.DrawLine(b + (float3)Rigidbody.velocity, b + (float3)Rigidbody.velocity + _delta_v);
		Gizmos.color = Color.magenta;
		Gizmos.DrawLine(b + (float3)Rigidbody.velocity + 0.05f, b + (float3)Rigidbody.velocity + _accel);
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
			
			//
			Gun		 .transform.localEulerAngles = float3(MouselookAng.y, 0, 0);
			FpsCamera.transform.localEulerAngles = float3(MouselookAng.y, 0, 0);
			transform.localEulerAngles			 = float3(0, MouselookAng.x, 0);
		}
	}
	#endregion
}
