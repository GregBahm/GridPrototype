Shader "Unlit/BaseGridShader"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _ShadowColor("Shadow Color", Color) = (1,1,1,1)
        _TuneA("Tune A", Float) = 8
        _TuneB("Tune B", Range(0, 10)) = .3
        _TuneC("Tune C", Range(0, 100)) = .9 
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
        Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _SCREEN_SPACE_OCCLUSION

            float _TuneA;
            float _TuneB;
            float _TuneC;

			float3 _Color;
      float3 _ShadowColor;

            float3 _DistToCursor;

            float4x4 _LightBoxTransform;
            sampler2D _TopLighting;
            sampler2D _BottomLighting;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float distToCursor : TEXCOORD1;
                float3 worldPos : TEXCOORD3;
            };

            float3 GetLighting(float3 worldPos)
            {
                float3 boxPos = mul(_LightBoxTransform, float4(worldPos, 1));
                boxPos = boxPos / 2 + .5;
                return boxPos;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.uv = v.uv;
                o.vertex = TransformObjectToHClip(v.vertex);
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.distToCursor = length(worldPos - _DistToCursor);
                o.worldPos = worldPos;
                return o;
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

            float4 frag(v2f i) : SV_Target
            {
                float3 ret = _Color;//  GetLighting(i.worldPos);
                half shadow = MainLightRealtimeShadow(TransformWorldToShadowCoord(i.worldPos));
                float ssao = GetSsao(i.vertex);
                ret = lerp(ret * _ShadowColor, ret, shadow);
                ret *= ssao;

                float alpha = (1 - i.distToCursor / 40);
                alpha = pow(saturate(alpha), 20);

                float grid = 1 - i.uv.x;
                grid = AdjustGridLine(grid);
                float3 lineVal = lerp(ret, 1, alpha * .2 );
				        ret = lerp(ret, lineVal, grid);
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
    }
}
