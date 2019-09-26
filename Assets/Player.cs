using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using System.Linq;

public class Player : MonoBehaviour {

	public GameObject		Gun;
	public Camera			FpsCamera;
	public Animator			Animator;
	
	public float MaxSpeed = 4f;
	public float SprintMultiplier = 1.8f;

	public float TerminalVel = 40;

	public float GroundCastOffset;
	public float GroundCastDist;
	public float GroundCastRadius;

	float Drag (float speed) { // Drag is equal to gravity at TerminalVel to enforce TerminalVel
		float accel = Physics.gravity.magnitude;
		
		float f = speed / TerminalVel;
		f = f * f;
		return f * accel;
	}

	bool GroundCollision (ref float3 standing_pos, bool gizmos=false) {
		float3 p1 = transform.TransformPoint(float3(0, GroundCastOffset, 0));
		float dist = GroundCastDist + GroundCastOffset;

		if (gizmos) Gizmos.DrawWireSphere(p1, GroundCastRadius);
		if (gizmos) Gizmos.DrawWireSphere(transform.TransformPoint(float3(0, GroundCastOffset - dist, 0)), GroundCastRadius);

		if (Physics.SphereCast(p1, GroundCastRadius, -transform.up, out RaycastHit hit, dist)) {
			if (gizmos) Gizmos.DrawWireSphere(hit.point, 0.1f);
			standing_pos = hit.point;
			return true;
		}
		return false;
	}
	void GizmoGroundCollision () {
		Gizmos.color = Color.green;
		
		float3 standing_pos = default;
		GroundCollision(ref standing_pos, true);
	}

	public float JumpForce = 10f;

	float3 vel = 0;
	
	bool IsGrounded;
	bool IsWalking => IsGrounded && length(vel) > 0.1f;

	void Update () {
		float dt = Time.deltaTime;

		Mouselook();

		float3 move_dir = 0;
		move_dir.x -= Input.GetKey(KeyCode.A) ? 1f : 0f;
		move_dir.x += Input.GetKey(KeyCode.D) ? 1f : 0f;
		move_dir.z -= Input.GetKey(KeyCode.S) ? 1f : 0f;
		move_dir.z += Input.GetKey(KeyCode.W) ? 1f : 0f;
		bool sprint = Input.GetKey(KeyCode.LeftShift);
		
		move_dir = normalizesafe(move_dir);
		move_dir = transform.TransformDirection(move_dir);

		float3 target_vel = move_dir * MaxSpeed * (sprint ? SprintMultiplier : 1);
		
		vel = float3(target_vel.x, vel.y, target_vel.z);

		float speed = length(vel);
		float3 vel_dir = normalizesafe(vel);

		float3 accel = (float3)Physics.gravity + (vel_dir * -Drag(length(speed)));

		vel += accel * dt;

		Debug.Log("---------------");
		
		float3 standing_pos = default;
		IsGrounded = GroundCollision(ref standing_pos);

		Debug.Log("transform.position.y: "+ transform.position.y);

		if (IsGrounded) {
			float3 ground_pen = (float3)transform.position - standing_pos;
			float ground_pen_dist = dot(ground_pen, -transform.up.normalized);
			ground_pen = ground_pen_dist * transform.up.normalized;

			Debug.Log("ground_pen_dist: "+ ground_pen_dist);

			if (ground_pen_dist >= -0.01f) { // small bias to prevent vibrating just above ground
				transform.position += (Vector3)ground_pen;
				vel -= (float3)transform.up.normalized * dot(vel, transform.up.normalized);
			}
		}
		
		bool jump = Input.GetKeyDown(KeyCode.Space) && IsGrounded;
		bool crouch = Input.GetKey(KeyCode.LeftControl);

		if (jump) {
			vel.y += JumpForce;
		}

		transform.position += (Vector3)(vel * dt);

		Animator.SetBool("isWalking", IsWalking);

		Debug.Log("vel: "+ vel);
	}

	void OnDrawGizmos () {
		GizmoGroundCollision();
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
