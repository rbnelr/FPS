﻿using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using System.Linq;

public class Player : MonoBehaviour {

	public GameObject		Gun;
	public Camera			FpsCamera;
	public Animator			Animator;
	public CapsuleCollider	CapsuleCollider;
	
	public float MaxSpeed = 4f;
	public float SprintMultiplier = 1.8f;

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

	bool GroundCollision (ref float3 standing_pos, bool gizmos=false) {
		float3 up = transform.up.normalized;

		float3 p1 = transform.TransformPoint(float3(0, GroundCastOffset, 0));
		float dist = GroundCastDist + GroundCastOffset;

		if (gizmos) Gizmos.DrawWireSphere(p1, GroundCastRadius);
		if (gizmos) Gizmos.DrawWireSphere(transform.TransformPoint(float3(0, GroundCastOffset - dist, 0)), GroundCastRadius);

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

		if (closest_dist < float.PositiveInfinity) {
			standing_pos = closest;
			return true;
		}
		return false;
	}
	float3 HorizCollision (bool gizmos=false) {
		float3 p1 = transform.TransformPoint((float3)CapsuleCollider.center - float3(0, CapsuleCollider.height/2 - CapsuleCollider.radius, 0));
		float3 p2 = transform.TransformPoint((float3)CapsuleCollider.center + float3(0, CapsuleCollider.height/2 - CapsuleCollider.radius, 0));

		if (gizmos) Gizmos.DrawWireSphere(transform.TransformPoint(CapsuleCollider.center), 0.02f);

		float3 totalDepen = 0;

		var colliders = Physics.OverlapCapsule(p1, p2, CapsuleCollider.radius);
		foreach (var coll in colliders) {
			if (coll == CapsuleCollider) continue; // no collision with itself

			Debug.Assert(coll.attachedRigidbody == null); // only against static colliders for now

			if (Physics.ComputePenetration(CapsuleCollider, CapsuleCollider.transform.position, CapsuleCollider.transform.rotation,
				coll, coll.transform.position, coll.transform.rotation, out Vector3 dir, out float dist)) {

				if (gizmos) Gizmos.DrawLine(transform.TransformPoint(CapsuleCollider.center), transform.TransformPoint(CapsuleCollider.center) + dir * dist);

				totalDepen += (float3)dir * dist;
			}
		}

		return totalDepen;
	}

	void OnDrawGizmos () {
		Gizmos.color = Color.green;
		float3 standing_pos = default;
		GroundCollision(ref standing_pos, true);

		Gizmos.color = Color.red;
		HorizCollision(true);
	}

	public float JumpForce = 10f;

	float3 vel = 0;
	
	bool IsGrounded;
	bool IsWalking => IsGrounded && length(vel) > 0.1f;

	void Update () {
		float dt = Time.deltaTime;
		float3 up = transform.up.normalized; // is this already normalized ?

		//return;

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
		
		if (IsGrounded) {
			vel = float3(target_vel.x, vel.y, target_vel.z);
		}

		// Find Grounding Collision
		float3 standing_pos = default;
		IsGrounded = GroundCollision(ref standing_pos);

		// Grounding Response
		if (IsGrounded) {
			float3 ground_pen = (float3)transform.position - standing_pos;
			float ground_pen_dist = dot(ground_pen, -up);
			ground_pen = ground_pen_dist * up;

			IsGrounded = ground_pen_dist >= -0.01f;
			if (IsGrounded) { // small bias to prevent vibrating just above ground
				transform.position += (Vector3)ground_pen;
				vel -= up * dot(vel, up);
			}
		}
		
		// Handle Other Collisions
		float3 depen = HorizCollision();
		float3 depen_dir = normalizesafe(depen);

		transform.position += (Vector3)depen * 0.9f;
		vel -= -depen_dir * max(dot(vel, -depen_dir), 0);

		Debug.Log("transform.position: "+ transform.position +" vel: "+ vel);

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

		transform.position += (Vector3)(vel * dt);

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
		}
	}
	#endregion
}
