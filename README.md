<h1>Rigid Body Engine in C#</h1>

<h2>Project Motivation</h2>

As a Physics student, I was keen to embark on a project that combined my existing mechanics knowledge with a new & relevant programming language, so I decided to build a rigid body engine with the end goal of making a simple 8ball pool game.

<h2>Graphics Rendering</h2>

I used OpenGL for graphics rendering. This proved to me slightly less trivial to implement than I assumed, so I spent some time watching free CS lectures on VBOs, VAOs, and the GPU, as well as following a tutorial on making a big red triangle appear on the screen.

Overjoyed to have overcome this challenge, I began to implement some very basic physics to get this shape (now a circle) to move around the screen. The natural follow-up was to get a collision going. Since all my shapes (at least for now) were identical in every way except individual positions and velocities, I chose to store the base vertex positions just once, rather than store the positions of every object's vertices, as from my research this seemed to be best practice. I then could project this set of vertices to different positions on the screen by defining projection matrices inside the GLSL code.

Getting the graphics working properly was the hardest part of this project, as it involved both the then-unfamiliar C# and solid background understanding. It was a valuable and enjoyable opportunity to practise learning, debugging, and researching new topics.

<h2>Next Steps</h2>

- Physics logic for e.g. squares colliding. This makes collisions detection more complex, as well as the need to find where the collision happens, and to calculate torques etc..

- To add debug graphics like velocity arrows, system energy, etc., as well as debug tools like slowing down time etc.

- To extend this to 3D with a pan-able camera
