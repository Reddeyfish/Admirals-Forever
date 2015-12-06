Shader "Hidden/Blit"
{
//v = max(0,sign(distance - _Value))
	Properties
	{
		[PerRendererData] _MainTex ("Texture", 2D) = "white" {}
		_EffectTex("Cutoff Map, Greyscale", 2D) = "white" {}
		_Cutoff ("Cutoff", Range(0,1)) = 0.5
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "IgnoreProjector"="True" "Queue"="Transparent"  "PreviewType"="Plane"}
		ZWrite Off
		Lighting Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D _EffectTex;
			fixed  _Cutoff;
			
			fixed4 frag (v2f_img i) : SV_Target
			{
				fixed4 cull = tex2D(_EffectTex, i.uv);

				fixed4 col = tex2D(_MainTex, i.uv) * step(_Cutoff, cull.b); //cull if we aren't higher than cutoff
							
				return col;
			}
			ENDCG
		}
	}
}