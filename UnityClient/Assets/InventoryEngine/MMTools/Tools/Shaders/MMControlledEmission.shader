Shader "MoreMountains/MMControlledEmission"
{
	Properties
	{
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		_DiffuseColor("DiffuseColor", Color) = (1,1,1,1)
		_Opacity("Opacity", Range( 0 , 1)) = 1
		[HDR]_EmissionColor("EmissionColor", Color) = (1,1,1,1)
		_EmissionForce("EmissionForce", Float) = 0
		[Toggle(_USEEMISSIONFRESNEL_ON)] _UseEmissionFresnel("UseEmissionFresnel", Float) = 0
		_EmissionFresnelBias("EmissionFresnelBias", Float) = 1
		_EmissionFresnelScale("EmissionFresnelScale", Float) = 1
		_EmissionFresnelPower("EmissionFresnelPower", Float) = 1
		[Toggle(_USEOPACITYFRESNEL_ON)] _UseOpacityFresnel("UseOpacityFresnel", Float) = 0
		[Toggle(_INVERTOPACITYFRESNEL_ON)] _InvertOpacityFresnel("InvertOpacityFresnel", Float) = 0
		_OpacityFresnelBias("OpacityFresnelBias", Float) = 1
		_OpacityFresnelScale("OpacityFresnelScale", Float) = 1
		_OpacityFresnelPower("OpacityFresnelPower", Float) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#pragma shader_feature _USEEMISSIONFRESNEL_ON
		#pragma shader_feature _USEOPACITYFRESNEL_ON
		#pragma shader_feature _INVERTOPACITYFRESNEL_ON
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
			float3 worldNormal;
		};

		uniform sampler2D _TextureSample0;
		uniform float4 _TextureSample0_ST;
		uniform float4 _DiffuseColor;
		uniform float _EmissionForce;
		uniform float4 _EmissionColor;
		uniform float _EmissionFresnelBias;
		uniform float _EmissionFresnelScale;
		uniform float _EmissionFresnelPower;
		uniform float _OpacityFresnelBias;
		uniform float _OpacityFresnelScale;
		uniform float _OpacityFresnelPower;
		uniform float _Opacity;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_TextureSample0 = i.uv_texcoord * _TextureSample0_ST.xy + _TextureSample0_ST.zw;
			o.Albedo = ( tex2D( _TextureSample0, uv_TextureSample0 ) * _DiffuseColor ).rgb;
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = i.worldNormal;
			float fresnelNdotV8 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode8 = ( _EmissionFresnelBias + _EmissionFresnelScale * pow( 1.0 - fresnelNdotV8, _EmissionFresnelPower ) );
			#ifdef _USEEMISSIONFRESNEL_ON
				float staticSwitch22 = fresnelNode8;
			#else
				float staticSwitch22 = 1.0;
			#endif
			o.Emission = ( _EmissionForce * _EmissionColor * staticSwitch22 ).rgb;
			float fresnelNdotV26 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode26 = ( _OpacityFresnelBias + _OpacityFresnelScale * pow( 1.0 - fresnelNdotV26, _OpacityFresnelPower ) );
			#ifdef _INVERTOPACITYFRESNEL_ON
				float staticSwitch31 = ( 1.0 - fresnelNode26 );
			#else
				float staticSwitch31 = fresnelNode26;
			#endif
			#ifdef _USEOPACITYFRESNEL_ON
				float staticSwitch27 = staticSwitch31;
			#else
				float staticSwitch27 = 1.0;
			#endif
			o.Alpha = ( staticSwitch27 * _Opacity * _DiffuseColor.a );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard alpha:fade keepalpha fullforwardshadows 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				float3 worldNormal : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.worldNormal = worldNormal;
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = IN.worldNormal;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}