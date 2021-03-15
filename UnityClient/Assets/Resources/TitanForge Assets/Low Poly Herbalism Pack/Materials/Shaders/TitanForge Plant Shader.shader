// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "TitanForge/TitanForge Plant Wind Shader"
{
	Properties
	{
		_AlbedoColor("Albedo Color", Color) = (1,1,1,0)
		_TextureAlbedo("Texture Albedo", 2D) = "white" {}
		_StemColor("Stem Color", Color) = (0.3333333,0.4,0.2588235,1)
		_LeafColor("Leaf Color", Color) = (0.2784314,0.3294118,0.2117647,1)
		_WoodColor("Wood Color", Color) = (0.4196079,0.3960785,0.345098,1)
		_FlowerColor("Flower Color", Color) = (0.4196079,0.4588236,0.6470588,1)
		_SecondaryColor("Secondary Color", Color) = (0.9137256,0.8117648,0.482353,1)
		_FruitColor("Fruit Color", Color) = (0.4196079,0.3607843,0.5176471,1)
		_Metallic("Metallic", Range( 0 , 1)) = 0
		_Smoothness("Smoothness", Range( 0 , 1)) = 0
		_WindNoise("Wind Noise", 2D) = "white" {}
		_WindStrength("Wind Strength", Range( 0 , 1)) = 0.5
		_WindSpeed("Wind Speed", Range( 0 , 1)) = 1
		_WindDensity("Wind Density", Range( 0 , 1)) = 0.5
		_ColorMask("Color Mask", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Off
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			float3 worldPos;
			float2 uv_texcoord;
		};

		uniform sampler2D _WindNoise;
		SamplerState sampler_WindNoise;
		uniform float _WindSpeed;
		uniform float _WindDensity;
		uniform float _WindStrength;
		uniform sampler2D _ColorMask;
		uniform float4 _ColorMask_ST;
		uniform float4 _AlbedoColor;
		uniform sampler2D _TextureAlbedo;
		uniform float4 _TextureAlbedo_ST;
		uniform float4 _StemColor;
		uniform float4 _LeafColor;
		uniform float4 _WoodColor;
		uniform float4 _FruitColor;
		uniform float4 _SecondaryColor;
		uniform float4 _FlowerColor;
		SamplerState sampler_TextureAlbedo;
		uniform float _Metallic;
		uniform float _Smoothness;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float2 temp_cast_0 = (_WindSpeed).xx;
			float3 ase_vertex3Pos = v.vertex.xyz;
			float2 panner419 = ( 1.0 * _Time.y * temp_cast_0 + ase_vertex3Pos.xy);
			float3 appendResult389 = (float3(( (-1.0 + (tex2Dlod( _WindNoise, float4( ( panner419 / (5.0 + (_WindDensity - 0.0) * (2.0 - 5.0) / (1.0 - 0.0)) ), 0, 0.0) ).r - 0.0) * (1.0 - -1.0) / (1.0 - 0.0)) * (0.0 + (_WindStrength - 0.0) * (0.1 - 0.0) / (1.0 - 0.0)) ) , 0.0 , 0.0));
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float3 worldToObj379 = mul( unity_WorldToObject, float4( ( appendResult389 + ase_worldPos ), 1 ) ).xyz;
			v.vertex.xyz += ( ( worldToObj379 - ase_vertex3Pos ) * v.color.r );
			v.vertex.w = 1;
			float3 ase_vertexNormal = v.normal.xyz;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = UnityObjectToWorldNormal( v.normal );
			float dotResult439 = dot( ase_worldViewDir , ase_worldNormal );
			float3 lerpResult437 = lerp( ase_vertexNormal , -ase_vertexNormal , step( dotResult439 , 0.0 ));
			v.normal = lerpResult437;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 appendResult41 = (float2(( 1.0 - i.uv_texcoord.x ) , i.uv_texcoord.y));
			float4 tex2DNode35 = tex2D( _ColorMask, appendResult41 );
			float3 appendResult66 = (float3(tex2DNode35.r , tex2DNode35.g , tex2DNode35.b));
			float2 uv_ColorMask = i.uv_texcoord * _ColorMask_ST.xy + _ColorMask_ST.zw;
			float4 tex2DNode23 = tex2D( _ColorMask, uv_ColorMask );
			float3 appendResult63 = (float3(tex2DNode23.r , tex2DNode23.g , tex2DNode23.b));
			float2 uv_TextureAlbedo = i.uv_texcoord * _TextureAlbedo_ST.xy + _TextureAlbedo_ST.zw;
			float4 tex2DNode10 = tex2D( _TextureAlbedo, uv_TextureAlbedo );
			float3 layeredBlendVar64 = appendResult63;
			float4 layeredBlend64 = ( lerp( lerp( lerp( ( _AlbedoColor * tex2DNode10 ) , _StemColor , layeredBlendVar64.x ) , _LeafColor , layeredBlendVar64.y ) , _WoodColor , layeredBlendVar64.z ) );
			float3 layeredBlendVar46 = appendResult66;
			float4 layeredBlend46 = ( lerp( lerp( lerp( layeredBlend64 , _FruitColor , layeredBlendVar46.x ) , _SecondaryColor , layeredBlendVar46.y ) , _FlowerColor , layeredBlendVar46.z ) );
			float4 lerpResult70 = lerp( layeredBlend46 , ( 0.75 * layeredBlend46 ) , tex2DNode10.a);
			float4 lerpResult73 = lerp( ( 1.25 * lerpResult70 ) , lerpResult70 , tex2DNode35.a);
			o.Albedo = lerpResult73.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Smoothness;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18500
0;73;1199;597;1676.084;190.575;2.221322;False;False
Node;AmplifyShaderEditor.CommentaryNode;415;-1231.593,-163.6656;Inherit;False;1652.697;618.882;Wind Shader;19;382;380;238;381;379;377;378;389;383;416;395;230;386;414;419;394;233;305;303;;0.02313991,0.5501875,0.9811321,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;233;-1156.195,57.86858;Inherit;False;Property;_WindSpeed;Wind Speed;12;0;Create;True;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;305;-1089.772,-81.53368;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;303;-1160.51,128.7632;Inherit;False;Property;_WindDensity;Wind Density;13;0;Create;True;0;0;False;0;False;0.5;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;298;-1236.606,-1337.321;Inherit;False;1656.23;1160.917;Main Shader;33;82;73;14;421;72;71;70;68;67;46;66;64;49;47;48;17;27;424;5;35;28;63;10;4;41;423;23;40;29;21;425;432;433;;0.2063012,0.7169812,0.6481563,1;0;0
Node;AmplifyShaderEditor.TFHCRemapNode;394;-1104.277,207.1106;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;4;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;29;-373.3475,-1277.933;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;21;-1087.475,-1186.457;Inherit;True;Property;_ColorMask;Color Mask;14;0;Create;True;0;0;False;0;False;None;39b33f9ce6982a54782625120b8793b1;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.PannerNode;419;-884.2178,-48.4977;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ColorNode;4;-1099.9,-787.6169;Inherit;False;Property;_AlbedoColor;Albedo Color;0;0;Create;True;0;0;False;0;False;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;10;-1180.084,-618.6924;Inherit;True;Property;_TextureAlbedo;Texture Albedo;1;0;Create;True;0;0;False;0;False;-1;None;71bb9c414357cff48bf97523322b14f0;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;40;-120.1267,-1292.823;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;23;-1089.921,-992.3651;Inherit;True;Property;_TextureSample1;Texture Sample 1;5;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;5;-841.151,-684.6492;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;414;-871.5464,105.066;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;63;-822.2772,-792.5845;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WireNode;432;-733.0212,-576.1042;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;230;-730.8356,62.75815;Inherit;True;Property;_WindNoise;Wind Noise;10;0;Create;True;0;0;False;0;False;-1;None;5f40609515dce394499942997a139a37;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;41;-131.4708,-1197.811;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.WireNode;423;-442.032,-1098.2;Inherit;False;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RangedFloatNode;386;-915.951,255.3672;Inherit;False;Property;_WindStrength;Wind Strength;11;0;Create;True;0;0;False;0;False;0.5;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;28;-692.2144,-379.6064;Inherit;False;Property;_WoodColor;Wood Color;4;0;Create;True;0;0;False;0;False;0.4196079,0.3960785,0.345098,1;0.4196078,0.3960784,0.3450979,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;27;-698.2504,-544.3171;Inherit;False;Property;_LeafColor;Leaf Color;3;0;Create;True;0;0;False;0;False;0.2784314,0.3294118,0.2117647,1;0.2784313,0.3294117,0.2117646,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;17;-695.2267,-714.024;Inherit;False;Property;_StemColor;Stem Color;2;0;Create;True;0;0;False;0;False;0.3333333,0.4,0.2588235,1;0.3333333,0.3999999,0.2588235,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;35;-218.7913,-1088.088;Inherit;True;Property;_TextureSample0;Texture Sample 0;9;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;416;-616.7233,257.4389;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;433;-482.0212,-567.1042;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TFHCRemapNode;395;-395.3988,-41.07987;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-1;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;424;-452.3153,-737.4526;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ColorNode;49;-688.2158,-1288.879;Inherit;False;Property;_FruitColor;Fruit Color;7;0;Create;True;0;0;False;0;False;0.4196079,0.3607843,0.5176471,1;0.4196078,0.3607842,0.517647,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;48;-693.5111,-1093.419;Inherit;False;Property;_SecondaryColor;Secondary Color;6;0;Create;True;0;0;False;0;False;0.9137256,0.8117648,0.482353,1;0.9137256,0.8117648,0.4823529,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;66;-147.7894,-879.6899;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ColorNode;47;-689.1005,-927.948;Inherit;False;Property;_FlowerColor;Flower Color;5;0;Create;True;0;0;False;0;False;0.4196079,0.4588236,0.6470588,1;0.4196078,0.4588236,0.6470588,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;383;-383.7157,128.3686;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LayeredBlendNode;64;-391.1433,-580.1973;Inherit;False;6;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;67;36.64968,-718.2151;Inherit;False;Constant;_DarkValue;Dark Value;11;0;Create;True;0;0;False;0;False;0.75;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LayeredBlendNode;46;-176.8216,-743.2831;Inherit;False;6;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;443;-330.408,464.0925;Inherit;False;784.5191;427.8283;Two Sided Normals;8;442;441;440;439;434;436;438;437;;0.3957005,0.6320754,0.2832413,1;0;0
Node;AmplifyShaderEditor.WorldPosInputsNode;378;-403.6527,241.1223;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DynamicAppendNode;389;-192.6158,-7.031331;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldNormalVector;442;-280.4078,700.7341;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleAddOpNode;377;-185.1227,124.7781;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;68;62.17391,-631.6121;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;425;-678.658,-208.8494;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;441;-271.7271,539.3126;Inherit;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.LerpOp;70;60.59325,-526.032;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;71;202.9651,-668.8448;Inherit;False;Constant;_LightValue;Light Value;11;0;Create;True;0;0;False;0;False;1.25;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;381;-204.0645,228.8186;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TransformPositionNode;379;9.27459,-8.274001;Inherit;False;World;Object;False;Fast;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;440;-73.1091,776.9208;Inherit;False;Constant;_Float0;Float 0;15;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;439;-18.53114,677.8784;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NormalVertexDataNode;434;-95.94791,514.0925;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NegateNode;436;121.1274,593.5079;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WireNode;421;178.8468,-753.7033;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;72;242.3875,-578.7103;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.VertexColorNode;238;48.8146,239.2289;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StepOpNode;438;115.5039,677.3561;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;380;70.56199,141.7937;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;382;228.5959,139.9437;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;82;-39.2894,-310.4795;Inherit;False;Property;_Smoothness;Smoothness;9;0;Create;True;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;73;240.9874,-488.233;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;14;-36.05746,-391.2654;Inherit;False;Property;_Metallic;Metallic;8;0;Create;True;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;437;270.111,516.0878;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;459.8334,-418.055;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;TitanForge/TitanForge Plant Wind Shader;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;394;0;303;0
WireConnection;419;0;305;0
WireConnection;419;2;233;0
WireConnection;40;0;29;1
WireConnection;23;0;21;0
WireConnection;5;0;4;0
WireConnection;5;1;10;0
WireConnection;414;0;419;0
WireConnection;414;1;394;0
WireConnection;63;0;23;1
WireConnection;63;1;23;2
WireConnection;63;2;23;3
WireConnection;432;0;5;0
WireConnection;230;1;414;0
WireConnection;41;0;40;0
WireConnection;41;1;29;2
WireConnection;423;0;21;0
WireConnection;35;0;423;0
WireConnection;35;1;41;0
WireConnection;416;0;386;0
WireConnection;433;0;432;0
WireConnection;395;0;230;1
WireConnection;424;0;63;0
WireConnection;66;0;35;1
WireConnection;66;1;35;2
WireConnection;66;2;35;3
WireConnection;383;0;395;0
WireConnection;383;1;416;0
WireConnection;64;0;424;0
WireConnection;64;1;433;0
WireConnection;64;2;17;0
WireConnection;64;3;27;0
WireConnection;64;4;28;0
WireConnection;46;0;66;0
WireConnection;46;1;64;0
WireConnection;46;2;49;0
WireConnection;46;3;48;0
WireConnection;46;4;47;0
WireConnection;389;0;383;0
WireConnection;377;0;389;0
WireConnection;377;1;378;0
WireConnection;68;0;67;0
WireConnection;68;1;46;0
WireConnection;425;0;10;4
WireConnection;70;0;46;0
WireConnection;70;1;68;0
WireConnection;70;2;425;0
WireConnection;379;0;377;0
WireConnection;439;0;441;0
WireConnection;439;1;442;0
WireConnection;436;0;434;0
WireConnection;421;0;35;4
WireConnection;72;0;71;0
WireConnection;72;1;70;0
WireConnection;438;0;439;0
WireConnection;438;1;440;0
WireConnection;380;0;379;0
WireConnection;380;1;381;0
WireConnection;382;0;380;0
WireConnection;382;1;238;1
WireConnection;73;0;72;0
WireConnection;73;1;70;0
WireConnection;73;2;421;0
WireConnection;437;0;434;0
WireConnection;437;1;436;0
WireConnection;437;2;438;0
WireConnection;0;0;73;0
WireConnection;0;3;14;0
WireConnection;0;4;82;0
WireConnection;0;11;382;0
WireConnection;0;12;437;0
ASEEND*/
//CHKSM=FE33B9C409A82BA3C3610E4EA2975B4B99289610