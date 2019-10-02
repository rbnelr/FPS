using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using System.Linq;

public class Player : MonoBehaviour {

	public GameObject		Gun;
	public Camera			FpsCamera;
	public Animator			Animator;
	public CapsuleCollider	CapsuleCollider;
	
	float3 pos;
	float3 vel = 0;
	quaternion ori;
	
	float3 up => mul(ori, float3(0,1,0));

	float3 TransformPoint (float3 p) => mul(ori, p) + pos;
	float3 TransformDirection (float3 p) => mul(ori, p);
	
	bool IsGrounded;
	bool IsWalking => IsGrounded && length(vel) > 0.1f;

	public float MaxSpeed = 4f;
	public float SprintMultiplier = 1.8f;

	public float JumpForce = 10f;

	public float TerminalVel = 40;

	public float MaxGroundAngle = 45;

	public float GroundCastOffset;
	public float GroundCastDist;
	public float GroundCastRadius;

	//public float HorizCollisLow;
	//public float HorizCollisHigh;
	//public float HorizCollisRadius;

	float Drag (float speed) { // Drag is equal to gravity at TerminalVel to enforce TerminalVel
		float accel = Physics.gravity.magnitude;
		
		float f = speed / TerminalVel;
		f = f * f;
		return f * accel;
	}

	void GroundCollision (bool gizmos=false) {
		float3 p1 = TransformPoint(float3(0, GroundCastOffset, 0));
		float dist = GroundCastDist + GroundCastOffset;

		if (gizmos) Gizmos.DrawWireSphere(p1, GroundCastRadius);
		if (gizmos) Gizmos.DrawWireSphere(TransformPoint(float3(0, GroundCastOffset - dist, 0)), GroundCastRadius);

		var hits = Physics.SphereCastAll(p1, GroundCastRadius, -up, dist);

		float3 closest = default;
		float closest_dist = float.PositiveInfinity;
		foreach (var hit in hits) {
			if (hit.collider == CapsuleCollider) continue; // no collision with itself
			
			bool valid = max(0f, dot(hit.normal, up)) > cos(math.radians(MaxGroundAngle));
			
			if (gizmos) Gizmos.color = valid ? Color.green : Color.red;
			if (gizmos) Gizmos.DrawWireSphere(hit.point, 0.1f);
			
			if (hit.distance < closest_dist && valid) {
				closest = hit.point;
				closest_dist = hit.distance;
			}
		}

		IsGrounded = closest_dist < float.PositiveInfinity;
		if (IsGrounded) {
			float3 standing_pos = closest;
			
			float3 ground_pen = pos - standing_pos;
			float ground_pen_dist = dot(ground_pen, -up);
			ground_pen = ground_pen_dist * up;

			IsGrounded = ground_pen_dist >= -0.01f;
			if (IsGrounded) { // small bias to prevent vibrating just above ground
				pos += ground_pen;
				vel -= up * min(dot(vel, up), 0);
			}
		}
	}
	void HorizCollision (bool gizmos=false) {
		float3 p1 = TransformPoint((float3)CapsuleCollider.center - float3(0, CapsuleCollider.height/2 - CapsuleCollider.radius, 0));
		float3 p2 = TransformPoint((float3)CapsuleCollider.center + float3(0, CapsuleCollider.height/2 - CapsuleCollider.radius, 0));

		if (gizmos) Gizmos.DrawWireSphere(TransformPoint(CapsuleCollider.center), 0.02f);
		
		float3 depen = 0;

		var colliders = Physics.OverlapCapsule(p1, p2, CapsuleCollider.radius);
		foreach (var coll in colliders) {
			if (coll == CapsuleCollider) continue; // no collision with itself

			Debug.Assert(coll.attachedRigidbody == null); // only against static colliders for now

			if (Physics.ComputePenetration(CapsuleCollider, pos, ori,
				coll, coll.transform.position, coll.transform.rotation, out Vector3 dir, out float dist)) {

				if (gizmos) Gizmos.DrawLine(TransformPoint(CapsuleCollider.center), TransformPoint(CapsuleCollider.center) + (float3)dir * dist);

				depen += (float3)dir * dist;
			}
		}
		
		float3 depen_dir = normalizesafe(depen);
				
		pos += depen;
		vel -= -depen_dir * max(dot(vel, -depen_dir), 0);
	}

	void OnDrawGizmos () {
		Gizmos.color = Color.green;
		//float3 standing_pos = default;
		//GroundCollision(true);

		Gizmos.color = Color.red;
		//HorizCollision(true);
	}

	void Update () {
		float dt = Time.deltaTime;
		pos = transform.position;
		ori = transform.rotation;
		
		Mouselook();

		float3 move_dir = 0;
		move_dir.x -= Input.GetKey(KeyCode.A) ? 1f : 0f;
		move_dir.x += Input.GetKey(KeyCode.D) ? 1f : 0f;
		move_dir.z -= Input.GetKey(KeyCode.S) ? 1f : 0f;
		move_dir.z += Input.GetKey(KeyCode.W) ? 1f : 0f;
		bool sprint = Input.GetKey(KeyCode.LeftShift);
		bool jump = Input.GetKeyDown(KeyCode.Space);
		bool crouch = Input.GetKey(KeyCode.LeftControl);
		
		//move_dir.z = +1;

		// Planar Movement
		move_dir = normalizesafe(move_dir);
		move_dir = TransformDirection(move_dir);

		float3 target_vel = move_dir * MaxSpeed * (sprint ? SprintMultiplier : 1);
		
		if (IsGrounded) {
			vel = float3(target_vel.x, vel.y, target_vel.z);
		}
		
		//
		float speed = length(vel);
		float3 vel_dir = normalizesafe(vel);

		// Drag
		vel += vel_dir * -Drag(length(speed)) * dt;

		// Gravity
		if (!IsGrounded) {
			vel += (float3)Physics.gravity * dt;
		}

		// Jumping
		if (jump && IsGrounded) {
			vel.y += JumpForce;
		}

		//// Update position with velocity
		pos += vel * dt;

		//// Now find collisions and handle them
		HorizCollision();
		GroundCollision();

		//
		transform.position = pos;

		Debug.Log("pos: "+ pos +" vel: "+ vel);

		//
		Animator.SetBool("isWalking", IsWalking);
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
			float mouseMult = MouselookSensitiviy * FpsCamera.fieldOfView / 2;
			MouselookAng += mouseMult * float2(Input.GetAxisRaw("Mouse X"), -Input.GetAxisRaw("Mouse Y"));
			MouselookAng.x = fmod(MouselookAng.x, 360f);
			MouselookAng.y = clamp(MouselookAng.y, -90 + LookDownLimit, +90 - LookUpLimit);
		
			Gun		 .transform.localEulerAngles = float3(MouselookAng.y, 0, 0);
			FpsCamera.transform.localEulerAngles = float3(MouselookAng.y, 0, 0);
			transform.localEulerAngles			 = float3(0, MouselookAng.x, 0);

			ori = transform.rotation;
		}
	}
	#endregion
}
