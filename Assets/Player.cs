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

	[Serializable]
	public class ControlledBone {
		public GameObject	Bone;
		public Quaternion	BaseOri;
		public float		ChainWeight;
		public bool			FlipDirection;
	}
	
	public ControlledBone[] LookYBoneChain;

	Transform LegLBone;
	Transform LegRBone;

	Quaternion LegLBaseOri;
	Quaternion LegRBaseOri;

	Quaternion GunBaseOri;
	
	bool Firstperson = true;

	private void Start () {
		Animator = GetComponentInChildren<Animator>();
		
		LegLBone = transform.Find("solider/Armature/Hips/UpperLeg.L");
		LegRBone = transform.Find("solider/Armature/Hips/UpperLeg.R");
		
		LegLBaseOri = LegLBone.transform.localRotation;
		LegRBaseOri = LegRBone.transform.localRotation;

		foreach (var b in LookYBoneChain) {
			b.BaseOri = b.Bone.transform.localRotation;
		}

		GunBaseOri = Gun.transform.localRotation;
	}

	private void FixedUpdate () {
		
	}
	
	Camera ActiveCamera => (Firstperson ? FpsCamera : TpsCamera).GetComponentInChildren<Camera>();
	bool IsWalking => false;

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

			Gun		 .transform.localRotation = GunBaseOri * Quaternion.AngleAxis(MouselookAng.y * 3f/5, Vector3.right);
			FpsCamera.transform.localEulerAngles = float3(MouselookAng.y * 1f/5, 0, 0);
			
			float totalWeight = LookYBoneChain.Sum(x => x.ChainWeight);

			foreach (var b in LookYBoneChain) {
				var rot = Quaternion.AngleAxis(MouselookAng.y * (b.FlipDirection ? -1f : +1f) * b.ChainWeight / totalWeight, Vector3.right);

				b.Bone.transform.localRotation = rot * b.BaseOri;
				if (b.Bone.name == "Hips") {

					LegLBone.localRotation = inverse(rot) * LegLBaseOri;
					LegRBone.localRotation = inverse(rot) * LegRBaseOri;
				}
			}
		}
	}
	#endregion
}
