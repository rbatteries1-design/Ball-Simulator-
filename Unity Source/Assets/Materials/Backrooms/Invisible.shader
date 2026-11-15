Shader "Endless Labyrinth/Invisible"
{
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            ColorMask 0
            ZWrite Off
        }
    }
    Fallback Off
}
