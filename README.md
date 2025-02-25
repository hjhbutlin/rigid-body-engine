<h1>Rigid Body Engine in C#</h1>

<h2>Project Motivation</h2>

As a Physics student, I was keen to embark on a project that combined my existing mechanics knowledge with a new & relevant programming language. Initially I settled on making a simulation of a pool break, but quickly changed my mind to focus on creating a more generalised rigid body engine.

<h2>Graphics Rendering</h2>

I had just wrapped up a project using SFML for 2D rendering, and while SFML would work fine for this project, I wanted to try something new and potentially more involved. After some research, I decided to use OpenGL for graphics rendering. This proved to me slightly less trivial to implement than I assumed, so I spent some time watching free CS lectures on VBOs, VAOs, and the GPU, as well as following a tutorial on making a big red triangle appear on the screen.

Overjoyed to have overcome this challenge, I began to implement some very basic physics to get this shape (now a circle) to move around the screen. The natural follow-up was to get a collision going. Since all my shapes (at least for now) were identical in every way except individual positions and velocities, I chose to store the base vertex positions just once, rather than store the positions of every object's vertices, as from my research this seemed to be best practice. I then could project this set of vertices to different positions on the screen by defining projection matrices inside the GLSL code.

Getting the graphics working properly was the hardest part of this project, as it involved both the then-unfamiliar C# and solid background understanding. It was a valuable and enjoyable opportunity to practise learning, debugging, and researching new topics.

<h2>Basic Engine</h2>

This project is centred on the interactions and collisions between rigid objects. Once the basics of OpenGL rendering were sorted, I began implementing a simple physics engine to deal with resolving collisions and friction. The engine has a modular design to facilitate the addition of e.g. gravity, and to make my life easier when I add the physics for collisions between not-circles (technical term: shapes that aren't circles). To this end, variables like angle of rotation and angular velocity were included in the initial code despite their redundancy in the circular world.

