Shader "MoreMountains/MMAdvancedToon"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		[Header(Albedo)]_MainTex("MainTex", 2D) = "white" {}
		_Tint("Tint", Color) = (1,1,1,1)
		[Header(Normal Map)]_Normal("Normal", 2D) = "bump" {}
		[Header(Ramp Texture)][Toggle(_USERAMPTEXTURE_ON)] _UseRampTexture("UseRampTexture", Float) = 0
		_RampTexture("RampTexture", 2D) = "white" {}
		[Header(Generated Ramp)]_RampDark("RampDark", Color) = (0.3490566,0.3490566,0.3490566,0)
		_RampLight("RampLight", Color) = (1,1,1,0)
		_StepWidth("StepWidth", Range( 0.05 , 1)) = 0.25
		[IntRange]_StepAmount("StepAmount", Range( 0 , 16)) = 2
		_RampOffset("RampOffset", Range( 0 , 1)) = 0.5
		[Header(Vertex Colors)][Toggle(_USEVERTEXCOLORS_ON)] _UseVertexColors("UseVertexColors", Float) = 0
		[Header(Shadow)]_ShadowColor("ShadowColor", Color) = (1,0,0.115766,1)
		_LightColor("LightColor", Color) = (1,1,1,1)
		_ShadowBlur("ShadowBlur", Range( 0.01 , 1)) = 1
		_ShadowStrength("ShadowStrength", Range( 0 , 1)) = 1
		_ShadowSize("ShadowSize", Range( 0.01 , 1)) = 0.5
		[KeywordEnum(Multiply,Replace,Lighten,HardMix)] _ShadowMixMode("ShadowMixMode", Float) = 0
		[Header(Specular)][Toggle(_USESPECULAR_ON)] _UseSpecular("UseSpecular", Float) = 0
		_SpecularSize("SpecularSize", Range( 0 , 1)) = 0.4
		_SpecularFalloff("SpecularFalloff", Range( 0 , 2)) = 1
		[HDR]_SpecularColor("SpecularColor", Color) = (2,2,2,1)
		_SpecularPower("SpecularPower", Float) = 1
		_SpecularForceUnderShadow("SpecularForceUnderShadow", Float) = 0
		[Header(Rim Light)][Toggle(_USERIMLIGHT_ON)] _UseRimLight("UseRimLight", Float) = 0
		_RimColor("RimColor", Color) = (0,0.7342432,1,1)
		_RimPower("RimPower", Range( 0 , 1)) = 0.6547081
		_RimAmount("RimAmount", Range( 0 , 1)) = 0.7
		[Toggle(_HIDERIMUNDERSHADOW_ON)] _HideRimUnderShadow("HideRimUnderShadow", Float) = 0
		[Toggle(_SHARPRIMLIGHT_ON)] _SharpRimLight("SharpRimLight", Float) = 1
		[Header(Emission)]_EmissionTexture("EmissionTexture", 2D) = "white" {}
		[HDR]_EmissionColor("EmissionColor", Color) = (2,2,2,1)
		_EmissionForce("EmissionForce", Float) = 0
		[Header(Animation)]_Framerate("Framerate", Float) = 5
		[Header(VertexOffset)][Toggle(_USEVERTEXOFFSET_ON)] _UseVertexOffset("UseVertexOffset", Float) = 0
		_VertexOffsetNoiseTexture("VertexOffsetNoiseTexture", 2D) = "white" {}
		_VertexOffsetFrequency("VertexOffsetFrequency", Float) = 2
		_VertexOffsetMagnitude("VertexOffsetMagnitude", Float) = 0.05
		_VertexOffsetX("VertexOffsetX", Float) = 0.5
		_VertexOffsetY("VertexOffsetY", Float) = 0.5
		_VertexOffsetZ("VertexOffsetZ", Float) = 0.5
		[Header(Outline)]_OutlineColor("OutlineColor", Color) = (0.5451996,1,0,1)
		_OutlineWidth("OutlineWidth", Float) = 0.1
		_OutlineAlpha("OutlineAlpha", Range( 0 , 1)) = 0
		[Header(SecondaryTexture)]_SecondaryTexture("SecondaryTexture", 2D) = "white" {}
		_SecondaryTextureStrength("SecondaryTextureStrength", Float) = 0
		_SecondaryTextureSize("SecondaryTextureSize", Float) = 1
		_SecondaryTextureSpeedFactor("SecondaryTextureSpeedFactor", Float) = 0
		[Header(ToneMapping)]_Desaturation("Desaturation", Range( 0 , 1)) = 0
		_Contrast("Contrast", Range( -1 , 0.99)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0"}
		Cull Front
		CGPROGRAM
		#pragma target 3.0
		#pragma surface outlineSurf Outline nofog  keepalpha noshadow noambient novertexlights nolightmap nodynlightmap nodirlightmap nometa noforwardadd vertex:outlineVertexDataFunc 
		void outlineVertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float outlineVar = _OutlineWidth;
			v.vertex.xyz += ( v.normal * outlineVar );
		}
		inline half4 LightingOutline( SurfaceOutput s, half3 lightDir, half atten ) { return half4 ( 0,0,0, s.Alpha); }
		void outlineSurf( Input i, inout SurfaceOutput o )
		{
			o.Emission = _OutlineColor.rgb;
			clip( _OutlineAlpha - _Cutoff );
		}
		ENDCG
		

		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#pragma shader_feature_local _USEVERTEXOFFSET_ON
		#pragma shader_feature_local _SHADOWMIXMODE_MULTIPLY _SHADOWMIXMODE_REPLACE _SHADOWMIXMODE_LIGHTEN _SHADOWMIXMODE_HARDMIX
		#pragma shader_feature_local _USESPECULAR_ON
		#pragma shader_feature_local _USERAMPTEXTURE_ON
		#pragma shader_feature_local _USEVERTEXCOLORS_ON
		#pragma shader_feature_local _USERIMLIGHT_ON
		#pragma shader_feature_local _SHARPRIMLIGHT_ON
		#pragma shader_feature_local _HIDERIMUNDERSHADOW_ON
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float2 uv_texcoord;
			float3 vertexToFrag80;
			float3 worldPos;
			float4 vertexColor : COLOR;
			float3 worldNormal;
			INTERNAL_DATA
		};

		struct SurfaceOutputCustomLightingCustom
		{
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half Metallic;
			half Smoothness;
			half Occlusion;
			half Alpha;
			Input SurfInput;
			UnityGIInput GIData;
		};

		uniform float _VertexOffsetMagnitude;
		uniform sampler2D _VertexOffsetNoiseTexture;
		uniform float _Framerate;
		uniform float _VertexOffsetFrequency;
		uniform float _VertexOffsetX;
		uniform float _VertexOffsetY;
		uniform float _VertexOffsetZ;
		uniform sampler2D _EmissionTexture;
		uniform float4 _EmissionTexture_ST;
		uniform float4 _EmissionColor;
		uniform float _EmissionForce;
		uniform float4 _RampDark;
		uniform float4 _RampLight;
		uniform sampler2D _Normal;
		uniform float4 _Normal_ST;
		uniform float _StepWidth;
		uniform float _StepAmount;
		uniform float _RampOffset;
		uniform sampler2D _RampTexture;
		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		uniform float4 _Tint;
		uniform sampler2D _SecondaryTexture;
		uniform float _SecondaryTextureSize;
		uniform float _SecondaryTextureSpeedFactor;
		uniform float _SecondaryTextureStrength;
		uniform float _SpecularPower;
		uniform float _SpecularSize;
		uniform float _SpecularFalloff;
		uniform float4 _ShadowColor;
		uniform float _ShadowStrength;
		uniform float4 _LightColor;
		uniform float _ShadowSize;
		uniform float _ShadowBlur;
		uniform float _SpecularForceUnderShadow;
		uniform float4 _SpecularColor;
		uniform float _RimAmount;
		uniform float _RimPower;
		uniform float4 _RimColor;
		uniform float _Desaturation;
		uniform float _Contrast;
		uniform float _OutlineWidth;
		uniform float4 _OutlineColor;
		uniform float _OutlineAlpha;
		uniform float _Cutoff = 0.5;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float4 temp_cast_0 = (0.0).xxxx;
			half steppedTime293 = ( round( ( _Time.y * _Framerate ) ) / _Framerate );
			float3 ase_vertex3Pos = v.vertex.xyz;
			float3 temp_output_281_0 = ( ase_vertex3Pos * _VertexOffsetFrequency );
			half2 vertexOffsetXUV302 = ( steppedTime293 + (temp_output_281_0).xy );
			half2 vertexOffsetYUV303 = ( ( steppedTime293 * 2.0 ) + (temp_output_281_0).yz );
			half2 vertexOffsetZUV304 = ( ( steppedTime293 * 4.0 ) + (temp_output_281_0).xz );
			float4 appendResult308 = (float4(( tex2Dlod( _VertexOffsetNoiseTexture, float4( vertexOffsetXUV302, 0, 0.0) ).r - _VertexOffsetX ) , ( tex2Dlod( _VertexOffsetNoiseTexture, float4( vertexOffsetYUV303, 0, 0.0) ).r - _VertexOffsetY ) , ( tex2Dlod( _VertexOffsetNoiseTexture, float4( vertexOffsetZUV304, 0, 0.0) ).r - _VertexOffsetZ ) , 0.0));
			#ifdef _USEVERTEXOFFSET_ON
				float4 staticSwitch350 = ( _VertexOffsetMagnitude * appendResult308 );
			#else
				float4 staticSwitch350 = temp_cast_0;
			#endif
			float3 vertexOffset311 = (staticSwitch350).xyz;
			float3 outline364 = 0;
			v.vertex.xyz += ( vertexOffset311 + outline364 );
			float2 uv_Normal = v.texcoord * _Normal_ST.xy + _Normal_ST.zw;
			float3 normal83 = UnpackNormal( tex2Dlod( _Normal, float4( uv_Normal, 0, 0.0) ) );
			float3 ase_worldNormal = UnityObjectToWorldNormal( v.normal );
			float3 ase_worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
			float3x3 tangentToWorld = CreateTangentToWorldPerVertex( ase_worldNormal, ase_worldTangent, v.tangent.w );
			float3 tangentNormal33 = normal83;
			float3 modWorldNormal33 = normalize( (tangentToWorld[0] * tangentNormal33.x + tangentToWorld[1] * tangentNormal33.y + tangentToWorld[2] * tangentNormal33.z) );
			o.vertexToFrag80 = modWorldNormal33;
		}

		inline half4 LightingStandardCustomLighting( inout SurfaceOutputCustomLightingCustom s, half3 viewDir, UnityGI gi )
		{
			UnityGIInput data = s.GIData;
			Input i = s.SurfInput;
			half4 c = 0;
			#ifdef UNITY_PASS_FORWARDBASE
			float ase_lightAtten = data.atten;
			if( _LightColor0.a == 0)
			ase_lightAtten = 0;
			#else
			float3 ase_lightAttenRGB = gi.light.color / ( ( _LightColor0.rgb ) + 0.000001 );
			float ase_lightAtten = max( max( ase_lightAttenRGB.r, ase_lightAttenRGB.g ), ase_lightAttenRGB.b );
			#endif
			#if defined(HANDLE_SHADOWS_BLENDING_IN_GI)
			half bakedAtten = UnitySampleBakedOcclusion(data.lightmapUV.xy, data.worldPos);
			float zDist = dot(_WorldSpaceCameraPos - data.worldPos, UNITY_MATRIX_V[2].xyz);
			float fadeDist = UnityComputeShadowFadeDistance(data.worldPos, zDist);
			ase_lightAtten = UnityMixRealtimeAndBakedShadows(data.atten, bakedAtten, UnityComputeShadowFade(fadeDist));
			#endif
			float3 normalizeResult81 = normalize( i.vertexToFrag80 );
			float3 ase_worldPos = i.worldPos;
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = Unity_SafeNormalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			float dotResult34 = dot( normalizeResult81 , ase_worldlightDir );
			float NdotL31 = dotResult34;
			float4 lerpResult277 = lerp( _RampDark , _RampLight , saturate( (( floor( ( NdotL31 / _StepWidth ) ) / _StepAmount )*0.5 + _RampOffset) ));
			float2 temp_cast_1 = (saturate( (NdotL31*0.5 + 0.5) )).xx;
			#ifdef _USERAMPTEXTURE_ON
				float4 staticSwitch3 = tex2D( _RampTexture, temp_cast_1 );
			#else
				float4 staticSwitch3 = lerpResult277;
			#endif
			float4 ramp51 = staticSwitch3;
			#if defined(LIGHTMAP_ON) && ( UNITY_VERSION < 560 || ( defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN) ) )//aselc
			float4 ase_lightColor = 0;
			#else //aselc
			float4 ase_lightColor = _LightColor0;
			#endif //aselc
			float2 uv_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			float4 temp_cast_3 = (1.0).xxxx;
			#ifdef _USEVERTEXCOLORS_ON
				float4 staticSwitch7 = i.vertexColor;
			#else
				float4 staticSwitch7 = temp_cast_3;
			#endif
			half steppedTime293 = ( round( ( _Time.y * _Framerate ) ) / _Framerate );
			float4 lerpResult386 = lerp( ( ( tex2D( _MainTex, uv_MainTex ) * _Tint ) * staticSwitch7 ) , tex2D( _SecondaryTexture, ( ( i.uv_texcoord * _SecondaryTextureSize ) + ( steppedTime293 * _SecondaryTextureSpeedFactor ) ) ) , _SecondaryTextureStrength);
			float4 albedo11 = lerpResult386;
			float4 temp_output_73_0 = ( ( ramp51 * float4( ase_lightColor.rgb , 0.0 ) ) * albedo11 );
			float temp_output_120_0 = ( 1.0 - _SpecularSize );
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float dotResult106 = dot( ase_worldViewDir , ase_worldNormal );
			float2 uv_Normal = i.uv_texcoord * _Normal_ST.xy + _Normal_ST.zw;
			float3 normal83 = UnpackNormal( tex2D( _Normal, uv_Normal ) );
			float dotResult110 = dot( ase_worldViewDir , -reflect( ase_worldlightDir , (WorldNormalVector( i , normal83 )) ) );
			float specular113 = ( pow( dotResult106 , _SpecularFalloff ) * dotResult110 );
			float specularDelta116 = fwidth( specular113 );
			float smoothstepResult121 = smoothstep( temp_output_120_0 , ( temp_output_120_0 + specularDelta116 ) , specular113);
			float temp_output_2_0_g2 = _ShadowStrength;
			float temp_output_3_0_g2 = ( 1.0 - temp_output_2_0_g2 );
			float3 appendResult7_g2 = (float3(temp_output_3_0_g2 , temp_output_3_0_g2 , temp_output_3_0_g2));
			float clampResult189 = clamp( ase_lightAtten , 0.0 , 1.0 );
			float lerpResult409 = lerp( clampResult189 , step( _ShadowSize , clampResult189 ) , _ShadowBlur);
			float temp_output_191_0 = pow( lerpResult409 , _ShadowBlur );
			float4 lerpResult194 = lerp( float4( ( ( _ShadowColor.rgb * temp_output_2_0_g2 ) + appendResult7_g2 ) , 0.0 ) , _LightColor , temp_output_191_0);
			float4 shadow195 = lerpResult194;
			float4 temp_cast_7 = (_SpecularForceUnderShadow).xxxx;
			float4 temp_output_274_0 = round( pow( max( shadow195 , float4( 0.9528302,0.9528302,0.9528302,0 ) ) , temp_cast_7 ) );
			float4 specularIntensity124 = ( ( _SpecularPower * smoothstepResult121 ) * temp_output_274_0 );
			float4 temp_output_131_0 = ( specular113 * _SpecularColor * saturate( specularIntensity124 ) );
			float4 computedSpecular133 = temp_output_131_0;
			#ifdef _USESPECULAR_ON
				float4 staticSwitch137 = ( ( temp_output_73_0 * ( 1.0 - specularIntensity124 ) ) + computedSpecular133 );
			#else
				float4 staticSwitch137 = temp_output_73_0;
			#endif
			float4 litColor422 = staticSwitch137;
			float shadowArea411 = temp_output_191_0;
			float4 blendOpSrc410 = litColor422;
			float4 blendOpDest410 = shadow195;
			float4 blendOpSrc430 = litColor422;
			float4 blendOpDest430 = shadow195;
			#if defined(_SHADOWMIXMODE_MULTIPLY)
				float4 staticSwitch420 = ( litColor422 * shadow195 );
			#elif defined(_SHADOWMIXMODE_REPLACE)
				float4 staticSwitch420 = ( ( litColor422 * shadowArea411 ) + ( shadow195 * ( 1.0 - shadowArea411 ) ) );
			#elif defined(_SHADOWMIXMODE_LIGHTEN)
				float4 staticSwitch420 = ( saturate( 	max( blendOpSrc410, blendOpDest410 ) ));
			#elif defined(_SHADOWMIXMODE_HARDMIX)
				float4 staticSwitch420 = ( saturate(  round( 0.5 * ( blendOpSrc430 + blendOpDest430 ) ) ));
			#else
				float4 staticSwitch420 = ( litColor422 * shadow195 );
			#endif
			float4 shadowMix435 = staticSwitch420;
			float4 temp_cast_8 = (0.0).xxxx;
			float rimAmount169 = _RimAmount;
			float dotResult89 = dot( (WorldNormalVector( i , normal83 )) , ase_worldViewDir );
			float NdotV90 = dotResult89;
			#ifdef _HIDERIMUNDERSHADOW_ON
				float staticSwitch166 = NdotL31;
			#else
				float staticSwitch166 = 1.0;
			#endif
			float temp_output_148_0 = ( ( 1.0 - NdotV90 ) * pow( staticSwitch166 , _RimPower ) );
			float smoothstepResult150 = smoothstep( ( rimAmount169 - 0.01 ) , ( 0.01 + rimAmount169 ) , temp_output_148_0);
			#ifdef _SHARPRIMLIGHT_ON
				float staticSwitch168 = smoothstepResult150;
			#else
				float staticSwitch168 = ( rimAmount169 * temp_output_148_0 );
			#endif
			#ifdef _USERIMLIGHT_ON
				float4 staticSwitch164 = ( staticSwitch168 * _RimColor );
			#else
				float4 staticSwitch164 = temp_cast_8;
			#endif
			float4 rimLight157 = staticSwitch164;
			float4 preToneMapping438 = ( shadowMix435 + rimLight157 );
			float grayscale442 = Luminance(preToneMapping438.rgb);
			float4 temp_cast_10 = (grayscale442).xxxx;
			float4 lerpResult444 = lerp( preToneMapping438 , temp_cast_10 , _Desaturation);
			float4 temp_cast_11 = (_Contrast).xxxx;
			float4 postToneMapping439 = (float4( 0,0,0,0 ) + (lerpResult444 - temp_cast_11) * (float4( 1,1,1,0 ) - float4( 0,0,0,0 )) / (float4( 1,1,1,0 ) - temp_cast_11));
			float4 lightCol68 = postToneMapping439;
			c.rgb = lightCol68.rgb;
			c.a = 1;
			return c;
		}

		inline void LightingStandardCustomLighting_GI( inout SurfaceOutputCustomLightingCustom s, UnityGIInput data, inout UnityGI gi )
		{
			s.GIData = data;
		}

		void surf( Input i , inout SurfaceOutputCustomLightingCustom o )
		{
			o.SurfInput = i;
			o.Normal = float3(0,0,1);
			float2 uv_EmissionTexture = i.uv_texcoord * _EmissionTexture_ST.xy + _EmissionTexture_ST.zw;
			float4 computedEmission182 = ( ( tex2D( _EmissionTexture, uv_EmissionTexture ) * _EmissionColor ) * _EmissionForce );
			o.Emission = computedEmission182.rgb;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf StandardCustomLighting keepalpha fullforwardshadows exclude_path:deferred vertex:vertexDataFunc 

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
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float3 customPack2 : TEXCOORD2;
				float4 tSpace0 : TEXCOORD3;
				float4 tSpace1 : TEXCOORD4;
				float4 tSpace2 : TEXCOORD5;
				half4 color : COLOR0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				vertexDataFunc( v, customInputData );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.customPack2.xyz = customInputData.vertexToFrag80;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				o.color = v.color;
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
				surfIN.vertexToFrag80 = IN.customPack2.xyz;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				surfIN.vertexColor = IN.color;
				SurfaceOutputCustomLightingCustom o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputCustomLightingCustom, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}