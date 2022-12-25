Shader "Unlit/DistortedUnlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Distort("Distort", Vector) = (0,0,0,0)
        _MainColor("Color (RGBC)", Color) = (1,1,1,0)
        _Falloff("Falloff Distance", Float) = 200
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                half4 color : _MainTex;
                float4 _MainColor : _MainColor;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Distort;
            float4 _MainColor;
            float _Falloff;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.vertex.x = (o.vertex.x + ((o.vertex.z * o.vertex.z) * _Distort.x));
                o.vertex.y = (o.vertex.y + ((o.vertex.z * o.vertex.z) * _Distort.y));
                UNITY_TRANSFER_FOG(o,o.vertex);
                float normalized_distance = (o.vertex.z / _Falloff);
                float visibility = (1.0 - (normalized_distance * normalized_distance));
                o.color = (visibility, 0.0, 0.0, 0.0);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = (tex2D(_MainTex, i.uv) * _MainColor);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
