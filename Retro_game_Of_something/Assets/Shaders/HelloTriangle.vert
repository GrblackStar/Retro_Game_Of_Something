#version v 
 
 uniform mat4 modelMatrix; 

layout(location = 0)in vec4 color;
layout(location = 1)in vec3 vertPos; 
 
// Goes to the frag shader.  
out vec4 vertColor; 
 
void main() { 
    // Pass to frag.
    vertColor = color;
    
    // Multiply by projection.
    gl_Position = modelMatrix * vec4(vertPos, 1.0);
}