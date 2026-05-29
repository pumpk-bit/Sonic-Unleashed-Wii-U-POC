Shader "Custom/SimpleLearningShader"
{
    // =========================================================
    // PROPERTIES
    // These appear in the Unity Inspector.
    // =========================================================
    Properties
    {
        // A texture slot in the material
        _MainTex ("Texture", 2D) = "white" {}

        // A color picker in the material
        _Color ("Tint Color", Color) = (1,1,1,1)
    }

    // =========================================================
    // SUBSHADER
    // This contains the actual rendering code.
    // =========================================================
    SubShader
    {
        // Defines how Unity treats this shader
        Tags { "RenderType"="Opaque" }

        // Level of shader complexity
        // Lower = simpler/faster
        LOD 100

        CGPROGRAM

        // =====================================================
        // SURFACE SHADER SETUP
        //
        // #pragma surface tells Unity:
        // "Generate lighting code automatically."
        //
        // 'surf' is the function name Unity will call.
        // 'Standard' means use Unity's standard lighting model.
        // =====================================================
        #pragma surface surf Standard

        // =====================================================
        // VARIABLES FROM PROPERTIES
        // =====================================================

        // Texture variable
        sampler2D _MainTex;

        // Color variable
        fixed4 _Color;

        // =====================================================
        // INPUT STRUCT
        //
        // Data sent FROM the mesh TO the shader.
        // =====================================================
        struct Input
        {
            // UV coordinates for the texture
            float2 uv_MainTex;
        };

        // =====================================================
        // SURFACE FUNCTION
        //
        // This runs for every pixel.
        //
        // IN  = input data
        // o   = output surface data
        // =====================================================
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // =================================================
            // SAMPLE THE TEXTURE
            //
            // tex2D(texture, uv)
            //
            // Reads a pixel color from the texture using UVs.
            // =================================================
            fixed4 texColor = tex2D(_MainTex, IN.uv_MainTex);

            // =================================================
            // MULTIPLY TEXTURE BY COLOR
            //
            // This lets us tint the texture.
            // =================================================
            fixed4 finalColor = texColor * _Color;

            // =================================================
            // OUTPUT VALUES
            // =================================================

            // Main visible color
            o.Albedo = finalColor.rgb;

            // Transparency
            o.Alpha = finalColor.a;

            // Metallic amount
            // 0 = non-metal
            o.Metallic = 0;

            // Smoothness
            // 0 = rough
            // 1 = shiny
            o.Smoothness = 0.2;
        }

        ENDCG
    }

    // =========================================================
    // FALLBACK
    //
    // If the GPU cannot run this shader,
    // Unity uses the Diffuse shader instead.
    // =========================================================
    FallBack "Diffuse"
}
