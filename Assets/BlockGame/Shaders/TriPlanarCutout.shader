Shader "Custom/TriPlanarCutOut"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
		_BumpMap("Bumpmap", 2D) = "bump" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
	Tags { "Queue" = "AlphaTest" "IgnoreProjector" = "True" "RenderType" = "TransparentCutout" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard addshadow vertex:vert

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
		sampler2D _BumpMap;

        struct Input
        {
            float2 uv_MainTex;
			float3 localCoord;
			float3 localNormal;
        };

		void vert(inout appdata_full v, out Input data)
		{
			UNITY_INITIALIZE_OUTPUT(Input, data);
			data.localCoord = v.vertex.xyz;
			data.localNormal = v.normal.xyz;
		}

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
			float3 bf = normalize(abs(IN.localNormal));
			bf /= dot(bf, (float3)1);

			// Triplanar mapping
			float2 tx = IN.localCoord.zy;
			float2 ty = IN.localCoord.xz;
			float2 tz = IN.localCoord.xy;


			float3 blockPos = IN.localCoord - floor(IN.localCoord);
			float2 cc = (blockPos.x < 0.0001 || blockPos.x > 0.9999 ? tx : float2(0, 0));
			cc = (blockPos.y < 0.0001 || blockPos.y > 0.9999 ? ty : cc);
			cc = (blockPos.z < 0.0001 || blockPos.z > 0.9999 ? tz : cc);
			// Base color
			half4 cx = tex2D(_MainTex, cc);// *bf.x;

			half4 color = cx * _Color;
			fixed4 c = color;
			clip((c.a - 0.5f));
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
			o.Normal = UnpackNormal(tex2D(_BumpMap, cc));
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
