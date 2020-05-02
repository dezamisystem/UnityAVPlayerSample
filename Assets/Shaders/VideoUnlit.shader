Shader "Unlit/VideoUnlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [Toggle] _FLIP_X("Flip X", Float) = 0
        [Toggle] _FLIP_Y("Flip Y", Float) = 0
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
            // Flip
            #pragma shader_feature _ _FLIP_X_ON
            #pragma shader_feature _ _FLIP_Y_ON

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
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float2 uv = i.uv;
                // Flip
#ifdef _FLIP_X_ON
                uv.x = 1.0 - uv.x;
#endif
#ifdef _FLIP_Y_ON
                uv.y = 1.0 - uv.y;
#endif
                fixed4 col = tex2D(_MainTex, uv);
                // fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                // UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
