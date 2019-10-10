using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;

public class Player : MonoBehaviour {

	public Gun				Gun;
	public GameObject		FpsCamera;
	public GameObject		TpsCamera;
	public Animator			Animator;
	public CapsuleCollider	CapsuleCollider;
	public Rigidbody		Rigidbody;
	public Transform		SpineBone;
	public Transform		ChestBone;
	public Transform		NeckBone;
	public Transform		HeadBone;

	Quaternion		GunBaseOri;
	Quaternion		SpineBaseOri;
	Quaternion		ChestBaseOri;
	Quaternion		NeckBaseOri ;
	Quaternion		HeadBaseOri ;
	
	bool Firstperson = true;

	private void Start () {
		Animator = GetComponentInChildren<Animator>();
		
		SpineBone = transform.Find("solider/Armature/Hips/Spine");
		ChestBone = SpineBone.Find("Chest");
		NeckBone = ChestBone.Find("Neck");
		HeadBone = NeckBone.Find("Head");

		SpineBaseOri = SpineBone.localRotation;
		ChestBaseOri = ChestBone.localRotation;
		NeckBaseOri  = NeckBone .localRotation;
		HeadBaseOri  = HeadBone .localRotation;

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
			
			SpineBone.localRotation = Quaternion.AngleAxis(MouselookAng.y * +1f/5, Vector3.right) * SpineBaseOri;
			ChestBone.localRotation = Quaternion.AngleAxis(MouselookAng.y * +1f/5, Vector3.right) * ChestBaseOri;
			NeckBone .localRotation = Quaternion.AngleAxis(MouselookAng.y * -1f/5, Vector3.right) * NeckBaseOri;
			HeadBone .localRotation = Quaternion.AngleAxis(MouselookAng.y * -1f/5, Vector3.right) * HeadBaseOri;
		}
	}
	#endregion
}
