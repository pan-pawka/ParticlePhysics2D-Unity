ParticlePhysics2D-Unity
=======================

A fast and stable particle based physics 2D engine for Unity 3D.  

![Verlet tree physics sim using ParticlePhysics2D](http://38.media.tumblr.com/72947caee1de726465db4b001544384c/tumblr_nuwya1k5B21riukqoo1_400.gif)
![Verlet tree physics sim using ParticlePhysics2D](http://31.media.tumblr.com/534287df9dd18e20d830051a5ebd00ae/tumblr_nuwya1k5B21riukqoo2_400.gif)  

### Brief
ParticlePhysics2D for Unity is originally inspired from JEFFREY BERNSTEIN's TRAER.PHYSICS 3.0, and evolved as a Unity3D plugin with a lot of unity-oriented features. This plugin is for the purpose of crafting a overwhelming plants world in my game project called [FISH](http://fishartgame.com) <br />

Not like the original Traer Phyiscs, this plugin only supports Verlet integration on both CPU and GPU , other integration methods such as Euler Intergration are dropped.

The system of ParticlePhysics2D bascially includes three layers of implementation.<br />

**Computation layer:** This is the core of ParticlePhysics2D, it computes the result and get the results back to ParticlePhysics2D. You can use differernt intergrator for the computation. Right now CPU Verlet Integrator and GPU Verlet Integrator are supported. GPU Verlet Integration is implemented by using vert-frag pipline and withou using Computer Shader. So it'll be compatible with as many platforms as possible. However, sever performance and stability issues are noticed while using GPU vert-frag intergrator, so probably in some future, when Unity have a wider support of Compute Shader, I'll make another GPU integrator by using compute shader.

**Data layer:** This layer holds all the particles and edges(springs) of ParticlePhysics2D, it performs data validation when used in the Expression layer. It supports basically tree types of data. 1. particles 2. edges(springConstraints) 3.agnleConstraints. 

**Form layer:** This layer utilizes the data contained in the data layer, and turns the data into meaningful forms. Note currently, a binary tree form(shape) is implemented for my own game. Normally end user will likely to work on this layer to create different shapes, and the computational backend should be hidden and transparent to end users. Refer to Branch_Mono.cs to see how to use this tool.


### Todo
1. More unity compatible.  
2. GPU Integrator using Compute Shader on platform that Unity supports.
3. Unity editor tool to create more meaningful shapes by implementing Form Layer
4. Imporve the stability of current GPU vert-frag integrator
5. Option to enable Multi-threaded CPU Integrator




Any feedback is greatly welcome.<br />
Feel free to drop an email at paraself[at]gmail.com.
