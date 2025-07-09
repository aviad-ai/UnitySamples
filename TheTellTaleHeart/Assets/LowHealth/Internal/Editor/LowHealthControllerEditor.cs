//    LowHealthController - Editor


using UnityEngine;
using UnityEditor;
#if UNITY_EDITOR

namespace Leguar.LowHealth.Internal {

	[CustomEditor(typeof(LowHealthController))]
	public class LowHealthControllerEditor : Editor {

		private SerializedProperty visionLossEnabled;
		private SerializedProperty visionLossStartsAt;
		private SerializedProperty visionLossMaxEffect;
		private SerializedProperty visionLossHeartBeat;

		private SerializedProperty detailLossEnabled;
		private SerializedProperty detailLossStartsAt;
		private SerializedProperty detailLossMaxEffect;

		private SerializedProperty colorLossEnabled;
		private SerializedProperty colorLossStartsAt;
		private SerializedProperty colorLossTowardRedStartsAt;

		private SerializedProperty doubleVisionEnabled;
		private SerializedProperty doubleVisionStartsAt;
		private SerializedProperty doubleVisionMaxEffect;
		private SerializedProperty doubleVisionWaving;

		private SerializedProperty stopEffectsWhenZeroHealth;

		private SerializedProperty showInEditor;
		private SerializedProperty simulateHealth;

		void OnEnable() {

			visionLossEnabled = serializedObject.FindProperty("visionLossEnabled");
			visionLossStartsAt = serializedObject.FindProperty("visionLossStartsAt");
			visionLossMaxEffect = serializedObject.FindProperty("visionLossMaxEffect");
			visionLossHeartBeat = serializedObject.FindProperty("visionLossHeartBeat");

			detailLossEnabled = serializedObject.FindProperty("detailLossEnabled");
			detailLossStartsAt = serializedObject.FindProperty("detailLossStartsAt");
			detailLossMaxEffect = serializedObject.FindProperty("detailLossMaxEffect");

			colorLossEnabled = serializedObject.FindProperty("colorLossEnabled");
			colorLossStartsAt = serializedObject.FindProperty("colorLossStartsAt");
			colorLossTowardRedStartsAt = serializedObject.FindProperty("colorLossTowardRedStartsAt");

			doubleVisionEnabled = serializedObject.FindProperty("doubleVisionEnabled");
			doubleVisionStartsAt = serializedObject.FindProperty("doubleVisionStartsAt");
			doubleVisionMaxEffect = serializedObject.FindProperty("doubleVisionMaxEffect");
			doubleVisionWaving = serializedObject.FindProperty("doubleVisionWaving");

			stopEffectsWhenZeroHealth = serializedObject.FindProperty("stopEffectsWhenZeroHealth");

			showInEditor = serializedObject.FindProperty("showInEditor");
			simulateHealth = serializedObject.FindProperty("simulateHealth");

			EditorApplication.update += editorUpdate;

		}

		void OnDisable() {

			EditorApplication.update -= editorUpdate;

		}

		public override void OnInspectorGUI() {

			serializedObject.Update();

			GUILayout.Space(15);

			visionLossEnabled.boolValue = GUILayout.Toggle(visionLossEnabled.boolValue, "Loss of vision");
			GUILayout.Space(5);
			EditorGUI.BeginDisabledGroup(!visionLossEnabled.boolValue);
			visionLossStartsAt.floatValue = floatField("Effect starts when health less than", visionLossStartsAt.floatValue, 0.05f, 1f);
			visionLossMaxEffect.floatValue = percentSlider("Effect maximum when health 0", visionLossMaxEffect.floatValue, 5, 100);
			visionLossHeartBeat.floatValue = percentSlider("Heartbeat effect", visionLossHeartBeat.floatValue, 0, 100);
			EditorGUI.EndDisabledGroup();

			GUILayout.Space(10);

			detailLossEnabled.boolValue = GUILayout.Toggle(detailLossEnabled.boolValue, "Loss of details");
			GUILayout.Space(5);
			EditorGUI.BeginDisabledGroup(!detailLossEnabled.boolValue);
			detailLossStartsAt.floatValue = floatField("Effect starts when health less than", detailLossStartsAt.floatValue, 0.05f, 1f);
			detailLossMaxEffect.floatValue = percentSlider("Effect maximum when health 0", detailLossMaxEffect.floatValue, 10, 100);
			EditorGUI.EndDisabledGroup();

			GUILayout.Space(10);

			colorLossEnabled.boolValue = GUILayout.Toggle(colorLossEnabled.boolValue, "Loss of colors");
			GUILayout.Space(5);
			EditorGUI.BeginDisabledGroup(!colorLossEnabled.boolValue);
			EditorGUI.BeginChangeCheck();
			colorLossStartsAt.floatValue = floatField("Effect starts when health less than", colorLossStartsAt.floatValue, 0f, 1f);
			if (EditorGUI.EndChangeCheck()) {
				if (colorLossStartsAt.floatValue<0.05f) {
					colorLossStartsAt.floatValue=0.05f;
				}
				if (colorLossStartsAt.floatValue<colorLossTowardRedStartsAt.floatValue) {
					colorLossTowardRedStartsAt.floatValue=colorLossStartsAt.floatValue;
				}
			}
			EditorGUI.BeginChangeCheck();
			colorLossTowardRedStartsAt.floatValue = floatField("Toward red when health less than", colorLossTowardRedStartsAt.floatValue, 0f, 1f);
			if (EditorGUI.EndChangeCheck()) {
				if (colorLossTowardRedStartsAt.floatValue>colorLossStartsAt.floatValue) {
					colorLossTowardRedStartsAt.floatValue=colorLossStartsAt.floatValue;
				}
			}
			EditorGUI.EndDisabledGroup();

			GUILayout.Space(10);

			doubleVisionEnabled.boolValue = GUILayout.Toggle(doubleVisionEnabled.boolValue, "Seeing double");
			GUILayout.Space(5);
			EditorGUI.BeginDisabledGroup(!doubleVisionEnabled.boolValue);
			doubleVisionStartsAt.floatValue = floatField("Effect starts when health less than", doubleVisionStartsAt.floatValue, 0.05f, 1f);
			doubleVisionMaxEffect.floatValue = percentSlider("Effect maximum when health 0", doubleVisionMaxEffect.floatValue, 1, 100);
			doubleVisionWaving.floatValue = percentSlider("Waving effect", doubleVisionWaving.floatValue, 0, 100);
			EditorGUI.EndDisabledGroup();

			GUILayout.Space(15);

			EditorGUI.BeginDisabledGroup((!visionLossEnabled.boolValue || visionLossHeartBeat.floatValue==0f) && (!doubleVisionEnabled.boolValue || doubleVisionWaving.floatValue==0f));
			stopEffectsWhenZeroHealth.boolValue = EditorGUILayout.Toggle("Stop beat/wave effects when health 0", stopEffectsWhenZeroHealth.boolValue);
			EditorGUI.EndDisabledGroup();

			GUILayout.Space(15);
			EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 1), Color.gray);
			GUILayout.Space(15);

			EditorGUI.BeginDisabledGroup(Application.isPlaying);
			bool oldShowInEditor = showInEditor.boolValue;
			showInEditor.boolValue = GUILayout.Toggle(showInEditor.boolValue, "Show in editor");
			GUILayout.Space(5);
			EditorGUI.BeginDisabledGroup(!showInEditor.boolValue);
			simulateHealth.floatValue = floatField("Player health", simulateHealth.floatValue, 0f, 1f);
			EditorGUI.EndDisabledGroup();
			EditorGUI.EndDisabledGroup();

			GUILayout.Space(10);

			serializedObject.ApplyModifiedProperties();

			if (showInEditor.boolValue || oldShowInEditor) {
				((LowHealthController)(target)).UpdateShaderProperties();
				UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
			}

		}

		private void editorUpdate() {
			((LowHealthController)(target)).internalEditorUpdate();
		}

		private float floatField(string label, float current, float min, float max) {
			return EditorGUILayout.Slider(label, current, min, max);
		}

		private float percentSlider(string label, float current, int min, int max) {
			return LowHealthDirectAccessEditor.percentSlider(label, current, min, max);
		}

	}

}
#endif