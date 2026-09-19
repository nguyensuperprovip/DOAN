Shader "Mosswood/AtmosphereSprite" {
 Properties { [PerRendererData] _MainTex("Sprite",2D)="white"{} _Tint("Tint",Color)=(1,1,1,1) _Fog("Fog",Color)=(0.01,0.025,0.09,1) _Mist("Mist",Range(0,1))=0 }
 SubShader { Tags {"Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline"} Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
 Pass { Tags {"LightMode"="Universal2D"}
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; float4 color:COLOR; };
 struct v2f { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; float4 color:COLOR; };
 sampler2D _MainTex; float4 _Tint,_Fog; float _Mist;
 v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color;return o;}
 fixed4 frag(v2f i):SV_Target {fixed4 c=tex2D(_MainTex,i.uv); c.rgb=lerp(max(c.rgb*_Tint.rgb,_Fog.rgb),_Fog.rgb,_Mist); return fixed4(c.rgb*i.color.rgb,c.a*i.color.a*_Tint.a);}
 ENDCG
 } } }
