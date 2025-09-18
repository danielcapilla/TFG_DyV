Shader "Custom/Outline Shader"
{
Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _EdgeColor ("Edge Color", Color) = (0,0,0,1)
        _Sensitivity ("Edge Sensitivity", Range(0.1,5)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _EdgeColor;
            float _Sensitivity;

            float luminance(float3 c)
            {
                return dot(c, float3(0.299, 0.587, 0.114));
            }

            fixed4 frag(v2f_img i) : SV_Target
            {
                float2 uv = i.uv;
                float2 texel = _MainTex_TexelSize.xy;

                // Sobel kernel
                float3 n[9];
                int k = 0;
                for (int y=-1; y<=1; y++)
                    for (int x=-1; x<=1; x++)
                        n[k++] = tex2D(_MainTex, uv + float2(x,y)*texel).rgb;

                float gx = -n[0].r - 2*n[3].r - n[6].r + n[2].r + 2*n[5].r + n[8].r;
                float gy = -n[0].r - 2*n[1].r - n[2].r + n[6].r + 2*n[7].r + n[8].r;

                float g = sqrt(gx*gx + gy*gy);

                if (g > _Sensitivity)
                    return _EdgeColor;
                else
                    return tex2D(_MainTex, uv);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
