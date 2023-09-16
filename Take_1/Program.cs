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
using Emotion.Graphics;
using Emotion.Graphics.Objects;
using Emotion.Graphics.Shading;
using Emotion.IO;
using Emotion.Primitives;
using Emotion.Testing;
using Emotion.Utility;
using Microsoft.VisualBasic.FileIO;
using OpenGL;
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

        Engine.SceneManager.SetScene(new TestScene2D());

        Engine.Run();
    }
}


public class TestScene2D : World2DBaseScene<Map2D>
{
    public override async Task LoadAsync()
    {
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