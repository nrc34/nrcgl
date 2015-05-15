﻿#version 100

precision mediump float;


//in Attributes
//{
//    vec3 position;
//    vec4 normal;
//    vec2 uv;
//    vec4 worldposition;
//} att;
//in vec3 position;
//in vec4 normal;
//in vec2 uv;
//in vec4 worldposition;
//out vec4 fragColor;

varying vec3 position;
varying vec4 normal;
varying vec2 uv;
varying vec4 worldposition;

void main()
{

    vec3 camera_position = vec3(0f, 3f, -5f);
    vec3 light_position = vec3(0f, 100f, 0f);
    float material_shininess = 10.0f;

    vec3 toEye = camera_position - worldposition.xyz;
    vec3 toLight = light_position - worldposition.xyz;

    vec3 nN = normalize(normal.xyz);
    vec3 nE = normalize(toEye);
    vec3 nL = normalize(toLight);
    vec3 nR = normalize(reflect(-nL, nN));

    float lambert = max(dot(nN, nL), 0.0f);
    float phong = 0.0f;
    if(lambert > 0.0f)
        phong = pow(max(dot(nE, nR), 0.0f), material_shininess);

    //fragColor = vec4(1.0, 1.0, 1.0, 1.0);
    gl_FragColor = vec4(vec3(1.0, 1.0, 0.3) * (lambert + phong), 1.0);
}
