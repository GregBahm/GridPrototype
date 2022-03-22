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
          "LightMode" = "ForwardBase"
      }
        LOD 100

        Pass
        {
            Name "ForwardLit"
            //Blend SrcAlpha OneMinusSrcAlpha
            //ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geo
            #pragma fragment frag
            #pragma multi_compile_fwdbase
          #pragma target 4.5


          #include "UnityCG.cginc"
          #include "UnityLightingCommon.cginc"
          #include "AutoLight.cginc"

            float4 _BaseMap_ST;
            float4 _BaseColor;
            float _Cutoff;

            float _TuneA;
            float _TuneB;
            float _TuneC;
            float _Depth;

			      float3 _Color;
            float3 _ShadowColor;

            sampler2D _BottomLighting;
            sampler2D _NoiseMap;

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
                float3 worldPos : TEXCOORD2;
                float3 normal : NORMAL;
            };

            struct g2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float distToCursor : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                float dist : TEXCOORD3;
                float3 normal : NORMAL;
                SHADOW_COORDS(4)
            };

            float3 GetBoxLighting(float3 worldPos)
            {
              float3 boxPos = mul(_LightBoxTransform, float4(worldPos, 1));
              boxPos += .5;
              return tex2D(_BottomLighting, boxPos.xz).rgb;
            }

            v2g vert (appdata v)
            {
                v2g o;
                o.uv = v.uv;
                o.vertex = v.vertex;
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.distToCursor = length(worldPos - _DistToCursor);
                o.worldPos = worldPos;
                o.normal = float3(0, 1, 0);
                return o;
            }

#define SliceCount 16

            void ApplyToTristream(v2g p[3], inout TriangleStream<g2f> triStream, float dist, float offset)
            {
              float windOffset = cos(_Time.z + p[0].worldPos.x) * dist * 0.02;
              float4 vertOffset = float4(windOffset.x, offset, 0, 0);
              g2f o;
              o.normal = float3(0, 1, 0);
              o.dist = dist;
              o.uv = p[0].uv;
              o.distToCursor = p[0].distToCursor;
              o.worldPos = p[0].worldPos;
              o.pos = UnityObjectToClipPos(p[0].vertex + vertOffset);
              TRANSFER_SHADOW(o)
              triStream.Append(o);

              o.uv = p[1].uv;
              o.distToCursor = p[1].distToCursor;
              o.worldPos = p[1].worldPos;
              o.pos = UnityObjectToClipPos(p[1].vertex + vertOffset);
              TRANSFER_SHADOW(o)
              triStream.Append(o);

              o.uv = p[2].uv;
              o.distToCursor = p[2].distToCursor;
              o.worldPos = p[2].worldPos;
              o.pos = UnityObjectToClipPos(p[2].vertex + vertOffset);
              TRANSFER_SHADOW(o)
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

            //float GetSsao(float4 clipSpaceVertex)
            //{
            //  float2 normalizedScreenSpaceUv = GetNormalizedScreenSpaceUV(clipSpaceVertex);
            //  AmbientOcclusionFactor aoFactor = GetScreenSpaceAmbientOcclusion(normalizedScreenSpaceUv);
            //  return aoFactor.directAmbientOcclusion; //aoFactor.indirectAmbientOcclusion;
            //}

            float4 frag(g2f i) : SV_Target
            {
                float4 noiseA = tex2D(_NoiseMap, i.worldPos.xz * .2);
                float4 noiseB = tex2D(_NoiseMap, i.worldPos.xz * -.12);
                float4 noiseC = tex2D(_NoiseMap, i.worldPos.xz * .5);
                
                float noise = lerp(noiseA.x, max(noiseB.x ,noiseC.x), .5);
                float alphaNoise = pow(noise, 1.1);
                
                float3 ret = _Color;
                float3 boxLighting = GetBoxLighting(i.worldPos);
                half shadow = SHADOW_ATTENUATION(i);
                float ssao = 1;// GetSsao(i.vertex);
                ret *= boxLighting;
                ret = lerp(ret * _ShadowColor, ret, shadow);
                ret = lerp(ret * float3(0, 0, 1), ret, ssao);
                
                ret += float3(.5, 1, 0) * pow(noise, 2) * .5 * shadow;
                ret = lerp(ret * float3(1, 1, 0), ret, i.dist);
                
                float grid = 1 - i.uv.x;
                grid = AdjustGridLine(grid);
                float cursorPower = (1 - i.distToCursor / 40);
                cursorPower = pow(saturate(cursorPower), 20);
                float3 lineVal = lerp(ret, ret * 2, cursorPower * 1);
                ret = lerp(ret, lineVal, grid);
                
                if(i.dist > 0)
                  clip(alphaNoise - .5);
                return float4(ret, 1);
            }
            ENDCG
        }
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On ZTest LEqual

            CGPROGRAM
            #pragma target 2.0

            #pragma multi_compile_shadowcaster

            #pragma vertex vertShadowCaster
            #pragma fragment fragShadowCaster

            #include "UnityStandardShadow.cginc"

            ENDCG
        }
    }
}
