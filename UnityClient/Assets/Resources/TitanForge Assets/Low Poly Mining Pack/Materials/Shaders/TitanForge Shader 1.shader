// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "TitanForge/TitanForge Shader"
{
	Properties
	{
		_AlbedoColor("Albedo Color", Color) = (1,1,1,0)
		_TextureAlbedo("Texture Albedo", 2D) = "white" {}
		_Color1("Color 1", Color) = (0.6235294,0.6078432,0.5686275,1)
		_Color2("Color 2", Color) = (0.3529412,0.3686275,0.4,1)
		_Color3("Color 3", Color) = (0.4196079,0.3960785,0.345098,1)
		_Color4("Color 4", Color) = (0.1372549,0.3647059,0.6784314,1)
		_Color5("Color 5", Color) = (0.4078432,0.3294118,0.172549,1)
		_Color6("Color 6", Color) = (0.6117647,0.509804,0.3882353,1)
		_Smoothness("Smoothness", Range( 0 , 1)) = 0
		_Metallic("Metallic", Range( 0 , 1)) = 0
		_MetalSmoothness("Metal Smoothness", Range( 0 , 1)) = 0
		_ColorMask("Color Mask", 2D) = "white" {}
		_GemstoneSmoothness("Gemstone Smoothness", Range( 0 , 1)) = 0
		_MetallicMask("Metallic Mask", 2D) = "white" {}
		_EmissionColor("Emission Color", Color) = (0,0,0,0)
		_Emission("Emission", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		Blend SrcAlpha OneMinusSrcAlpha
		
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _ColorMask;
		uniform float4 _ColorMask_ST;
		uniform float4 _AlbedoColor;
		uniform sampler2D _TextureAlbedo;
		uniform float4 _TextureAlbedo_ST;
		uniform float4 _Color1;
		uniform float4 _Color2;
		uniform float4 _Color3;
		uniform float4 _Color6;
		uniform float4 _Color5;
		uniform float4 _Color4;
		SamplerState sampler_TextureAlbedo;
		uniform sampler2D _MetallicMask;
		SamplerState sampler_MetallicMask;
		uniform float4 _MetallicMask_ST;
		uniform float _Emission;
		uniform float4 _EmissionColor;
		uniform float _Metallic;
		uniform float _Smoothness;
		uniform float _MetalSmoothness;
		uniform float _GemstoneSmoothness;

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
			float4 layeredBlend64 = ( lerp( lerp( lerp( ( _AlbedoColor * tex2DNode10 ) , _Color1 , layeredBlendVar64.x ) , _Color2 , layeredBlendVar64.y ) , _Color3 , layeredBlendVar64.z ) );
			float3 layeredBlendVar46 = appendResult66;
			float4 layeredBlend46 = ( lerp( lerp( lerp( layeredBlend64 , _Color6 , layeredBlendVar46.x ) , _Color5 , layeredBlendVar46.y ) , _Color4 , layeredBlendVar46.z ) );
			float4 lerpResult70 = lerp( layeredBlend46 , ( 1.5 * layeredBlend46 ) , tex2DNode10.a);
			float4 lerpResult73 = lerp( lerpResult70 , ( 0.66 * lerpResult70 ) , tex2DNode35.a);
			o.Albedo = lerpResult73.rgb;
			float2 uv_MetallicMask = i.uv_texcoord * _MetallicMask_ST.xy + _MetallicMask_ST.zw;
			float4 tex2DNode445 = tex2D( _MetallicMask, uv_MetallicMask );
			o.Emission = ( ( tex2DNode445.a * _Emission ) * _EmissionColor ).rgb;
			o.Metallic = ( _Metallic * tex2DNode445.r );
			float4 layeredBlendVar453 = tex2DNode445;
			float layeredBlend453 = ( lerp( lerp( lerp( lerp( _Smoothness , _MetalSmoothness , layeredBlendVar453.x ) , _GemstoneSmoothness , layeredBlendVar453.y ) , _Smoothness , layeredBlendVar453.z ) , 0.0 , layeredBlendVar453.w ) );
			o.Smoothness = layeredBlend453;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18500
0;73;1228;631;1819.99;1159.132;2.126721;False;False
Node;AmplifyShaderEditor.CommentaryNode;298;-1236.606,-1337.321;Inherit;False;1656.23;1160.917;Main Shader;31;73;421;72;71;70;68;67;46;66;64;49;47;48;17;27;424;5;35;28;63;10;4;41;423;23;40;29;21;425;432;433;;0.2063012,0.7169812,0.6481563,1;0;0
Node;AmplifyShaderEditor.SamplerNode;10;-1180.084,-618.6924;Inherit;True;Property;_TextureAlbedo;Texture Albedo;1;0;Create;True;0;0;False;0;False;-1;bcd74a2ece0b9704aa045c742a8a6e53;bcd74a2ece0b9704aa045c742a8a6e53;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;21;-1087.475,-1186.457;Inherit;True;Property;_ColorMask;Color Mask;11;0;Create;True;0;0;False;0;False;2ab6b1e255f617245b7e1203668a268d;2ab6b1e255f617245b7e1203668a268d;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TextureCoordinatesNode;29;-373.3475,-1277.933;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;4;-1099.9,-787.6169;Inherit;False;Property;_AlbedoColor;Albedo Color;0;0;Create;True;0;0;False;0;False;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;5;-841.151,-684.6492;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;40;-120.1267,-1292.823;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;23;-1089.921,-992.3651;Inherit;True;Property;_TextureSample1;Texture Sample 1;5;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;41;-131.4708,-1197.811;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;63;-822.2772,-792.5845;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WireNode;432;-733.0212,-576.1042;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;423;-442.032,-1098.2;Inherit;False;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.ColorNode;27;-698.2504,-544.3171;Inherit;False;Property;_Color2;Color 2;3;0;Create;True;0;0;False;0;False;0.3529412,0.3686275,0.4,1;0.3529411,0.3686274,0.3999999,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;28;-692.2144,-379.6064;Inherit;False;Property;_Color3;Color 3;4;0;Create;True;0;0;False;0;False;0.4196079,0.3960785,0.345098,1;0.4196078,0.3960784,0.3450979,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;17;-695.2267,-714.024;Inherit;False;Property;_Color1;Color 1;2;0;Create;True;0;0;False;0;False;0.6235294,0.6078432,0.5686275,1;0.4705882,0.5607843,0.764706,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WireNode;424;-452.3153,-737.4526;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;35;-218.7913,-1088.088;Inherit;True;Property;_TextureSample0;Texture Sample 0;9;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WireNode;433;-482.0212,-567.1042;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;49;-696.2836,-1284.388;Inherit;False;Property;_Color6;Color 6;7;0;Create;True;0;0;False;0;False;0.6117647,0.509804,0.3882353,1;0.6117647,0.5098039,0.3882352,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;48;-693.5111,-1093.419;Inherit;False;Property;_Color5;Color 5;6;0;Create;True;0;0;False;0;False;0.4078432,0.3294118,0.172549,1;0.4078431,0.3294117,0.1725489,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LayeredBlendNode;64;-391.1433,-580.1973;Inherit;False;6;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;47;-694.1974,-928.283;Inherit;False;Property;_Color4;Color 4;5;0;Create;True;0;0;False;0;False;0.1372549,0.3647059,0.6784314,1;0.3764706,0.3137255,0.7333333,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;66;-147.7894,-879.6899;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LayeredBlendNode;46;-176.8216,-743.2831;Inherit;False;6;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;71;47.9651,-711.8448;Inherit;False;Constant;_LightValue;Light Value;11;0;Create;True;0;0;False;0;False;1.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;443;-660.8664,-161.6656;Inherit;False;972.1091;657.4537;Metallic & Emission;11;454;453;452;451;450;449;448;447;446;445;444;;0.510413,0.5607195,0.6981132,1;0;0
Node;AmplifyShaderEditor.WireNode;425;-678.658,-208.8494;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;68;62.17391,-631.6121;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;67;228.6497,-664.2151;Inherit;False;Constant;_DarkValue;Dark Value;11;0;Create;True;0;0;False;0;False;0.66;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;70;60.59325,-526.032;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;445;-597.5342,-37.42329;Inherit;True;Property;_MetallicMask;Metallic Mask;13;0;Create;True;0;0;False;0;False;-1;bbd3ea7d050076d4899eeb62955fa14c;bbd3ea7d050076d4899eeb62955fa14c;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;444;-610.8665,380.3653;Inherit;False;Property;_Emission;Emission;15;0;Create;True;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;72;242.3875,-578.7103;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;451;-603.7913,158.2051;Inherit;False;Property;_Smoothness;Smoothness;8;0;Create;True;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;450;-587.8431,-111.6656;Inherit;False;Property;_Metallic;Metallic;9;0;Create;True;0;0;False;0;False;0;0.8;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;421;178.8468,-753.7033;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;448;-602.7574,230.788;Inherit;False;Property;_MetalSmoothness;Metal Smoothness;10;0;Create;True;0;0;False;0;False;0;0.75;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;449;-151.8659,191.3654;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;446;-604.7574,305.7882;Inherit;False;Property;_GemstoneSmoothness;Gemstone Smoothness;12;0;Create;True;0;0;False;0;False;0;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;447;-170.7568,288.7882;Inherit;False;Property;_EmissionColor;Emission Color;14;0;Create;True;0;0;False;0;False;0,0,0,0;0,0,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;73;243.9874,-487.233;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;452;142.243,93.78819;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;454;-137.7568,-95.21182;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LayeredBlendNode;453;-163.5339,8.576727;Inherit;False;6;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;455.2245,-487.1883;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;TitanForge/TitanForge Shader;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;455;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;5;0;4;0
WireConnection;5;1;10;0
WireConnection;40;0;29;1
WireConnection;23;0;21;0
WireConnection;41;0;40;0
WireConnection;41;1;29;2
WireConnection;63;0;23;1
WireConnection;63;1;23;2
WireConnection;63;2;23;3
WireConnection;432;0;5;0
WireConnection;423;0;21;0
WireConnection;424;0;63;0
WireConnection;35;0;423;0
WireConnection;35;1;41;0
WireConnection;433;0;432;0
WireConnection;64;0;424;0
WireConnection;64;1;433;0
WireConnection;64;2;17;0
WireConnection;64;3;27;0
WireConnection;64;4;28;0
WireConnection;66;0;35;1
WireConnection;66;1;35;2
WireConnection;66;2;35;3
WireConnection;46;0;66;0
WireConnection;46;1;64;0
WireConnection;46;2;49;0
WireConnection;46;3;48;0
WireConnection;46;4;47;0
WireConnection;425;0;10;4
WireConnection;68;0;71;0
WireConnection;68;1;46;0
WireConnection;70;0;46;0
WireConnection;70;1;68;0
WireConnection;70;2;425;0
WireConnection;72;0;67;0
WireConnection;72;1;70;0
WireConnection;421;0;35;4
WireConnection;449;0;445;4
WireConnection;449;1;444;0
WireConnection;73;0;70;0
WireConnection;73;1;72;0
WireConnection;73;2;421;0
WireConnection;452;0;449;0
WireConnection;452;1;447;0
WireConnection;454;0;450;0
WireConnection;454;1;445;1
WireConnection;453;0;445;0
WireConnection;453;1;451;0
WireConnection;453;2;448;0
WireConnection;453;3;446;0
WireConnection;453;4;451;0
WireConnection;0;0;73;0
WireConnection;0;2;452;0
WireConnection;0;3;454;0
WireConnection;0;4;453;0
ASEEND*/
//CHKSM=E7C21F79BC7DB8C26AA394865D2A9C7DB22062AC