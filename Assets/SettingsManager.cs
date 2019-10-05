using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;

public class SettingsManager : MonoBehaviour {
	[System.Serializable]
	public class GraphicsSettings {
		
		[Header("Graphics Settings")]
		
		public bool VSync = false;

		[Tooltip("-1 = no target (unlimited on pc)")]
		public int TargetFramerate = 200; // 

		public void Update (GraphicsSettings prev=null) {
			if (prev == null || prev.VSync != VSync)
				QualitySettings.vSyncCount = VSync ? 1 : 0;
			if (prev != null) prev.VSync = VSync;
			
			if (prev == null || prev.TargetFramerate != TargetFramerate)
				Application.targetFrameRate = TargetFramerate;
			if (prev != null) prev.TargetFramerate = TargetFramerate;

		}
	}

	public GraphicsSettings Graphics = new GraphicsSettings();
	GraphicsSettings prevGraphics = new GraphicsSettings();
	
	void Start () {
		Graphics.Update();
	}
	void Update () {
		Graphics.Update(prevGraphics);
	}
}
