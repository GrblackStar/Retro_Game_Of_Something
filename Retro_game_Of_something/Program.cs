#region Using

using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Emotion.Common;
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


public class Cup : GameObject2D

{
    public float velosity = (10f / (1000 / 60f)) * Engine.DeltaTime;

    public Color color = Color.White;
    public Vector2 Scale = new Vector2(10, 15);

    public TextureAsset textureAsset;

    public int dropsCount = 0;

    //List<LemonDrop> lemonDrops;



    protected override void UpdateInternal(float dt)
    {
        
        if (Engine.Host.IsKeyHeld(Platform.Input.Key.UpArrow))
        {
            Y = Position.Y - velosity;
        }
        if (Engine.Host.IsKeyHeld(Platform.Input.Key.DownArrow))
        {
            Y = Position.Y + velosity;
        }
        if (Engine.Host.IsKeyHeld(Platform.Input.Key.RightArrow))
        {
            X = Position.X + velosity;
        }
        if (Engine.Host.IsKeyHeld(Platform.Input.Key.LeftArrow))
        {
            X = Position.X - velosity;
        }

       // Map.GetObjects();

        /*
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
        */
        
    }

    public override async Task LoadAssetsAsync()
    {
        textureAsset = await Engine.AssetLoader.GetAsync<TextureAsset>("a_cup.png");
    }
    public void Load()
    {
        
    }


    protected override void RenderInternal(RenderComposer composer)
    {
        Vector2 juiceSize = textureAsset.Texture.Size - new Vector2(20, 20);
        Vector2 juiceCurrent = juiceSize * new Vector2(1f, dropsCount / 20f);
        composer.RenderSprite(Position + new Vector3(10, 10, 0) + new Vector3(0, juiceSize.Y - juiceCurrent.Y, 0), juiceCurrent, Color.PrettyYellow);
        composer.RenderSprite(Position, textureAsset.Texture.Size, color, textureAsset.Texture);
        
    }

}

public class TestScene2D : World2DBaseScene<Map2D>
{
    private ShaderAsset _triangleShader;

	public override async Task LoadAsync()
	{
		//_editor.EnterEditor();
        
        cup = new Cup();
        cup.Position = new Vector3(50, 50, 0);
        cup.Load();
		lemin = new Lemon();
        lemonDrops = new();

        await ChangeMapAsync(new Map2D(new Vector2(0, 0), "vlad"));
        CurrentMap.AddObject(cup);

        //CurrentMap

        _triangleShader = Engine.AssetLoader.Get<ShaderAsset>("Shaders/HelloTriangle.xml");

        //return Task.CompletedTask;
	}

	public Vector3 Position;
	public Lemon lemin;
    public Cup cup;
    public List<LemonDrop> lemonDrops;

    struct Colored
    {
        public Color color;
        public Vector3 position;

        public Colored(Color color, Vector3 position)
        {
            this.color = color;
            this.position = position;
        }
    }

    public override void Draw(RenderComposer composer)
	{
		composer.SetUseViewMatrix(false);
		composer.RenderSprite(Vector3.Zero, composer.CurrentTarget.Size, Color.CornflowerBlue);

        {
            composer.SetShader(_triangleShader.Shader);

            uint vbo = Gl.GenBuffer();
            /*
            Vector3 a = new Vector3(1, 0, 0);
            Vector3 b = new Vector3(0, 1, 0);
            Vector3 c = new Vector3(-1, 0, 0);
            Vector3[] triangle = new Vector3[] { a, b, c };
            */

            Colored a = new Colored(Color.PrettyYellow, new Vector3(0.5f, -0.2f, 0));
            Colored b = new Colored(Color.PrettyBlue, new Vector3(0, 0.5f, 0));
            Colored c = new Colored(Color.PrettyRed, new Vector3(-0.5f, -0.2f, 0));
            Colored[] triangle = new Colored[] { a, b, c };

            Gl.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            // sizeof(float)*3 -> the vectors; sizeof(byte)*4 -> Color 
            // sizeof(float)*3 + sizeof(byte)*4) -> one Colored object
            // sizeof(float)*3 + sizeof(byte)*4) * 3 -> the triangle array
            Gl.BufferData(BufferTarget.ArrayBuffer, (sizeof(float) * 3 + sizeof(byte) * 4) * 3, triangle, BufferUsage.StaticDraw);

            uint vao = Gl.GenVertexArray();

            Gl.BindVertexArray(vao);

            Gl.EnableVertexAttribArray(0);
            Gl.VertexAttribPointer(0, 4, VertexAttribType.UnsignedByte, true, sizeof(float) * 3 + sizeof(byte) * 4, 0);

            Gl.EnableVertexAttribArray(1);
            Gl.VertexAttribPointer(1, 3, VertexAttribType.Float, false, sizeof(float) * 3 + sizeof(byte) * 4, sizeof(byte) * 4);

            Gl.DrawArrays(PrimitiveType.Triangles, 0, 3);

            Gl.DeleteBuffers(vbo);
            Gl.DeleteVertexArrays(vao);

            composer.SetShader(null);

            VertexBuffer.Bound = 0;
            IndexBuffer.Bound = 0;
            VertexArrayObject.Bound = 0;
        }

        composer.RenderString(new Vector3(0, 0, 0), Color.Black, $"Score: {cup.dropsCount}", FontAsset.GetDefaultBuiltIn().GetAtlas(20f));
		composer.ClearDepth();
		composer.SetUseViewMatrix(true);

		//composer.RenderEllipse(Position, new Vector2(10, 15), Color.PrettyYellow, true);
		lemin.Draw(composer);
        //cup.Draw(composer);

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
        //cup.Update(lemonDrops);
        foreach (var drop in lemonDrops)
        {
            drop.Update();
        }

        base.Update();
    }


}