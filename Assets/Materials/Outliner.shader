// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "Ben/Outliner"
{
	Properties
	{	
		[Header(Standard Shader Properties)]
		[Space(10)]
		_MainTex ("Texture", 2D) = "white" {}
		_BumpMap("Normals Map", 2D) = "bump" {}
		_Color("Main Color", Color) = (.5,.5,.5,1)
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0

		[Header(Outline Properties)]
		[Space(10)]
		_OutlineColor("Outline Color", Color) = (1,1,1,1)
		_OutlineWidth("Outline Width", Range(1,10)) = 1
		_OutlineTransparency("Outline Transparency", Range(0,1)) = 0.25
	}


	SubShader
	{
		Tags { "RenderType"="Opaque" }
		Cull Off
		LOD 300

		Blend SrcAlpha OneMinusSrcAlpha

		Pass//first Outline Render
		{
			ZWrite Off
			Cull Back
			//ZTest Always

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include"UnityCG.cginc"
			#include "UnityPBSLighting.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 pos : POSITION;
				//float4 color : COLOR;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			float4 _OutlineColor;
			float _OutlineWidth;
			float _OutlineTransparency;
			sampler2D _MainTex;
			half _Glossiness;
			half _Metallic;

			v2f vert(appdata v)
			{
				v.vertex.xyz *= (_OutlineWidth);
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
		
				return o;
			}
		
			half4 frag(v2f i) : COLOR
			{
				_OutlineColor.a = _OutlineTransparency;
				return _OutlineColor;
			}
			ENDCG

		}

		Blend One Zero
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _BumpMap;

		struct Input {
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			//o.Normals = _BumpMap;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG			
		
	}
}

