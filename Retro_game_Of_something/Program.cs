#region Using

using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Game.World2D;
using Emotion.Game.World2D.SceneControl;
using Emotion.Graphics;
using Emotion.IO;
using Emotion.Primitives;
using Emotion.Testing;
using Emotion.Utility;

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

public class Lemon
{
	public float velosity = (10f / (1000 / 60f)) * Engine.DeltaTime;
	public Vector3 Position;

	public Color color =  Color.PrettyYellow;
	public Vector2 Scale = new Vector2(10, 15);

    public float SpawnTime = 10;
    public float timepassed = 0;

    public void Update(List<LemonDrop> lemonDrops)
	{
        if (Engine.Host.IsKeyHeld(Platform.Input.Key.W))
        {
            Position.Y = Position.Y - velosity;
        }
        if (Engine.Host.IsKeyHeld(Platform.Input.Key.S))
        {
            Position.Y = Position.Y + velosity;
        }
        if (Engine.Host.IsKeyHeld(Platform.Input.Key.D))
        {
            Position.X = Position.X + velosity;
        }
        if (Engine.Host.IsKeyHeld(Platform.Input.Key.A))
        {
            Position.X = Position.X - velosity;
        }

        if (Engine.Host.IsKeyDown(Platform.Input.Key.Space))
        {
            LemonDrop lemonDrop = new LemonDrop();
            lemonDrop.Position = Position;
            lemonDrops.Add(lemonDrop);

        }
    }

	public void Draw(RenderComposer composer)
	{
        composer.RenderEllipse(Position, Scale, color, true);
    }

}


public class LemonDrop
{
    public float velosity = (10f / (1000 / 60f)) * Engine.DeltaTime;
    public Vector3 Position;

    public Color color = Color.PrettyYellow;
    public Vector2 Scale = new Vector2(5, 5);


    public void Update()
    {
        Position.Y += velosity;
    }

    public void Draw(RenderComposer composer)
    {
        composer.RenderEllipse(Position, Scale, color, true);
    }

}


public class Cup
{
    public float velosity = (10f / (1000 / 60f)) * Engine.DeltaTime;
    public Vector3 Position;

    public Color color = Color.White;
    public Vector2 Scale = new Vector2(10, 15);

    public TextureAsset textureAsset;

    public int dropsCount = 0;

    public void Update(List<LemonDrop> lemonDrops)
    {
        
        if (Engine.Host.IsKeyHeld(Platform.Input.Key.UpArrow))
        {
            Position.Y = Position.Y - velosity;
        }
        if (Engine.Host.IsKeyHeld(Platform.Input.Key.DownArrow))
        {
            Position.Y = Position.Y + velosity;
        }
        if (Engine.Host.IsKeyHeld(Platform.Input.Key.RightArrow))
        {
            Position.X = Position.X + velosity;
        }
        if (Engine.Host.IsKeyHeld(Platform.Input.Key.LeftArrow))
        {
            Position.X = Position.X - velosity;
        }


        for (int i = 0; i < lemonDrops.Count; i++)
        {
            LemonDrop drop = lemonDrops[i];
            if (Math.Abs((drop.Position.Y + drop.Scale.Y) - this.Position.Y) <= 10)
            {
                dropsCount++;
                lemonDrops.Remove(drop);
                i--;
            }
        }
        
    }

    public void Load()
    {
        textureAsset = Engine.AssetLoader.Get<TextureAsset>("a_cup.png");
    }

    public void Draw(RenderComposer composer)
    {
        Vector2 juiceSize = textureAsset.Texture.Size - new Vector2(20, 20);
        Vector2 juiceCurrent = juiceSize * new Vector2(1f, dropsCount / 20f);
        composer.RenderSprite(Position + new Vector3(10, 10, 0) + new Vector3(0, juiceSize.Y - juiceCurrent.Y, 0), juiceCurrent, Color.PrettyYellow);
        composer.RenderSprite(Position, textureAsset.Texture.Size, color, textureAsset.Texture);
        
    }

}

public class TestScene2D : World2DBaseScene<Map2D>
{
	public override Task LoadAsync()
	{
		//_editor.EnterEditor();

        cup = new Cup();
        cup.Position = new Vector3(50, 50, 0);
        cup.Load();
		lemin = new Lemon();
        lemonDrops = new();
        


		return Task.CompletedTask;
	}

	public Vector3 Position;
	public Lemon lemin;
    public Cup cup;
    public List<LemonDrop> lemonDrops;

	public override void Draw(RenderComposer composer)
	{
		composer.SetUseViewMatrix(false);
		composer.RenderSprite(Vector3.Zero, composer.CurrentTarget.Size, Color.CornflowerBlue);
        composer.RenderString(new Vector3(0, 0, 0), Color.Black, $"Score: {cup.dropsCount}", FontAsset.GetDefaultBuiltIn().GetAtlas(20f));
		composer.ClearDepth();
		composer.SetUseViewMatrix(true);

		//composer.RenderEllipse(Position, new Vector2(10, 15), Color.PrettyYellow, true);
		lemin.Draw(composer);
        cup.Draw(composer);

        foreach (var drop in lemonDrops)
        {
            drop.Draw(composer);
        }

		base.Draw(composer);
	}

    public override void Update()
    {
		/*
		float velosity = (10f / (1000 / 60f)) * Engine.DeltaTime;

		if (Engine.Host.IsKeyHeld(Platform.Input.Key.W))
		{
			Position.Y = Position.Y - velosity;
		}
        if (Engine.Host.IsKeyHeld(Platform.Input.Key.S))
        {
            Position.Y = Position.Y + velosity;
        }
        if (Engine.Host.IsKeyHeld(Platform.Input.Key.D))
        {
			Position.X = Position.X + velosity;
        }
        if (Engine.Host.IsKeyHeld(Platform.Input.Key.A))
        {
            Position.X = Position.X - velosity;
        }
		*/

		lemin.Update(lemonDrops);
        cup.Update(lemonDrops);
        foreach (var drop in lemonDrops)
        {
            drop.Update();
        }

        base.Update();
    }


}