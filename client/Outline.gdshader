shader_type spatial;
render_mode unshaded;

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture;
uniform sampler2D DEPTH_TEXTURE : hint_depth_texture ;
uniform vec4 outline_color : source_color;
uniform float outline_width = 2.0;

void fragment() {
	vec4 pixelatedtext = texture(SCREEN_TEXTURE, SCREEN_UV);
	float depth = texture(DEPTH_TEXTURE, SCREEN_UV).r;
	vec2 pixel_size = 1.0 / VIEWPORT_SIZE;
	ALBEDO = pixelatedtext.rgb;
	for(int y = -1*int(outline_width); y <= 1*int(outline_width); y++)
	for(int x = -1*int(outline_width); x <= 1*int(outline_width); x++)
	{
		vec4 tex = texture(DEPTH_TEXTURE, SCREEN_UV + vec2(float(x),float(y)) * pixel_size );
		float dif = (tex.r-depth);
		if(dif > 0.001 || dif < -0.001)
		{
			ALBEDO = outline_color.rgb;
			ALPHA = 1.0;
			break;
		}
	}
	
}