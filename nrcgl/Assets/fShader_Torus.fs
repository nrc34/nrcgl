precision mediump float;


in Attributes
{
    vec3 position;
    vec4 normal;
    vec2 uv;
    vec4 worldposition;
} att;

void main()
{

    vec3 camera_position = vec3(0f, 5f, -10f);
    vec3 light_position = vec3(100f, 100f, 100f);
    float material_shininess = 20f;

    vec3 toEye = camera_position - att.worldposition.xyz;
    vec3 toLight = light_position - att.worldposition.xyz;

    vec3 nN = normalize(att.normal.xyz);
    vec3 nE = normalize(toEye);
    vec3 nL = normalize(toLight);
    vec3 nR = normalize(reflect(-nL, nN));

    float lambert = max(dot(nN, nL), 0);
    float phong = 0;
    if(lambert > 0)
        phong = pow(max(dot(nE, nR), 0), material_shininess);


    gl_FragColor = vec4(vec3(1.0, 1.0, 1.0) * (lambert + phong), 1.0);
}
