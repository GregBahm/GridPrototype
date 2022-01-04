Shader "URP Piece Shader"
{
  Properties
  {
    _Test("Test", Float) = 0
  }

    SubShader
    {
      // With SRP we introduce a new "RenderPipeline" tag in Subshader. This allows to create shaders
      // that can match multiple render pipelines. If a RenderPipeline tag is not set it will match
      // any render pipeline. In case you want your subshader to only run in LWRP set the tag to
      // "UniversalRenderPipeline"
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "UniversalMaterialType" = "Lit"
            "Queue" = "Geometry"
        }
      LOD 300

        Pass //Universal Forward
        {
            Name "Universal Forward"
            Tags
            {
                "LightMode" = "UniversalForward"
            }

      // Render State
      Cull Back
      Blend One Zero
      ZTest LEqual
      ZWrite On

      HLSLPROGRAM

      // Pragmas
  #pragma vertex vert
  #pragma fragment frag

      // Keywords
      #pragma multi_compile _ _SCREEN_SPACE_OCCLUSION
        #pragma multi_compile _ LIGHTMAP_ON
        #pragma multi_compile _ DIRLIGHTMAP_COMBINED
        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
        #pragma multi_compile _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS _ADDITIONAL_OFF
        #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
        #pragma multi_compile _ _SHADOWS_SOFT
        #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
        #pragma multi_compile _ SHADOWS_SHADOWMASK
      // GraphKeywords: <None>

      // Defines
            #define _NORMALMAP 1
            #define _NORMAL_DROPOFF_TS 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD1
            #define VARYINGS_NEED_POSITION_WS
            #define VARYINGS_NEED_NORMAL_WS
            #define VARYINGS_NEED_TANGENT_WS
            #define VARYINGS_NEED_VIEWDIRECTION_WS
            #define VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_FORWARD
            /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

      // Includes
      #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
  #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
  #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
  #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
  #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
  #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
  #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

      // --------------------------------------------------
      // Structs and Packing

      struct Attributes
  {
      float3 positionOS : POSITION;
      float3 normalOS : NORMAL;
      float4 tangentOS : TANGENT;
      float4 uv1 : TEXCOORD1;
      #if UNITY_ANY_INSTANCING_ENABLED
      uint instanceID : INSTANCEID_SEMANTIC;
      #endif
  };
  struct Varyings
  {
      float4 positionCS : SV_POSITION;
      float3 positionWS;
      float3 normalWS;
      float4 tangentWS;
      float3 viewDirectionWS;
      #if defined(LIGHTMAP_ON)
      float2 lightmapUV;
      #endif
      #if !defined(LIGHTMAP_ON)
      float3 sh;
      #endif
      float4 fogFactorAndVertexLight;
      float4 shadowCoord;
      #if UNITY_ANY_INSTANCING_ENABLED
      uint instanceID : CUSTOM_INSTANCE_ID;
      #endif
      #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
      uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
      #endif
      #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
      uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
      #endif
      #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
      FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
      #endif
  };
  struct SurfaceDescriptionInputs
  {
      float3 TangentSpaceNormal;
  };
  struct VertexDescriptionInputs
  {
      float3 ObjectSpaceNormal;
      float3 ObjectSpaceTangent;
      float3 ObjectSpacePosition;
  };
  struct PackedVaryings
  {
      float4 positionCS : SV_POSITION;
      float3 interp0 : TEXCOORD0;
      float3 interp1 : TEXCOORD1;
      float4 interp2 : TEXCOORD2;
      float3 interp3 : TEXCOORD3;
      #if defined(LIGHTMAP_ON)
      float2 interp4 : TEXCOORD4;
      #endif
      #if !defined(LIGHTMAP_ON)
      float3 interp5 : TEXCOORD5;
      #endif
      float4 interp6 : TEXCOORD6;
      float4 interp7 : TEXCOORD7;
      #if UNITY_ANY_INSTANCING_ENABLED
      uint instanceID : CUSTOM_INSTANCE_ID;
      #endif
      #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
      uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
      #endif
      #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
      uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
      #endif
      #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
      FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
      #endif
  };

  // Graph Properties
  CBUFFER_START(UnityPerMaterial)
    float _Test;
  CBUFFER_END
      PackedVaryings PackVaryings(Varyings input)
  {
      PackedVaryings output;
      output.positionCS = input.positionCS;
      output.interp0.xyz = input.positionWS;
      output.interp1.xyz = input.normalWS;
      output.interp2.xyzw = input.tangentWS;
      output.interp3.xyz = input.viewDirectionWS;
      #if defined(LIGHTMAP_ON)
      output.interp4.xy = input.lightmapUV;
      #endif
      #if !defined(LIGHTMAP_ON)
      output.interp5.xyz = input.sh;
      #endif
      output.interp6.xyzw = input.fogFactorAndVertexLight;
      output.interp7.xyzw = input.shadowCoord;
      #if UNITY_ANY_INSTANCING_ENABLED
      output.instanceID = input.instanceID;
      #endif
      #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
      output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
      #endif
      #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
      output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
      #endif
      #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
      output.cullFace = input.cullFace;
      #endif
      return output;
  }
  Varyings UnpackVaryings(PackedVaryings input)
  {
      Varyings output;
      output.positionCS = input.positionCS;
      output.positionWS = input.interp0.xyz;
      output.normalWS = input.interp1.xyz;
      output.tangentWS = input.interp2.xyzw;
      output.viewDirectionWS = input.interp3.xyz;
      #if defined(LIGHTMAP_ON)
      output.lightmapUV = input.interp4.xy;
      #endif
      #if !defined(LIGHTMAP_ON)
      output.sh = input.interp5.xyz;
      #endif
      output.fogFactorAndVertexLight = input.interp6.xyzw;
      output.shadowCoord = input.interp7.xyzw;
      #if UNITY_ANY_INSTANCING_ENABLED
      output.instanceID = input.instanceID;
      #endif
      #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
      output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
      #endif
      #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
      output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
      #endif
      #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
      output.cullFace = input.cullFace;
      #endif
      return output;
  }

  // --------------------------------------------------
  // Graph


// Object and Global properties

    // Graph Functions
    // GraphFunctions: <None>

    // Graph Vertex
    struct VertexDescription
{
    float3 Position;
    float3 Normal;
    float3 Tangent;
};

VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
{
    VertexDescription description = (VertexDescription)0;
    description.Position = IN.ObjectSpacePosition;
    description.Normal = IN.ObjectSpaceNormal;
    description.Tangent = IN.ObjectSpaceTangent;
    return description;
}

// Graph Pixel
struct SurfaceDescription
{
    float3 BaseColor;
    float3 NormalTS;
    float3 Emission;
    float Metallic;
    float Smoothness;
    float Occlusion;
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    surface.BaseColor = IsGammaSpace() ? float3(0.5, 0.5, 0.5) : SRGBToLinear(float3(0.5, 0.5, 0.5));
    surface.NormalTS = IN.TangentSpaceNormal;
    surface.Emission = float3(0, 0, 0);
    surface.Metallic = 0;
    surface.Smoothness = 0.5;
    surface.Occlusion = 1;
    return surface;
}

// --------------------------------------------------
// Build Graph Inputs

VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
{
    VertexDescriptionInputs output;
    ZERO_INITIALIZE(VertexDescriptionInputs, output);

    output.ObjectSpaceNormal = input.normalOS;
    output.ObjectSpaceTangent = input.tangentOS.xyz;
    output.ObjectSpacePosition = input.positionOS ;

    return output;
}
    SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
{
    SurfaceDescriptionInputs output;
    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);



    output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);


#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
#else
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
#endif
#undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

    return output;
}

    // --------------------------------------------------
    // Main

    #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBRForwardPass.hlsl"

    ENDHLSL
}
      // Used for rendering shadowmaps
      UsePass "Universal Render Pipeline/Lit/ShadowCaster"

              // Used for depth prepass
              // If shadows cascade are enabled we need to perform a depth prepass. 
              // We also need to use a depth prepass in some cases camera require depth texture
              // (e.g, MSAA is enabled and we can't resolve with Texture2DMS
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
        UsePass "Universal Render Pipeline/Lit/DepthNormals" // - Greg Added this to no avail
    }

}