Shader "Voxel/ProceduralVoxelShader"
{
  Properties
  {
    _Color("Color", Color) = (1, 1, 1, 1)
  }
  SubShader
  {

      Cull Off

      Pass {

          Tags {"LightMode" = "ForwardBase"}

          CGPROGRAM

          #pragma vertex vert
          #pragma fragment frag
          #pragma multi_compile_fwdbase
          #pragma target 4.5

          #include "UnityCG.cginc"
          #include "UnityLightingCommon.cginc"
          #include "AutoLight.cginc"
          
          fixed4 _Color;
          sampler2D _BottomLighting;
          sampler2D _TopLighting;
          float4x4 _LightBoxTransform;

          struct VoxelRenderData
          {
              float2 AnchorA;
              float2 AnchorB;
              float2 AnchorC;
              float2 AnchorD;
              float Height;
              float FlipNormal;
          };

          struct v2f
          {
              float4 pos : SV_POSITION;
              float2 uv : TEXCOORD0;
              float3 normal : NORMAL;
              float3 worldPos : TEXCOORD1;
              float3 viewDir : VIEWDIR;
              SHADOW_COORDS(3)
          };

          StructuredBuffer<VoxelRenderData> _RenderDataBuffer;


          float2 GetRemapped(float2 toRemap, float2 x1y1, float2 x0y1, float2 x0y0, float2 x1y0)
          {
            float2 y0 = lerp(x0y0, x1y0, toRemap.x);
            float2 y1 = lerp(x0y1, x1y1, toRemap.x);
            return lerp(y0, y1, toRemap.y);
          }

          float3 GetTransformedBaseVert(float3 vert, 
            float2 anchorA, 
            float2 anchorB, 
            float2 anchorC, 
            float2 anchorD)
          {
            float2 toRemap = float2(vert.x + .5, 1 - (vert.z + .5));
            float2 remapped = GetRemapped(toRemap, anchorA, anchorB, anchorC, anchorD);
            return float3(remapped.x, vert.y, remapped.y);
          }

          float3 GetTransformedNormal(float3 vert, 
            float3 normal,
            float2 anchorA,
            float2 anchorB,
            float2 anchorC,
            float2 anchorD,
            float flipNormal)
          {
            float2 toRemap = float2(vert.x + .5, 1 - (vert.z + .5));

            float2 bc = normalize(anchorB - anchorC);
            float2 ad = normalize(anchorA - anchorD);
            float2 ab = normalize(anchorA - anchorB);
            float2 dc = normalize(anchorD - anchorC);

            float2 newZ = lerp(bc, ad, toRemap.x);
            float2 newX = lerp(ab, dc, toRemap.y);

            float2 a = newX + newZ;
            float2 b = newZ;
            float2 c = 0;
            float2 d = newX;

            float2 remapped = GetRemapped(normal.xz, a, d, c, b);
            remapped *= flipNormal;
            float3 ret = float3(-remapped.y, normal.y, remapped.x);
            ret = normalize(ret);
            return ret;
          }

          v2f vert(appdata_full v, uint instanceID : SV_InstanceID)
          {
              VoxelRenderData data = _RenderDataBuffer[instanceID];

              float3 localPosition = v.vertex.xyz;
              float3 worldPosition = GetTransformedBaseVert(localPosition,
                data.AnchorA,
                data.AnchorB,
                data.AnchorC,
                data.AnchorD);
              worldPosition.y += data.Height;

              float3 worldNormal = GetTransformedNormal(
                v.vertex,
                v.normal,
                data.AnchorA,
                data.AnchorB,
                data.AnchorC,
                data.AnchorD,
                data.FlipNormal);

              v2f o;
              o.pos = mul(UNITY_MATRIX_VP, float4(worldPosition, 1.0f));
              o.normal = worldNormal;
              o.uv = v.texcoord;
              o.worldPos = worldPosition;
              o.viewDir = _WorldSpaceCameraPos.xyz - worldPosition;
              TRANSFER_SHADOW(o)
              return o;
          }

          float GetBaseShade(float3 worldNormal)
          {
            float baseShade = dot(worldNormal.xyz, _WorldSpaceLightPos0.xyz);
            baseShade = saturate(baseShade * 5);
            return baseShade;
          }

          float3 GetBoxLighting(float3 worldPos)
          {
            float3 boxPos = mul(_LightBoxTransform, float4(worldPos, 1));
            boxPos += .5;
            float3 bottomSample = tex2D(_BottomLighting, boxPos.xz).rgb;
            float3 topSample = tex2D(_TopLighting, boxPos.xz).rgb;
            float3 ret = lerp(bottomSample, topSample, boxPos.y);
            return ret;
          }


          fixed4 frag(v2f i) : SV_Target
          {
            //return float4(i.normal, 1);
              float3 boxLighting = GetBoxLighting(i.worldPos);
              float baseShade = GetBaseShade(i.normal);
              fixed shadow = SHADOW_ATTENUATION(i);
              shadow = saturate(shadow * 5);
              shadow = min(shadow, baseShade);
              float3 ret = _Color * 1.25;
              ret *= lerp(boxLighting * .75, 1, .5);
              ret *= lerp(ret * float3(0.3, .6, 1), ret, shadow);

              float3 halfAngle = normalize(normalize(i.viewDir) + _WorldSpaceLightPos0.xyz);
              float spec = pow(saturate(dot(halfAngle, i.normal)), 50) * 2;
              ret = lerp(ret * (-spec * .3 + 1), ret * (spec * .3 + 1), saturate(shadow));
              float fog = saturate(i.pos.z * 200 - 0);
              ret = lerp(float3(0, .33, 1), ret,  fog);
              return float4(ret, 1);
          }
          ENDCG
      }

      Pass
      {
          Tags{ "LightMode" = "ShadowCaster" }
          CGPROGRAM
          #pragma vertex VSMain
          #pragma fragment PSMain

          struct VoxelRenderData
          {
              float2 AnchorA;
              float2 AnchorB;
              float2 AnchorC;
              float2 AnchorD;
              float Height;
              float FlipNormal;
          };

          StructuredBuffer<VoxelRenderData> _RenderDataBuffer;

          float2 GetRemapped(float2 toRemap, float2 x1y1, float2 x0y1, float2 x0y0, float2 x1y0)
          {
            float2 y0 = lerp(x0y0, x1y0, toRemap.x);
            float2 y1 = lerp(x0y1, x1y1, toRemap.x);
            return lerp(y0, y1, toRemap.y);
          }

          float3 GetTransformedBaseVert(float3 vert,
            float2 anchorA,
            float2 anchorB,
            float2 anchorC,
            float2 anchorD)
          {
            float2 toRemap = float2(vert.x + .5, 1 - (vert.z + .5));
            float2 remapped = GetRemapped(toRemap, anchorA, anchorB, anchorC, anchorD);
            return float3(remapped.x, vert.y, remapped.y);
          }

          float4 VSMain(float4 vertex:POSITION, uint instanceID : SV_InstanceID) : SV_POSITION
          {
              VoxelRenderData data = _RenderDataBuffer[instanceID];
              float3 localPosition = vertex.xyz;
              float3 worldPosition = GetTransformedBaseVert(localPosition,
                data.AnchorA,
                data.AnchorB,
                data.AnchorC,
                data.AnchorD);
              worldPosition.y += data.Height;
              return mul(UNITY_MATRIX_VP, float4(worldPosition, 1.0f));
          }

          float4 PSMain(float4 vertex:SV_POSITION) : SV_TARGET
          {
              return 0;
          }

          ENDCG
      }
  }
}