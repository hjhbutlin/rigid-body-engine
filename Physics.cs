/*using OpenTK.Mathematics;

namespace physics_engine
{
    internal class RigidBody
    {
        public List<Vector3> Vertices;
        public Vector3 Position;
        public Vector3 Velocity;
        public float Theta; // angle of rotation
        public float Omega; // rotational velocity
        public float Mass;

        public RigidBody(List<Vector3> vertices, Vector3 position, Vector3 velocity, float theta, float omega, float mass)
        {
            Vertices = vertices;
            Position = position;
            Velocity = velocity;
            Theta = theta;
            Omega = omega;
            Mass = mass;
        }

        public void CentreOfMass()
        {
            // TODO; for now assume position is CoM
        }

        public void EffectForce(Vector3 force)
        {
            Velocity += force / Mass;
        }

        public void EffectTorque()

        // timestep dt
        public void Update(float dt)
        {

        }
    }
}*/