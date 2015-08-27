ParticlePhysics2D-Unity
=======================

A fast and stable particle based physics 2D engine for Unity 3D.


### Brief
ParticlePhysics2D for Unity is originally ported from JEFFREY BERNSTEIN's TRAER.PHYSICS 3.0, with addition of a lot of unity-oriented features, for the purpose of crafting a overwhelming plants world in my game project called [FISH](http://fishartgame.com) <br />
<del>With existing Modified Euler Intergration and Runge Kutta Integration, another Verlet Integration is also added.

Only Verlet integration on both CPU and GPU is supported now, other integration methods are dropped.

The system of ParticlePhysics2D bascially includes three layers of implementation.<br />

**Computation layer:** This is the core of ParticlePhysics2D, it computes the result and get the results back to ParticlePhysics2D. You can use differernt intergrator for the computation. Right now a Verlet Integrator is supported on CPU. GPU Verlet Integration will be supported soon by using vert-frag pipline and withou using Computer Shader. So it'll be compatible with as many platforms as possible.

**Data layer:** This layer holds all the particles and edges(springs) of ParticlePhysics2D, it performs data validation when used in the Expression layer. It supports basically tree types of data. 1. particles 2. edges(springConstraints) 3.agnleConstraints. 

**Expression layer:** This layer utilizes the data contained in the data layer, and turns the data into meaningful forms. Note currently, a tree form is implemented for expression layer. Normally end user will likely to work on this layer to create different shapes, and the computational stuff should be hidden and transparent to end users. Refer to Branch_Mono.cs to see how to use this tool.


### Todo
1. More unity compatible. <br />
2. <del>Collision detection  
3. circle-circle CPU collision solver is done, Gpu solver coming soon <br />
3. Unity editor tool to create ParticlePhysics2D system in design time. <br />
4. GPU Verlet Integration




Any feedback is greatly welcome.<br />
Feel free to drop an email at paraself[at]gmail.com.
