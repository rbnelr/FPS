using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;

[RequireComponent(typeof(Animator))]
public class SoldierAnim : MonoBehaviour {

	public bool IsWalking = false;

	public float TorsoAngle = 0;

	public Transform LookAt;
	public Transform LHand, RHand;
	public Transform LElbow, RElbow;

	Animator Animator;
	private void Start () {
		Animator = GetComponent<Animator>();
	}

	void LateUpdate () {
		transform.localRotation = Quaternion.AngleAxis(TorsoAngle, Vector3.up);
		Animator.SetBool("isWalking", IsWalking);
	}

	private void OnAnimatorIK (int layerIndex) {
		Animator.SetLookAtWeight(1);
		Animator.SetLookAtPosition(LookAt.position);
		
		SetIKTarget(AvatarIKGoal.RightHand, RHand, 1);
		SetIKTarget(AvatarIKHint.RightElbow, RElbow, 1);
		SetIKTarget(AvatarIKGoal.LeftHand, LHand, 1);
		SetIKTarget(AvatarIKHint.LeftElbow, LElbow, 1);

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
