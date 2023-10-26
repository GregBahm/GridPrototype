Shader "Voxel/VoxelBaseShader"
{
  Properties
  {
    _Color("Color", Color) = (1, 1, 1, 1)
        _BaseMap("Base Map", 2D) = "white"
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
          float3 _AnchorA;
          float3 _AnchorB;
          float3 _AnchorC;
          float3 _AnchorD;
          float3 _Color;
          float _Cull;
        CBUFFER_END

      ENDHLSL

      Pass // ForwardLit
      {
        Name "ForwardLit"
        Tags { "LightMode" = "UniversalForward" }

        HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
          
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
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
                float baseShade = dot(worldNormal.xyz, _MainLightPosition.xyz);
                baseShade = saturate(baseShade * 5);
                return baseShade;
            }

            float2 GetRemapped(float2 toRemap, float2 x1y1, float2 x0y1, float2 x0y0, float2 x1y0)
            {
              float2 y0 = lerp(x0y0, x1y0, toRemap.x);
              float2 y1 = lerp(x0y1, x1y1, toRemap.x);
              return lerp(y0, y1, toRemap.y);
            }

            float3 GetTransformedBaseVert(float3 vert)
            {
                float2 toRemap = float2(vert.x + .5, 1 - (vert.z + .5));
                float2 remapped = GetRemapped(toRemap, _AnchorA.xz, _AnchorB.xz, _AnchorC.xz, _AnchorD.xz);
                return float3(remapped.x, vert.y, remapped.y);
            }

            float3 GetTransformedNormal(float3 vert, float3 normal)
            {
              float2 toRemap = float2(vert.x + .5, 1 - (vert.z + .5));

                float2 bc = normalize(_AnchorB.xz - _AnchorC.xz);
                float2 ad = normalize(_AnchorA.xz - _AnchorD.xz);
                float2 ab = normalize(_AnchorA.xz - _AnchorB.xz);
                float2 dc = normalize(_AnchorD.xz - _AnchorC.xz);

                float2 newZ = lerp(bc, ad, toRemap.x);
                float2 newX = lerp(ab, dc, toRemap.y);

                float2 a = newX + newZ;
                float2 b = newZ;
                float2 c = 0;
                float2 d = newX;

                float2 remapped = GetRemapped(normal.xz, a, d, c, b);
                if (_Cull < 2)
                  remapped *= -1;
                return float3(-remapped.y, normal.y, remapped.x);
            }

            v2f vert(appdata v)
            {
                v2f o;
                float3 transformedVert = GetTransformedBaseVert(v.vertex);
                o.vertex = TransformObjectToHClip(transformedVert);
                o.uv = v.uv;
                o.baseVert = v.vertex;
                o.col = v.col;
                o.normal = GetTransformedNormal(v.vertex, v.normal);
                o.worldPos = mul(unity_ObjectToWorld, float4(transformedVert, 1)).xyz;

                return o;
            }

            float GetSsao(float4 clipSpaceVertex)
            {
                float2 normalizedScreenSpaceUv = GetNormalizedScreenSpaceUV(clipSpaceVertex);
                AmbientOcclusionFactor aoFactor = GetScreenSpaceAmbientOcclusion(normalizedScreenSpaceUv);
                return aoFactor.directAmbientOcclusion; // aoFactor.indirectAmbientOcclusion;
            }

            float4 frag(v2f i) : SV_Target
            {
                float3 boxLighting = GetBoxLighting(i.worldPos);
                float baseShade = GetBaseShade(i.normal);
                float ssao = GetSsao(i.vertex);
                half shadow = MainLightRealtimeShadow(TransformWorldToShadowCoord(i.worldPos));
                shadow = saturate(shadow *  5);
                shadow = min(shadow, baseShade);
                float3 ret = _Color * 1.25;
                ret *= lerp(boxLighting * .75, 1, .5);
                ret *= lerp(ret * float3(0.3, .6, 1), ret, shadow);
                float shadedSsao = lerp(pow(ssao, 2), pow(ssao, .5), shadow);
                ret *= shadedSsao;
                return float4(ret, 1);
            }
            ENDHLSL
        }
        Pass// DepthOnly
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ColorMask 0
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex DisplacedDepthOnlyVertex
            #pragma fragment DepthOnlyFragment

          // Material Keywords
          #pragma shader_feature_local_fragment _ALPHATEST_ON
          #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

          // GPU Instancing
          #pragma multi_compile_instancing
          //#pragma multi_compile _ DOTS_INSTANCING_ON

          #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
          #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
          #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"


          float2 GetRemapped(float2 toRemap, float2 x1y1, float2 x0y1, float2 x0y0, float2 x1y0)
          {
            float2 y0 = lerp(x0y0, x1y0, toRemap.x);
            float2 y1 = lerp(x0y1, x1y1, toRemap.x);
            return lerp(y0, y1, toRemap.y);
          }

          float3 GetTransformedBaseVert(float3 vert)
          {
            float2 toRemap = float2(vert.x + .5, 1 - (vert.z + .5));
            float2 remapped = GetRemapped(toRemap, _AnchorA.xz, _AnchorB.xz, _AnchorC.xz, _AnchorD.xz);
            return float3(remapped.x, vert.y, remapped.y);
          }

          Varyings DisplacedDepthOnlyVertex(Attributes input)
          {
            Varyings output = (Varyings)0;
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

            // Example Displacement
            input.position.xyz = GetTransformedBaseVert(input.position.xyz);

            output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
            output.positionCS = TransformObjectToHClip(input.position.xyz);
            return output;
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


            float2 GetRemapped(float2 toRemap, float2 x1y1, float2 x0y1, float2 x0y0, float2 x1y0)
            {
              float2 y0 = lerp(x0y0, x1y0, toRemap.x);
              float2 y1 = lerp(x0y1, x1y1, toRemap.x);
              return lerp(y0, y1, toRemap.y);
            }

            float3 GetTransformedBaseVert(float3 vert)
            {
              float2 toRemap = float2(vert.x + .5, 1 - (vert.z + .5));
              float2 remapped = GetRemapped(toRemap, _AnchorA.xz, _AnchorB.xz, _AnchorC.xz, _AnchorD.xz);
              return float3(remapped.x, vert.y, remapped.y);
            }

            float3 GetTransformedNormal(float3 vert, float3 normal)
            {
              float2 toRemap = float2(vert.x + .5, 1 - (vert.z + .5));

              float2 bc = normalize(_AnchorB.xz - _AnchorC.xz);
              float2 ad = normalize(_AnchorA.xz - _AnchorD.xz);
              float2 ab = normalize(_AnchorA.xz - _AnchorB.xz);
              float2 dc = normalize(_AnchorD.xz - _AnchorC.xz);

              float2 newZ = lerp(bc, ad, toRemap.x);
              float2 newX = lerp(ab, dc, toRemap.y);

              float2 a = newX + newZ;
              float2 b = newZ;
              float2 c = 0;
              float2 d = newX;

              float2 remapped = GetRemapped(normal.xz, a, d, c, b);
              if (_Cull < 2)
                remapped *= -1;
              return float3(-remapped.y, normal.y, remapped.x);
            }

            Varyings DisplacedDepthNormalsVertex(Attributes input)
            {
              Varyings output = (Varyings)0;
              UNITY_SETUP_INSTANCE_ID(input);
              UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

              float3 baseVert = input.positionOS.xyz;
              input.positionOS.xyz = GetTransformedBaseVert(baseVert);

              input.normal = GetTransformedNormal(baseVert, input.normal);
              output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
              
              VertexNormalInputs normalInput = GetVertexNormalInputs(input.normal, input.tangentOS);
              output.normalWS = NormalizeNormalPerVertex(normalInput.normalWS);
              return output;
            }

          ENDHLSL
        }
        Pass // ShadowCaster, for casting shadows
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
