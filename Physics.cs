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

        public void EdgeCheckAndResolve(float ballRadius, int width, int height, float restitution)
        {
            if (Position.X - ballRadius <= -width)
                {
                    Velocity = new Vector2(Velocity.X * -restitution, Velocity.Y);
                    Position = new Vector2(-width + ballRadius, Position.Y);
                }
                else if (Position.X + ballRadius >= width)
                {
                    Velocity = new Vector2(Velocity.X * -restitution, Velocity.Y);
                    Position = new Vector2(width - ballRadius, Position.Y);
                }

                if (Position.Y - ballRadius <= -height)
                {
                    Velocity = new Vector2(Velocity.X, Velocity.Y * -restitution);
                    Position = new Vector2(Position.X, -height + ballRadius);
                }
                else if (Position.Y + ballRadius >= height)
                {
                    Velocity = new Vector2(Velocity.X, Velocity.Y * -restitution);
                    Position = new Vector2(Position.X, height - ballRadius);
                }
        }
        
        public static void CollisionCheckAndResolve(RigidBody ball1, RigidBody ball2, float ballRadius, float restitution) {

            Vector2 relativePosition = ball1.Position - ball2.Position;
            float distance = relativePosition.Length;

            if (distance <= 2 * ballRadius)
            {
                Vector2 normal = Vector2.Normalize(relativePosition);
                Vector2 relativeVelocity = ball1.Velocity - ball2.Velocity;
                float speedProjection = Vector2.Dot(relativeVelocity, normal);

                // no collision if balls are already moving apart
                if (speedProjection > 0)
                {
                    return;
                }

                // reduced mass and restitution
                Vector2 impulse = normal * (1 + restitution) * speedProjection / ((1 / ball1.Mass) + (1 / ball2.Mass));
                ball1.Velocity -= impulse / ball1.Mass;
                ball2.Velocity += impulse / ball2.Mass;
                
                System.Console.WriteLine(ball1.Velocity);
                System.Console.WriteLine(ball2.Velocity);
            }
        }
    }
}