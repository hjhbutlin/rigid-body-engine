#version 330 core

layout(location = 0) in vec2 vPosition;  // base vertex position
layout(location = 1) in vec2 instancePosition;
layout(location = 2) in float instanceRotation;
layout(location = 3) in vec4 instanceColour;

out vec4 FragColour;

uniform mat4 Projection;  // Orthographic projection matrix

void main()
{    
    FragColour = instanceColour;
    mat4 rotation = mat4(1.0);
    rotation[0] = vec4(cos(instanceRotation), -sin(instanceRotation), 0.0, 0.0);  // rotation matrix
    rotation[1] = vec4(sin(instanceRotation), cos(instanceRotation), 0.0, 0.0);
    
    // Combine the transformations: projection * (rotation * translation)

    gl_Position =  Projection * (vec4(instancePosition, 0.0, 1.0) + rotation * vec4(vPosition, 0.0, 1.0));  // apply transformation
}