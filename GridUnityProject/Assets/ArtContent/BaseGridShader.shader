Shader "Unlit/BaseGridShader"
{
    Properties
    {
		_Color("Color", Color) = (1,1,1,1)
        _TuneA("Tune A", Float) = 8
        _TuneB("Tune B", Range(0, 10)) = .3
        _TuneC("Tune C", Range(0, 100)) = .9 
    }
    SubShader
    { 
		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
        Tags { "RenderType"="Opaque" }
        LOD 100
        Pass
        {
            Tags {"LightMode" = "ForwardBase"}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#pragma multi_compile_fwdbase
			#include "AutoLight.cginc"

            #include "UnityCG.cginc"

            float _TuneA;
            float _TuneB;
            float _TuneC;

			float3 _Color;

            float3 _DistToCursor;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float distToCursor : TEXCOORD1;
				float4 _ShadowCoord : TEXCOORD2;
                float3 worldPos : TEXCOORD3;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.uv = v.uv;
                o.vertex = UnityObjectToClipPos(v.vertex);
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.distToCursor = length(worldPos - _DistToCursor);
				o._ShadowCoord = ComputeScreenPos(o.vertex);
                o.worldPos = worldPos;
                return o;
            }

            float AdjustGridLine(float grid)
            {
                return saturate(pow(grid, _TuneA) * _TuneB - _TuneC);
            }

            fixed4 frag(v2f i) : SV_Target
            {
				float shadowness = SHADOW_ATTENUATION(i);

                float alpha = (1 - i.distToCursor / 40);
                alpha = pow(saturate(alpha), 20);

                float gridAroundCells = i.uv.y;
                gridAroundCells = AdjustGridLine(gridAroundCells);
                float gridThroughCells = 1 - i.uv.x; 
                gridThroughCells = AdjustGridLine(gridThroughCells);
                float grid = gridAroundCells;
                //return gridAroundCells;
                
                shadowness = lerp(shadowness, 1, .5);
				float3 ret = _Color * shadowness;
                ret += i.worldPos.y * .05;
                float height = saturate(1 - i.worldPos.y * 10);
                float3 lineVal = lerp(ret, 1, alpha * .2 * height);
				ret = lerp(ret, lineVal, grid);
                return float4(ret, 1);
            }
            ENDCG
        }
    }
}
