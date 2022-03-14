Shader "Voxel/ProceduralVoxelShader"
{
  Properties
  {
    _Color("Color", Color) = (1, 1, 1, 1)
  }
  SubShader{

      Pass {

          Tags {"LightMode" = "ForwardBase"}

          CGPROGRAM

          #pragma vertex vert
          #pragma fragment frag
          #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
          #pragma target 4.5

          #include "UnityCG.cginc"
          #include "UnityLightingCommon.cginc"
          #include "AutoLight.cginc"

          StructuredBuffer<float4> positionBuffer;

          struct v2f
          {
              float4 pos : SV_POSITION;
              float3 ambient : TEXCOORD1;
              float3 diffuse : TEXCOORD2;
              float3 color : TEXCOORD3;
              SHADOW_COORDS(4)
          };

          v2f vert(appdata_full v, uint instanceID : SV_InstanceID)
          {
              float4 data = positionBuffer[instanceID];

              float3 localPosition = v.vertex.xyz * data.w;
              float3 worldPosition = data.xyz + localPosition;
              float3 worldNormal = v.normal;



              float3 ndotl = saturate(dot(worldNormal, _WorldSpaceLightPos0.xyz));
              float3 ambient = ShadeSH9(float4(worldNormal, 1.0f));
              float3 diffuse = (ndotl * _LightColor0.rgb);
              float3 color = v.color;

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