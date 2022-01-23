﻿Shader "Voxel/VoxelWallShader"
{
  Properties
  {
    [MainColor] _BaseColor("Color", Color) = (1, 1, 1, 1)
    _BaseMap("Base Map", 2D) = "white" {}
    _NoiseMap("Noise Map", 2D) = "white" {}
    [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Float) = 2
    [HideInInspector]_AnchorA("Anchor A", Vector) = (0, 0, 0, 0)
    [HideInInspector]_AnchorB("Anchor B", Vector) = (1, 0, 0, 0)
    [HideInInspector]_AnchorC("Anchor C", Vector) = (0, 0, 1, 0)
    [HideInInspector]_AnchorD("Anchor D", Vector) = (1, 0, 1, 0)
  }
    SubShader
  {
      Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline" }
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
           //#pragma multi_compile_fwdbase
           //#include "AutoLight.cginc"

           #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
           #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
           #pragma multi_compile _ _SHADOWS_SOFT

           #pragma multi_compile _ _SCREEN_SPACE_OCCLUSION

            struct appdata
            {
                float4 vertex : POSITION;
                float3 col : COLOR;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 baseVert : TEXCOORD1;
                float3 baseNormal : TEXCOORD4;
                float3 worldPos : TEXCOORD3;
                float3 col : COLOR;
                float3 normal : NORMAL;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_NoiseMap);
            SAMPLER(sampler_NoiseMap);

            CBUFFER_START(UnityPerMaterial)
            float3 _AnchorA;
            float3 _AnchorB;
            float3 _AnchorC;
            float3 _AnchorD;
            CBUFFER_END

            float4x4 _LightBoxTransform;
            sampler2D _TopLighting;
            sampler2D _BottomLighting;

            float3 GetLighting(float3 worldPos, float3 worldNormal)
            {
                float baseShade = dot(worldNormal, float3(0, 1, .5));
                baseShade = lerp(baseShade, 1, .9);
                return baseShade;
                float3 boxPos = mul(_LightBoxTransform, float4(worldPos, 1));
                boxPos = boxPos / 2 + .5;
                return boxPos;
            }

            float3 GetTransformedBaseVert(float3 vert)
            {
                vert.xz += .5;
                float3 anchorStart = lerp(_AnchorB, _AnchorA, vert.x);
                float3 anchorEnd = lerp(_AnchorC, _AnchorD, vert.x);
                float3 flatPosition = lerp(anchorStart, anchorEnd, vert.z);
                return float3(flatPosition.x, vert.y, flatPosition.z);
            }

            float2 GetUv(float3 objSpace, float3 normal)
            {
              float2 xy = objSpace.xy;
              float2 zy = objSpace.zy;
              float2 ret = lerp(xy, zy, normal.x);
              return ret;
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.uv = GetUv(v.vertex, v.normal);
                float3 transformedVert = GetTransformedBaseVert(v.vertex);
                o.vertex = TransformObjectToHClip(transformedVert);
                o.baseVert = v.vertex;
                o.col = v.col;
                o.baseNormal = v.normal;
                o.normal = GetTransformedBaseVert(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, float4(transformedVert, 1)).xyz;
                return o;
            }

            float GetSsao(float4 clipSpaceVertex)
            {
                ///struct AmbientOcclusionFactor
                //{
                //  half indirectAmbientOcclusion;
                //  half directAmbientOcclusion;
                //};

                float2 normalizedScreenSpaceUv = GetNormalizedScreenSpaceUV(clipSpaceVertex);
                AmbientOcclusionFactor aoFactor = GetScreenSpaceAmbientOcclusion(normalizedScreenSpaceUv);
                return aoFactor.directAmbientOcclusion;
            }

            float GetNoise(float3 worldPos)
            {
              worldPos *= float3(.5, .4, .5);
              float2 uvA = worldPos.xy;
              float noiseA = SAMPLE_TEXTURE2D(_NoiseMap, sampler_NoiseMap, uvA).r;
              
              float2 uvB = worldPos.zy;
              float noiseB = SAMPLE_TEXTURE2D(_NoiseMap, sampler_NoiseMap, uvB).r;

              float2 uvC = worldPos.xz;
              float noiseC = SAMPLE_TEXTURE2D(_NoiseMap, sampler_NoiseMap, uvC).r;

              //return noiseC;
              float ret = lerp(noiseA, noiseB, noiseC);
              ret = ret * 40 - 10;
              ret = saturate(ret);
              ret = pow(ret, 4);
              return ret;
            }

            float4 frag(v2f i) : SV_Target
            {
                float3 worldNorm = mul(unity_ObjectToWorld, i.normal);
                worldNorm = normalize(worldNorm);

                float3 ret = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv * 10).rgb;
                ret = lerp(ret, 1, .75);

                float noise = GetNoise( i.worldPos);
                //return noise;
                ret = lerp(ret, 1, noise);
                float ssao = GetSsao(i.vertex);
                float3 lighting = GetLighting(i.worldPos, worldNorm);

                ret *= _BaseColor.rgb;
                ret *= lighting;
                ret *= ssao;

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
            //
            float3 GetTransformedBaseVert(float3 vert)
            {
                vert.xz += .5;
                float3 anchorStart = lerp(_AnchorB, _AnchorA, vert.x);
                float3 anchorEnd = lerp(_AnchorC, _AnchorD, vert.x);
                float3 flatPosition = lerp(anchorStart, anchorEnd, vert.z);
                return float3(flatPosition.x, vert.y, flatPosition.z);
            }
              // Note if we do any vertex displacement, we'll need to change the vertex function. e.g. :
            Varyings DisplacedDepthNormalsVertex(Attributes input)
            {
              Varyings output = (Varyings)0;
              UNITY_SETUP_INSTANCE_ID(input);
              UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

              input.positionOS.xyz = GetTransformedBaseVert(input.positionOS.xyz);
              input.normal.xyz = GetTransformedBaseVert(input.normal.xyz);

              output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
              output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
              VertexNormalInputs normalInput = GetVertexNormalInputs(input.normal, input.tangentOS);
                output.normalWS = NormalizeNormalPerVertex(normalInput.normalWS);
              return output;
            }

          ENDHLSL
        }
  }
}