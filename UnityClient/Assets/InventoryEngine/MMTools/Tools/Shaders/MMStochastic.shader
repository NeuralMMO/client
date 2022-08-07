Shader "MoreMountains/MMStochastic"
{
	Properties
	{
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_BumpMap("BumpMap", 2D) = "bump" {}
		_BumpScale("BumpScale", Float) = 1
		_Smoothness("Smoothness", 2D) = "white" {}
		[Toggle]_Stochastic("Stochastic", Float) = 0
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		#pragma exclude_renderers gles
		#pragma surface surf Standard fullforwardshadows
		#pragma target 3.0

		fixed4 _Color;

		sampler2D _MainTex;
		sampler2D _BumpMap;
		sampler2D _Smoothness;

		uniform float _BumpScale;

		uniform float _Stochastic;

		struct Input
		{
			float2 uv_MainTex;
		};

		UNITY_INSTANCING_BUFFER_START(Props)
		UNITY_INSTANCING_BUFFER_END(Props)

		float2 hash2D2D(float2 s)
		{
			return frac(sin(fmod(float2(dot(s, float2(127.1,311.7)), dot(s, float2(269.5,183.3))), 3.14159))*43758.5453);
		}

		float4 tex2DStochastic(sampler2D tex, float2 UV)
		{
			float4x3 BW_vx;
			float2 skewUV = mul(float2x2 (1.0 , 0.0 , -0.57735027 , 1.15470054), UV * 3.464);
			float2 vxID = float2 (floor(skewUV));
			float3 barycentric = float3 (frac(skewUV), 0);
			barycentric.z = 1.0 - barycentric.x - barycentric.y;

			BW_vx = ((barycentric.z > 0) ?
				float4x3(float3(vxID, 0), float3(vxID + float2(0, 1), 0), float3(vxID + float2(1, 0), 0), barycentric.zyx) :
				float4x3(float3(vxID + float2 (1, 1), 0), float3(vxID + float2 (1, 0), 0), float3(vxID + float2 (0, 1), 0), float3(-barycentric.z, 1.0 - barycentric.y, 1.0 - barycentric.x)));

			float2 dx = ddx(UV);
			float2 dy = ddy(UV);

			return mul(tex2D(tex, UV + hash2D2D(BW_vx[0].xy), dx, dy), BW_vx[3].x) +
					mul(tex2D(tex, UV + hash2D2D(BW_vx[1].xy), dx, dy), BW_vx[3].y) +
					mul(tex2D(tex, UV + hash2D2D(BW_vx[2].xy), dx, dy), BW_vx[3].z);
		}

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			float4 bumpSample;
			float4 albedoSample = 1;
			float4 smoothnessSample;

			if (_Stochastic)
			{
				albedoSample = tex2DStochastic(_MainTex, IN.uv_MainTex);
				bumpSample = tex2DStochastic(_BumpMap, IN.uv_MainTex);
				smoothnessSample = tex2DStochastic(_Smoothness, IN.uv_MainTex);
			}
			else
			{
				albedoSample = tex2D(_MainTex, IN.uv_MainTex);
				bumpSample = tex2D(_BumpMap, IN.uv_MainTex);
				smoothnessSample = tex2D(_Smoothness, IN.uv_MainTex);
			}

			o.Alpha = albedoSample.a;

			o.Albedo = albedoSample.rgb;
			o.Normal = UnpackScaleNormal(bumpSample, _BumpScale);
			o.Smoothness = smoothnessSample.r;
		}
		ENDCG
	}
	FallBack "Diffuse"
}