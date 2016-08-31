ParticlePhysics2D-Unity
=======================

A fast and stable particle based physics 2D engine for Unity 3D.  

![Verlet tree physics sim using ParticlePhysics2D](http://38.media.tumblr.com/72947caee1de726465db4b001544384c/tumblr_nuwya1k5B21riukqoo1_400.gif)
![Verlet tree physics sim using ParticlePhysics2D](http://31.media.tumblr.com/534287df9dd18e20d830051a5ebd00ae/tumblr_nuwya1k5B21riukqoo2_400.gif)  
![Verlet tree physics sim using ParticlePhysics2D](http://67.media.tumblr.com/409507b1b7ec67353c850a24ddce66e1/tumblr_ntmf85Hn111riukqoo1_400.gif)  
![Verlet tree physics sim using ParticlePhysics2D](http://66.media.tumblr.com/2584f7b7fe87b7156796fd5c4a9daa3c/tumblr_ntlstqtfky1riukqoo1_400.gif)  


### Brief
ParticlePhysics2D for Unity is originally inspired from [JEFFREY BERNSTEIN's TRAER.PHYSICS 3.0](http://murderandcreate.com/physics/), and evolved as a Unity3D plugin with a lot of unity-oriented features. This plugin is for the purpose of crafting a overwhelming plants world in my game project called [FISH](http://fishartgame.com) <br />

Not like the original Traer Phyiscs, this plugin only supports Verlet integration on both CPU and GPU , other integration methods such as Euler Intergration are dropped.

The system of ParticlePhysics2D bascially includes three layers of implementation.<br />

**Computation layer:** This is the core of ParticlePhysics2D, it computes the result and get the results back to ParticlePhysics2D. You can use differernt intergrator for the computation. Right now CPU Verlet Integrator and GPU Verlet Integrator are supported. GPU Verlet Integration is implemented by using vert-frag pipline and withou using Computer Shader. So it'll be compatible with as many platforms as possible. However, sever performance and stability issues are noticed while using GPU vert-frag intergrator, so probably in some future, when Unity have a wider support of Compute Shader, I'll make another GPU integrator by using compute shader.

**Data layer:** This layer holds all the particles and edges(springs) of ParticlePhysics2D, it performs data validation when used in the form layer. It supports basically three types of data. 1. particles 2. edges(springConstraints) 3.agnleConstraints. 

**Form layer:** This layer utilizes the data contained in the data layer, and turns the data into meaningful forms. Note currently, a binary tree form(shape) is implemented for my own game. Normally end user will likely to work on this layer to create different shapes, and the computational backend should be hidden and transparent to end users. Refer to Branch_Mono.cs to see how to use this tool.

### Usage
```c#
using ParticlePhysics2D;

Simulation sim;

//init the simulation object
void Start() {
	sim.Init();
	//make some shape
	Particle2D p1 = sim.makeParticle(Vector2.zero);
	Particle2D p2 = sim.makeParticle(Vector2.one);
	Particle2D p3 = sim.makeParticle(Vector2.right);
	Spring2D s1 = sim.makeSpring(p1,p2);
	Spring2D s2 = sim.makeSpring(p1,p3);
	sim.makeAngleConstraint(s1,s2);
}

//update the simulation
void LateUpdate(){
	sim.tick();
}
```
### Todo
1. More unity compatible.  
2. GPU Integrator using Compute Shader on platform that Unity supports.
3. Unity editor tool to create more meaningful shapes by implementing Form Layer
4. ~~Improve the stability of current GPU vert-frag integrator~~ (Switching to compute shader)
5. Option to enable Multi-threaded CPU Integrator
6. Collision detection on GPU
7. ~~Optimize how data is transfer from GPU to CPU~~ (Switching to compute shader)
8. Shader keywords to switch between high precision float (32-bit) and half float (16-bit)
9. ~~GPU integrator only works with OpenGL now, need to make it work with DX as well.~~ (Switching to compute shader)

### About GPU Integrator
I've spend a lot of time making the current vert-frag GPU integrator, however due to some Unity bugs, it is extremely unstable. Therefore the current GPU integration will be dropped. [Unity will support Metal's compute shader soon in this year(2016)](https://blogs.unity3d.com/2016/06/17/wwdc-unity-metal-tessellation-demo/), so hopefully I can use compute shader to make a new GPU integrator.

### About the upcomming multi-threaded integrator
Bascially there will be a threading mamanger with fixed amount of threads. Everytime when you new a simulation object, it will be assigned to a thread by threading manager.

### About collision detection and response
![Verlet tree physics sim using ParticlePhysics2D](http://66.media.tumblr.com/69226a17eebf6aad9b47eb06ba834295/tumblr_nte2mggFDC1riukqoo2_500.gif)  
Binary bounding circles are generated in each frame for broad-phase, then in narrow-phase, only circle-circle collision is supported by now. However by incoporating Unity's physics 2d, more types of collider will be supported.

Any feedback is greatly welcome.<br />
Feel free to drop an email at paraself[at]gmail.com.
