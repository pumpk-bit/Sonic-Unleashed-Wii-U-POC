Shader "Custom/ScrollingTexture"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ScrollX ("Scroll Speed X", Float) = 0.1
        _ScrollY ("Scroll Speed Y", Float) = 0.0
        _TilingX ("Tiling X", Float) = 1.0
        _TilingY ("Tiling Y", Float) = 1.0
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

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _ScrollX;
            float _ScrollY;

            float _TilingX;
            float _TilingY;

            v2f vert (appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);

                // Repeat texture
                float2 uv = v.uv;
                uv.x *= _TilingX;
                uv.y *= _TilingY;

                // Move texture over time
                uv.x += _Time.y * _ScrollX;
                uv.y += _Time.y * _ScrollY;

                o.uv = uv;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return tex2D(_MainTex, i.uv);
            }

            ENDCG
        }
    }
}
