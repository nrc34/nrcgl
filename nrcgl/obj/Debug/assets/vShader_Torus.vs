attribute vec3 vertex_position;
attribute vec3 vertex_normal;
attribute vec2 vertex_texcoord;
attribute vec4 vertex_color;

uniform mat4 uMVPMatrix;

void main()
{
	gl_Position = uMVPMatrix * vec4(vertex_position, 1f);
}
