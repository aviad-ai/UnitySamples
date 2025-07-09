//    LowHealthDirectAccess


using UnityEngine;

namespace Leguar.LowHealth {

	/// <summary>
	/// This class provides direct access to Low Health (shader) effects.
	/// 
	/// Attach this script to the camera where you want low health effects to be displayed and then use provided methods in this class to set the effects.
	/// If you change the fields on this class directly, call UpdateShaderProperties() afterwards to enable any new values.
	/// 
	/// Note that this class gives just simple, direct control to all low health shader effects. For more automation, you may want to use LowHealthController.
	/// </summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	public class LowHealthDirectAccess : MonoBehaviour {

		private static readonly int SP_VLOSS = Shader.PropertyToID("_VLoss");
		private static readonly int SP_DLOSS = Shader.PropertyToID("_DLoss");
		private static readonly int SP_CLOSS = Shader.PropertyToID("_CLoss");
		private static readonly int SP_CLRED = Shader.PropertyToID("_CLRed");
		private static readonly int SP_DVISION = Shader.PropertyToID("_DVision");

		public float visionLossEffect;
		public float detailLossEffect;
		public float colorLossEffect;
		public float colorLossTowardRed;
		public float doubleVisionEffect;

		private Material lhMaterial;
		
		void Reset() {
			if (!Application.isPlaying) {
				ResetToDefaultValues();
			}
		}

		void Start() {
			if (Application.isPlaying) {
				UpdateShaderProperties();
			}
		}

		void OnRenderImage(RenderTexture source, RenderTexture destination) {
			if (ensureMaterial()) {
				UpdateShaderProperties();
			}
			Graphics.Blit(source, destination, lhMaterial);
		}

		/// <summary>
		/// Reset all fields in this class to their default zero values.
		/// </summary>
		public void ResetToDefaultValues() {
			visionLossEffect = 0f;
			detailLossEffect = 0f;
			colorLossEffect = 0f;
			colorLossTowardRed = 0f;
			doubleVisionEffect = 0f;
			UpdateShaderProperties();
		}

		/// <summary>
		/// Set amount of vision loss effect.
		/// </summary>
		/// <param name="visionLossEffect">
		/// Value between 0 and 1 inclusive.
		/// </param>
		public void SetVisionLossEffect(float visionLossEffect) {
			this.visionLossEffect = Mathf.Clamp01(visionLossEffect);
			UpdateShaderProperties();
		}

		/// <summary>
		/// Set amount of detail loss effect.
		/// </summary>
		/// <param name="detailLossEffect">
		/// Value between 0 and 1 inclusive.
		/// </param>
		public void SetDetailLossEffect(float detailLossEffect) {
			this.detailLossEffect = Mathf.Clamp01(detailLossEffect);
			UpdateShaderProperties();
		}

		/// <summary>
		/// Set amount of color and contrast loss effect.
		/// </summary>
		/// <param name="colorLossEffect">
		/// Value between 0 and 1 inclusive.
		/// </param>
		/// <param name="colorLossTowardRed">
		/// Value between 0 and 1 inclusive, 1 being maximum red.
		/// </param>
		public void SetColorLossEffect(float colorLossEffect, float colorLossTowardRed) {
			this.colorLossEffect = Mathf.Clamp01(colorLossEffect);
			this.colorLossTowardRed = Mathf.Clamp01(colorLossTowardRed);
			UpdateShaderProperties();
		}

		/// <summary>
		/// Set amount of double vision effect.
		/// </summary>
		/// <param name="doubleVisionEffect">
		/// Value between 0 and 1 inclusive.
		/// </param>
		public void SetDoubleVisionEffect(float doubleVisionEffect) {
			this.doubleVisionEffect = Mathf.Clamp01(doubleVisionEffect);
			UpdateShaderProperties();
		}

		/// <summary>
		/// Update shader properties. This is typically called automatically whenever needed.
		/// </summary>
		public void UpdateShaderProperties() {
			ensureMaterial();
			updateShaderProperties(lhMaterial, visionLossEffect, detailLossEffect, colorLossEffect, colorLossTowardRed, doubleVisionEffect*doubleVisionEffect);
		}

		internal static void updateShaderProperties(Material targetMaterial, float vLoss, float dLoss, float cLoss, float clRed, float dVision) {
			targetMaterial.SetFloat(SP_VLOSS, vLoss*0.9f);
			targetMaterial.SetFloat(SP_DLOSS, dLoss*0.1f);
			targetMaterial.SetFloat(SP_CLOSS, cLoss);
			targetMaterial.SetFloat(SP_CLRED, clRed);
			targetMaterial.SetFloat(SP_DVISION, dVision*0.15f);
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
