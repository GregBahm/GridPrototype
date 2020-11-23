Shader "Unlit/VoxelVisualShader"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Pass
        {
		Cull Off
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
				return float4(i.baseVert, 1);
            }
            ENDCG
        }
    }
}
