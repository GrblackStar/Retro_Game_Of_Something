#version v
 
// Comes in from the vertex shader. 
in vec4 vertColor;
 
out vec4 fragColor; 

void main() { 
    fragColor = vertColor;
}