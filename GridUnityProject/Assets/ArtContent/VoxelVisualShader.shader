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
            #pragma multi_compile_fwdbase
            #include "AutoLight.cginc"

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
                float4 _ShadowCoord : TEXCOORD2;
                float3 worldPos : TEXCOORD3;
                float3 col : COLOR;
            };

            float3 _AnchorA;
			float3 _AnchorB;
			float3 _AnchorC;
			float3 _AnchorD; 

			float3 GetTransformedBaseVert(float3 vert)
			{
                vert.y *= .5;
				vert.xz += .5;
                //vert.x = 1 - vert.x;

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
                o._ShadowCoord = ComputeScreenPos(o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float shadowness = SHADOW_ATTENUATION(i);
                float ret = i.col.r;
                //ret *= lerp(.8, 1, shadowness);

                ret += (i.worldPos.y * .1) - .2;
                ret = lerp(ret, .6, i.col.g);
                ret = lerp(ret, .4, i.col.b);
				return float4(ret.xxx, 1);
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
                vert.y *= .5;
                vert.xz += .5;
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
