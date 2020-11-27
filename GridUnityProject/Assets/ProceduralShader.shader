Shader "ProceduralShader" {
	Properties{
		_Size("Size", Float) = 1
	}
		SubShader{

			Pass {

				CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				float _Size;
				Buffer<float3> positionBuffer;

				struct v2f
				{
					float4 pos : SV_POSITION;
					float3 basePos :TEXCOORD0;
				};

				v2f vert(appdata_base v, uint instanceID : SV_InstanceID)
				{
					float3 data = positionBuffer[instanceID];

					float3 worldPosition = data + v.vertex.xyz * _Size;
					float3 worldNormal = v.normal;

					v2f o;
					o.pos = mul(UNITY_MATRIX_VP, float4(worldPosition, 1.0f));
					o.basePos = v.vertex.xzy;
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					return float4(i.basePos, 1);
				}

				ENDCG
			}
	}
}