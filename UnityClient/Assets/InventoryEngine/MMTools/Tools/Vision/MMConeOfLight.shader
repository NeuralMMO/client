Shader "MoreMountains/ConeOfLight"
{
	Properties
	{
		_MainTex("Diffuse Texture", 2D) = "white" {}
		_Contrast("Contrast", Float) = 0.5
		_Color("Color", Color) = (1,1,1,1)
	}

	SubShader
	{
		Tags
		{
			"ForceNoShadowCasting" = "True"
			"Queue" = "Transparent"
			"RenderType" = "Transparent"
			"IgnoreProjector" = "True"
		}

		Pass
		{
			ZTest Always
			AlphaTest Greater 0.0
			Blend DstColor One
	
			CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				uniform sampler2D _MainTex;
				uniform float _Contrast;
				uniform float4 _Color;

				struct VertexInput
				{
					float4 vertex : POSITION;
					float4 uv : TEXCOORD0;
					float4 color : COLOR;
				};

				struct VertexOutput
				{
					float4 pos : SV_POSITION;
					float2 uv : TEXCOORD0;
					float4 color : COLOR;
				};

				VertexOutput vert(VertexInput input)
				{
					VertexOutput output;
					output.uv = input.uv;
					output.color = input.color;
					output.pos = UnityObjectToClipPos(input.vertex);
					return output;
				}

				float4 frag(VertexOutput input) : COLOR
				{
					float4 diffuse = tex2D(_MainTex, input.uv);
					diffuse.rgb = diffuse.rgb * _Color.rgb * input.color.rgb;
					diffuse.rgb *= diffuse.a * _Color.a * input.color.a;
					diffuse *= _Contrast;
					return float4(diffuse);
				}

			ENDCG
		}
	}
}