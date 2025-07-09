//    LowHealthController


using UnityEngine;

namespace Leguar.LowHealth {

	/// <summary>
	/// Main class of Low Health effects.
	/// 
	/// Attach this script to the camera where you want low health effects to be displayed. Set desired values in Unity inspector window or by setting values toi fields of this
	/// class. Then use SetPlayerHealth() methods in this class to set the player's health runtime. Any wanted effects will take place smoothly and automatically.
	/// </summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	public class LowHealthController : MonoBehaviour {

		/// <summary>
		/// Vision loss enabled or disabled.
		/// </summary>
		public bool visionLossEnabled;
		/// <summary>
		/// Health level (from 0 to 1) where vision loss effect starts.
		/// </summary>
		public float visionLossStartsAt;
		/// <summary>
		/// Vision loss effect maximum amount when player health is 0.
		/// </summary>
		public float visionLossMaxEffect;
		/// <summary>
		/// Additional heartbeat effect. 0 to disable effect, 1 for maximum effect.
		/// </summary>
		public float visionLossHeartBeat;

		/// <summary>
		/// Detail loss enabled or disabled.
		/// </summary>
		public bool detailLossEnabled;
		/// <summary>
		/// Health level (from 0 to 1) where detail loss effect starts.
		/// </summary>
		public float detailLossStartsAt;
		/// <summary>
		/// Detail loss effect maximum amount when player health is 0.
		/// </summary>
		public float detailLossMaxEffect;

		/// <summary>
		/// Color loss enabled or disabled.
		/// </summary>
		public bool colorLossEnabled;
		/// <summary>
		/// Health level (from 0 to 1) where color loss effect starts.
		/// </summary>
		public float colorLossStartsAt;
		/// <summary>
		/// Health level (from 0 to 1) where screen starts to turn red instead of gray. 0 to disable effect.
		/// Setting this value equal or higher than 'colorLossStartsAt' makes color loss to turn red instead of gray from very beginning.
		/// </summary>
		public float colorLossTowardRedStartsAt;

		/// <summary>
		/// Double vision enabled or disabled.
		/// </summary>
		public bool doubleVisionEnabled;
		/// <summary>
		/// Health level (from 0 to 1) where double vision effect starts.
		/// </summary>
		public float doubleVisionStartsAt;
		/// <summary>
		/// Double vision effect maximum amount when player health is 0.
		/// </summary>
		public float doubleVisionMaxEffect;
		/// <summary>
		/// Additional waving effect to make double vision less static. 0 to disable effect, 1 for maximum effect.
		/// </summary>
		public float doubleVisionWaving;

		/// <summary>
		/// Stop any moving effects when player health is 0.
		/// </summary>
		public bool stopEffectsWhenZeroHealth;

		/// <summary>
		/// Set effects visible in editor. This value is used by Unity inspector and there's no need to access this value from code.
		/// </summary>
		public bool showInEditor;
		/// <summary>
		/// Simulated player health in editor. This value is used by Unity inspector and there's no need to access this value from code.
		/// </summary>
		public float simulateHealth;

		private Material lhMaterial;
		private float playerHealth = 1f;

		private bool smoothPlayerHealth;
		private float sphStart;
		private float sphEnd;
		private float sphTime;
		private float sphCounter;

		private float visionLossHeartBeatCounter;
		private float doubleVisionWaveCounter;

		void Reset() {
			if (!Application.isPlaying) {
				ResetToDefaultValues();
			}
		}

		void Start() {
			if (Application.isPlaying) {
				playerHealth=1f;
				UpdateShaderProperties();
			}
		}

		void Update() {
			if (Application.isPlaying) {
				bool needUpdate = false;
				if (smoothPlayerHealth) {
					sphCounter+=Time.deltaTime;
					if (sphCounter<sphTime) {
						float percent = sphCounter/sphTime; // 0 .. 1
						percent = Mathf.Sin(-Mathf.PI*0.5f+Mathf.PI*percent); // -1 .. 1
						percent = (percent+1f)*0.5f; // 0 .. 1
						internalSetPlayerHealthInstantly(sphStart+(sphEnd-sphStart)*percent);
					} else {
						internalSetPlayerHealthInstantly(sphEnd);
						smoothPlayerHealth = false;
					}
					needUpdate = true;
				}
				if (!stopEffectsWhenZeroHealth || playerHealth>0f) {
					if (doubleVisionEnabled && playerHealth<doubleVisionStartsAt && doubleVisionWaving>0f) {
						doubleVisionWaveCounter+=Time.deltaTime;
						needUpdate = true;
					}
					if (visionLossEnabled && playerHealth<visionLossStartsAt && visionLossHeartBeat>0f) {
						visionLossHeartBeatCounter+=Time.deltaTime;
						visionLossHeartBeatCounter-=Mathf.Floor(visionLossHeartBeatCounter);
						needUpdate = true;
					}
				}
				if (needUpdate) {
					UpdateShaderProperties();
				}
			}
		}

		void OnRenderImage(RenderTexture source, RenderTexture destination) {
			if (ensureMaterial()) {
				UpdateShaderProperties();
			}
			Graphics.Blit(source, destination, lhMaterial);
		}

		/// <summary>
		/// Set player health instantly. Any effect that should be visible with this health, will take place immediately.
		/// </summary>
		/// <param name="playerHealth">
		/// Player new health, value between 0.0 and 1.0. If value is less than 0 or more than 1, value is set to 0 or 1.
		/// </param>
		public void SetPlayerHealthInstantly(float playerHealth) {
			internalSetPlayerHealthInstantly(playerHealth);
			UpdateShaderProperties();
		}

		private void internalSetPlayerHealthInstantly(float playerHealth) {
			this.playerHealth = Mathf.Clamp01(playerHealth);
		}

		/// <summary>
		/// Set player health smoothly. All the effects will take place relatively slowly, giving nice transition between health states.
		/// </summary>
		/// <param name="playerHealth">
		/// Player new health, value between 0.0 and 1.0. If value is less than 0 or more than 1, value is set to 0 or 1.
		/// </param>
		/// <param name="seconds">
		/// Seconds how long it takes before all the effects are fully visible.
		/// </param>
		public void SetPlayerHealthSmoothly(float playerHealth, float seconds) {
			smoothPlayerHealth=true;
			sphStart=this.playerHealth;
			sphEnd=playerHealth;
			sphTime=seconds;
			sphCounter=0f;
		}

		/// <summary>
		/// Reset all fields in this class to their default values. Typically this is used only internally by Unity inspector.
		/// </summary>
		public void ResetToDefaultValues() {
			visionLossEnabled = true;
			visionLossStartsAt = 0.55f;
			visionLossMaxEffect = 0.6f;
			visionLossHeartBeat = 0f;
			detailLossEnabled = true;
			detailLossStartsAt = 0.45f;
			detailLossMaxEffect = 0.8f;
			colorLossEnabled = true;
			colorLossStartsAt = 0.65f;
			colorLossTowardRedStartsAt = 0.15f;
			doubleVisionEnabled = true;
			doubleVisionStartsAt = 0.25f;
			doubleVisionMaxEffect = 0.35f;
			doubleVisionWaving = 0.5f;
			stopEffectsWhenZeroHealth = true;
			showInEditor = false;
			simulateHealth = 1f;
			UpdateShaderProperties();
		}

		/// <summary>
		/// Update shader properties. This is typically called automatically by this script or Unity inspector whenever needed.
		/// </summary>
		public void UpdateShaderProperties() {

			float health = (Application.isPlaying ? playerHealth : (showInEditor ? simulateHealth : 1f));

			float vPercent = 0f;
			if (visionLossEnabled && health<visionLossStartsAt) {
				vPercent = (1f-(health/visionLossStartsAt))*visionLossMaxEffect;
				if (visionLossHeartBeat>0f) {
					float p = getHeartBeatCurve(visionLossHeartBeatCounter);
					vPercent -= (vPercent*p*visionLossHeartBeat*0.3f);
				}
			}

			float dlPercent = 0f;
			if (detailLossEnabled && health<detailLossStartsAt) {
				dlPercent = (1f-(health/detailLossStartsAt))*detailLossMaxEffect;
			}

			float clPercent = 0f;
			float clRed = 0f;
			if (colorLossEnabled && health<colorLossStartsAt) {
				clPercent = 1f-(health/colorLossStartsAt);
				if (health<colorLossTowardRedStartsAt) {
					clRed = 1f-(health/colorLossTowardRedStartsAt);
				}
			}

			float dvPercent = 0f;
			if (doubleVisionEnabled && health<doubleVisionStartsAt) {
				dvPercent = (1f-(health/doubleVisionStartsAt))*doubleVisionMaxEffect;
				dvPercent *= dvPercent;
				if (doubleVisionWaving>0f) {
					float wave = 1f-(Mathf.Sin(doubleVisionWaveCounter)+1f)*0.4f*doubleVisionWaving;
					dvPercent *= wave;
				}
			}

			ensureMaterial();
			LowHealthDirectAccess.updateShaderProperties(lhMaterial, vPercent, dlPercent, clPercent, clRed, dvPercent);

		}

		private float getHeartBeatCurve(float t) {
			if (t<0.1f) {
				return t*10f;
			} else {
				return (1f-((t-0.1f)/0.9f));
			}
		}

		public void internalEditorUpdate() {
			if (!Application.isPlaying && showInEditor && (!stopEffectsWhenZeroHealth || simulateHealth>0f)) {
				if (visionLossEnabled && visionLossHeartBeat>0f) {
					visionLossHeartBeatCounter+=Time.deltaTime;
					visionLossHeartBeatCounter-=Mathf.Floor(visionLossHeartBeatCounter);
					UpdateShaderProperties();
				}
				if (doubleVisionEnabled && doubleVisionWaving>0f) {
					doubleVisionWaveCounter+=Time.deltaTime;
					UpdateShaderProperties();
				}
			}
		}

		private bool ensureMaterial() {
			if (lhMaterial==null) {
				Shader lhShader = Shader.Find("Custom/LowHealthShader");
				lhMaterial = new Material(lhShader);
				return true;
			}
			return false;
		}

	}

}
