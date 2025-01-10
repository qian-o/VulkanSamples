using System.Numerics;
using Tests.Core.Helpers;

namespace Tests.AndroidApp.Controls;

public partial class CameraController : ContentView
{
    public static readonly BindableProperty PositionProperty = BindableProperty.Create(nameof(Position), typeof(Vector3), typeof(CameraController), Vector3.Zero);
    public static readonly BindableProperty ForwardProperty = BindableProperty.Create(nameof(Forward), typeof(Vector3), typeof(CameraController), Vector3.UnitZ);
    public static readonly BindableProperty RightProperty = BindableProperty.Create(nameof(Right), typeof(Vector3), typeof(CameraController), Vector3.UnitX);
    public static readonly BindableProperty UpProperty = BindableProperty.Create(nameof(Up), typeof(Vector3), typeof(CameraController), Vector3.UnitY);
    public static readonly BindableProperty NearPlaneProperty = BindableProperty.Create(nameof(NearPlane), typeof(float), typeof(CameraController), 0.1f);
    public static readonly BindableProperty FarPlaneProperty = BindableProperty.Create(nameof(FarPlane), typeof(float), typeof(CameraController), 1000f);
    public static readonly BindableProperty FovProperty = BindableProperty.Create(nameof(Fov), typeof(float), typeof(CameraController), 45.0f);

    private Vector2? lastPanPosition;

    public CameraController()
    {
        InitializeComponent();
    }

    public Vector3 Position
    {
        get => (Vector3)GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    public Vector3 Forward
    {
        get => (Vector3)GetValue(ForwardProperty);
        set => SetValue(ForwardProperty, value);
    }

    public Vector3 Right
    {
        get => (Vector3)GetValue(RightProperty);
        set => SetValue(RightProperty, value);
    }

    public Vector3 Up
    {
        get => (Vector3)GetValue(UpProperty);
        set => SetValue(UpProperty, value);
    }

    public float NearPlane
    {
        get => (float)GetValue(NearPlaneProperty);
        set => SetValue(NearPlaneProperty, value);
    }

    public float FarPlane
    {
        get => (float)GetValue(FarPlaneProperty);
        set => SetValue(FarPlaneProperty, value);
    }

    public float Fov
    {
        get => (float)GetValue(FovProperty);
        set => SetValue(FovProperty, value);
    }

    private void TouchArea_PanUpdated(object sender, PanUpdatedEventArgs e)
    {
        if (e.StatusType == GestureStatus.Completed)
        {
            lastPanPosition = null;

            return;
        }

        if (lastPanPosition.HasValue)
        {
            Vector2 pan = new Vector2((float)e.TotalX, (float)e.TotalY) - lastPanPosition.Value;

            float yaw = -pan.X * 0.01f;
            float pitch = -pan.Y * 0.01f;

            float newPitch = MathF.Asin(Forward.Y) + pitch;

            float clipRadians = (MathF.PI / 2) - 0.01f;
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

            lastPanPosition = new Vector2((float)e.TotalX, (float)e.TotalY);
        }
        else
        {
            lastPanPosition = new Vector2((float)e.TotalX, (float)e.TotalY);
        }
    }

    public void Update()
    {
        if (Joystick.IsMoving)
        {
            Position += Vector3.Normalize(Vector3.Transform(Forward, Matrix4x4.CreateFromAxisAngle(Up, -Joystick.Radians))) * 0.1f;
        }
    }

    public void Transform(Matrix4x4 matrix)
    {
        Position = Vector3.Transform(Position, matrix);
        Forward = Vector3.TransformNormal(Forward, matrix);

        Right = Vector3.Normalize(Vector3.Cross(Forward, Vector3.UnitY));
        Up = Vector3.Normalize(Vector3.Cross(Right, Forward));
    }

    public Matrix4x4 GetView()
    {
        return Matrix4x4.CreateLookAt(Position, Position + Forward, Up);
    }

    public Matrix4x4 GetProjection(double width, double height)
    {
        return Matrix4x4.CreatePerspectiveFieldOfView(Fov.ToRadians(), (float)(width / height), NearPlane, FarPlane);
    }
}