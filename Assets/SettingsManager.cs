using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using System.Collections.Generic;
using System.Linq;

public class SettingsManager : MonoBehaviour {
	[Header("Graphics Settings")]
		
	public bool VSync = false;

	[Tooltip("-1 = no target (unlimited on pc)")]
	public int TargetFramerate = 200;

	public bool PotatoMode = false;

	bool prevVSync = false;
	int prevTargetFramerate = 200;
	bool prevPotatoMode = false;
	
	void Start () {
		GetMaterials();

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

	void OnDestroy () {
		ResetPotatoMode();
	}

	Material[] mats;
	Shader standardShader, potatoShader, standartRoughnessShader;

	Dictionary<Material, Shader> originalShaders = new Dictionary<Material, Shader>();

	void GetMaterials () {
		mats = Resources.LoadAll<Material>("Asset");
		standardShader = Shader.Find("Standard");
		potatoShader = Shader.Find("Custom/PotatoModeShader");
		standartRoughnessShader = Shader.Find("Custom/StandartRoughness");

		originalShaders = mats.ToDictionary(x => x, x => x.shader);
	}

	void SetPotatoMode (bool on) {
		foreach (var m in originalShaders) {
			m.Key.shader = on ? potatoShader : m.Value;
		}
	}

	
	void ResetPotatoMode () {
		foreach (var m in originalShaders) {
			m.Key.shader = m.Value;
		}
	}
}
