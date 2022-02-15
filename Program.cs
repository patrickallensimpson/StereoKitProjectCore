using StereoKit;
using System;

namespace StereoKitProjectCore
{
    class Program
    {

        static void ShowController(Handed hand)
        {
            Controller c = Input.Controller(hand);
            if (!c.IsTracked) return;

            Hierarchy.Push(c.pose.ToMatrix());
            // Pick the controller color based on trackin info state
            Color color = Color.Black;
            if (c.trackedPos == TrackState.Inferred) color.g = 0.5f;
            if (c.trackedPos == TrackState.Known) color.g = 1;
            if (c.trackedRot == TrackState.Inferred) color.b = 0.5f;
            if (c.trackedRot == TrackState.Known) color.b = 1;
            Default.MeshCube.Draw(Default.Material, Matrix.S(new Vec3(3, 3, 8) * U.cm), color);

            // Show button info on the back of the controller
            Hierarchy.Push(Matrix.TR(0, 1.6f * U.cm, 0, Quat.LookAt(Vec3.Zero, new Vec3(0, 1, 0), new Vec3(0, 0, -1))));

            // Show the tracking states as text
            Text.Add(c.trackedPos == TrackState.Known ? "(pos)" : (c.trackedPos == TrackState.Inferred ? "~pos~" : "pos"), Matrix.TS(0, -0.03f, 0, 0.25f));
            Text.Add(c.trackedRot == TrackState.Known ? "(rot)" : (c.trackedRot == TrackState.Inferred ? "~rot~" : "rot"), Matrix.TS(0, -0.02f, 0, 0.25f));

            // Show the controller's buttons
            Text.Add(Input.ControllerMenuButton.IsActive() ? "(menu)" : "menu", Matrix.TS(0, -0.01f, 0, 0.25f));
            Text.Add(c.IsX1Pressed ? "(X1)" : "X1", Matrix.TS(0, 0.00f, 0, 0.25f));
            Text.Add(c.IsX2Pressed ? "(X2)" : "X2", Matrix.TS(0, 0.01f, 0, 0.25f));

            // Show the analog stick's information
            Vec3 stickAt = new Vec3(0, 0.03f, 0);
            Lines.Add(stickAt, stickAt + c.stick.XY0 * 0.01f, Color.White, 0.001f);
            if (c.IsStickClicked) Text.Add("O", Matrix.TS(stickAt, 0.25f));

            // And show the trigger and grip buttons
            Default.MeshCube.Draw(Default.Material, Matrix.TS(0, -0.015f, -0.005f, new Vec3(0.01f, 0.04f, 0.01f)) * Matrix.TR(new Vec3(0, 0.02f, 0.03f), Quat.FromAngles(-45 + c.trigger * 40, 0, 0)));
            Default.MeshCube.Draw(Default.Material, Matrix.TS(0.0149f * (hand == Handed.Right ? 1 : -1), 0, 0.015f, new Vec3(0.01f * (1 - c.grip), 0.04f, 0.01f)));

            Hierarchy.Pop();
            Hierarchy.Pop();

            // And show the pointer
            Default.MeshCube.Draw(Default.Material, c.aim.ToMatrix(new Vec3(1, 1, 4) * U.cm), Color.HSV(0, 0.5f, 0.8f).ToLinear());
        }

        struct Entity
        {
            public Pose pose;
            public Model model;

            public Entity(Pose pose,Model model) { this.pose = pose; this.model = model; }
        }

        static void Main(string[] args)
        {
            // Initialize StereoKit
            SKSettings settings = new SKSettings
            {
                appName = "StereoKitProjectCore",
                assetsFolder = "Assets",
            };
            if (!SK.Initialize(settings))
                Environment.Exit(1);

            // Create assets used by the app
            Entity cube = new Entity()
            {
                pose = new Pose(0, 0, -0.5f, Quat.Identity),
                model = Model.FromMesh(
                    Mesh.GenerateRoundedCube(Vec3.One * 0.1f, 0.02f),
                    Default.MaterialUI)
            };
            /*
            Pose cubePose = new Pose(0, 0, -0.5f, Quat.Identity);
            Model cube = Model.FromMesh(
                Mesh.GenerateRoundedCube(Vec3.One * 0.1f, 0.02f),
                Default.MaterialUI);
            */
            Entity ball = new Entity()
            {
                pose = new Pose(0, 0, 0.5f, Quat.Identity),
                model = Model.FromMesh(
                    Mesh.GenerateSphere(0.1f, 8),
                    Default.MaterialUI)
            };
            /*
            Pose ballPose = new Pose(0, 0, 0.5f, Quat.Identity);
            Model ball = Model.FromMesh(
                Mesh.GenerateSphere(0.1f, 8),
                Default.MaterialUI);
            */

            Matrix floorTransform = Matrix.TS(0, -1.5f, 0, new Vec3(30, 0.1f, 30));
            Material floorMaterial = new Material(Shader.FromFile("floor.hlsl"));
            floorMaterial.Transparency = Transparency.Blend;


            // Core application loop
            while (SK.Step(() =>
            {
                if (SK.System.displayType == Display.Opaque)
                    Default.MeshCube.Draw(floorMaterial, floorTransform);

                UI.Handle("Cube", ref cube.pose, cube.model.Bounds);
                cube.model.Draw(cube.pose.ToMatrix());
                UI.Handle("Ball", ref ball.pose, ball.model.Bounds);
                ball.model.Draw(ball.pose.ToMatrix(), Color.HSV(0, 0.5f, 0.8f).ToLinear());
                //ShowController(Handed.Left);
                //ShowController(Handed.Right);
            })) ;
            SK.Shutdown();
        }
    }
}
