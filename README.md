<h1>8 Ball Pool in C#</h1>

<h2>Project Motivation</h2>

As a Physics student, I was keen to embark on a project that combined my existing mechanics knowledge with a new & relevant programming language, so I decided to build a rigid body engine with the end goal of making a simple 8-ball pool game.

<h2>Graphics Rendering</h2>

I used OpenGL for graphics rendering. This proved to me slightly less trivial to implement than I assumed, so I spent some time watching free CS lectures on VBOs, VAOs, and the GPU, as well as following a tutorial on making a big red triangle appear on the screen.

I began to implement some very basic physics to get this shape (now a circle) to move around the screen. I added instance VBOs for position and rotation, and transformed the base set of vertices to different positions on the screen by defining projection matrices inside the GLSL code.

Getting the graphics working properly was the hardest part of this project, as it involved both the then-unfamiliar C# and solid background understanding. It was a valuable and enjoyable opportunity to practise learning, debugging, and researching new topics.

<h2>Next Steps</h2>

- Physics logic for e.g. squares colliding. This makes collisions detection more complex, as well as the need to find where the collision happens, and to calculate torques etc..

- To add debug graphics like velocity arrows, system energy, etc., as well as debug tools like slowing down time etc.

- To extend this to 3D with a pan-able camera
