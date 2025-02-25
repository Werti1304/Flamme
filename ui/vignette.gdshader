shader_type canvas_item;

uniform float darkness_intensity : hint_range(0.0, 1.0) = 1;

void fragment() {
    vec2 uv = SCREEN_UV * 2.0 - 1.0; // Center UV to (-1,1)
    float distance_from_center = length(uv);
    
    float vignette = smoothstep(0.5, 1.5, distance_from_center);
    vec4 color = vec4(0.0, 0.0, 0.0, vignette * darkness_intensity);
    
    COLOR = color;
}