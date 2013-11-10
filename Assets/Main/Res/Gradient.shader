Shader "Unlit With Gradient Color" {
       Properties {
                _MainTex ("Base (RGB)", 2D) = "white" {}
				_Color1 ("Color1", Color) = (1,1,1,1)
				_Color2 ("Color2", Color) = (1,1,1,1)
				_Color3 ("Color3", Color) = (1,1,1,1)
        }

        SubShader {
                Tags {"Queue"="Overlay"}
                LOD 200
        
        CGPROGRAM
        #pragma surface surf NoLighting alpha

        sampler2D _MainTex;
        fixed4 _Color1;
        fixed4 _Color2;
        fixed4 _Color3;
        float _Slider1;

        struct Input {
                float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutput o) {

                half y = IN.uv_MainTex.y;
                half4 grad1 = lerp(_Color1, _Color2, y);
                half4 grad2 = lerp(_Color2, _Color3, y / 2.0);
                half4 grad = lerp(grad1, grad2, y);

				float4 tex = tex2D (_MainTex, IN.uv_MainTex);

                o.Albedo = grad.rgb*tex.rgb*2;
                o.Alpha = grad.a;
        }

		fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten)
		{
			fixed4 c;
			c.rgb = s.Albedo; 
			c.a = s.Alpha;
			return c;
		}

        ENDCG
        }
}