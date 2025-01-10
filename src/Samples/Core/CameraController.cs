using System.Numerics;
using Hexa.NET.ImGui;
using Tests.Core.Helpers;

namespace Tests.Core;

public class CameraController
{
    private Vector2? lastMousePosition;

    public CameraController(ViewController viewController)
    {
        viewController.MouseDown += MouseDown;
        viewController.MouseUp += MouseUp;
        viewController.MouseMove += MouseMove;
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

    public void Update(float deltaTime)
    {
        const float speed = 3.0f;

        if (ImGuiP.IsKeyDown(ImGuiKey.W))
        {
            Position += Forward * deltaTime * speed;
        }

        if (ImGuiP.IsKeyDown(ImGuiKey.S))
        {
            Position -= Forward * deltaTime * speed;
        }

        if (ImGuiP.IsKeyDown(ImGuiKey.A))
        {
            Position -= Right * deltaTime * speed;
        }

        if (ImGuiP.IsKeyDown(ImGuiKey.D))
        {
            Position += Right * deltaTime * speed;
        }

        if (ImGuiP.IsKeyDown(ImGuiKey.Q))
        {
            Position -= Up * deltaTime * speed;
        }

        if (ImGuiP.IsKeyDown(ImGuiKey.E))
        {
            Position += Up * deltaTime * speed;
        }
    }

    public void ShowEditor()
    {
        float nearPlane = NearPlane;
        ImGui.DragFloat("Near Plane", ref nearPlane, 0.1f);
        NearPlane = nearPlane;

        float farPlane = FarPlane;
        ImGui.DragFloat("Far Plane", ref farPlane, 0.1f);
        FarPlane = farPlane;

        float fov = Fov;
        ImGui.DragFloat("Fov", ref fov, 0.1f);
        Fov = fov;
    }

    private void MouseDown(object? sender, ImGuiMouseButtonEventArgs e)
    {
        if (e.Button == ImGuiMouseButton.Right)
        {
            lastMousePosition = e.Position;
        }
    }

    private void MouseUp(object? sender, ImGuiMouseButtonEventArgs e)
    {
        if (e.Button == ImGuiMouseButton.Right)
        {
            lastMousePosition = null;
        }
    }

    private void MouseMove(object? sender, ImGuiMouseMoveEventArgs e)
    {
        if (lastMousePosition.HasValue)
        {
            Vector2 delta = e.Position - lastMousePosition.Value;

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

            lastMousePosition = e.Position;
        }
    }

    private static float DegToRad(float degrees)
    {
        return degrees * MathF.PI / 180.0f;
    }
}
