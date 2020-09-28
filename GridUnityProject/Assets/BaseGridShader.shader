Shader "Unlit/BaseGridShader"
{
    Properties
    {
        _TuneA("Tune A", Float) = 8
        _TuneB("Tune B", Range(0, 1)) = .3
        _TuneC("Tune C", Range(0, 1)) = .9 
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Blend One One

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            float _TuneA;
            float _TuneB;
            float _TuneC;

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
                o.uv = 1 - v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                //return float4(1 - i.uv.x, 1 - i.uv.y, 0, 1);
                //float ret = max(i.uv.x, i.uv.y);
                float ret = pow(pow(i.uv.x, _TuneA) + pow(i.uv.y, _TuneA), _TuneB);
                ret = saturate(ret - _TuneC) * 5;
                return float4(saturate(ret).xxx, 1);
            }
            ENDCG
        }
    }
}
