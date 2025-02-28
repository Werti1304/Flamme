shader_type canvas_item;

uniform float darkness_intensity : hint_range(0.0, 1.0) = 1;
uniform float darkness_out : hint_range(0.0, 1.0) = 0.5;
uniform float darkness_in : hint_range(0.0, 2.0) = 1.5;

void fragment() {
    vec2 uv = SCREEN_UV * 2.0 - 1.0; // Center UV to (-1,1)
    float distance_from_center = length(uv);

    float vignette = smoothstep(darkness_out, darkness_in, distance_from_center);
    vec4 color = vec4(0.0, 0.0, 0.0, vignette * darkness_intensity);

    COLOR = color;
}