Shader "Unlit/ExteriorDesignationQuadShader"
{
    Properties
    {
        _ColorA("Color A", Color) = (1,1,1,1)
        _ColorB("Color B", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            float4 _ColorA;
            float4 _ColorB;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 a = i.uv.x * _ColorA;
                float4 b = i.uv.y * _ColorB;
                float4 ret = max(a, b);
                float pinStripe = 1 - saturate(abs(ret.a - .575) * 10);
                pinStripe = saturate(pinStripe * 20 - 18.5);
                return ret + pinStripe * .25;
            }
            ENDCG
        }
    }
}
