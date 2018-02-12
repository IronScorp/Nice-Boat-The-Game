// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Particles/Foam" {
Properties {
    _MainTex ("Particle Texture", 2D) = "white" {}
    _DiffuseTex ("Diffuse Texture", 2D) = "white" {}
    _InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
}

Category {
    Tags { "Queue"="Transparent+5" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
    Blend SrcAlpha OneMinusSrcAlpha
    ColorMask RGB
    Cull Off Lighting Off ZWrite Off

    SubShader {
        Pass {

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_particles
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "WaterInclude.cginc"

            sampler2D _MainTex;
            sampler2D _DiffuseTex;
            fixed4 _TintColor;

            uniform float waves[30]; 

            struct appdata_t {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                #ifdef SOFTPARTICLES_ON
                float4 projPos : TEXCOORD2;
                #endif
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float4 _MainTex_ST;

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                float3 ver = v.vertex.xyz, norm = float3(0,0,0);
				add_wave(ver, norm, waves);
				ver.y += 0.1;
				o.vertex = UnityObjectToClipPos(ver);

                o.color = v.color;
                o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
                return o;
            }

            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
            float _InvFade;

            fixed4 frag (v2f i) : SV_Target
            {
                half4 prev = tex2D(_MainTex, i.texcoord);
                prev.a = prev.x;
                prev.a *= i.color.a * tex2D(_DiffuseTex, i.texcoord).a;
                UNITY_APPLY_FOG(i.fogCoord, prev); // fog towards white due to our blend mode
                return prev;
            }
            ENDCG
        }
    }
}
}
