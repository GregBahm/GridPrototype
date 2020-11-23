Shader "Unlit/VoxelVisualShader"
{
    Properties
    {
    }
    SubShader
    {
        Pass
        {
            Cull Off
            Tags { "RenderType" = "Opaque" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
				float3 baseVert : TEXCOORD1;
            };

            float3 _AnchorA;
			float3 _AnchorB;
			float3 _AnchorC;
			float3 _AnchorD;

			float3 GetTransformedBaseVert(float3 vert)
			{
				vert *= .25;
				vert += .5;
                vert.x = 1 - vert.x;

				float3 anchorStart = lerp(_AnchorA, _AnchorB, vert.x);
				float3 anchorEnd = lerp(_AnchorD, _AnchorC, vert.x);
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
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				return 1;
            }
            ENDCG
        }
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
                vert *= .25;
                vert += .5;
                vert.x = 1 - vert.x;

                float3 anchorStart = lerp(_AnchorA, _AnchorB, vert.x);
                float3 anchorEnd = lerp(_AnchorD, _AnchorC, vert.x);
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
    }
}
