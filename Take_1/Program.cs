#region Using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Common.Threading;
using Emotion.Game.World2D;
using Emotion.Game.World2D.SceneControl;
using Emotion.Game.World3D.SceneControl;
using Emotion.Graphics;
using Emotion.Graphics.Camera;
using Emotion.Graphics.Objects;
using Emotion.Graphics.Shading;
using Emotion.IO;
using Emotion.Primitives;
using Emotion.Testing;
using Emotion.Utility;
using Microsoft.VisualBasic.FileIO;
using OpenGL;
using Take_1;
using WinApi.User32;

#endregion
namespace Emotion.ExecTest;

public class Program
{
    private static void Main(string[] args)
    {
        var config = new Configurator
        {
            DebugMode = true
        };

        Engine.Setup(config);

        Engine.SceneManager.SetScene(new TakeOneGame());

        Engine.Run();
    }
}


public class TakeOneGame : World3DBaseScene<Take1Map>
{
    public override async Task LoadAsync()
    {
        var cam3D = new Camera3D(new Vector3(100));
        cam3D.LookAtPoint(Vector3.Zero);
        Engine.Renderer.Camera = cam3D;

        await ChangeMapAsync(new Take1Map(new Vector2(10, 10)));
        //return Task.CompletedTask;
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