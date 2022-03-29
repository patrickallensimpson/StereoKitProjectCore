using StereoKit;
using System;

namespace StereoKitProjectCore
{
    // make application than can create multiple shapes of different colors and allows use to clear all of them or some of them.


    enum ObjectType
    {
        Cube,
        Ball,
        Cyclinder
            // And show the pointer
            Default.MeshCube.Draw(Default.Material, c.aim.ToMatrix(new Vec3(1, 1, 4) * U.cm), Color.HSV(0, 0.5f, 0.8f).ToLinear());
    }

    class Entity
    {

        public Color color;
        public Model model;
        public Pose pose;
        public string name = System.Guid.NewGuid().ToString();

        public void Draw()
        {
            UI.Handle(name, ref this.pose, model.Bounds);
            model.Draw(pose.ToMatrix());
        }
    }

    class Program
    {
        static Pose windowPose = new Pose(-.2f, 0, -0.6f, Quat.LookDir(1, 0, 1));
        static ObjectType objectType = ObjectType.Cube;

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
        }

        static void drawWindow()
        {
            UI.WindowBegin("Objects", ref windowPose, new Vec2(40, 0) * U.cm, UIWin.Normal);
            UI.Label("Object To Create:");
            if (UI.Radio("Cube", objectType == ObjectType.Cube)) { objectType = ObjectType.Cube; }
            UI.SameLine();
            if (UI.Radio("Ball", objectType == ObjectType.Ball)) { objectType = ObjectType.Ball; }
            UI.SameLine();
            if (UI.Radio("Cyclinder", objectType == ObjectType.Cyclinder)) { objectType = ObjectType.Cyclinder; }
            if (UI.Button("New"))
            {
                Console.WriteLine("Created a new object");
                CreateNewEntity(objectType);
            }
            UI.WindowEnd();
        }

        static System.Collections.Generic.List<Entity> entities = new System.Collections.Generic.List<Entity>();

        static Matrix floorTransform = Matrix.TS(0, -1.5f, 0, new Vec3(30, 0.1f, 30));
        static Material floorMaterial = new Material(Shader.FromFile("floor.hlsl")) { Transparency = Transparency.Blend };

        static void RenderFloor()
        {
            if (SK.System.displayType == Display.Opaque)
                Default.MeshCube.Draw(floorMaterial, floorTransform);
        }

        static void Render()
        {
            drawWindow();
            foreach (var e in entities)
            {
                e.Draw();
            }
        }

        static Model cube = Model.FromMesh(
            Mesh.GenerateRoundedCube(Vec3.One * 0.1f, 0.02f),
            Default.MaterialUI);
        static Model ball = Model.FromMesh(
            Mesh.GenerateSphere(0.1f),
            Default.MaterialUI);
        static Model cyclinder = Model.FromMesh(
            Mesh.GenerateCylinder(0.1f, 0.2f, Vec3.One),
            Default.MaterialUI);

        static void CreateNewEntity(ObjectType objectType)
        {
            Model model = objectType switch
            {
                ObjectType.Cube => cube,
                ObjectType.Ball => ball,
                ObjectType.Cyclinder => cyclinder,
                _ => throw new Exception("Opps!")
            };
            Color color = Color.HSV(0, 1, 1);
            Pose pose = new Pose(0, 0, -0.5f, Quat.Identity);

            var entity = new Entity() { model = model, color = color, pose = pose };

            entities.Add(entity);
        }
        //static Pose applyVelocity(int deltaX, int deltaY, int deltaZ, ref Pose p)
        //{
        //    Pose nP = new Pose();
        //    return np;
        //}
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
            
            // Core application loop
            while (SK.Step(() =>
            {
                RenderFloor();

                Render();

                ShowController(Handed.Left);
                ShowController(Handed.Right);
                //UI.Handle("Cube", ref cubePose, cube.Bounds);
                //cube.Draw(cubePose.ToMatrix());

                //UI.Handle("Cube2", ref cubePose2, cube.Bounds);
                //cube.Draw(cubePose2.ToMatrix());
            })) ;
            SK.Shutdown();
        }
    }
}
