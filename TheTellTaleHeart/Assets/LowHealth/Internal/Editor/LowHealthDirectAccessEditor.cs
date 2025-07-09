//    LowHealthDirectControl - Editor


using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
namespace Leguar.LowHealth.Internal {

	[CustomEditor(typeof(LowHealthDirectAccess))]
	public class LowHealthDirectAccessEditor : Editor {

		private SerializedProperty visionLossEffect;
		private SerializedProperty detailLossEffect;
		private SerializedProperty colorLossEffect;
		private SerializedProperty colorLossTowardRed;
		private SerializedProperty doubleVisionEffect;

		void OnEnable() {

			visionLossEffect = serializedObject.FindProperty("visionLossEffect");
			detailLossEffect = serializedObject.FindProperty("detailLossEffect");
			colorLossEffect = serializedObject.FindProperty("colorLossEffect");
			colorLossTowardRed = serializedObject.FindProperty("colorLossTowardRed");
			doubleVisionEffect = serializedObject.FindProperty("doubleVisionEffect");

		}

		public override void OnInspectorGUI() {

			serializedObject.Update();

			GUILayout.Space(10);

			visionLossEffect.floatValue = percentSlider("Loss of vision", visionLossEffect.floatValue, 0, 100);
			GUILayout.Space(5);
			detailLossEffect.floatValue = percentSlider("Loss of details", detailLossEffect.floatValue, 0, 100);
			GUILayout.Space(5);
			colorLossEffect.floatValue = percentSlider("Loss of colors", colorLossEffect.floatValue, 0, 100);
			EditorGUI.BeginDisabledGroup(colorLossEffect.floatValue<=0f);
			colorLossTowardRed.floatValue = percentSlider("Color loss toward red", colorLossTowardRed.floatValue, 0, 100);
			EditorGUI.EndDisabledGroup();
			GUILayout.Space(5);
			doubleVisionEffect.floatValue = percentSlider("Seeing double", doubleVisionEffect.floatValue, 0, 100);

			GUILayout.Space(10);

			serializedObject.ApplyModifiedProperties();

			((LowHealthDirectAccess)(target)).UpdateShaderProperties();
			UnityEditorInternal.InternalEditorUtility.RepaintAllViews();

		}

		internal static float percentSlider(string label, float current, int min, int max) {

			GUILayout.BeginHorizontal();
			int oldValue = (int)(current*100f+0.5f);
			int newValue = EditorGUILayout.IntSlider(label, oldValue, min, max);
			GUILayout.Label("%");
			if (newValue!=oldValue) {
				return (newValue/100f);
			}
			GUILayout.EndHorizontal();
			return current;

		}

	}

}
#endif
