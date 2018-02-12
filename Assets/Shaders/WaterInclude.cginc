
#ifndef WATER_CG_INCLUDED
#define WATER_CG_INCLUDED

#include "UnityCG.cginc"

half _GerstnerIntensity;

inline half3 PerPixelNormal(sampler2D bumpMap, half4 coords, half3 vertexNormal, half bumpStrength) 
{
	half3 bump = (UnpackNormal(tex2D(bumpMap, coords.xy)) + UnpackNormal(tex2D(bumpMap, coords.zw))) * 0.5;
	half3 worldNormal = vertexNormal + bump.xxy * bumpStrength * half3(1,0,1);
	return normalize(worldNormal);
} 

inline half3 PerPixelNormalUnpacked(sampler2D bumpMap, half4 coords, half bumpStrength) 
{
	half4 bump = tex2D(bumpMap, coords.xy) + tex2D(bumpMap, coords.zw);
	bump = bump * 0.5;
	half3 normal = UnpackNormal(bump);
	normal.xy *= bumpStrength;
	return normalize(normal);
} 

inline half3 GetNormal(half4 tf) {
	#ifdef WATER_VERTEX_DISPLACEMENT_ON
		return half3(2,1,2) * tf.rbg - half3(1,0,1);
	#else
		return half3(0,1,0);
	#endif	
}

inline half GetDistanceFadeout(half screenW, half speed) {
	return 1.0f / abs(0.5f + screenW * speed);	
}

half4 GetDisplacement3(half4 tileableUv, half4 tiling, half4 directionSpeed, sampler2D mapA, sampler2D mapB, sampler2D mapC)
{
	half4 displacementUv = tileableUv * tiling + _Time.xxxx * directionSpeed;
	#ifdef WATER_VERTEX_DISPLACEMENT_ON			
		half4 tf = tex2Dlod(mapA, half4(displacementUv.xy, 0.0,0.0));
		tf += tex2Dlod(mapB, half4(displacementUv.zw, 0.0,0.0));
		tf += tex2Dlod(mapC, half4(displacementUv.xw, 0.0,0.0));
		tf *= 0.333333; 
	#else
		half4 tf = half4(0.5,0.5,0.5,0.0);
	#endif
	
	return tf;
}

half4 GetDisplacement2(half4 tileableUv, half4 tiling, half4 directionSpeed, sampler2D mapA, sampler2D mapB)
{
	half4 displacementUv = tileableUv * tiling + _Time.xxxx * directionSpeed;
	#ifdef WATER_VERTEX_DISPLACEMENT_ON			
		half4 tf = tex2Dlod(mapA, half4(displacementUv.xy, 0.0,0.0));
		tf += tex2Dlod(mapB, half4(displacementUv.zw, 0.0,0.0));
		tf *= 0.5; 
	#else
		half4 tf = half4(0.5,0.5,0.5,0.0);
	#endif
	
	return tf;
}

inline void ComputeScreenAndGrabPassPos (float4 pos, out float4 screenPos, out float4 grabPassPos) 
{
	#if UNITY_UV_STARTS_AT_TOP
		float scale = -1.0;
	#else
		float scale = 1.0f;
	#endif
	
	screenPos = ComputeNonStereoScreenPos(pos); 
	grabPassPos.xy = ( float2( pos.x, pos.y*scale ) + pos.w ) * 0.5;
	grabPassPos.zw = pos.zw;
}


inline half3 PerPixelNormalUnpacked(sampler2D bumpMap, half4 coords, half bumpStrength, half2 perVertxOffset)
{
	half4 bump = tex2D(bumpMap, coords.xy) + tex2D(bumpMap, coords.zw);
	bump = bump * 0.5;
	half3 normal = UnpackNormal(bump);
	normal.xy *= bumpStrength;
	normal.xy += perVertxOffset;
	return normalize(normal);	
}

inline half3 PerPixelNormalLite(sampler2D bumpMap, half4 coords, half3 vertexNormal, half bumpStrength) 
{
	half4 bump = tex2D(bumpMap, coords.xy);
	bump.xy = bump.wy - half2(0.5, 0.5);
	half3 worldNormal = vertexNormal + bump.xxy * bumpStrength * half3(1,0,1);
	return normalize(worldNormal);
} 

inline half4 Foam(sampler2D shoreTex, half4 coords, half amount) 
{
	half4 foam = ( tex2D(shoreTex, coords.xy) * tex2D(shoreTex,coords.zw) ) - 0.125;
	foam.a = amount;
	return foam;
}

inline half4 Foam(sampler2D shoreTex, half4 coords) 
{
	half4 foam = (tex2D(shoreTex, coords.xy) * tex2D(shoreTex,coords.zw)) - 0.125;
	return foam;
}

inline half Fresnel(half3 viewVector, half3 worldNormal, half bias, half power)
{
	half facing =  clamp(1.0-max(dot(-viewVector, worldNormal), 0.0), 0.0,1.0);	
	half refl2Refr = saturate(bias+(1.0-bias) * pow(facing,power));	
	return refl2Refr;	
}

inline half FresnelViaTexture(half3 viewVector, half3 worldNormal, sampler2D fresnel)
{
	half facing =  saturate(dot(-viewVector, worldNormal));	
	half fresn = tex2D(fresnel, half2(facing, 0.5f)).b;	
	return fresn;
}

inline void VertexDisplacementHQ(	sampler2D mapA, sampler2D mapB,
									sampler2D mapC, half4 uv,
									half vertexStrength, half3 normal,
									out half4 vertexOffset, out half2 normalOffset) 
{	
	half4 tf = tex2Dlod(mapA, half4(uv.xy, 0.0,0.0));
	tf += tex2Dlod(mapB, half4(uv.zw, 0.0,0.0));
	tf += tex2Dlod(mapC, half4(uv.xw, 0.0,0.0));
	tf /= 3.0; 
	
	tf.rga = tf.rga-half3(0.5,0.5,0.0);
				
	// height displacement in alpha channel, normals info in rgb
	
	vertexOffset = tf.a * half4(normal.xyz, 0.0) * vertexStrength;							
	normalOffset = tf.rg;
}

inline void VertexDisplacementLQ(	sampler2D mapA, sampler2D mapB,
									sampler2D mapC, half4 uv,
									half vertexStrength, half normalsStrength,
									out half4 vertexOffset, out half2 normalOffset) 
{
	// @NOTE: for best performance, this should really be properly packed!
	
	half4 tf = tex2Dlod(mapA, half4(uv.xy, 0.0,0.0));
	tf += tex2Dlod(mapB, half4(uv.zw, 0.0,0.0));
	tf *= 0.5; 
	
	tf.rga = tf.rga-half3(0.5,0.5,0.0);
				
	// height displacement in alpha channel, normals info in rgb
	
	vertexOffset = tf.a * half4(0,1,0,0) * vertexStrength;							
	normalOffset = tf.rg * normalsStrength;
}

half4  ExtinctColor (half4 baseColor, half extinctionAmount) 
{
	// tweak the extinction coefficient for different coloring
	 return baseColor - extinctionAmount * half4(0.15, 0.03, 0.01, 0.0);
}

	// Trochoidal waves
	void add_wave(inout float3 vertex, inout float3 normal, float waves[30]){
		vertex.y = 0;
		normal = float3(0, 1/waves[2], 0);

		float t = _Time[1];
		float A = vertex.x * waves[2], B = vertex.z * waves[2];
		for (int i = 0; i < waves[0]; i++) {
			int ind = 5 + i*5;
			float C = A*waves [ind] + B * waves [ind+1] + t * waves[1] + waves [ind+3], s = sin(C), p = pow(abs(s),  waves [ind+2]), 
			      n = waves[3] * waves [ind+4] * ( waves [ind+2] * p / s * cos(C) + sin (C*2) * 2 / waves [ind+2]);
			vertex.y += (abs(p) - cos(C*2) / waves [ind+2]) * waves [ind+4];
			normal.x -= waves [ind]   * n;
			normal.z -= waves [ind+1] * n;
		}
		vertex.y *= waves[3];
		normal = normalize(normal);
	}

	void add_noise(inout float3 vertex, sampler2D _BumpMap, float waves[30]){
		float t = _Time[1] / 20 * waves[1];
		float4 coords = float4(vertex.x + t, vertex.z + t, 0, 0) * waves[2];
		vertex.y += (tex2Dlod(_BumpMap, coords).a - 0.5) * waves[4];
	}

#endif

