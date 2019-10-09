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

	public bool PotatoMode = false;

	bool prevVSync = false;
	int prevTargetFramerate = 200;
	bool prevPotatoMode = false;

	public Material[] Materials; // Mats on which to enable potato mode
	
	void Start () {
		QualitySettings.vSyncCount = VSync ? 1 : 0;
		Application.targetFrameRate = TargetFramerate;
		SetPotatoMode(PotatoMode);
		
		prevVSync = VSync;
		prevTargetFramerate = TargetFramerate;
		prevPotatoMode = PotatoMode;
	}
	void Update () {
		if (VSync != prevVSync)
			QualitySettings.vSyncCount = VSync ? 1 : 0;
		if (TargetFramerate != prevTargetFramerate)
			Application.targetFrameRate = TargetFramerate;
		if (PotatoMode != prevPotatoMode)
			SetPotatoMode(PotatoMode);

		prevVSync = VSync;
		prevTargetFramerate = TargetFramerate;
		prevPotatoMode = PotatoMode;
	}

	void SetPotatoMode (bool on) {
		//foreach (var m in Materials) {
		//	m.SetKeyword("DIRECTIONAL", !on);
		//	m.SetKeyword("LIGHTMAP_ON", !on);
		//}

		ShaderExt.SetKeyword("POTATO_MODE", on);
	}
}

public static class ShaderExt {
	public static void SetKeyword (string keyword, bool state) {
		if (state)
			Shader.EnableKeyword(keyword);
		else
			Shader.DisableKeyword(keyword);
	}
	public static void SetKeyword (this Material m, string keyword, bool state) {
		if (state)
			m.EnableKeyword(keyword);
		else
			m.DisableKeyword(keyword);
	}
}

