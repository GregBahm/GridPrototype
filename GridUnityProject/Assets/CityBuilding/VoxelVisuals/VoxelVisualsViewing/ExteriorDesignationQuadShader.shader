Shader "Unlit/ExteriorDesignationQuadShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _AdjacentColorA("Adjacent A", Color) = (1,1,1,1)
        _AdjacentColorB("Adjacent B", Color) = (1,1,1,1)
        _DiagonalColor("Diagonal", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" }
        Pass
        {
            Blend One One
            //Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            sampler2D _MainTex;

            float4 _AdjacentColorA;
            float4 _AdjacentColorB;
            float4 _DiagonalColor;

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

            half GetDiagonalAlpha(half2 uv)
            {
                half x = 1 - uv.x;
                half y = 1 - uv.y;
                return 1 - sqrt(x * x + y * y);
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 a = i.uv.x * _AdjacentColorA;
                float4 b = i.uv.y * _AdjacentColorB;
                float diagAlpha = GetDiagonalAlpha(i.uv);
                float4 c = diagAlpha * _DiagonalColor;
                float4 ret = max(a, b);
                ret = max(ret, c);
                float pinStripe = 1 - saturate(abs(ret.a - .575) * 10);
                pinStripe = saturate(pinStripe * 20 - 18.5);
                fixed4 col = tex2D(_MainTex, i.uv);
                ret = lerp(ret, ret * col.r, .5);
                ret = pow(ret, 2);
                ret += pinStripe * .5;
                return ret;
            }
            ENDCG
        }
    }
}
