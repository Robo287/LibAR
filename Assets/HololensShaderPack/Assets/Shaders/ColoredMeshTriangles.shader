/*
Shader that has randomly color triangles mixed and wireframe.
Use different color lookup textures to create different color schemes
*/
Shader "Hololens Shader Pack/ColoredMeshTriangles"
{
    Properties
    {
        _WireColor("Wire color", Color) = (1.0, 1.0, 1.0, 1.0)
        _WireThickness("Wire thickness", Range(0, .01)) = .005
		_MainTex("Color Lookup", 2D) = "white" {}

		[Header(Pulse Transition)]
		[Toggle] _PulseEnabled("Enabled", Float) = 0
		_Center("Center", Vector) = (0,0,0,0)
		_TransitionOffset("Pulse Offset", Range(-1, 10)) = 0.5
		_TransitionWidth("Pulse Width", Range(0, 2.0)) = 1.0
		_DetailedTransitionWidth("Detail Width Factor", Range(0, 1)) = 0.5
		_Power("Smoothness", Range(0, 2.0)) = 1.0

		[Header(Near Fade)]
		[Toggle] _FadeEnabled("Enabled", Float) = 0
		_FadeEnd("Fade End (Near Plane)", Range(0, 1)) = 0.85
		_FadeRange("Fade Range", Range(0, 2.0)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
		// Enable this line for transparency and disable the one above
		//Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }

        Pass
        {
			// Enable this line for transparency
			//Blend SrcAlpha OneMinusSrcAlpha
            Offset 50, 100

            CGPROGRAM
			#include "HoloCP.cginc"
            #pragma vertex vert
            #pragma geometry geom 
            #pragma fragment frag

            // We only target the HoloLens (and the Unity editor), so take advantage of shader model 5.
            #pragma target 5.0
            #pragma only_renderers d3d11

            #include "UnityCG.cginc"

            float4 _WireColor;
            float _WireThickness;

			sampler2D _MainTex;
			float4 _MainTex_ST;

			fixed _PulseEnabled;
			fixed _TransitionOffset;
			fixed _TransitionWidth;
			fixed _DetailedTransitionWidth;
			fixed4 _Center;
			fixed _Power;

			fixed _FadeEnabled;
			fixed _FadeEnd;
			fixed _FadeRange;

            struct v2g
            {
                float4 viewPos : SV_POSITION;
				float4 world : TEXCOORD0;
				fixed fade : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2g vert(appdata_base v)
            {
                UNITY_SETUP_INSTANCE_ID(v);
                v2g o;
                o.viewPos = UnityObjectToClipPos(v.vertex);
				o.world = mul(unity_ObjectToWorld, v.vertex);
				o.fade.x = ComputeNearPlaneTransition(v.vertex, _FadeEnd, _FadeRange);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                return o;
            }

            struct g2f
            {
                float4 viewPos : SV_POSITION;
                float3 dist : TEXCOORD1;
				float id : TEXCOORD2;
				float4 transition : TEXCOORD3;
				fixed fade : TEXCOORD4;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            [maxvertexcount(3)]
            void geom(triangle v2g i[3], inout TriangleStream<g2f> triStream)
            {
                // Calculate the vectors that define the triangle from the input points.
                float2 point0 = i[0].viewPos.xy / i[0].viewPos.w;
                float2 point1 = i[1].viewPos.xy / i[1].viewPos.w;
                float2 point2 = i[2].viewPos.xy / i[2].viewPos.w;

				float id = frac(dot(i[0].world.xyz + i[1].world.xyz + i[2].world.xyz, float3(3.1, 5.7, 7.7)));

                // Calculate the area of the triangle.
                float2 vector0 = point2 - point1;
                float2 vector1 = point2 - point0;
                float2 vector2 = point1 - point0;
                float area = abs(vector1.x * vector2.y - vector1.y * vector2.x);

                float3 distScale[3];
                distScale[0] = float3(area / length(vector0), 0, 0);
                distScale[1] = float3(0, area / length(vector1), 0);
                distScale[2] = float3(0, 0, area / length(vector2));

				fixed4 transition = 1;

                g2f o;
                [unroll]
                for (uint idx = 0; idx < 3; ++idx)
                {
                   o.viewPos = i[idx].viewPos;
				   o.dist = distScale[idx] * o.viewPos.w;
				   o.id = id;
				   if (_PulseEnabled > 0)
				   {
					   transition = pow(getPulse(i[idx].world.xyz, _Center, _TransitionOffset, _TransitionWidth, _DetailedTransitionWidth), _Power);
				   }
				   o.transition = transition;
				   o.fade = i[idx].fade;
                   UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(i[idx], o);
                   triStream.Append(o);
                }
            }

            float4 frag(g2f i) : COLOR
            {
				float dist = min(i.dist[0], min(i.dist[1], i.dist[2]));
				float3 fw = fwidth(i.dist);
				float halfWidth = min(fw[0], min(fw[1], fw[2]));
				float I = smoothstep(_WireThickness + halfWidth, _WireThickness - halfWidth, dist); 
				float4 o = lerp(tex2D(_MainTex, float2(i.id, 0.5)) * i.transition.y, _WireColor, I) * i.transition.x;
				if (_FadeEnabled)
				{
					o *= i.fade.x;
				}
                return o;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
