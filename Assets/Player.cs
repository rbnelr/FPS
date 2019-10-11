using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System;

public class Player : MonoBehaviour {

	public Gun				Gun;
	public GameObject		FpsCamera;
	public GameObject		TpsCamera;
	public Animator			Animator;
	public CapsuleCollider	CapsuleCollider;
	public Rigidbody		Rigidbody;
	
	bool Firstperson = true;
	
	Camera ActiveCamera => (Firstperson ? FpsCamera : TpsCamera).GetComponentInChildren<Camera>();
	bool IsWalking => Input.GetKey(KeyCode.W);

	void LateUpdate () {
		if (Input.GetKeyDown(KeyCode.F))
			Firstperson = !Firstperson;
		FpsCamera.SetActive(Firstperson);
		TpsCamera.SetActive(!Firstperson);

		Mouselook();
		
		//
		Animator.SetBool("isWalking", IsWalking);
	}

	private void OnDrawGizmos () {
		
	}

	#region Mouselook
	public float MouselookSensitiviy = 1f / 5f; // screen radii per mouse input units
		
	public float LookDownLimit = 5f;
	public float LookUpLimit = 5f;

	public bool MouselookActive = true;
	public float2 MouselookAng = float2(0,0);

	public GameObject EyesCenter;
	public GameObject LookAt;
	public GameObject RHand;
	public GameObject RElbow;
	public GameObject LHand;
	public GameObject LElbow;

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
			transform.localEulerAngles			 = float3(0, MouselookAng.x, 0);

			Gun		 .transform.localRotation = Quaternion.AngleAxis(MouselookAng.y, Vector3.right);
			FpsCamera.transform.position = EyesCenter.transform.position;
			FpsCamera.transform.localRotation = Quaternion.AngleAxis(MouselookAng.y, Vector3.right);
		}
	}
	#endregion

	private void OnAnimatorIK (int layerIndex) {
		Animator.SetLookAtWeight(1);
		Animator.SetLookAtPosition(LookAt.transform.position);
		
		SetIKTarget(AvatarIKGoal.RightHand, RHand.transform, 1);
		SetIKTarget(AvatarIKHint.RightElbow, RElbow.transform, 1);
		SetIKTarget(AvatarIKGoal.LeftHand, LHand.transform, 1);
		SetIKTarget(AvatarIKHint.LeftElbow, LElbow.transform, 1);

	}

	void SetIKTarget (AvatarIKGoal target, Transform t, float weight) {
		Animator.SetIKPositionWeight(target, weight);
		Animator.SetIKPosition(target, t.position);
		
		Animator.SetIKRotationWeight(target, weight);
		Animator.SetIKRotation(target, t.rotation);
	}
	void SetIKTarget (AvatarIKHint target, Transform t, float weight) {
		Animator.SetIKHintPositionWeight(target, weight);
		Animator.SetIKHintPosition(target, t.position);
	}
}
