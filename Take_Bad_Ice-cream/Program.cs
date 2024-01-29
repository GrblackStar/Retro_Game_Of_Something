using Emotion.Common;
using Emotion.Game.World2D.SceneControl;
using Emotion.Game.World2D;
using Emotion.Common.Threading;
using Emotion.IO;
using System.Numerics;
using Emotion.Graphics.Objects;
using Emotion.Graphics;
using OpenGL;
using Emotion.Primitives;

namespace Take_Bad_Ice_cream
{
    public class Program
    {
        private static void Main(string[] args)
        {
            //Silk.NET.Assimp.Assimp.GetApi();
            var config = new Configurator
            {
                DebugMode = true
            };

            Engine.Setup(config);

            Engine.SceneManager.SetScene(new TestScene2D());

            Engine.Run();
        }
    }


    public class TestScene2D : World2DBaseScene<Map2D>
    {
        public override async Task LoadAsync()
        {
            //_editor.EnterEditor();
            var mapAsset = Engine.AssetLoader.Get<XMLAsset<Map2D>>("level_1_map.xml");
            await ChangeMapAsync(mapAsset.Content);
        }


        public override void Draw(RenderComposer composer)
        {
            composer.SetUseViewMatrix(false);
            composer.RenderSprite(Vector3.Zero, composer.CurrentTarget.Size, Color.CornflowerBlue);

            composer.ClearDepth();
            composer.SetUseViewMatrix(true);

            base.Draw(composer);
        }

        public override void Update()
        {
            base.Update();
        }


    }

}