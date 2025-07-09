//  Custom Shader - Low Health


Shader "Custom/LowHealthShader" {
	
	Properties {
		_MainTex("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (1,1,1,1)
		_VLoss("VLoss", Range(0.0, 0.9)) = 0.0
		_CLoss("CLoss", Range(0.0, 1.0)) = 0.0
		_CLRed("CLRed", Range(0.0, 1.0)) = 0.0
		_DLoss("DLoss", Range(0.0, 0.1)) = 0.0
		_DVision("DVision", Range(0.0, 0.15)) = 0.0
	}

	SubShader {

		Pass {

			Cull Off
			ZTest Always
			ZWrite Off

			CGPROGRAM

			#pragma vertex vert_img
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _Color;
			float _VLoss;
			float _CLoss;
			float _CLRed;
			float _DLoss;
			float _DVision;

			float4 frag(v2f_img input) : COLOR {

				float4 color = tex2D(_MainTex, input.uv);

				if (_DLoss>0) {

					float4 total = float4(0,0,0,0);
					int count = 0;
					int yd = 1;
					const float gm = 0.1*_DLoss;
					for (float x=-1.0; x<1.01; x+=0.1) {
						const float y = sqrt(1.01-abs(x))*yd;
						if (_DVision<=0.0) {
							total += tex2D(_MainTex, float2(input.uv.x+x*gm, input.uv.y+y*gm));
						} else {
							const float4 p1 = tex2D(_MainTex, float2(input.uv.x+x*gm - _DVision, input.uv.y+y*gm));
							const float4 p2 = tex2D(_MainTex, float2(input.uv.x+x*gm + _DVision, input.uv.y+y*gm));
							total += (p1+p2)/2;
						}
						count++;
						yd = -yd;
					}
					color = total/count;

				} else if (_DVision>0) {
					
					const float4 p1 = tex2D(_MainTex, float2(input.uv.x - _DVision, input.uv.y));
					const float4 p2 = tex2D(_MainTex, float2(input.uv.x + _DVision, input.uv.y));
					color = (p1+p2)/2;

				}

				if (_CLoss>0) {

					float br = color.r*0.32 + color.g*0.46 + color.b*0.22;
					const float4 bw = float4(br,br*(1-_CLRed),br*(1-_CLRed),1);
					const float f = 1-(1-_CLoss)*(1-_CLoss);
					color = color*(1-f) + bw*f;

				}

				if (_VLoss>0) {
					
					const float vCenter = clamp((_VLoss-0.5)*2,0,1);
					const float vCorner = clamp(_VLoss*2,0,1);
					const float maxDistFromCenter = distance(float2(0.0, 0.0), float2(0.5, 0.5));
					const float screenDistFromCenter = distance(input.uv.xy, float2(0.5, 0.5));
					const float distFromCenter = screenDistFromCenter/maxDistFromCenter;
					const float _VLoss = vCenter*(1-distFromCenter)+vCorner*distFromCenter;
					color *= (1-_VLoss);

				}

				return color;

			}

			ENDCG

		}

	}

}
