Shader "Mosswood/LivingSilhouette" {
 SubShader { Tags {"Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline"}
  Pass { Tags {"LightMode"="SRPDefaultUnlit"} Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   struct appdata {float4 vertex:POSITION;float4 color:COLOR;};
   struct v2f {float4 vertex:SV_POSITION;float4 color:COLOR;};
   v2f vert(appdata v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.color=v.color;
#ifndef UNITY_COLORSPACE_GAMMA
o.color.rgb=GammaToLinearSpace(o.color.rgb);
#endif
return o;}
   fixed4 frag(v2f i):SV_Target{return i.color;}
   ENDCG
  }
 }
}

