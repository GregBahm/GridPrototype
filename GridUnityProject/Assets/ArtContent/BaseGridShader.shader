Shader "Unlit/BaseGridShader"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _ShadowColor("Shadow Color", Color) = (1,1,1,1)
        _TuneA("Tune A", Float) = 8
        _TuneB("Tune B", Range(0, 10)) = .3
        _TuneC("Tune C", Range(0, 100)) = .9
        _Depth("Depth", Range(0, .01)) = .005
        _NoiseMap("Noise Map", 2D) = "white" {}
    }
    SubShader
    {
      Tags
      {
          "RenderType" = "Opaque"
          "RenderPipeline" = "UniversalRenderPipeline"
      }
        LOD 100

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
            Tags { "LightMode" = "UniversalForward" "Queue"="Transparent"}
            //Blend SrcAlpha OneMinusSrcAlpha
            //ZWrite Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma geometry geo
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _SCREEN_SPACE_OCCLUSION

            float _TuneA;
            float _TuneB;
            float _TuneC;
            float _Depth;

			      float3 _Color;
            float3 _ShadowColor;
            TEXTURE2D(_BottomLighting);
            SAMPLER(sampler_BottomLighting);
            TEXTURE2D(_NoiseMap);
            SAMPLER(sampler_NoiseMap);

            float3 _DistToCursor;

            float4x4 _LightBoxTransform;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2g
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float distToCursor : TEXCOORD1;
                float3 worldPos : TEXCOORD3;
            };

            struct g2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float distToCursor : TEXCOORD1;
                float3 worldPos : TEXCOORD3;
                float dist : TEXCOORD2;
            };

            float3 GetBoxLighting(float3 worldPos)
            {
              float3 boxPos = mul(_LightBoxTransform, float4(worldPos, 1));
              boxPos += .5;
              return SAMPLE_TEXTURE2D(_BottomLighting, sampler_BottomLighting, boxPos.xz).rgb;
            }

            v2g vert (appdata v)
            {
                v2g o;
                o.uv = v.uv;
                o.vertex = v.vertex;
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.distToCursor = length(worldPos - _DistToCursor);
                o.worldPos = worldPos;
                return o;
            }

#define SliceCount 16

            void ApplyToTristream(v2g p[3], inout TriangleStream<g2f> triStream, float dist, float offset)
            {
              float windOffset = cos(_Time.z + p[0].worldPos.x) * dist * 0.02;
              float4 vertOffset = float4(windOffset.x, offset, 0, 0);
              g2f o;
              o.dist = dist;
              o.uv = p[0].uv;
              o.distToCursor = p[0].distToCursor;
              o.worldPos = p[0].worldPos;
              o.vertex = TransformObjectToHClip(p[0].vertex + vertOffset);
              triStream.Append(o);

              o.uv = p[1].uv;
              o.distToCursor = p[1].distToCursor;
              o.worldPos = p[1].worldPos;
              o.vertex = TransformObjectToHClip(p[1].vertex + vertOffset);
              triStream.Append(o);

              o.uv = p[2].uv;
              o.distToCursor = p[2].distToCursor;
              o.worldPos = p[2].worldPos;
              o.vertex = TransformObjectToHClip(p[2].vertex + vertOffset);
              triStream.Append(o);
            }

            [maxvertexcount(3 * SliceCount)]
            void geo(triangle v2g p[3], inout TriangleStream<g2f> triStream)
            {
              for (int i = 0; i < SliceCount; i++)
              {
                float dist = (float)i / SliceCount;
                float offset = i * _Depth;
                ApplyToTristream(p, triStream, dist, offset);
                triStream.RestartStrip();
              }
            }

            float AdjustGridLine(float grid)
            {
                return saturate(pow(grid, _TuneA) * _TuneB - _TuneC);
            }

            float GetSsao(float4 clipSpaceVertex)
            {
              float2 normalizedScreenSpaceUv = GetNormalizedScreenSpaceUV(clipSpaceVertex);
              AmbientOcclusionFactor aoFactor = GetScreenSpaceAmbientOcclusion(normalizedScreenSpaceUv);
              return aoFactor.directAmbientOcclusion; //aoFactor.indirectAmbientOcclusion;
            }

            float4 frag(g2f i) : SV_Target
            {
                float4 noiseA = SAMPLE_TEXTURE2D(_NoiseMap, sampler_NoiseMap, i.worldPos.xz * .2);
                float4 noiseB = SAMPLE_TEXTURE2D(_NoiseMap, sampler_NoiseMap, i.worldPos.xz * -.12);
                float4 noiseC = SAMPLE_TEXTURE2D(_NoiseMap, sampler_NoiseMap, i.worldPos.xz * .5);

                float noise = lerp(noiseA.x, max(noiseB.x ,noiseC.x), .5);
                float alphaNoise = pow(noise, 1.1);

                float3 ret = _Color;
                float3 boxLighting = GetBoxLighting(i.worldPos) + .5;
                half shadow = MainLightRealtimeShadow(TransformWorldToShadowCoord(i.worldPos));
                float ssao = GetSsao(i.vertex);
                ret *= boxLighting;
                ret = lerp(ret * _ShadowColor, ret, shadow);
                ret *= ssao;

                ret += float3(1, 1, 0) * pow(noise, 2) * .5 * shadow;

                float grid = 1 - i.uv.x;
                grid = AdjustGridLine(grid);
                return grid;
                ret = lerp(ret * _ShadowColor, ret, i.dist);

                float cursorPower = (1 - i.distToCursor / 40);
                cursorPower = pow(saturate(cursorPower), 20);
                float3 lineVal = lerp(ret, ret * 2, cursorPower * 1);
                ret = lerp(ret, lineVal, grid);

                if(i.dist > 0)
                  clip(alphaNoise - .5);
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

          Varyings DisplacedDepthNormalsVertex(Attributes input)
          {
            Varyings output = (Varyings)0;
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

            output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
            VertexNormalInputs normalInput = GetVertexNormalInputs(float3(0, 1, 0), input.tangentOS);
              output.normalWS = NormalizeNormalPerVertex(normalInput.normalWS);
            return output;
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

              float3 _AnchorA;
              float3 _AnchorB;
              float3 _AnchorC;
              float3 _AnchorD;
              float _Cull;


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
    }
}
