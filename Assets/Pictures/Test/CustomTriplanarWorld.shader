Shader "Custom/TriplanarWorld"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Scale ("Scale", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows

        sampler2D _MainTex;
        float _Scale;

        struct Input
        {
            float3 worldPos;
            float3 worldNormal;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float3 n = abs(normalize(IN.worldNormal));

            float2 xUV = IN.worldPos.zy * _Scale;
            float2 yUV = IN.worldPos.xz * _Scale;
            float2 zUV = IN.worldPos.xy * _Scale;

            fixed4 x = tex2D(_MainTex, xUV);
            fixed4 y = tex2D(_MainTex, yUV);
            fixed4 z = tex2D(_MainTex, zUV);

            o.Albedo = x.rgb * n.x + y.rgb * n.y + z.rgb * n.z;
            o.Alpha = 1;
        }
        ENDCG
    }

    FallBack "Diffuse"
}