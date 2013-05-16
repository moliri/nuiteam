Shader "Omek/Player Mask Shader (Simple Plane Glow)" {
	Properties {
		_MainTex ("Trans (A)", 2D) = "white" {}
		_Color ("Player color", Color) = (1,1,1,1)
		_PeekDistance ("Peek Distance", Range (0.0, 0.02)) = 0.005
		_InsideFactor ("Inside factor", Range (0.0, 1)) = 0.1
		_OutsideFactor ("Outside factor", Range (0.0, 0.3)) = 0.1
		//_ColorFactor ("Outside factor", Range (0.0, 5)) = 1
		//_AlphaFactor ("Outside factor", Range (0.0, 1)) = 1
	}
	 
	SubShader {
		Tags {	"Queue"="Transparent" }
		LOD 200
		ZWrite Off
		ZTest Always
		Blend SrcAlpha OneMinusSrcAlpha
		
		Pass {
			Tags { "LightMode"="Always"}
			
			CGPROGRAM
			#pragma exclude_renderers flash
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct v2f {
				float4 pos : SV_POSITION;
				float2	uv : TEXCOORD0;
				
			};

			uniform float4 _MainTex_ST, _BumpMap_ST;

			v2f vert(appdata_tan v)
			{
				v2f o;
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
				//o.pos = v.vertex;
				o.uv = TRANSFORM_TEX(v.texcoord,_MainTex);
				
				return o; 
			}

			uniform sampler2D _MainTex;
			uniform half _PeekDistance;
			uniform half _InsideFactor;
			uniform half _OutsideFactor;
			//half _ColorFactor;
			//half _AlphaFactor;
			
			uniform half4 _Color;
				
			float4 frag (v2f i) : COLOR
			{
				half4 c = tex2D (_MainTex, i.uv);
			
				half numNeighbors = 0;
				numNeighbors += tex2D(_MainTex, i.uv + half2(_PeekDistance, 0)).a;
				numNeighbors += tex2D(_MainTex, i.uv + half2(0, _PeekDistance)).a;
				numNeighbors += tex2D(_MainTex, i.uv + half2(-_PeekDistance, 0)).a;
				numNeighbors += tex2D(_MainTex, i.uv + half2(0, -_PeekDistance)).a;
				
				numNeighbors += tex2D(_MainTex, i.uv + half2(_PeekDistance*2, 0)).a;
				numNeighbors += tex2D(_MainTex, i.uv + half2(0, _PeekDistance*2)).a;
				numNeighbors += tex2D(_MainTex, i.uv + half2(-_PeekDistance*2, 0)).a;
				numNeighbors += tex2D(_MainTex, i.uv + half2(0, -_PeekDistance*2)).a;				
				
				half colorBonus  =  (1-c.a)*(numNeighbors*_InsideFactor) + (c.a)*(1 - numNeighbors*_OutsideFactor);
				
				half3 color = half3(colorBonus * 2, colorBonus* 2, colorBonus* 2);
				half alpha = colorBonus;
				
				return half4(color.r, color.g, color.b, alpha) * _Color;
			}

			ENDCG

		}
	}
	
}
