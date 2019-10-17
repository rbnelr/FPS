using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System;

public class Player : MonoBehaviour {

	public Gun				Gun;
	public Transform		GunStockPos;
	public GameObject		FpsCamera;
	public GameObject		TpsCamera;
	public CapsuleCollider	CapsuleCollider;
	public Rigidbody		Rigidbody;
	public SoldierAnim		SoldierAnim;
	
	bool Firstperson = true;
	
	Camera ActiveCamera => (Firstperson ? FpsCamera : TpsCamera).GetComponentInChildren<Camera>();
	
	void Update () {
		if (Input.GetKeyDown(KeyCode.F))
			Firstperson = !Firstperson;
		FpsCamera.SetActive(Firstperson);
		TpsCamera.SetActive(!Firstperson);

		Mouselook();
		
		//
		Gun.transform.position = GunStockPos.position;
		SoldierAnim.IsWalking = Input.GetKey(KeyCode.W);
		SoldierAnim.TorsoAngle = Gun.TorsoAngle;
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
}
