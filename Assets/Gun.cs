using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;

public class Gun : MonoBehaviour {
	public GameObject Muzzle;

	public float TorsoAngle = 45;

	private void OnDrawGizmos () {
		Gizmos.color = Color.red;
		Ray r = new Ray(Muzzle.transform.TransformPoint(float3(0)), Muzzle.transform.TransformDirection(float3(0,0,1)).normalized);
		Gizmos.DrawLine(r.origin, r.origin + r.direction * 0.3f);
	}
}
