//https://docs.unity3d.com/Manual/SL-VertexFragmentShaderExamples.html

Shader "Custom/BlockShader" {
    Properties {
        _TextureArr("Texture Array", 2DArray) = "" {}
        _Cutoff("Alpha Cutoff", Range(0,1)) = 0.5
    }
    SubShader
    {
        Pass
        {
            Tags { "LightMode"="ForwardBase" }
            LOD 200
    
            CGPROGRAM
            #pragma fragment frag 
            #pragma vertex vert             
            
            // uses texture arrays, so needs DX10/ES3 which is 3.5 target
            #pragma target 3.5
            
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"
    
            UNITY_DECLARE_TEX2DARRAY(_TextureArr);
            float _Cutoff;
    
            struct vertInput
            {            
                float3 uv : TEXCOORD0;
                float2 light : TEXCOORD1;
                float3 color : COLOR;
                float3 normal : NORMAL;
                float4 pos : POSITION;
            };  
            
            struct vertOutput
            {
                float3 uv : TEXCOORD0;
                float3 color : COLOR;
                fixed3 sun : COLOR1; 
                fixed3 block : COLOR2; 
                float4 pos : SV_POSITION;
            };  
    
            vertOutput vert(vertInput input)
            {
                vertOutput o;
                o.pos = UnityObjectToClipPos(input.pos);
                o.uv = input.uv;
                int blockInt = input.light.x;
                int sunInt = input.light.y;
                float3 blockLight = float3(((blockInt >> 10) & 31) / 31.0, ((blockInt >> 5) & 31) / 31.0, (blockInt & 31) / 31.0);
                float3 sunLight = float3(((sunInt >> 10) & 31) / 31.0, ((sunInt >> 5) & 31) / 31.0, (sunInt & 31) / 31.0);
                o.color = input.color;                
                o.sun = sunLight * _LightColor0;
                o.block = blockLight;
                return o;
            }
    
            half4 frag(vertOutput input) : COLOR
            {
                half4 c = UNITY_SAMPLE_TEX2DARRAY (_TextureArr, input.uv);
                clip(c.a - _Cutoff);
                fixed3 lighting = max(input.sun, input.block);
                c.rgb = c.rgb * input.color * lighting;
                return c;
            }
            ENDCG
        }
        // pull in shadow caster from VertexLit built-in shader
        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
        
    Fallback "Diffuse"
}
