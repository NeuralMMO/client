// Shader created with Shader Forge v1.38 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:9361,x:32440,y:31885,varname:node_9361,prsc:2|emission-7931-OUT,voffset-2065-OUT;n:type:ShaderForge.SFN_Tex2dAsset,id:1094,x:30011,y:32063,ptovrint:False,ptlb:FlowMap,ptin:_FlowMap,varname:node_1094,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:6f69c6af0205556439ba73d144413aa2,ntxv:3,isnm:True;n:type:ShaderForge.SFN_Panner,id:388,x:30011,y:32309,varname:node_388,prsc:2,spu:1,spv:0|UVIN-9992-UVOUT,DIST-4414-OUT;n:type:ShaderForge.SFN_Panner,id:2673,x:30011,y:32472,varname:node_2673,prsc:2,spu:0,spv:1|UVIN-9992-UVOUT,DIST-1424-OUT;n:type:ShaderForge.SFN_TexCoord,id:9992,x:29500,y:32143,varname:node_9992,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Time,id:3379,x:29500,y:32315,varname:node_3379,prsc:2;n:type:ShaderForge.SFN_Multiply,id:4414,x:29739,y:32315,varname:node_4414,prsc:2|A-3379-T,B-515-OUT;n:type:ShaderForge.SFN_ValueProperty,id:515,x:29500,y:32470,ptovrint:False,ptlb:U_Speed,ptin:_U_Speed,varname:node_515,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_ValueProperty,id:7148,x:29500,y:32583,ptovrint:False,ptlb:V_Speed,ptin:_V_Speed,varname:node_7148,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Multiply,id:1424,x:29727,y:32507,varname:node_1424,prsc:2|A-3379-T,B-7148-OUT;n:type:ShaderForge.SFN_Tex2d,id:7769,x:30267,y:32497,varname:node_7769,prsc:2,tex:6f69c6af0205556439ba73d144413aa2,ntxv:0,isnm:False|UVIN-2673-UVOUT,TEX-1094-TEX;n:type:ShaderForge.SFN_Tex2d,id:1054,x:30267,y:32331,varname:node_1054,prsc:2,tex:6f69c6af0205556439ba73d144413aa2,ntxv:0,isnm:False|UVIN-388-UVOUT,TEX-1094-TEX;n:type:ShaderForge.SFN_Append,id:4380,x:30501,y:32331,varname:node_4380,prsc:2|A-1054-R,B-7769-G;n:type:ShaderForge.SFN_Multiply,id:2099,x:30716,y:32342,varname:node_2099,prsc:2|A-4380-OUT,B-4084-OUT;n:type:ShaderForge.SFN_ValueProperty,id:4084,x:30501,y:32553,ptovrint:False,ptlb:Strench,ptin:_Strench,varname:node_4084,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.2;n:type:ShaderForge.SFN_Add,id:1888,x:30874,y:32201,varname:node_1888,prsc:2|A-9992-UVOUT,B-2099-OUT;n:type:ShaderForge.SFN_Tex2dAsset,id:7679,x:31056,y:32003,ptovrint:False,ptlb:MainTexture,ptin:_MainTexture,varname:node_7679,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:ee6dc1be456732d48b3c7bfa74fa6eb0,ntxv:2,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:9700,x:31263,y:32211,varname:node_9700,prsc:2,tex:ee6dc1be456732d48b3c7bfa74fa6eb0,ntxv:0,isnm:False|UVIN-7014-UVOUT,TEX-7679-TEX;n:type:ShaderForge.SFN_Panner,id:7014,x:31056,y:32201,varname:node_7014,prsc:2,spu:1,spv:0|UVIN-1888-OUT,DIST-1688-OUT;n:type:ShaderForge.SFN_Color,id:6477,x:31437,y:32051,ptovrint:False,ptlb:color,ptin:_color,varname:node_6477,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:0.475862,c3:0,c4:1;n:type:ShaderForge.SFN_Slider,id:1688,x:30692,y:32523,ptovrint:False,ptlb:TextureDistanse,ptin:_TextureDistanse,varname:node_1688,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:1;n:type:ShaderForge.SFN_Panner,id:1175,x:31056,y:32409,varname:node_1175,prsc:2,spu:0.1,spv:0.1|UVIN-9992-UVOUT,DIST-1166-OUT;n:type:ShaderForge.SFN_Tex2d,id:7690,x:31263,y:32409,varname:node_7690,prsc:2,tex:ee6dc1be456732d48b3c7bfa74fa6eb0,ntxv:0,isnm:False|UVIN-1175-UVOUT,TEX-7679-TEX;n:type:ShaderForge.SFN_Multiply,id:8985,x:31489,y:32314,varname:node_8985,prsc:2|A-9700-R,B-7690-G;n:type:ShaderForge.SFN_Multiply,id:4951,x:31743,y:32209,varname:node_4951,prsc:2|A-6477-RGB,B-8985-OUT,C-3744-OUT;n:type:ShaderForge.SFN_Fresnel,id:3941,x:31032,y:32683,varname:node_3941,prsc:2|EXP-2286-OUT;n:type:ShaderForge.SFN_ValueProperty,id:2286,x:30849,y:32702,ptovrint:False,ptlb:FresnelStrench,ptin:_FresnelStrench,varname:node_2286,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_OneMinus,id:3101,x:31254,y:32668,varname:node_3101,prsc:2|IN-3941-OUT;n:type:ShaderForge.SFN_Multiply,id:4363,x:31506,y:32681,varname:node_4363,prsc:2|A-3101-OUT,B-3101-OUT,C-9984-OUT;n:type:ShaderForge.SFN_Clamp01,id:3013,x:31688,y:32681,varname:node_3013,prsc:2|IN-4363-OUT;n:type:ShaderForge.SFN_Multiply,id:1103,x:31977,y:32468,varname:node_1103,prsc:2|A-4951-OUT,B-3013-OUT,C-6801-OUT;n:type:ShaderForge.SFN_Get,id:4937,x:31032,y:32953,varname:node_4937,prsc:2|IN-8714-OUT;n:type:ShaderForge.SFN_Set,id:8714,x:29672,y:32094,varname:UV,prsc:2|IN-9992-UVOUT;n:type:ShaderForge.SFN_Add,id:3325,x:31513,y:32940,varname:node_3325,prsc:2|A-8730-RGB,B-5947-OUT;n:type:ShaderForge.SFN_Slider,id:5947,x:31123,y:33153,ptovrint:False,ptlb:TextureStrench,ptin:_TextureStrench,varname:node_5947,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:-1,cur:1,max:1;n:type:ShaderForge.SFN_Clamp01,id:6801,x:31726,y:32923,varname:node_6801,prsc:2|IN-3325-OUT;n:type:ShaderForge.SFN_Tex2d,id:8730,x:31236,y:32953,ptovrint:False,ptlb:UndoEffectTexture,ptin:_UndoEffectTexture,varname:node_8730,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:ee6dc1be456732d48b3c7bfa74fa6eb0,ntxv:2,isnm:False|UVIN-4937-OUT;n:type:ShaderForge.SFN_Multiply,id:7922,x:31971,y:32209,varname:node_7922,prsc:2|A-4951-OUT,B-6801-OUT;n:type:ShaderForge.SFN_SwitchProperty,id:7931,x:32159,y:32209,ptovrint:False,ptlb:Fresnel,ptin:_Fresnel,varname:node_7931,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False|A-7922-OUT,B-1103-OUT;n:type:ShaderForge.SFN_Slider,id:3744,x:31410,y:32552,ptovrint:False,ptlb:EmmisionStrench,ptin:_EmmisionStrench,varname:node_3744,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1.8,max:8;n:type:ShaderForge.SFN_NormalVector,id:5557,x:31755,y:31850,prsc:2,pt:False;n:type:ShaderForge.SFN_Multiply,id:2065,x:31974,y:31850,varname:node_2065,prsc:2|A-4961-OUT,B-5557-OUT,C-964-OUT;n:type:ShaderForge.SFN_Clamp01,id:4961,x:31755,y:31718,varname:node_4961,prsc:2|IN-7690-RGB;n:type:ShaderForge.SFN_Slider,id:964,x:31650,y:32045,ptovrint:False,ptlb:Relief,ptin:_Relief,varname:node_964,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0.1,cur:0.5,max:2;n:type:ShaderForge.SFN_Multiply,id:1166,x:30692,y:32617,varname:node_1166,prsc:2|A-4620-T,B-2714-OUT;n:type:ShaderForge.SFN_Time,id:4620,x:30447,y:32641,varname:node_4620,prsc:2;n:type:ShaderForge.SFN_Slider,id:2714,x:30455,y:32783,ptovrint:False,ptlb:TextureChange speed,ptin:_TextureChangespeed,varname:node_2714,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:6.917714,max:20;n:type:ShaderForge.SFN_ValueProperty,id:9984,x:31254,y:32834,ptovrint:False,ptlb:Outline fresnel,ptin:_Outlinefresnel,varname:node_9984,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:2;proporder:6477-1094-515-7148-4084-7679-1688-2286-5947-8730-7931-3744-964-2714-9984;pass:END;sub:END;*/

Shader "Lava" {
    Properties {
        _color ("color", Color) = (1,0.475862,0,1)
        _FlowMap ("FlowMap", 2D) = "bump" {}
        _U_Speed ("U_Speed", Float ) = 0
        _V_Speed ("V_Speed", Float ) = 0
        _Strench ("Strench", Float ) = 0.2
        _MainTexture ("MainTexture", 2D) = "black" {}
        _TextureDistanse ("TextureDistanse", Range(0, 1)) = 1
        _FresnelStrench ("FresnelStrench", Float ) = 1
        _TextureStrench ("TextureStrench", Range(-1, 1)) = 1
        _UndoEffectTexture ("UndoEffectTexture", 2D) = "black" {}
        [MaterialToggle] _Fresnel ("Fresnel", Float ) = 0
        _EmmisionStrench ("EmmisionStrench", Range(0, 8)) = 1.8
        _Relief ("Relief", Range(0.1, 2)) = 0.5
        _TextureChangespeed ("TextureChange speed", Range(0, 20)) = 6.917714
        _Outlinefresnel ("Outline fresnel", Float ) = 2
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
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles n3ds wiiu 
            #pragma target 3.0
            uniform sampler2D _FlowMap; uniform float4 _FlowMap_ST;
            uniform float _U_Speed;
            uniform float _V_Speed;
            uniform float _Strench;
            uniform sampler2D _MainTexture; uniform float4 _MainTexture_ST;
            uniform float4 _color;
            uniform float _TextureDistanse;
            uniform float _FresnelStrench;
            uniform float _TextureStrench;
            uniform sampler2D _UndoEffectTexture; uniform float4 _UndoEffectTexture_ST;
            uniform fixed _Fresnel;
            uniform float _EmmisionStrench;
            uniform float _Relief;
            uniform float _TextureChangespeed;
            uniform float _Outlinefresnel;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                UNITY_FOG_COORDS(3)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                float4 node_4620 = _Time;
                float2 node_1175 = (o.uv0+(node_4620.g*_TextureChangespeed)*float2(0.1,0.1));
                float4 node_7690 = tex2Dlod(_MainTexture,float4(TRANSFORM_TEX(node_1175, _MainTexture),0.0,0));
                v.vertex.xyz += (saturate(node_7690.rgb)*v.normal*_Relief);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
////// Lighting:
////// Emissive:
                float4 node_3379 = _Time;
                float2 node_388 = (i.uv0+(node_3379.g*_U_Speed)*float2(1,0));
                float3 node_1054 = UnpackNormal(tex2D(_FlowMap,TRANSFORM_TEX(node_388, _FlowMap)));
                float2 node_2673 = (i.uv0+(node_3379.g*_V_Speed)*float2(0,1));
                float3 node_7769 = UnpackNormal(tex2D(_FlowMap,TRANSFORM_TEX(node_2673, _FlowMap)));
                float2 node_7014 = ((i.uv0+(float2(node_1054.r,node_7769.g)*_Strench))+_TextureDistanse*float2(1,0));
                float4 node_9700 = tex2D(_MainTexture,TRANSFORM_TEX(node_7014, _MainTexture));
                float4 node_4620 = _Time;
                float2 node_1175 = (i.uv0+(node_4620.g*_TextureChangespeed)*float2(0.1,0.1));
                float4 node_7690 = tex2D(_MainTexture,TRANSFORM_TEX(node_1175, _MainTexture));
                float3 node_4951 = (_color.rgb*(node_9700.r*node_7690.g)*_EmmisionStrench);
                float2 UV = i.uv0;
                float2 node_4937 = UV;
                float4 _UndoEffectTexture_var = tex2D(_UndoEffectTexture,TRANSFORM_TEX(node_4937, _UndoEffectTexture));
                float3 node_6801 = saturate((_UndoEffectTexture_var.rgb+_TextureStrench));
                float node_3101 = (1.0 - pow(1.0-max(0,dot(normalDirection, viewDirection)),_FresnelStrench));
                float3 emissive = lerp( (node_4951*node_6801), (node_4951*saturate((node_3101*node_3101*_Outlinefresnel))*node_6801), _Fresnel );
                float3 finalColor = emissive;
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        Pass {
            Name "ShadowCaster"
            Tags {
                "LightMode"="ShadowCaster"
            }
            Offset 1, 1
            Cull Back
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_SHADOWCASTER
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles n3ds wiiu 
            #pragma target 3.0
            uniform sampler2D _MainTexture; uniform float4 _MainTexture_ST;
            uniform float _Relief;
            uniform float _TextureChangespeed;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                V2F_SHADOW_CASTER;
                float2 uv0 : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                float4 node_4620 = _Time;
                float2 node_1175 = (o.uv0+(node_4620.g*_TextureChangespeed)*float2(0.1,0.1));
                float4 node_7690 = tex2Dlod(_MainTexture,float4(TRANSFORM_TEX(node_1175, _MainTexture),0.0,0));
                v.vertex.xyz += (saturate(node_7690.rgb)*v.normal*_Relief);
                o.pos = UnityObjectToClipPos( v.vertex );
                TRANSFER_SHADOW_CASTER(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3 normalDirection = i.normalDir;
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
