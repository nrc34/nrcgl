﻿#version 100

attribute vec3 vertex_position;
attribute vec3 vertex_normal;
attribute vec2 vertex_texcoord;
attribute vec4 vertex_color;

uniform mat4 modelview_matrix;
uniform mat4 projection_matrix;
uniform mat4 model_matrix;
uniform mat4 mvp_matrix;

//out Attributes
//{
//	vec3 position;
//	vec4 normal;
//	vec2 uv;
//	vec4 worldposition;
//} att;

varying vec3 position;
varying vec4 normal;
varying vec2 uv;
varying vec4 worldposition;


void main()
{

	vec3 vertexViewSpacePosition = (modelview_matrix * vec4(vertex_position, 1.0)).xyz;
    
	position		= vertexViewSpacePosition;
	normal 			= model_matrix * vec4(vertex_normal, 0);
	uv				= vertex_texcoord;
	worldposition   = model_matrix * vec4(vertex_position, 1.0);

	//gl_Position = projection_matrix * vec4(vertexViewSpacePosition, 1.0);

	gl_Position = mvp_matrix * vec4(vertex_position, 1.0);

	//gl_Position = (modelview_matrix * vec4(vertex_position, 1.0));
}
