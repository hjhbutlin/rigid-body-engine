<h1>Rigid Body Engine in C#</h1>

<h2>Project Motivation</h2>

As a Physics student, I was keen to embark on a project that combined my existing mechanics knowledge with a new & relevant programming language. Initially I settled on making a simulation of a pool break, but quickly changed my mind to focus on creating a more generalised rigid body engine.

<h2>Graphics Rendering</h2>

I had just wrapped up a project using SFML for 2D rendering, and while SFML would work fine for this project, I wanted to try something new and potentially more involved. After some research, I decided to use OpenGL for graphics rendering. This proved to me slightly less trivial to implement than I assumed, so I spent some time watching free CS lectures on VBOs, VAOs, and the GPU, as well as following a tutorial on making a big red triangle appear on the screen.

Overjoyed to have overcome this challenge, I began to implement some very basic physics to get this shape (now a circle) to move around the screen. The natural follow-up was to get a collision going. Since all my shapes (at least for now) were identical in every way except individual positions and velocities, I chose to store the base vertex positions just once, rather than store the positions of every object's vertices, as from my research this seemed to be best practice. I then could project this set of vertices to different positions on the screen by defining projection matrices inside the GLSL code.

Getting the graphics working properly was the hardest part of this project, as it involved both the then-unfamiliar C# and solid background understanding. It was a valuable and enjoyable opportunity to practise learning, debugging, and researching new topics.

<h2>Basic Engine</h2>

This project is centred on the interactions and collisions between rigid objects. Once the basics of OpenGL rendering were sorted, I began implementing a simple physics engine to deal with resolving collisions and friction. The engine has a modular design to facilitate the addition of e.g. gravity, and to make my life easier when I add the physics for collisions between not-circles (technical term: shapes that aren't circles). To this end, variables like angle of rotation and angular velocity were included in the initial code despite their redundancy in the circular world.

Collisions detection was _almost_ trivial, since the objects were circles, so I only had to keep track of the relative distances between particles. However, once I set up the initial conditions for a pool break (15 balls in a hexagonal-close-packed triangle, 1 ball positioned some distance away moving quickly towards the triangle), I found that mutiple collisions in quick succession broke the simulation (3+ bodies colliding with each other at once on the same physics frame), as balls were keen to overlap.

<h2>Next Steps</h2>

- To fix this problem, either by handling collisions as involving _n_ bodies rather than limiting the logic to just 2, or by introducing some kind of collision priority.

- Physics logic for e.g. squares colliding. This makes collisions detection more complex, as well as the need to find where the collision happens, and to calculate torques etc..

- To add debug graphics like velocity arrows, system energy, etc., as well as debug tools like slowing down time etc.

- To extend this to 3D with a pan-able camera
