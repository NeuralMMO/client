/*SF_DATA;ver:1.40;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,cpap:True,lico:1,lgpr:1,limd:3,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:True,hqlp:False,rprd:True,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,atcv:False,rfrpo:False,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:2865,x:33326,y:32869,varname:node_2865,prsc:2|diff-7807-OUT,spec-81-OUT,gloss-4146-OUT,emission-6558-OUT;n:type:ShaderForge.SFN_Multiply,id:6343,x:32165,y:33205,varname:node_6343,prsc:2|A-7736-RGB,B-6665-RGB;n:type:ShaderForge.SFN_Color,id:6665,x:31955,y:33287,ptovrint:False,ptlb:Color,ptin:_Color,varname:_Color,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:1,c3:1,c4:1;n:type:ShaderForge.SFN_Tex2d,id:7736,x:31945,y:33074,ptovrint:True,ptlb:Texture Albedo,ptin:_MainTex,varname:_MainTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:True,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:2022,x:32426,y:33214,ptovrint:False,ptlb:Metallic & Emission Map,ptin:_MetallicEmissionMap,varname:_MetallicMap,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:True,tagnrm:False,ntxv:2,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:126,x:32165,y:32532,varname:node_126,prsc:2,ntxv:2,isnm:False|TEX-785-TEX;n:type:ShaderForge.SFN_Color,id:7245,x:32165,y:32693,ptovrint:False,ptlb:Metal Color,ptin:_MetalColor,varname:_MetalColor,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.3764706,c2:0.3764706,c3:0.3764706,c4:1;n:type:ShaderForge.SFN_Color,id:2181,x:32165,y:32873,ptovrint:False,ptlb:Stone Color,ptin:_StoneColor,varname:_StoneColor,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.3529412,c2:0.3686275,c3:0.4,c4:1;n:type:ShaderForge.SFN_Color,id:1745,x:32165,y:33053,ptovrint:False,ptlb:Wood Color,ptin:_WoodColor,varname:_WoodColor,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.4196079,c2:0.3960785,c3:0.345098,c4:1;n:type:ShaderForge.SFN_Color,id:616,x:32451,y:32546,ptovrint:False,ptlb:Gemstone Color,ptin:_GemstoneColor,varname:_GemstoneColor,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.1372549,c2:0.3647059,c3:0.6784314,c4:1;n:type:ShaderForge.SFN_ChannelBlend,id:6773,x:32426,y:33037,varname:node_6773,prsc:2,chbt:1|M-126-RGB,R-7245-RGB,G-2181-RGB,B-1745-RGB,BTM-6343-OUT;n:type:ShaderForge.SFN_Slider,id:2196,x:32269,y:33478,ptovrint:False,ptlb:Metal Smoothness,ptin:_MetalSmoothness,varname:_Smoothness,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.6,max:1;n:type:ShaderForge.SFN_Color,id:5893,x:32451,y:32714,ptovrint:False,ptlb:Leather Color,ptin:_LeatherColor,varname:_LeatherColor,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.4078432,c2:0.3294118,c3:0.172549,c4:1;n:type:ShaderForge.SFN_Color,id:7,x:32451,y:32875,ptovrint:False,ptlb:Cloth Color,ptin:_ClothColor,varname:_ClothColor,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.6117647,c2:0.509804,c3:0.3882353,c4:1;n:type:ShaderForge.SFN_TexCoord,id:2838,x:31956,y:32255,varname:node_2838,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Tex2d,id:9193,x:32451,y:32397,varname:node_9193,prsc:2,ntxv:0,isnm:False|UVIN-5077-UVOUT,TEX-785-TEX;n:type:ShaderForge.SFN_Tex2dAsset,id:785,x:31956,y:32503,ptovrint:False,ptlb:Color Mask,ptin:_ColorMask,varname:_ColorMask,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:True,tagnrm:False,ntxv:2,isnm:False;n:type:ShaderForge.SFN_ChannelBlend,id:4541,x:32694,y:32779,varname:node_4541,prsc:2,chbt:1|M-9193-RGB,R-616-RGB,G-5893-RGB,B-7-RGB,BTM-6773-OUT;n:type:ShaderForge.SFN_Multiply,id:6558,x:32972,y:33289,varname:node_6558,prsc:2|A-2022-A,B-2433-RGB;n:type:ShaderForge.SFN_Color,id:2433,x:32972,y:33483,ptovrint:False,ptlb:Emission Color,ptin:_EmissionColor,varname:node_2433,prsc:2,glob:False,taghide:False,taghdr:True,tagprd:False,tagnsco:False,tagnrm:False,c1:0,c2:0,c3:0,c4:1;n:type:ShaderForge.SFN_Slider,id:6821,x:32617,y:33536,ptovrint:False,ptlb:Smoothness,ptin:_Smoothness,varname:node_6821,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:1;n:type:ShaderForge.SFN_Slider,id:5502,x:32269,y:33387,ptovrint:False,ptlb:Metallic,ptin:_Metallic,varname:node_5502,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.8,max:1;n:type:ShaderForge.SFN_Multiply,id:81,x:32680,y:33069,varname:node_81,prsc:2|A-2022-R,B-5502-OUT;n:type:ShaderForge.SFN_Panner,id:5077,x:32189,y:32331,varname:node_5077,prsc:2,spu:1,spv:0|UVIN-2838-UVOUT,DIST-8938-OUT;n:type:ShaderForge.SFN_Vector1,id:6194,x:31618,y:32390,varname:node_6194,prsc:2,v1:0.75;n:type:ShaderForge.SFN_Vector1,id:6695,x:31618,y:32453,varname:node_6695,prsc:2,v1:0.5;n:type:ShaderForge.SFN_Vector1,id:9105,x:32692,y:32566,varname:node_9105,prsc:2,v1:0.75;n:type:ShaderForge.SFN_Vector1,id:387,x:32909,y:32583,varname:node_387,prsc:2,v1:1.25;n:type:ShaderForge.SFN_Multiply,id:1089,x:32942,y:32668,varname:node_1089,prsc:2|A-387-OUT,B-4449-OUT;n:type:ShaderForge.SFN_Lerp,id:4449,x:32901,y:32808,varname:node_4449,prsc:2|A-4541-OUT,B-3362-OUT,T-126-A;n:type:ShaderForge.SFN_Multiply,id:3362,x:32715,y:32644,varname:node_3362,prsc:2|A-9105-OUT,B-4541-OUT;n:type:ShaderForge.SFN_Lerp,id:7807,x:33120,y:32869,varname:node_7807,prsc:2|A-4449-OUT,B-1089-OUT,T-7736-A;n:type:ShaderForge.SFN_ChannelBlend,id:4146,x:32745,y:33232,varname:node_4146,prsc:2,chbt:1|M-2022-RGB,R-2196-OUT,G-1292-OUT,B-6821-OUT,BTM-6821-OUT;n:type:ShaderForge.SFN_Slider,id:1292,x:32269,y:33566,ptovrint:False,ptlb:Gemstone Smoothness,ptin:_GemstoneSmoothness,varname:node_1292,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.6,max:1;n:type:ShaderForge.SFN_SwitchProperty,id:8938,x:31825,y:32419,ptovrint:False,ptlb:Small Texture,ptin:_SmallTexture,varname:node_8938,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:True|A-6194-OUT,B-6695-OUT;proporder:7736-6665-6821-1292-5502-2196-7245-2181-1745-616-5893-7-2022-2433-785-8938;pass:END;sub:END;*/

Shader "TitanForge/TitanForge Shader" {
    Properties {
        [NoScaleOffset]_MainTex ("Texture Albedo", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Smoothness ("Smoothness", Range(0, 1)) = 0
        _GemstoneSmoothness ("Gemstone Smoothness", Range(0, 1)) = 0.6
        _Metallic ("Metallic", Range(0, 1)) = 0.8
        _MetalSmoothness ("Metal Smoothness", Range(0, 1)) = 0.6
        _MetalColor ("Metal Color", Color) = (0.3764706,0.3764706,0.3764706,1)
        _StoneColor ("Stone Color", Color) = (0.3529412,0.3686275,0.4,1)
        _WoodColor ("Wood Color", Color) = (0.4196079,0.3960785,0.345098,1)
        _GemstoneColor ("Gemstone Color", Color) = (0.1372549,0.3647059,0.6784314,1)
        _LeatherColor ("Leather Color", Color) = (0.4078432,0.3294118,0.172549,1)
        _ClothColor ("Cloth Color", Color) = (0.6117647,0.509804,0.3882353,1)
        [NoScaleOffset]_MetallicEmissionMap ("Metallic & Emission Map", 2D) = "black" {}
        [HDR]_EmissionColor ("Emission Color", Color) = (0,0,0,1)
        [NoScaleOffset]_ColorMask ("Color Mask", 2D) = "black" {}
        [MaterialToggle] _SmallTexture ("Small Texture", Float ) = 0.5
    }
    SubShader {
        Tags {
            "RenderType"="Opaque"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
            #define _GLOSSYENV 1
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma target 3.0
            uniform sampler2D _MainTex;
            uniform sampler2D _MetallicEmissionMap;
            uniform sampler2D _ColorMask;
            UNITY_INSTANCING_BUFFER_START( Props )
                UNITY_DEFINE_INSTANCED_PROP( float4, _Color)
                UNITY_DEFINE_INSTANCED_PROP( float4, _MetalColor)
                UNITY_DEFINE_INSTANCED_PROP( float4, _StoneColor)
                UNITY_DEFINE_INSTANCED_PROP( float4, _WoodColor)
                UNITY_DEFINE_INSTANCED_PROP( float4, _GemstoneColor)
                UNITY_DEFINE_INSTANCED_PROP( float, _MetalSmoothness)
                UNITY_DEFINE_INSTANCED_PROP( float4, _LeatherColor)
                UNITY_DEFINE_INSTANCED_PROP( float4, _ClothColor)
                UNITY_DEFINE_INSTANCED_PROP( float4, _EmissionColor)
                UNITY_DEFINE_INSTANCED_PROP( float, _Smoothness)
                UNITY_DEFINE_INSTANCED_PROP( float, _Metallic)
                UNITY_DEFINE_INSTANCED_PROP( float, _GemstoneSmoothness)
                UNITY_DEFINE_INSTANCED_PROP( fixed, _SmallTexture)
            UNITY_INSTANCING_BUFFER_END( Props )
            struct VertexInput {
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
            };
            struct VertexOutput {
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float4 posWorld : TEXCOORD3;
                float3 normalDir : TEXCOORD4;
                float3 tangentDir : TEXCOORD5;
                float3 bitangentDir : TEXCOORD6;
                LIGHTING_COORDS(7,8)
                UNITY_FOG_COORDS(9)
                #if defined(LIGHTMAP_ON) || defined(UNITY_SHOULD_SAMPLE_SH)
                    float4 ambientOrLightmapUV : TEXCOORD10;
                #endif
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                UNITY_SETUP_INSTANCE_ID( v );
                UNITY_TRANSFER_INSTANCE_ID( v, o );
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                #ifdef LIGHTMAP_ON
                    o.ambientOrLightmapUV.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                    o.ambientOrLightmapUV.zw = 0;
                #elif UNITY_SHOULD_SAMPLE_SH
                #endif
                #ifdef DYNAMICLIGHTMAP_ON
                    o.ambientOrLightmapUV.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
                #endif
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                UNITY_SETUP_INSTANCE_ID( i );
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float3 viewReflectDirection = reflect( -viewDirection, normalDirection );
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
                float Pi = 3.141592654;
                float InvPi = 0.31830988618;
///////// Gloss:
                float4 _MetallicEmissionMap_var = tex2D(_MetallicEmissionMap,i.uv0);
                float _Smoothness_var = UNITY_ACCESS_INSTANCED_PROP( Props, _Smoothness );
                float _MetalSmoothness_var = UNITY_ACCESS_INSTANCED_PROP( Props, _MetalSmoothness );
                float _GemstoneSmoothness_var = UNITY_ACCESS_INSTANCED_PROP( Props, _GemstoneSmoothness );
                float gloss = (lerp( lerp( lerp( _Smoothness_var, _MetalSmoothness_var, _MetallicEmissionMap_var.rgb.r ), _GemstoneSmoothness_var, _MetallicEmissionMap_var.rgb.g ), _Smoothness_var, _MetallicEmissionMap_var.rgb.b ));
                float perceptualRoughness = 1.0 - (lerp( lerp( lerp( _Smoothness_var, _MetalSmoothness_var, _MetallicEmissionMap_var.rgb.r ), _GemstoneSmoothness_var, _MetallicEmissionMap_var.rgb.g ), _Smoothness_var, _MetallicEmissionMap_var.rgb.b ));
                float roughness = perceptualRoughness * perceptualRoughness;
                float specPow = exp2( gloss * 10.0 + 1.0 );
/////// GI Data:
                UnityLight light;
                #ifdef LIGHTMAP_OFF
                    light.color = lightColor;
                    light.dir = lightDirection;
                    light.ndotl = LambertTerm (normalDirection, light.dir);
                #else
                    light.color = half3(0.f, 0.f, 0.f);
                    light.ndotl = 0.0f;
                    light.dir = half3(0.f, 0.f, 0.f);
                #endif
                UnityGIInput d;
                d.light = light;
                d.worldPos = i.posWorld.xyz;
                d.worldViewDir = viewDirection;
                d.atten = attenuation;
                #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
                    d.ambient = 0;
                    d.lightmapUV = i.ambientOrLightmapUV;
                #else
                    d.ambient = i.ambientOrLightmapUV;
                #endif
                #if UNITY_SPECCUBE_BLENDING || UNITY_SPECCUBE_BOX_PROJECTION
                    d.boxMin[0] = unity_SpecCube0_BoxMin;
                    d.boxMin[1] = unity_SpecCube1_BoxMin;
                #endif
                #if UNITY_SPECCUBE_BOX_PROJECTION
                    d.boxMax[0] = unity_SpecCube0_BoxMax;
                    d.boxMax[1] = unity_SpecCube1_BoxMax;
                    d.probePosition[0] = unity_SpecCube0_ProbePosition;
                    d.probePosition[1] = unity_SpecCube1_ProbePosition;
                #endif
                d.probeHDR[0] = unity_SpecCube0_HDR;
                d.probeHDR[1] = unity_SpecCube1_HDR;
                Unity_GlossyEnvironmentData ugls_en_data;
                ugls_en_data.roughness = 1.0 - gloss;
                ugls_en_data.reflUVW = viewReflectDirection;
                UnityGI gi = UnityGlobalIllumination(d, 1, normalDirection, ugls_en_data );
                lightDirection = gi.light.dir;
                lightColor = gi.light.color;
////// Specular:
                float NdotL = saturate(dot( normalDirection, lightDirection ));
                float LdotH = saturate(dot(lightDirection, halfDirection));
                float _Metallic_var = UNITY_ACCESS_INSTANCED_PROP( Props, _Metallic );
                float3 specularColor = (_MetallicEmissionMap_var.r*_Metallic_var);
                float specularMonochrome;
                float _SmallTexture_var = lerp( 0.75, 0.5, UNITY_ACCESS_INSTANCED_PROP( Props, _SmallTexture ) );
                float2 node_5077 = (i.uv0+_SmallTexture_var*float2(1,0));
                float4 node_9193 = tex2D(_ColorMask,node_5077);
                float4 node_126 = tex2D(_ColorMask,i.uv0);
                float4 _MainTex_var = tex2D(_MainTex,i.uv0);
                float4 _Color_var = UNITY_ACCESS_INSTANCED_PROP( Props, _Color );
                float4 _MetalColor_var = UNITY_ACCESS_INSTANCED_PROP( Props, _MetalColor );
                float4 _StoneColor_var = UNITY_ACCESS_INSTANCED_PROP( Props, _StoneColor );
                float4 _WoodColor_var = UNITY_ACCESS_INSTANCED_PROP( Props, _WoodColor );
                float4 _GemstoneColor_var = UNITY_ACCESS_INSTANCED_PROP( Props, _GemstoneColor );
                float4 _LeatherColor_var = UNITY_ACCESS_INSTANCED_PROP( Props, _LeatherColor );
                float4 _ClothColor_var = UNITY_ACCESS_INSTANCED_PROP( Props, _ClothColor );
                float3 node_4541 = (lerp( lerp( lerp( (lerp( lerp( lerp( (_MainTex_var.rgb*_Color_var.rgb), _MetalColor_var.rgb, node_126.rgb.r ), _StoneColor_var.rgb, node_126.rgb.g ), _WoodColor_var.rgb, node_126.rgb.b )), _GemstoneColor_var.rgb, node_9193.rgb.r ), _LeatherColor_var.rgb, node_9193.rgb.g ), _ClothColor_var.rgb, node_9193.rgb.b ));
                float3 node_4449 = lerp(node_4541,(0.75*node_4541),node_126.a);
                float3 diffuseColor = lerp(node_4449,(1.25*node_4449),_MainTex_var.a); // Need this for specular when using metallic
                diffuseColor = DiffuseAndSpecularFromMetallic( diffuseColor, specularColor, specularColor, specularMonochrome );
                specularMonochrome = 1.0-specularMonochrome;
                float NdotV = abs(dot( normalDirection, viewDirection ));
                float NdotH = saturate(dot( normalDirection, halfDirection ));
                float VdotH = saturate(dot( viewDirection, halfDirection ));
                float visTerm = SmithJointGGXVisibilityTerm( NdotL, NdotV, roughness );
                float normTerm = GGXTerm(NdotH, roughness);
                float specularPBL = (visTerm*normTerm) * UNITY_PI;
                #ifdef UNITY_COLORSPACE_GAMMA
                    specularPBL = sqrt(max(1e-4h, specularPBL));
                #endif
                specularPBL = max(0, specularPBL * NdotL);
                #if defined(_SPECULARHIGHLIGHTS_OFF)
                    specularPBL = 0.0;
                #endif
                half surfaceReduction;
                #ifdef UNITY_COLORSPACE_GAMMA
                    surfaceReduction = 1.0-0.28*roughness*perceptualRoughness;
                #else
                    surfaceReduction = 1.0/(roughness*roughness + 1.0);
                #endif
                specularPBL *= any(specularColor) ? 1.0 : 0.0;
                float3 directSpecular = attenColor*specularPBL*FresnelTerm(specularColor, LdotH);
                half grazingTerm = saturate( gloss + specularMonochrome );
                float3 indirectSpecular = (gi.indirect.specular);
                indirectSpecular *= FresnelLerp (specularColor, grazingTerm, NdotV);
                indirectSpecular *= surfaceReduction;
                float3 specular = (directSpecular + indirectSpecular);
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                half fd90 = 0.5 + 2 * LdotH * LdotH * (1-gloss);
                float nlPow5 = Pow5(1-NdotL);
                float nvPow5 = Pow5(1-NdotV);
                float3 directDiffuse = ((1 +(fd90 - 1)*nlPow5) * (1 + (fd90 - 1)*nvPow5) * NdotL) * attenColor;
                float3 indirectDiffuse = float3(0,0,0);
                indirectDiffuse += gi.indirect.diffuse;
                float3 diffuse = (directDiffuse + indirectDiffuse) * diffuseColor;
////// Emissive:
                float4 _EmissionColor_var = UNITY_ACCESS_INSTANCED_PROP( Props, _EmissionColor );
                float3 emissive = (_MetallicEmissionMap_var.a*_EmissionColor_var.rgb);
/// Final Color:
                float3 finalColor = diffuse + specular + emissive;
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        Pass {
            Name "FORWARD_DELTA"
            Tags {
                "LightMode"="ForwardAdd"
            }
            Blend One One
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
            #define _GLOSSYENV 1
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma target 3.0
            uniform sampler2D _MainTex;
            uniform sampler2D _MetallicEmissionMap;
            uniform sampler2D _ColorMask;
            UNITY_INSTANCING_BUFFER_START( Props )
                UNITY_DEFINE_INSTANCED_PROP( float4, _Color)
                UNITY_DEFINE_INSTANCED_PROP( float4, _MetalColor)
                UNITY_DEFINE_INSTANCED_PROP( float4, _StoneColor)
                UNITY_DEFINE_INSTANCED_PROP( float4, _WoodColor)
                UNITY_DEFINE_INSTANCED_PROP( float4, _GemstoneColor)
                UNITY_DEFINE_INSTANCED_PROP( float, _MetalSmoothness)
                UNITY_DEFINE_INSTANCED_PROP( float4, _LeatherColor)
                UNITY_DEFINE_INSTANCED_PROP( float4, _ClothColor)
                UNITY_DEFINE_INSTANCED_PROP( float4, _EmissionColor)
                UNITY_DEFINE_INSTANCED_PROP( float, _Smoothness)
                UNITY_DEFINE_INSTANCED_PROP( float, _Metallic)
                UNITY_DEFINE_INSTANCED_PROP( float, _GemstoneSmoothness)
                UNITY_DEFINE_INSTANCED_PROP( fixed, _SmallTexture)
            UNITY_INSTANCING_BUFFER_END( Props )
            struct VertexInput {
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
            };
            struct VertexOutput {
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float4 posWorld : TEXCOORD3;
                float3 normalDir : TEXCOORD4;
                float3 tangentDir : TEXCOORD5;
                float3 bitangentDir : TEXCOORD6;
                LIGHTING_COORDS(7,8)
                UNITY_FOG_COORDS(9)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                UNITY_SETUP_INSTANCE_ID( v );
                UNITY_TRANSFER_INSTANCE_ID( v, o );
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                UNITY_SETUP_INSTANCE_ID( i );
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
                float Pi = 3.141592654;
                float InvPi = 0.31830988618;
///////// Gloss:
                float4 _MetallicEmissionMap_var = tex2D(_MetallicEmissionMap,i.uv0);
                float _Smoothness_var = UNITY_ACCESS_INSTANCED_PROP( Props, _Smoothness );
                float _MetalSmoothness_var = UNITY_ACCESS_INSTANCED_PROP( Props, _MetalSmoothness );
                float _GemstoneSmoothness_var = UNITY_ACCESS_INSTANCED_PROP( Props, _GemstoneSmoothness );
                float gloss = (lerp( lerp( lerp( _Smoothness_var, _MetalSmoothness_var, _MetallicEmissionMap_var.rgb.r ), _GemstoneSmoothness_var, _MetallicEmissionMap_var.rgb.g ), _Smoothness_var, _MetallicEmissionMap_var.rgb.b ));
                float perceptualRoughness = 1.0 - (lerp( lerp( lerp( _Smoothness_var, _MetalSmoothness_var, _MetallicEmissionMap_var.rgb.r ), _GemstoneSmoothness_var, _MetallicEmissionMap_var.rgb.g ), _Smoothness_var, _MetallicEmissionMap_var.rgb.b ));
                float roughness = perceptualRoughness * perceptualRoughness;
                float specPow = exp2( gloss * 10.0 + 1.0 );
////// Specular:
                float NdotL = saturate(dot( normalDirection, lightDirection ));
                float LdotH = saturate(dot(lightDirection, halfDirection));
                float _Metallic_var = UNITY_ACCESS_INSTANCED_PROP( Props, _Metallic );
                float3 specularColor = (_MetallicEmissionMap_var.r*_Metallic_var);
                float specularMonochrome;
                float _SmallTexture_var = lerp( 0.75, 0.5, UNITY_ACCESS_INSTANCED_PROP( Props, _SmallTexture ) );
                float2 node_5077 = (i.uv0+_SmallTexture_var*float2(1,0));
                float4 node_9193 = tex2D(_ColorMask,node_5077);
                float4 node_126 = tex2D(_ColorMask,i.uv0);
                float4 _MainTex_var = tex2D(_MainTex,i.uv0);
                float4 _Color_var = UNITY_ACCESS_INSTANCED_PROP( Props, _Color );
                float4 _MetalColor_var = UNITY_ACCESS_INSTANCED_PROP( Props, _MetalColor );
                float4 _StoneColor_var = UNITY_ACCESS_INSTANCED_PROP( Props, _StoneColor );
                float4 _WoodColor_var = UNITY_ACCESS_INSTANCED_PROP( Props, _WoodColor );
                float4 _GemstoneColor_var = UNITY_ACCESS_INSTANCED_PROP( Props, _GemstoneColor );
                float4 _LeatherColor_var = UNITY_ACCESS_INSTANCED_PROP( Props, _LeatherColor );
                float4 _ClothColor_var = UNITY_ACCESS_INSTANCED_PROP( Props, _ClothColor );
                float3 node_4541 = (lerp( lerp( lerp( (lerp( lerp( lerp( (_MainTex_var.rgb*_Color_var.rgb), _MetalColor_var.rgb, node_126.rgb.r ), _StoneColor_var.rgb, node_126.rgb.g ), _WoodColor_var.rgb, node_126.rgb.b )), _GemstoneColor_var.rgb, node_9193.rgb.r ), _LeatherColor_var.rgb, node_9193.rgb.g ), _ClothColor_var.rgb, node_9193.rgb.b ));
                float3 node_4449 = lerp(node_4541,(0.75*node_4541),node_126.a);
                float3 diffuseColor = lerp(node_4449,(1.25*node_4449),_MainTex_var.a); // Need this for specular when using metallic
                diffuseColor = DiffuseAndSpecularFromMetallic( diffuseColor, specularColor, specularColor, specularMonochrome );
                specularMonochrome = 1.0-specularMonochrome;
                float NdotV = abs(dot( normalDirection, viewDirection ));
                float NdotH = saturate(dot( normalDirection, halfDirection ));
                float VdotH = saturate(dot( viewDirection, halfDirection ));
                float visTerm = SmithJointGGXVisibilityTerm( NdotL, NdotV, roughness );
                float normTerm = GGXTerm(NdotH, roughness);
                float specularPBL = (visTerm*normTerm) * UNITY_PI;
                #ifdef UNITY_COLORSPACE_GAMMA
                    specularPBL = sqrt(max(1e-4h, specularPBL));
                #endif
                specularPBL = max(0, specularPBL * NdotL);
                #if defined(_SPECULARHIGHLIGHTS_OFF)
                    specularPBL = 0.0;
                #endif
                specularPBL *= any(specularColor) ? 1.0 : 0.0;
                float3 directSpecular = attenColor*specularPBL*FresnelTerm(specularColor, LdotH);
                float3 specular = directSpecular;
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                half fd90 = 0.5 + 2 * LdotH * LdotH * (1-gloss);
                float nlPow5 = Pow5(1-NdotL);
                float nvPow5 = Pow5(1-NdotV);
                float3 directDiffuse = ((1 +(fd90 - 1)*nlPow5) * (1 + (fd90 - 1)*nvPow5) * NdotL) * attenColor;
                float3 diffuse = directDiffuse * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse + specular;
                fixed4 finalRGBA = fixed4(finalColor * 1,0);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        Pass {
            Name "Meta"
            Tags {
                "LightMode"="Meta"
            }
            Cull Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_META 1
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
            #define _GLOSSYENV 1
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #include "UnityMetaPass.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma target 3.0
            uniform sampler2D _MainTex;
            uniform sampler2D _MetallicEmissionMap;
            uniform sampler2D _ColorMask;
            UNITY_INSTANCING_BUFFER_START( Props )
                UNITY_DEFINE_INSTANCED_PROP( float4, _Color)
                UNITY_DEFINE_INSTANCED_PROP( float4, _MetalColor)
                UNITY_DEFINE_INSTANCED_PROP( float4, _StoneColor)
                UNITY_DEFINE_INSTANCED_PROP( float4, _WoodColor)
                UNITY_DEFINE_INSTANCED_PROP( float4, _GemstoneColor)
                UNITY_DEFINE_INSTANCED_PROP( float, _MetalSmoothness)
                UNITY_DEFINE_INSTANCED_PROP( float4, _LeatherColor)
                UNITY_DEFINE_INSTANCED_PROP( float4, _ClothColor)
                UNITY_DEFINE_INSTANCED_PROP( float4, _EmissionColor)
                UNITY_DEFINE_INSTANCED_PROP( float, _Smoothness)
                UNITY_DEFINE_INSTANCED_PROP( float, _Metallic)
                UNITY_DEFINE_INSTANCED_PROP( float, _GemstoneSmoothness)
                UNITY_DEFINE_INSTANCED_PROP( fixed, _SmallTexture)
            UNITY_INSTANCING_BUFFER_END( Props )
            struct VertexInput {
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
            };
            struct VertexOutput {
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float4 posWorld : TEXCOORD3;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                UNITY_SETUP_INSTANCE_ID( v );
                UNITY_TRANSFER_INSTANCE_ID( v, o );
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST );
                return o;
            }
            float4 frag(VertexOutput i) : SV_Target {
                UNITY_SETUP_INSTANCE_ID( i );
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                UnityMetaInput o;
                UNITY_INITIALIZE_OUTPUT( UnityMetaInput, o );
                
                float4 _MetallicEmissionMap_var = tex2D(_MetallicEmissionMap,i.uv0);
                float4 _EmissionColor_var = UNITY_ACCESS_INSTANCED_PROP( Props, _EmissionColor );
                o.Emission = (_MetallicEmissionMap_var.a*_EmissionColor_var.rgb);
                
                float _SmallTexture_var = lerp( 0.75, 0.5, UNITY_ACCESS_INSTANCED_PROP( Props, _SmallTexture ) );
                float2 node_5077 = (i.uv0+_SmallTexture_var*float2(1,0));
                float4 node_9193 = tex2D(_ColorMask,node_5077);
                float4 node_126 = tex2D(_ColorMask,i.uv0);
                float4 _MainTex_var = tex2D(_MainTex,i.uv0);
                float4 _Color_var = UNITY_ACCESS_INSTANCED_PROP( Props, _Color );
                float4 _MetalColor_var = UNITY_ACCESS_INSTANCED_PROP( Props, _MetalColor );
                float4 _StoneColor_var = UNITY_ACCESS_INSTANCED_PROP( Props, _StoneColor );
                float4 _WoodColor_var = UNITY_ACCESS_INSTANCED_PROP( Props, _WoodColor );
                float4 _GemstoneColor_var = UNITY_ACCESS_INSTANCED_PROP( Props, _GemstoneColor );
                float4 _LeatherColor_var = UNITY_ACCESS_INSTANCED_PROP( Props, _LeatherColor );
                float4 _ClothColor_var = UNITY_ACCESS_INSTANCED_PROP( Props, _ClothColor );
                float3 node_4541 = (lerp( lerp( lerp( (lerp( lerp( lerp( (_MainTex_var.rgb*_Color_var.rgb), _MetalColor_var.rgb, node_126.rgb.r ), _StoneColor_var.rgb, node_126.rgb.g ), _WoodColor_var.rgb, node_126.rgb.b )), _GemstoneColor_var.rgb, node_9193.rgb.r ), _LeatherColor_var.rgb, node_9193.rgb.g ), _ClothColor_var.rgb, node_9193.rgb.b ));
                float3 node_4449 = lerp(node_4541,(0.75*node_4541),node_126.a);
                float3 diffColor = lerp(node_4449,(1.25*node_4449),_MainTex_var.a);
                float specularMonochrome;
                float3 specColor;
                float _Metallic_var = UNITY_ACCESS_INSTANCED_PROP( Props, _Metallic );
                diffColor = DiffuseAndSpecularFromMetallic( diffColor, (_MetallicEmissionMap_var.r*_Metallic_var), specColor, specularMonochrome );
                float _Smoothness_var = UNITY_ACCESS_INSTANCED_PROP( Props, _Smoothness );
                float _MetalSmoothness_var = UNITY_ACCESS_INSTANCED_PROP( Props, _MetalSmoothness );
                float _GemstoneSmoothness_var = UNITY_ACCESS_INSTANCED_PROP( Props, _GemstoneSmoothness );
                float roughness = 1.0 - (lerp( lerp( lerp( _Smoothness_var, _MetalSmoothness_var, _MetallicEmissionMap_var.rgb.r ), _GemstoneSmoothness_var, _MetallicEmissionMap_var.rgb.g ), _Smoothness_var, _MetallicEmissionMap_var.rgb.b ));
                o.Albedo = diffColor + specColor * roughness * roughness * 0.5;
                
                return UnityMetaFragment( o );
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
