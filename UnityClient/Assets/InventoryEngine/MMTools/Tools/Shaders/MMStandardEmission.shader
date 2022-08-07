Shader "MoreMountains/MMStandardEmission"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("MainTex", 2D) = "white" {}
		_MetallicGlossMap("MetallicGlossMap", 2D) = "gray" {}
		_Metallic("Metallic", Range( 0 , 1)) = 1
		_Glossiness("Glossiness", Range( 0 , 1)) = 0.5
		_BumpMap("BumpMap", 2D) = "bump" {}
		_BumpScale("BumpScale", Float) = 1
		_OcclusionMap("OcclusionMap", 2D) = "white" {}
		_OcclussionStrength("OcclussionStrength", Range( 0 , 1)) = 1
		[Enum(Off,0,On,1)][Header(Depth Blend)]_ZWrite("ZWrite", Range( 0 , 1)) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("ZTest", Range( 0 , 255)) = 167.5171
		[Enum(UnityEngine.Rendering.BlendMode)][Header(Blend Modes)]_BlendSrc("BlendSrc", Range( 0 , 255)) = 1
		[Enum(UnityEngine.Rendering.BlendMode)]_BlendDst("BlendDst", Range( 0 , 255)) = 10
		[Enum(UnityEngine.Rendering.CullMode)][Header(Cull)]_CullMode("CullMode", Range( 0 , 255)) = 0
		[Enum(UnityEngine.Rendering.ColorWriteMask)]_ColorMask("ColorMask", Range( 0 , 255)) = 255
		[IntRange][Header(Stencil)]_Stencil("Stencil", Range( 0 , 255)) = 0
		[IntRange]_StencilReadMask("StencilReadMask", Range( 0 , 255)) = 15
		[IntRange]_StencilWriteMask("StencilWriteMask", Range( 0 , 255)) = 15
		[Enum(UnityEngine.Rendering.CompareFunction)]_StencilComp("StencilComp", Range( 0 , 255)) = 0
		[Enum(UnityEngine.Rendering.StencilOp)]_StencilOpPassFront("StencilOpPassFront", Range( 0 , 255)) = 0
		[Enum(UnityEngine.Rendering.StencilOp)]_StencilOpFailFront("StencilOpFailFront", Range( 0 , 255)) = 0
		[Enum(UnityEngine.Rendering.StencilOp)]_StencilOpZFailFront("StencilOpZFailFront", Range( 0 , 255)) = 0
		_EmissionMap("EmissionMap", 2D) = "white" {}
		_EmissionColor("EmissionColor", Color) = (0,0,0,0)
		_EmissionIntensity("EmissionIntensity", Float) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull [_CullMode]
		ZWrite [_ZWrite]
		ZTest [_ZTest]
		Stencil
		{
			Ref [_Stencil]
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
			Comp [_StencilComp]
			Pass [_StencilOpPassFront]
			Fail [_StencilOpFailFront]
			ZFail [_StencilOpZFailFront]
		}
		CGPROGRAM
		#include "UnityStandardUtils.cginc"
		#pragma target 3.0
		#pragma exclude_renderers vulkan xbox360 xboxone ps4 psp2 n3ds wiiu 
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform half _ZTest;
		uniform half _StencilOpPassFront;
		uniform half _StencilComp;
		uniform float _StencilWriteMask;
		uniform float _StencilReadMask;
		uniform float _Stencil;
		uniform half _BlendSrc;
		uniform half _ZWrite;
		uniform half _CullMode;
		uniform half _BlendDst;
		uniform half _StencilOpFailFront;
		uniform half _ColorMask;
		uniform half _StencilOpZFailFront;
		uniform sampler2D _BumpMap;
		uniform float4 _BumpMap_ST;
		uniform float _BumpScale;
		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		uniform half4 _Color;
		uniform sampler2D _EmissionMap;
		uniform float4 _EmissionMap_ST;
		uniform float4 _EmissionColor;
		uniform float _EmissionIntensity;
		uniform sampler2D _MetallicGlossMap;
		uniform float4 _MetallicGlossMap_ST;
		uniform half _Metallic;
		uniform half _Glossiness;
		uniform sampler2D _OcclusionMap;
		uniform float4 _OcclusionMap_ST;
		uniform half _OcclussionStrength;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_BumpMap = i.uv_texcoord * _BumpMap_ST.xy + _BumpMap_ST.zw;
			o.Normal = UnpackScaleNormal( tex2D( _BumpMap, uv_BumpMap ), _BumpScale );
			float2 uv_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			o.Albedo = ( tex2D( _MainTex, uv_MainTex ) * _Color ).rgb;
			float2 uv_EmissionMap = i.uv_texcoord * _EmissionMap_ST.xy + _EmissionMap_ST.zw;
			half3 Emission49 = ( (tex2D( _EmissionMap, uv_EmissionMap )).rgb * (_EmissionColor).rgb * _EmissionIntensity );
			o.Emission = Emission49;
			float2 uv_MetallicGlossMap = i.uv_texcoord * _MetallicGlossMap_ST.xy + _MetallicGlossMap_ST.zw;
			float4 tex2DNode20 = tex2D( _MetallicGlossMap, uv_MetallicGlossMap );
			o.Metallic = ( tex2DNode20.r * _Metallic );
			o.Smoothness = ( tex2DNode20.a * _Glossiness );
			float2 uv_OcclusionMap = i.uv_texcoord * _OcclusionMap_ST.xy + _OcclusionMap_ST.zw;
			float lerpResult13 = lerp( tex2D( _OcclusionMap, uv_OcclusionMap ).g , 1.0 , _OcclussionStrength);
			o.Occlusion = lerpResult13;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
}