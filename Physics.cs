using System.Security.Cryptography;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace spherical_pool_in_a_vacuum
{
    internal class RigidBody
    {
        public Vector2 Position {get; set;}
        public Vector2 Velocity {get; set;}
        public float Theta; // angle of rotation
        public float Omega; // rotational velocity
        public float Mass;

        public RigidBody(Vector2 position, Vector2 velocity, float theta, float omega, float mass)
        {
            Position = position;
            Velocity = velocity;
            Theta = theta;
            Omega = omega;
            Mass = mass;
        }

        public void EffectForce(Vector2 force)
        {
            Velocity += force / Mass;
        }

        public void EffectTorque()
        {
            // TODO; for now no change in rotation
        }
        // timestep dt
        public void Update(float dt)
        {
            Position += Velocity * dt;
            Theta += Omega * dt;
        }

        
    }
}