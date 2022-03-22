Shader "Voxel/ProceduralVoxelShader"
{
  Properties
  {
    _Color("Color", Color) = (1, 1, 1, 1)
  }
  SubShader{

      Pass {

          Tags {"LightMode" = "ForwardBase"}

          Cull Off

          CGPROGRAM

          #pragma vertex vert
          #pragma fragment frag
          #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
          #pragma target 4.5

          #include "UnityCG.cginc"
          #include "UnityLightingCommon.cginc"
          #include "AutoLight.cginc"
          
          fixed4 _Color;

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
              float3 ambient : TEXCOORD1;
              float3 diffuse : TEXCOORD2;
              float3 color : TEXCOORD3;
              SHADOW_COORDS(4)
          };

          StructuredBuffer<VoxelRenderData> _RenderDataBuffer;


          float2 GetRemapped(float2 toRemap, float2 x1y1, float2 x0y1, float2 x0y0, float2 x1y0)
          {
            float2 y0 = lerp(x0y0, x1y0, toRemap.x);
            float2 y1 = lerp(x0y1, x1y1, toRemap.x);
            return lerp(y0, y1, toRemap.y);
          }

          float3 GetTransformedBaseVert(float3 vert, float2 anchorA, float2 anchorB, float2 anchorC, float2 anchorD)
          {
            float2 toRemap = float2(vert.x + .5, 1 - (vert.z + .5));
            float2 remapped = GetRemapped(toRemap, anchorA, anchorB, anchorC, anchorD);
            return float3(remapped.x, vert.y, remapped.y);
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
              float3 worldNormal = v.normal;// *data.FlipNormal;



              float3 ndotl = saturate(dot(worldNormal, _WorldSpaceLightPos0.xyz));
              float3 ambient = ShadeSH9(float4(worldNormal, 1.0f));
              float3 diffuse = (ndotl * _LightColor0.rgb);
              float3 color = _Color;

              v2f o;
              o.pos = mul(UNITY_MATRIX_VP, float4(worldPosition, 1.0f));
              o.ambient = ambient;
              o.diffuse = diffuse;
              o.color = color;
              TRANSFER_SHADOW(o)
              return o;
          }

          fixed4 frag(v2f i) : SV_Target
          {
              fixed shadow = SHADOW_ATTENUATION(i);
              fixed4 albedo = 1;
              float3 lighting = i.diffuse * shadow + i.ambient;
              fixed4 output = fixed4(albedo.rgb * i.color * lighting, albedo.w);
              UNITY_APPLY_FOG(i.fogCoord, output);
              return output; 
          }
          ENDCG
      }
  }
}