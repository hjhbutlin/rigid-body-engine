#version 330 core
layout(location = 0) in vec2 aPosition;  // position
uniform mat4 projection; // orthographic projection matrix

void main()
{
    gl_Position = projection * vec4(aPosition, 0.0, 1.0); // Z is 0 (we're in 2D ;p )
}