Shader "Ben/Wireframe01"
{
    Properties
    {
       
		 [NoScaleOffset] _ColorTex ("XYZ Texture", 2D) = "white" {}
        _WireframeVal ("Wireframe width", Range(0., 0.5)) = 0.05
		_Wireframe2nd("Outline width", Range(0,1)) = .03
        _WireColor ("Wire color", color) = (1., 1., 1., 1.)
		_WireColor2 ("Outline Color", color) = (0,0,0,1)
        [Toggle] _RemoveDiag("Remove diagonals?", Float) = 0.
		_Num("Line Length",Range(0,1)) = .25
		_Offset("UV", float) = 0
		[HideInInspector] _CamPos ("", vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		Cull Front
		Blend SrcAlpha OneMinusSrcAlpha
		
        Pass
        {
            Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geom
 
            // Change "shader_feature" with "pragma_compile" if you want set this keyword from c# code
            #pragma shader_feature __ _REMOVEDIAG_ON
 
            #include "UnityCG.cginc"
			
			fixed4 _CamPos;
			sampler2D _ColorTex;
			fixed4 _ColorTex_ST;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
			};

            struct v2f {
                float4 worldPos : SV_POSITION;
				float2 texcoord : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
            };
 
            struct g2f {
                float4 pos : SV_POSITION;
				float2 texcoord : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
                float3 bary : TEXCOORD2;
				
            };
 
            v2f vert(appdata v) {
                v2f o;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.texcoord = v.uv;
				o.uv2 = v.uv2;

				
                return o;
            }

 
            [maxvertexcount(3)]
            void geom(triangle v2f IN[3], inout TriangleStream<g2f> triStream) {
                float3 param = float3(0., 0., 0.);
 
                #if _REMOVEDIAG_ON
                float EdgeA = length(IN[0].worldPos - IN[1].worldPos);
                float EdgeB = length(IN[1].worldPos - IN[2].worldPos);
                float EdgeC = length(IN[2].worldPos - IN[0].worldPos);
				
                if(EdgeA > EdgeB && EdgeA > EdgeC)
                    param.y = 1.;
                else if (EdgeB > EdgeC && EdgeB > EdgeA)
                    param.x = 1.;
                else
                    param.z = 1.;
                #endif
 
                g2f o;
                o.pos = mul(UNITY_MATRIX_VP, IN[0].worldPos);
                o.bary = float3(1., 0., 0.) + param;
				o.texcoord = IN[0].texcoord;
				o.uv2 = IN[0].uv2;
                triStream.Append(o);
                o.pos = mul(UNITY_MATRIX_VP, IN[1].worldPos);
                o.bary = float3(0., 0., 1.) + param;
				o.texcoord = IN[1].texcoord;
				o.uv2 = IN[1].uv2;
                triStream.Append(o);
                o.pos = mul(UNITY_MATRIX_VP, IN[2].worldPos);
                o.bary = float3(0., 1., 0.) + param;
				o.texcoord = IN[2].texcoord;
				o.uv2 = IN[2].uv2;
                triStream.Append(o);
				_CamPos = o.pos;
            }
 
            float _WireframeVal, _Wireframe2nd, _Scale = 1, _Num, _Offset;
            fixed4 _WireColor, _WireColor2, _WireColor3, _WireColor4;
			
			
 



            fixed4 frag(g2f i, v2f j) : SV_Target {

			float4 col = _WireColor;
			float2 uv = float2(i.uv2.x + _Offset, i.uv2.y);
			float4 tex2 = tex2D(_ColorTex, uv)*_WireColor2;
			
            if(!any(bool3(i.bary.x < _WireframeVal, i.bary.y < _WireframeVal, i.bary.z < _WireframeVal)))
            {
				discard;
			}
			if(i.texcoord.y < 1-_Num/3 && 1-i.texcoord.y < 1-_Num)
			{
				discard;
			}
			if(i.texcoord.x < 1-_Num/3 && 1-i.texcoord.x < 1-_Num)
			{
				discard;
			}

			if(i.texcoord.y < 1-_Num +_Wireframe2nd && 1-i.texcoord.y < 1-_Num +_Wireframe2nd)
			{
				//col = tex2;
			}
			if(i.texcoord.x < 1-_Num +_Wireframe2nd && 1-i.texcoord.x < 1-_Num +_Wireframe2nd)
			{
				//col = tex2;
			}
			
			if(!any(bool3(i.bary.x < _WireframeVal-_Wireframe2nd, i.bary.y < _WireframeVal-_Wireframe2nd, i.bary.z < _WireframeVal-_Wireframe2nd)))
            {
				col = tex2;
			}

			
			

                return col;
            }
 
            ENDCG
        }
 
       
    }
}