Shader "MoreMountains/MMRipple"
{
	Properties
	{
		_RippleAlpha("Ripple Alpha", Float) = 1
		_RippleIntensity("Ripple Intensity", Float) = 1
		_Hue("Hue", Color) = (1, 1, 1, 1)
		_NormalMap("Normal Map", 2D) = "white" {}
		_Density("Soft Particles Factor", Range(0, 3)) = 1
	}

	SubShader
	{
		Tags{ "Queue" = "Transparent+1" "RenderType" = "Transparent" }
		Zwrite Off
		Blend SrcAlpha OneMinusSrcAlpha
		GrabPass { "_BackgroundTexture" }
		Pass
		{
			CGPROGRAM
			#include "UnityCG.cginc"

			#pragma multi_compile_particles
			#pragma fragment frag
			#pragma vertex vert

			float _RippleAlpha;
			float _RippleIntensity;
			fixed4 _Hue;
			sampler2D _BackgroundTexture;
			sampler2D _NormalMap;
			sampler2D_float _CameraDepthTexture;
			float _Density;

			struct v2f
			{
				float4 grabScreenPosition : TEXCOORD0;
				float4 position : SV_POSITION;
				fixed4 color : COLOR;
				float2 normalMap : TEXCOORD1;

				#ifdef SOFTPARTICLES_ON
					float4 computedScreenPosition : TEXCOORD2;
				#endif
			};

			v2f vert(appdata_full v)
			{
				v2f o;
				o.position = UnityObjectToClipPos(v.vertex);

				#ifdef SOFTPARTICLES_ON
					o.computedScreenPosition = ComputeScreenPos(o.position);
					COMPUTE_EYEDEPTH(o.computedScreenPosition.z);
				#endif

				o.grabScreenPosition = ComputeGrabScreenPos(o.position);

				o.color = v.color;
				o.normalMap = v.texcoord;

				return o;
			}

			half4 frag(v2f i) : SV_Target
			{
				#ifdef SOFTPARTICLES_ON
					float sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.computedScreenPosition)));
					float partZ = i.computedScreenPosition.z;
					float fade = saturate(_Density * (sceneZ - partZ));
					i.color.a *= fade;
				#endif

				half3 ripple = UnpackNormal(tex2D(_NormalMap, i.normalMap.xy));
				i.grabScreenPosition.xy += ripple.xy / ripple.z * _RippleIntensity * i.color.a;
				half4 backgroundColor = tex2Dproj(_BackgroundTexture, i.grabScreenPosition);
				_Hue.a = _RippleAlpha;
				return backgroundColor * _Hue;
			}
			ENDCG
		}

	}
	FallBack "Particle/AlphaBlended"
}
