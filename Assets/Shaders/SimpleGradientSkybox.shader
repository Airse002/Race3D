Shader "Custom/SimpleGradientSkybox"
{
    Properties
    {
        _TopColor ("Top Color", Color) = (0.2, 0.3, 0.5, 1)
        _BottomColor ("Bottom Color", Color) = (0.1, 0.1, 0.2, 1)
        _Exponent ("Gradient Exponent", Range(0.1, 8.0)) = 1.5
    }
    
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off
        ZWrite Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 texcoord : TEXCOORD0;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 texcoord : TEXCOORD0;
            };
            
            fixed4 _TopColor;
            fixed4 _BottomColor;
            float _Exponent;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Normalizuj směr
                float3 dir = normalize(i.texcoord);
                
                // Výška (y komponenta) od -1 (dole) do 1 (nahoře)
                // Převeď na 0 (dole) až 1 (nahoře)
                float t = (dir.y + 1.0) * 0.5;
                
                // Aplikuj exponent pro ostřejší/jemnější přechod
                t = pow(t, _Exponent);
                
                // Lerp mezi spodní a vrchní barvou
                fixed4 color = lerp(_BottomColor, _TopColor, t);
                
                return color;
            }
            ENDCG
        }
    }
}
