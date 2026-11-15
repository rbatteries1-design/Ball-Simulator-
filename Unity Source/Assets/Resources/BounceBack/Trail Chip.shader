Shader "Bounce Back/Trail Chip"
{
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            BindChannels
            {
                Bind "Vertex", vertex
                Bind "Color", color
            }
        }
    }
}
