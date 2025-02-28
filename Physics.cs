using OpenTK.Mathematics;

namespace spherical_pool_in_a_vacuum
{
    public class RigidBody
    {
        public Vector2 Position {get; set;}
        public Vector2 Velocity {get; set;}
        public float Theta; // angle of rotation
        public float Omega; // rotational velocity
        public float Mass;
        public bool Potted;

        public RigidBody(Vector2 position, Vector2 velocity, float theta, float omega, float mass, bool potted)
        {
            Position = position;
            Velocity = velocity;
            Theta = theta;
            Omega = omega;
            Mass = mass;
            Potted = potted;
        }

        public void EffectForce(Vector2 force)
        {
            Velocity += force / Mass;
        }

        public void overridePosition(float dx, float dy)
        {
            Position += new Vector2(dx, dy);
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

        public bool IsPotted(int width, int height)
        {
            float pocketDetectionRadius = 6* Sim.ballRadius * Sim.ballRadius;
            Vector2 topLeft = new Vector2(-width + Sim.boundaryLeft, -height + Sim.boundaryTop);

            if ((topLeft - Position).LengthSquared < pocketDetectionRadius)
            {
                return true;
            }

            Vector2 topRight = new Vector2(width - Sim.boundaryRight, -height + Sim.boundaryTop);
            if ((topRight - Position).LengthSquared < pocketDetectionRadius)
            {
                return true;
            }

            Vector2 bottomLeft = new Vector2(- width + Sim.boundaryLeft, height - Sim.boundaryBottom);
            if ((bottomLeft - Position).LengthSquared < pocketDetectionRadius)
            {
                return true;
            }

            Vector2 bottomRight = new Vector2(width - Sim.boundaryRight, height - Sim.boundaryBottom);
            if ((bottomRight - Position).LengthSquared < pocketDetectionRadius)
            {
                return true;
            }

            Vector2 middleLeft = new Vector2(-width + Sim.boundaryLeft, 0);
            if ((middleLeft - Position).LengthSquared < pocketDetectionRadius)
            {
                return true;
            }

            Vector2 middleRight = new Vector2(width - Sim.boundaryRight, 0);
            if ((middleRight - Position).LengthSquared < pocketDetectionRadius)
            {
                return true;
            }

            return false;
        }

        public void EdgeCheckAndResolve(int width, int height, float restitution)
        {
            if (Position.X - Sim.ballRadius <= -width + Sim.boundaryLeft)
            {
                Velocity = new Vector2(Velocity.X * -restitution, Velocity.Y);
                Position = new Vector2(-width + Sim.ballRadius + Sim.boundaryLeft, Position.Y);
            }
            else if (Position.X + Sim.ballRadius >= width - Sim.boundaryRight)
            {
                Velocity = new Vector2(Velocity.X * -restitution, Velocity.Y);
                Position = new Vector2(width - Sim.ballRadius - Sim.boundaryRight, Position.Y);
            }

            if (Position.Y - Sim.ballRadius <= -height + Sim.boundaryTop)
            {
                Velocity = new Vector2(Velocity.X, Velocity.Y * -restitution);
                Position = new Vector2(Position.X, -height + Sim.ballRadius + Sim.boundaryTop);
            }
            else if (Position.Y + Sim.ballRadius >= height - Sim.boundaryBottom)
            {
                Velocity = new Vector2(Velocity.X, Velocity.Y * -restitution);
                Position = new Vector2(Position.X, height - Sim.ballRadius - Sim.boundaryBottom);
            }
        }
        
        public static void CollisionCheckAndResolve(RigidBody ball1, RigidBody ball2, float restitution) {

            Vector2 relativePosition = ball1.Position - ball2.Position;
            float distance = relativePosition.Length;

            if (distance <= 2 * Sim.ballRadius)
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