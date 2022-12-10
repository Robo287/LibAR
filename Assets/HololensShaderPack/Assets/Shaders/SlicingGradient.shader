Shader "Hololens Shader Pack/SlicingGradient"
{
	Properties
	{
		_InnerColor("Inner Color", Color) = (0.26,0.19,0.16,0.0)
		_OuterColor("Outer Color", Color) = (0.26,0.19,0.16,0.0)
		_Offset("Offset", Range(0.0,1.0)) = 0.0
		_Scale("Scale", Range(0.0,10.0)) = 1.0
		_RimPower("Rim Power", Range(0.1,8.0)) = 3.0

		[Header(Intersection plane)]
		[Toggle] _ColorEnabled("Use Custom Intersection Color", Float) = 0
		_IntersectionColor("Intersection Color", Color) = (1,1,1,1)
		_SlicePlane("SlicePlane", Vector) = (0, 1, 0, 0.5)
	}

	SubShader
	{
		// Note render before other geometry to prevent stencil from messing up
		Tags{ "Queue" = "Geometry-1" }

		Pass
		{
			// 1 Render front faces with full shading
			Cull Back
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			fixed4 _InnerColor;
			fixed4 _OuterColor;
			fixed _Offset;
			fixed _Scale;
			fixed _RimPower;
			fixed4 _SlicePlane;

			struct v2f
			{
				fixed4 viewPos : SV_POSITION;
				fixed3 normal : NORMAL;
				fixed3 worldSpaceViewDir : TEXCOORD0;
				fixed4 world : TEXCOORD1;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			bool abovePlane(fixed3 position, fixed4 plane)
			{
				fixed rDotn = dot(plane.xyz, position);
				return (rDotn > plane.w);
			}

			v2f vert(appdata_full v)
			{
				UNITY_SETUP_INSTANCE_ID(v);
				v2f o;
				o.viewPos = UnityObjectToClipPos(v.vertex);
				o.worldSpaceViewDir = WorldSpaceViewDir(v.vertex);
				o.normal = mul(unity_ObjectToWorld, fixed4(v.normal, 0.0)).xyz;
				o.world = mul(unity_ObjectToWorld, v.vertex);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				if (abovePlane(i.world.xyz, _SlicePlane))
				{
					discard;
				}
				fixed4 color = 1;

				fixed angle = dot(normalize(i.worldSpaceViewDir), normalize(i.normal));
				fixed rim = 1.0 - saturate(angle);
				color.xyz = saturate(lerp(_InnerColor.rgb, _OuterColor.rgb, (_Offset + _Scale * pow(rim, _RimPower))));

				return color;
			}
			ENDCG
		}

		Pass
		{
			// 2 Render back faces without color write
			Cull Front
			ZWrite Off
			ColorMask 0

			Stencil
			{
				Ref 2
				Comp always
				Pass Replace
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			fixed4 _SlicePlane;

			struct v2f
			{
				fixed4 viewPos : SV_POSITION;
				fixed4 world : TEXCOORD1;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			bool abovePlane(fixed3 position, fixed4 plane)
			{
				fixed rDotn = dot(plane.xyz, position);
				return (rDotn > plane.w);
			}

			v2f vert(appdata_base v)
			{
				UNITY_SETUP_INSTANCE_ID(v);
				v2f o;
				o.viewPos = UnityObjectToClipPos(v.vertex);
				o.world = mul(unity_ObjectToWorld, v.vertex);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				return o;
			}


			fixed4 frag(v2f i) : SV_Target
			{
				if (abovePlane(i.world, _SlicePlane))
				{
					discard;
				}
				return 1;
			}
			ENDCG
		}

		Pass
		{
			// 3 Render backfaces but only that were not overwritten by front
			Cull Front
			ZTest Always
			Stencil
			{
				Ref 2
				Comp Equal
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			fixed4 _InnerColor;
			fixed4 _OuterColor;
			fixed4 _IntersectionColor;
			fixed _Offset;
			fixed _Scale;
			fixed _RimPower;
			fixed4 _SlicePlane;
			fixed _ColorEnabled;

			struct v2f
			{
				fixed4 viewPos : SV_POSITION;
				fixed3 worldSpaceViewDir : TEXCOORD0;
				fixed4 world : TEXCOORD1;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2f vert(appdata_base v)
			{
				UNITY_SETUP_INSTANCE_ID(v);
				v2f o;
				o.viewPos = UnityObjectToClipPos(v.vertex);
				o.worldSpaceViewDir = WorldSpaceViewDir(v.vertex);
				o.world = mul(unity_ObjectToWorld, v.vertex);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				return o;
			}

			bool abovePlane(fixed3 position, fixed4 plane)
			{
				fixed rDotn = dot(plane.xyz, position);
				return (rDotn > plane.w);
			}

			bool IntersectRayPlane(fixed3 rayOrigin, fixed3 rayDirection, fixed4 plane, out fixed3 intersectionPoint)
			{
				fixed rDotn = dot(rayDirection, plane.xyz);

				//parallel to plane or pointing away from plane?
				if (rDotn < 0.0000001)
					return false;

				fixed s = dot(plane.xyz, ((plane.xyz * plane.w) - rayOrigin)) / rDotn;

				intersectionPoint = rayOrigin + s * rayDirection;

				return true;
			}

			struct fragmentOutput {
				fixed4 color : SV_Target;
				fixed zvalue : SV_Depth;
			};

			fragmentOutput frag(v2f i)
			{
				fragmentOutput o;
				o.color = fixed4(0,1,1,1);

				if (abovePlane(i.world, _SlicePlane))
				{
					discard;
				}

				fixed3 isp;
				if (IntersectRayPlane(i.world, normalize(i.worldSpaceViewDir), _SlicePlane, isp))
				{
					if (_ColorEnabled)
					{
						o.color = _IntersectionColor;
					}
					else
					{
						fixed angle = dot(normalize(i.worldSpaceViewDir), _SlicePlane.xyz);
						fixed rim = 1.0 - saturate(angle);
						o.color.xyz = saturate(lerp(_InnerColor.rgb, _OuterColor.rgb, (_Offset + _Scale * pow(rim, _RimPower))));
					}

					float4 clip_pos = mul(UNITY_MATRIX_VP, fixed4(isp,1));
					o.zvalue = clip_pos.z / clip_pos.w;
				}
				
				return o;
			}
			ENDCG
		}
	}
}
