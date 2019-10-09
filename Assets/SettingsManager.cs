using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]
public class SettingsManager : MonoBehaviour {
	[Header("Graphics Settings")]
		
	public bool VSync = false;

	[Tooltip("-1 = no target (unlimited on pc)")]
	public int TargetFramerate = 200;

	bool prevVSync = false;
	int prevTargetFramerate = 200;
	
	void Start () {
		QualitySettings.vSyncCount = VSync ? 1 : 0;
		Application.targetFrameRate = TargetFramerate;
		
		prevVSync = VSync;
		prevTargetFramerate = TargetFramerate;
	}
	void Update () {
		if (VSync != prevVSync)
			QualitySettings.vSyncCount = VSync ? 1 : 0;
		if (TargetFramerate != prevTargetFramerate)
			Application.targetFrameRate = TargetFramerate;

		prevVSync = VSync;
		prevTargetFramerate = TargetFramerate;
	}
}

