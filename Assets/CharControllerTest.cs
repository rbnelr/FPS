using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;

public class CharControllerTest : MonoBehaviour {

	public float Speed = 10f;

	private void Update () {
		Mouselook();

		float3 move_dir = 0;
		move_dir.x -= Input.GetKey(KeyCode.A) ? 1f : 0f;
		move_dir.x += Input.GetKey(KeyCode.D) ? 1f : 0f;
		move_dir.z -= Input.GetKey(KeyCode.S) ? 1f : 0f;
		move_dir.z += Input.GetKey(KeyCode.W) ? 1f : 0f;
		bool walk = Input.GetKey(KeyCode.LeftShift);
		bool jump = Input.GetKeyDown(KeyCode.Space);
		bool crouch = Input.GetKey(KeyCode.LeftControl);

		move_dir *= walk ? 2f : 1f;

		GetComponent<CharacterController>().SimpleMove(transform.TransformVector(move_dir * Speed));
	}
	
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
			float mouseMult = MouselookSensitiviy * 70f / 2;
			MouselookAng += mouseMult * float2(Input.GetAxisRaw("Mouse X"), -Input.GetAxisRaw("Mouse Y"));
			MouselookAng.x = fmod(MouselookAng.x, 360f);
			MouselookAng.y = clamp(MouselookAng.y, -90 + LookDownLimit, +90 - LookUpLimit);
			
			//
			//Gun		 .transform.localEulerAngles = float3(MouselookAng.y, 0, 0);
			//FpsCamera.transform.localEulerAngles = float3(MouselookAng.y, 0, 0);
			transform.localEulerAngles			 = float3(0, MouselookAng.x, 0);
		}
	}
}
