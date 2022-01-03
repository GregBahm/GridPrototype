Shader "Unlit/VoxelVisualShader"
{
    Properties
    {
    }
    SubShader
    {
        Pass
        {
            Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            //#pragma multi_compile_fwdbase
            //#include "AutoLight.cginc"
      
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 col : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
				        float3 baseVert : TEXCOORD1;
            //    float4 _ShadowCoord : TEXCOORD2;
                float3 worldPos : TEXCOORD3;
                float3 col : COLOR;
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

            v2f vert (appdata v)
            {
                v2f o;
				        float3 transformedVert = GetTransformedBaseVert(v.vertex);
                o.vertex = UnityObjectToClipPos(transformedVert);
				        o.uv = v.uv;
				        o.baseVert = v.vertex;
                o.col = v.col;
                o.worldPos = mul(unity_ObjectToWorld, float4(transformedVert, 1)).xyz;
            //    o._ShadowCoord = ComputeScreenPos(o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 baseLighting = GetLighting(i.worldPos);

                //float shadowness = SHADOW_ATTENUATION(i);
                float baseTone = i.col.r;
                //baseTone *= lerp(.8, 1, shadowness);
                baseTone = lerp(baseTone, .6, i.col.g);
                baseTone = lerp(baseTone, .4, i.col.b);
                
                float3 ret = baseLighting * baseTone;
				        return float4(ret, 1);
            }
            ENDHLSL
        }
       Pass
        {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}

            ZWrite On
            ColorMask 0

            Cull Back

            HLSLPROGRAM
           
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0
   
            // GPU Instancing
            #pragma multi_compile_instancing

            #pragma vertex vert
            #pragma fragment frag
               
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
               
            CBUFFER_START(UnityPerMaterial)
            CBUFFER_END
               
            struct VertexInput
            {
                float4 vertex : POSITION;                   
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

                struct VertexOutput
                {           
                float4 vertex : SV_POSITION;
                 
                UNITY_VERTEX_INPUT_INSTANCE_ID           
                UNITY_VERTEX_OUTPUT_STEREO                 
                };

            VertexOutput vert(VertexInput v)
            {
                VertexOutput o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = TransformObjectToHClip(v.vertex.xyz);

                return o;
            }

            half4 frag(VertexOutput IN) : SV_TARGET
            {       
                return 0;
            }
            ENDHLSL
        }
 
      /*
        Pass
        {
            Cull Off
            Tags {"LightMode" = "ShadowCaster"}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"


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

            struct v2f 
            {
                V2F_SHADOW_CASTER;
            };

            v2f vert(appdata_base v)
            {
                v2f o;
                v.vertex = float4(GetTransformedBaseVert(v.vertex), 1);
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
        */
    }
}
