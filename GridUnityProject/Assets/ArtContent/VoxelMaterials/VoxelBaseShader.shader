Shader "Voxel/VoxelBaseShader"
{
  Properties
  {
    _Color("Color", Color) = (1, 1, 1, 1)
    [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Float) = 2
    [HideInInspector]_AnchorA("Anchor A", Vector) = (0, 0, 0, 0)
    [HideInInspector]_AnchorB("Anchor B", Vector) = (1, 0, 0, 0)
    [HideInInspector]_AnchorC("Anchor C", Vector) = (0, 0, 1, 0)
    [HideInInspector]_AnchorD("Anchor D", Vector) = (1, 0, 1, 0)
  }
    SubShader
  {
      Tags 
      { 
          "RenderType" = "Opaque"
          "RenderPipeline" = "UniversalRenderPipeline" 
      }
      Cull[_Cull]

      HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

        CBUFFER_START(UnityPerMaterial)
          float4 _BaseMap_ST;
          float4 _BaseColor;
          float _Cutoff;
        CBUFFER_END
      ENDHLSL

      Pass
      {
        Name "ForwardLit"
        Tags { "LightMode" = "UniversalForward" }

        HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _SCREEN_SPACE_OCCLUSION

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 col : COLOR;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 baseVert : TEXCOORD1;
                float3 worldPos : TEXCOORD3;
                float3 col : COLOR;
                float3 normal : NORMAL;
            };

            CBUFFER_START(UnityPerMaterial)
            float3 _AnchorA;
            float3 _AnchorB;
            float3 _AnchorC;
            float3 _AnchorD;
            float3 _Color;
            CBUFFER_END


            float4x4 _LightBoxTransform;

            TEXTURE2D(_BottomLighting);
            SAMPLER(sampler_BottomLighting);
            TEXTURE2D(_TopLighting);
            SAMPLER(sampler_TopLighting);

            float3 GetBoxLighting(float3 worldPos)
            {
              float3 boxPos = mul(_LightBoxTransform, float4(worldPos, 1));
              boxPos += .5;
              float3 bottomSample = SAMPLE_TEXTURE2D(_BottomLighting, sampler_BottomLighting, boxPos.xz).rgb;
              float3 topSample = SAMPLE_TEXTURE2D(_TopLighting, sampler_TopLighting, boxPos.xz).rgb;
              return lerp(bottomSample, topSample, boxPos.y);
            }

            float GetBaseShade(float3 worldNormal)
            {
                float baseShade = dot(worldNormal, _MainLightPosition.xyz);
                baseShade = lerp(baseShade, 1, .9);
                return baseShade;
            }

            float3 GetTransformedBaseVert(float3 vert)
            {
                vert.xz += .5;
                float3 anchorStart = lerp(_AnchorB, _AnchorA, vert.x);
                float3 anchorEnd = lerp(_AnchorC, _AnchorD, vert.x);
                float3 flatPosition = lerp(anchorStart, anchorEnd, vert.z);
                return float3(flatPosition.x, vert.y, flatPosition.z);
            }

            v2f vert(appdata v)
            {
                v2f o;
                float3 transformedVert = GetTransformedBaseVert(v.vertex);
                o.vertex = TransformObjectToHClip(transformedVert);
                o.uv = v.uv;
                o.baseVert = v.vertex;
                o.col = v.col;
                o.normal = GetTransformedBaseVert(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, float4(transformedVert, 1)).xyz;

                return o;
            }

            float GetSsao(float4 clipSpaceVertex)
            {
                float2 normalizedScreenSpaceUv = GetNormalizedScreenSpaceUV(clipSpaceVertex);
                AmbientOcclusionFactor aoFactor = GetScreenSpaceAmbientOcclusion(normalizedScreenSpaceUv);
                return aoFactor.directAmbientOcclusion; //aoFactor.indirectAmbientOcclusion;
            }

            float4 frag(v2f i) : SV_Target
            {

                float3 worldNorm = mul(unity_ObjectToWorld, i.normal);
                worldNorm = normalize(worldNorm);
                float3 boxLighting = GetBoxLighting(i.worldPos);
                float baseShade = GetBaseShade(worldNorm);
                float ssao = GetSsao(i.vertex);
                half shadow = MainLightRealtimeShadow(TransformWorldToShadowCoord(i.worldPos));

                float3 ret = _Color;
                ret *= baseShade;
                ret *= boxLighting + .5;
                ret = lerp(ret * float3(0, .25, .5), ret, ssao);
                ret *= lerp(ret * float3(.5, .75, 1), ret, shadow);

                return float4(ret, 1);
            }
            ENDHLSL
        }
        Pass  // DepthNormals
        {
          Name "DepthNormals"
          Tags { "LightMode" = "DepthNormals" }

          ZWrite On
          ZTest LEqual

          HLSLPROGRAM
            #pragma vertex DisplacedDepthNormalsVertex
            #pragma fragment DepthNormalsFragment

            // Material Keywords
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            // GPU Instancing
            #pragma multi_compile_instancing
            //#pragma multi_compile _ DOTS_INSTANCING_ON

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthNormalsPass.hlsl"

            float3 _AnchorA;
            float3 _AnchorB;
            float3 _AnchorC;
            float3 _AnchorD;

            float3 GetTransformedBaseVert(float3 vert)
            {
                vert.xz += .5;
                float3 anchorStart = lerp(_AnchorB, _AnchorA, vert.x);
                float3 anchorEnd = lerp(_AnchorC, _AnchorD, vert.x);
                float3 flatPosition = lerp(anchorStart, anchorEnd, vert.z);
                return float3(flatPosition.x, vert.y, flatPosition.z);
            }

            Varyings DisplacedDepthNormalsVertex(Attributes input)
            {
              Varyings output = (Varyings)0;
              UNITY_SETUP_INSTANCE_ID(input);
              UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

              input.positionOS.xyz = GetTransformedBaseVert(input.positionOS.xyz);
              input.normal.xyz = GetTransformedBaseVert(input.normal.xyz);

              output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
              VertexNormalInputs normalInput = GetVertexNormalInputs(input.normal, input.tangentOS);
              output.normalWS = NormalizeNormalPerVertex(normalInput.normalWS);
              return output;
            }

          ENDHLSL
        }
          // ShadowCaster, for casting shadows
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex DisplacedShadowPassVertex
            #pragma fragment ShadowPassFragment

            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            // GPU Instancing
            #pragma multi_compile_instancing
            //#pragma multi_compile _ DOTS_INSTANCING_ON

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"

            float3 _AnchorA;
            float3 _AnchorB;
            float3 _AnchorC;
            float3 _AnchorD;

            float3 GetTransformedBaseVert(float3 vert)
            {
                vert.xz += .5;
                float3 anchorStart = lerp(_AnchorB, _AnchorA, vert.x);
                float3 anchorEnd = lerp(_AnchorC, _AnchorD, vert.x);
                float3 flatPosition = lerp(anchorStart, anchorEnd, vert.z);
                return float3(flatPosition.x, vert.y, flatPosition.z);
            }
            Varyings DisplacedShadowPassVertex(Attributes input) 
            {
              Varyings output = (Varyings)0;
              UNITY_SETUP_INSTANCE_ID(input);

              input.positionOS.xyz = GetTransformedBaseVert(input.positionOS.xyz);

              output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
              output.positionCS = GetShadowPositionHClip(input);
              return output;
            }
            ENDHLSL
        }
  }
}
