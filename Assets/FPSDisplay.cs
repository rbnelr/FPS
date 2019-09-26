using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;

public class FPSDisplay : MonoBehaviour {

	public int AvgFramesCount = 10;
	float[] dts;
	int cur = 0;

	float avg = 0f;

	private void Update () {
		if (dts == null || dts.Length != AvgFramesCount) {
			dts = new float[AvgFramesCount];
			for (int i=0; i<dts.Length; ++i)
				dts[i] = 0.2f;
			cur = 0;
		}

		dts[cur++] = Time.unscaledDeltaTime;
		cur %= dts.Length;
		
		avg = 0;
		for (int i=0; i<dts.Length; ++i)
			avg += dts[i];

		avg /= dts.Length;
	}

	void OnGUI () {
		GUI.Label(new Rect(float2(2, 20), float2(300, 20)), string.Format("{0:0000.0} fps ({1:000.00} ms)", 1f / avg, avg * 1000)); //change cur1 between two or three rects and continuously increment cur2 to get an illusion of dialogue.
	}
}
