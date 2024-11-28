using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Tests.Core.Helpers;

namespace Tests.WPF;

public class CameraController(Control control)
{
    private Vector2? lastMousePosition;

    public void Update(float deltaTime)
    {
        if (!control.IsFocused)
        {
            return;
        }

        const float speed = 0.5f;

        if (Keyboard.IsKeyDown(Key.W))
        {
            Position += Forward * deltaTime * speed;
        }

        if (Keyboard.IsKeyDown(Key.S))
        {
            Position -= Forward * deltaTime * speed;
        }

        if (Keyboard.IsKeyDown(Key.A))
        {
            Position -= Right * deltaTime * speed;
        }

        if (Keyboard.IsKeyDown(Key.D))
        {
            Position += Right * deltaTime * speed;
        }

        if (Keyboard.IsKeyDown(Key.Q))
        {
            Position -= Up * deltaTime * speed;
        }

        if (Keyboard.IsKeyDown(Key.E))
        {
            Position += Up * deltaTime * speed;
        }

        if (Mouse.RightButton == MouseButtonState.Pressed)
        {
            if (lastMousePosition == null)
            {
                Point position = Mouse.GetPosition(control);

                lastMousePosition = new Vector2((float)position.X, (float)position.Y);
            }
        }
        else
        {
            lastMousePosition = null;
        }

        if (lastMousePosition.HasValue)
        {
            Point position = Mouse.GetPosition(control);

            Vector2 pos = new((float)position.X, (float)position.Y);

            Vector2 delta = pos - lastMousePosition.Value;

            float yaw = -delta.X * 0.01f;
            float pitch = -delta.Y * 0.01f;

            float newPitch = MathF.Asin(Forward.Y) + pitch;

            float clipRadians = DegToRad(89.0f);
            if (newPitch > clipRadians)
            {
                newPitch = clipRadians;
            }
            else if (newPitch < -clipRadians)
            {
                newPitch = -clipRadians;
            }

            pitch = newPitch - MathF.Asin(Forward.Y);

            Forward = Vector3.TransformNormal(Forward, Matrix4x4.CreateFromAxisAngle(Up, yaw));
            Forward = Vector3.TransformNormal(Forward, Matrix4x4.CreateFromAxisAngle(Right, pitch));

            Right = Vector3.Normalize(Vector3.Cross(Forward, Vector3.UnitY));
            Up = Vector3.Normalize(Vector3.Cross(Right, Forward));

            lastMousePosition = pos;
        }
    }

    public Vector3 Position { get; set; } = Vector3.Zero;

    public Vector3 Forward { get; set; } = Vector3.UnitZ;

    public Vector3 Right { get; set; } = Vector3.UnitX;

    public Vector3 Up { get; set; } = Vector3.UnitY;

    public float NearPlane { get; set; } = 0.1f;

    public float FarPlane { get; set; } = 1000.0f;

    public float Fov { get; set; } = 45.0f;

    public Matrix4x4 ViewMatrix => Matrix4x4.CreateLookAt(Position, Position + Forward, Up);

    public Matrix4x4 ProjectionMatrix(double width, double height)
    {
        return Matrix4x4.CreatePerspectiveFieldOfView(Fov.ToRadians(), (float)(width / height), NearPlane, FarPlane);
    }

    public void Transform(Matrix4x4 matrix)
    {
        Position = Vector3.Transform(Position, matrix);
        Forward = Vector3.TransformNormal(Forward, matrix);

        Right = Vector3.Normalize(Vector3.Cross(Forward, Vector3.UnitY));
        Up = Vector3.Normalize(Vector3.Cross(Right, Forward));
    }

    private static float DegToRad(float degrees)
    {
        return degrees * MathF.PI / 180.0f;
    }
}
