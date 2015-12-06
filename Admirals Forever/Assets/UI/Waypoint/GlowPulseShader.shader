Shader "Sprites/GlowPulseShader"
{
//v = max(0,sign(distance - _Value))
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_CullingMask ("TextureMask (black = cull)", 2D) = "white" {}
		_Gradient ("Gradient (red = value, green = saturation)", 2D) = "black" {}
		_PulsesPerSecond("PulsesPerSecond", Range(0,10)) = 1
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "IgnoreProjector"="True" "Queue"="Transparent"  "PreviewType"="Plane"}
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
		Lighting Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				fixed4 color : COLOR;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
			};

			sampler2D _MainTex;
			sampler2D _CullingMask;
			sampler2D _Gradient;
			float4 _MainTex_ST;
			half _PulsesPerSecond;

			float2 uvToVector(float2 uv)
			{
				return uv - 0.5;
			}

			float2 vectorToUV(float2 vect)
			{
				return vect + float2(0.5, 0.5);
			}

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;
				o.color = v.color;
				return o;
			}
			fixed exponentialTween(half input)
			{
				return 1/(1 * input + 1);
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 culling = tex2D(_CullingMask, i.uv);
				half4 texValue = tex2D(_MainTex, i.uv);	
				fixed value = fmod(texValue.r + _PulsesPerSecond * _Time.y, 1);
				fixed4 resultColor = tex2D(_Gradient, half2(value, 0.5)); //not the actual values
				resultColor.rgb = resultColor.r * lerp(fixed3(1, 1, 1), i.color, resultColor.g); //now the actual values
				resultColor.a *= culling.r;
				return resultColor;
			}
			ENDCG
		}
	}
}