Shader "Unlit/ProceduralShader"
{
  Properties
  {
  }
  SubShader
  {
      Tags { "RenderType" = "Opaque" }
			Pass
			{
				CGPROGRAM
				#pragma vertex vert  
				#pragma fragment frag
				#pragma target 5.0
				#pragma multi_compile_instancing

				#include "UnityCG.cginc"

				StructuredBuffer<float4> colors;

				struct v2f
				{
					float4 pos : SV_POSITION;
					float4 col : TEXCOORD1;
				};

				v2f vert(appdata_full v, uint inst : SV_InstanceID)
				{
						v2f o;
						o.pos = UnityObjectToClipPos(v.vertex);
						o.col = colors[inst];
						return o;
				}

				fixed4 frag(v2f i) : COLOR
				{
						return i.col;
				}
				ENDCG
			}
  }
}