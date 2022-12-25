// Upgrade NOTE: replaced '_Projector' with 'unity_Projector'
// Upgrade NOTE: replaced '_ProjectorClip' with 'unity_ProjectorClip'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Projector/Multiply Atlas" {
    Properties{
        _ShadowTex("Cookie", 2D) = "gray" {}
        [NoScaleOffset] _FalloffTex("FallOff", 2D) = "white" {}
    }
        Subshader{
            Tags {"Queue" = "Transparent"}
            Pass {
                ZWrite Off
                ColorMask RGB
                Blend DstColor Zero
                Offset -1, -1

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_fog
                #include "UnityCG.cginc"

                struct v2f {
                    float4 uvShadow : TEXCOORD0;
                    float4 uvFalloff : TEXCOORD1;
                    UNITY_FOG_COORDS(2)
                    float4 pos : SV_POSITION;
                };

                float4x4 unity_Projector;
                float4x4 unity_ProjectorClip;

                v2f vert(float4 vertex : POSITION)
                {
                    v2f o;
                    o.pos = UnityObjectToClipPos(vertex);
                    o.uvShadow = mul(unity_Projector, vertex);
                    o.uvFalloff = mul(unity_ProjectorClip, vertex);
                    UNITY_TRANSFER_FOG(o,o.pos);
                    return o;
                }
                float4 _ShadowTex_ST;
                sampler2D _ShadowTex;
                sampler2D _FalloffTex;

                fixed4 frag(v2f i) : SV_Target
                {
                    float2 texSUV = i.uvShadow.xy / i.uvShadow.w;
                    fixed4 texS = tex2D(_ShadowTex, TRANSFORM_TEX(texSUV, _ShadowTex));

                    float2 maskUV = 1.0 - abs(texSUV * 2.0 - 1.0);
                    maskUV = saturate(maskUV / fwidth(texSUV * 2.0));
                    texS.a *= maskUV.x * maskUV.y;

                    texS.a = 1.0 - texS.a;

                    fixed4 texF = tex2Dproj(_FalloffTex, UNITY_PROJ_COORD(i.uvFalloff));
                    fixed4 res = lerp(fixed4(1,1,1,0), texS, texF.a);

                    UNITY_APPLY_FOG_COLOR(i.fogCoord, res, fixed4(1,1,1,1));
                    return res;
                }
                ENDCG
            }
    }
}