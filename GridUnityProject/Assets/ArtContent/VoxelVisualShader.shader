Shader "Unlit/VoxelVisualShader"
{
  Properties
  {
    [MainTexture] _BaseMap("Base Map (RGB) Smoothness / Alpha (A)", 2D) = "white" {}
    [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Float) = 2
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
          float4 _EmissionColor;
          float4 _SpecColor;
          float _Cutoff;
          float _Smoothness;
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
            CBUFFER_END

            float4x4 _LightBoxTransform;
            sampler2D _TopLighting;
            sampler2D _BottomLighting;

            float3 GetLighting(float3 worldPos)
            {
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
                ///struct AmbientOcclusionFactor
                //{
                //  half indirectAmbientOcclusion;
                //  half directAmbientOcclusion;
                //};

                float2 normalizedScreenSpaceUv = GetNormalizedScreenSpaceUV(clipSpaceVertex);
                AmbientOcclusionFactor aoFactor = GetScreenSpaceAmbientOcclusion(normalizedScreenSpaceUv);
                return aoFactor.directAmbientOcclusion;
            }

            float4 frag(v2f i) : SV_Target
            {
                float3 baseLighting = GetLighting(i.worldPos);

                float3 worldNorm = mul(unity_ObjectToWorld, i.normal);
                worldNorm = normalize(worldNorm);
                //return float4(i.normal * .5 + .5, 1);

                //float shadowness = SHADOW_ATTENUATION(i);
                float baseTone = i.col.r;
                //baseTone *= lerp(.8, 1, shadowness);
                baseTone = lerp(baseTone,  2, i.col.b);
                baseTone = lerp(baseTone, .4, i.col.r);

                float3 ret = baseLighting * baseTone;
                float ssao = GetSsao(i.vertex);
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
