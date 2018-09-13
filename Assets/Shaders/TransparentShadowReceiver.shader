Shader "Transparent_ShadowSupport" {
     Properties {
         _Color ("Main Color", Color) = (1,1,1,1)
         _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
         _Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
     }
     
     SubShader {
         Tags {"Queue"="AlphaTest" "RenderType"="TransparentCutout"}
         LOD 200
         Blend Zero SrcColor//블렌딩 사용 Zero:소스 또는 대상 값을 제거 , SrcColor:값에 소스 색상 값을 곱합니다
         Offset 0, -1
     
     CGPROGRAM
     
     #pragma surface surf ShadowOnly alphatest:_Cutoff fullforwardshadows
         
     fixed4 _Color;
     
     struct Input {
         float2 uv_MainTex;
     };
     
     inline fixed4 LightingShadowOnly (SurfaceOutput s, fixed3 lightDir, fixed atten) {
         fixed4 c;
       
         c.rgb = s.Albedo*atten;
         c.a = s.Alpha;
         return c;
     }
     
     void surf (Input IN, inout SurfaceOutput o) {
         fixed4 c = _Color;
         o.Albedo = c.rgb;
         o.Alpha = 1.0f;
     }
     ENDCG
     }
     Fallback "Transparent/Cutout/VertexLit"
}