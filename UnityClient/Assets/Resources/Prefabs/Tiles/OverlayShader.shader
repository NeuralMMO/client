// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/OverlayShader"
{
    Properties
    {
        [NoScaleOffset] _Overlay("Base (RGB) Trans (A)", 2D) = "white" {}
    }
        SubShader
    {
        Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
        LOD 100

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _Overlay;

            v2f vert(float4 pos : POSITION, float2 uv : TEXCOORD0)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(pos);
                o.worldPos = mul(unity_ObjectToWorld, pos);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                //pos = floor(i.worldPos * 20);
                //checker = _Overlay * checker;
                float4 pos = i.worldPos;
                pos.xz = (floor(pos.xz + 0.5) + 0.5) / 80;

                //return fixed4(1.0, 0.0, 0.0, 1.0);
                fixed4 tex = tex2Dlod(_Overlay, float4(pos.z, pos.x, 0, 0));
                float intensity = max(tex.r, max(tex.g, tex.b));
                if (intensity == 0)
                {
                    return fixed4(0, 0, 0, 0);
                }
                tex.a   = min(0.9, intensity);
                tex.a   = max(0.7, intensity);
                tex.rgb = tex.rgb / tex.a;
                return tex;

                return fixed4(pos.x, pos.z, 0, 1.0);
                return fixed4(1.0, 0.0, 0.0, 1.0);
                return normalize(floor(i.worldPos));
                return normalize(floor(i.vertex / 20));
            }
            ENDCG
        }
    }
}
