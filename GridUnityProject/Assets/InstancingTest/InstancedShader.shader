Shader "Unlit/InstancedShader"
{
  Properties
  {
      _Color("Color 1", Color) = (1,1,1,1)
  }
    SubShader
      {
          Tags { "RenderType" = "Opaque" "DisableBatching" = "True" }

          Pass
          {
              HLSLPROGRAM
              #pragma vertex vert
              #pragma fragment frag
              #pragma multi_compile_instancing

              #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

              struct appdata
              {
                  float4 vertex : POSITION;
                  float2 uv : TEXCOORD0;
                  UNITY_VERTEX_INPUT_INSTANCE_ID
              };

              struct v2f
              {
                  float4 vertex : SV_POSITION;
                  float2 uv : TEXCOORD0;
                  UNITY_VERTEX_INPUT_INSTANCE_ID
              };



              StructuredBuffer<float4> colors;

              UNITY_INSTANCING_BUFFER_START(MyProps)
                  UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
              UNITY_INSTANCING_BUFFER_END(MyProps)

              v2f vert(appdata v)
              {
                  v2f o;

                  UNITY_SETUP_INSTANCE_ID(v);
                  UNITY_TRANSFER_INSTANCE_ID(v, o);

                  o.vertex = TransformObjectToHClip(v.vertex.xyz);
                  o.uv = v.uv;
                  return o;
              }

              float4 frag(v2f i) : SV_Target
              {
                  return 1;// colors[i.instanceID];
                  //UNITY_SETUP_INSTANCE_ID(i);

                  //float4 color = UNITY_ACCESS_INSTANCED_PROP(MyProps, _Color);
                  //return color;
              }
              ENDHLSL
          }
      }
}