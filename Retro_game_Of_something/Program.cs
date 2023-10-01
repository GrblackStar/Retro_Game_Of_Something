#region Using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
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

public class Lemon : GameObject2D
{
	public float velosity = (10f / (1000 / 60f)) * Engine.DeltaTime;

	public Color color =  Color.PrettyYellow;
	public Vector2 Scale = new Vector2(10, 15);

    public float SpawnTime = 10;
    public float timepassed = 0;

    protected override void UpdateInternal(float dt)
    {
        if (Engine.Host.IsKeyHeld(Platform.Input.Key.W))
        {
            Y = Position.Y - velosity;
        }
        if (Engine.Host.IsKeyHeld(Platform.Input.Key.S))
        {
            Y = Position.Y + velosity;
        }
        if (Engine.Host.IsKeyHeld(Platform.Input.Key.D))
        {
            X = Position.X + velosity;
        }
        if (Engine.Host.IsKeyHeld(Platform.Input.Key.A))
        {
            X = Position.X - velosity;
        }

        if (Engine.Host.IsKeyDown(Platform.Input.Key.Space))
        {
            LemonDrop lemonDrop = new LemonDrop();
            lemonDrop.Position = Position;
            //lemonDrops.Add(lemonDrop);
            
            // HOOOOLY HELL IT WORKED
            Map.AddObject(lemonDrop);

        }
    }

    protected override void RenderInternal(RenderComposer composer)
    {
        composer.RenderEllipse(Position, Scale, color, true);
    }


}


public class LemonDrop : GameObject2D
{
    public float velosity = (10f / (1000 / 60f)) * Engine.DeltaTime;

    public Color color = Color.PrettyYellow;
    public Vector2 Scale = new Vector2(5, 5);


    protected override void UpdateInternal(float dt)
    {
        Y += velosity;
    }

    protected override void RenderInternal(RenderComposer composer)
    {
        composer.RenderEllipse(Position, Scale, color, true);
    }

}


public class Cup : GameObject2D

{
    public float velosity = (10f / (1000 / 60f)) * Engine.DeltaTime;

    public Color color = Color.White;

    public TextureAsset textureAsset;

    public int dropsCount = 0;
    List<LemonDrop> lemonDrops = new List<LemonDrop>();

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
        
        List<LemonDrop> drops = new List<LemonDrop>();
        Map.GetObjectsByType<LemonDrop>(drops, 0, Bounds);

        foreach (var drop in drops)
        {
                dropsCount++;
                Map.RemoveObject(drop);
        }
    }

    public override async Task LoadAssetsAsync()
    {
        textureAsset = await Engine.AssetLoader.GetAsync<TextureAsset>("a_cup.png");
        Size = textureAsset.Texture.Size;

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
    private Colored[] _triangle = new Colored[] {
                new Colored(Color.PrettyYellow, new Vector3(0.5f, -0.2f, 0)),
                new Colored(Color.PrettyBlue, new Vector3(0, 0.5f, 0)),
                new Colored(Color.PrettyRed, new Vector3(-0.5f, -0.2f, 0)),

               new Colored(Color.PrettyYellow, new Vector3(-0.5f, 0.6f, 0)),
               new Colored(Color.PrettyBlue, new Vector3(-0.48f, 0.6f, 0)),
               new Colored(Color.PrettyRed, new Vector3(-0.48f, 0.95f, 0)),

               new Colored(Color.PrettyYellow, new Vector3(-0.5f, 0.6f, 0)),
               new Colored(Color.PrettyBlue, new Vector3(-0.48f, 0.95f, 0)),
               new Colored(Color.PrettyRed, new Vector3(-0.5f, 0.95f, 0)),

               new Colored(Color.PrettyYellow, new Vector3(-0.4f, 0.6f, 0)),
               new Colored(Color.PrettyBlue, new Vector3(-0.38f, 0.6f, 0)),
               new Colored(Color.PrettyRed, new Vector3(-0.38f, 0.95f, 0)),

               new Colored(Color.PrettyYellow, new Vector3(-0.4f, 0.6f, 0)),
               new Colored(Color.PrettyBlue, new Vector3(-0.38f, 0.95f, 0)),
               new Colored(Color.PrettyRed, new Vector3(-0.4f, 0.95f, 0)),

               new Colored(Color.PrettyYellow, new Vector3(-0.48f, 0.765f, 0)),
               new Colored(Color.PrettyBlue, new Vector3(-0.4f, 0.765f, 0)),
               new Colored(Color.PrettyRed, new Vector3(-0.4f, 0.785f, 0)),
               
               new Colored(Color.PrettyYellow, new Vector3(-0.48f, 0.765f, 0)),
               new Colored(Color.PrettyBlue, new Vector3(-0.4f, 0.785f, 0)),
               new Colored(Color.PrettyRed, new Vector3(-0.48f, 0.785f, 0)),

               new Colored(Color.PrettyYellow, new Vector3(-0.3f, 0.6f, 0)),
               new Colored(Color.PrettyBlue, new Vector3(-0.28f, 0.6f, 0)),
               new Colored(Color.PrettyRed, new Vector3(-0.28f, 0.95f, 0)),

               new Colored(Color.PrettyYellow, new Vector3(-0.3f, 0.6f, 0)),
               new Colored(Color.PrettyBlue, new Vector3(-0.28f, 0.95f, 0)),
               new Colored(Color.PrettyRed, new Vector3(-0.3f, 0.95f, 0)),

               new Colored(Color.PrettyYellow, new Vector3(-0.28f, 0.93f, 0)),
               new Colored(Color.PrettyBlue, new Vector3(-0.2f, 0.93f, 0)),
               new Colored(Color.PrettyRed, new Vector3(-0.2f, 0.95f, 0)),

               new Colored(Color.PrettyYellow, new Vector3(-0.28f, 0.93f, 0)),
               new Colored(Color.PrettyBlue, new Vector3(-0.2f, 0.95f, 0)),
               new Colored(Color.PrettyRed, new Vector3(-0.28f, 0.95f, 0)),

               new Colored(Color.PrettyYellow, new Vector3(-0.28f, 0.765f, 0)),
               new Colored(Color.PrettyBlue, new Vector3(-0.22f, 0.765f, 0)),
               new Colored(Color.PrettyRed, new Vector3(-0.22f, 0.785f, 0)),

               new Colored(Color.PrettyYellow, new Vector3(-0.28f, 0.765f, 0)),
               new Colored(Color.PrettyBlue, new Vector3(-0.22f, 0.785f, 0)),
               new Colored(Color.PrettyRed, new Vector3(-0.28f, 0.785f, 0)),

               new Colored(Color.PrettyYellow, new Vector3(-0.28f, 0.6f, 0)),
               new Colored(Color.PrettyBlue, new Vector3(-0.2f, 0.6f, 0)),
               new Colored(Color.PrettyRed, new Vector3(-0.2f, 0.62f, 0)),

               new Colored(Color.PrettyYellow, new Vector3(-0.28f, 0.6f, 0)),
               new Colored(Color.PrettyBlue, new Vector3(-0.2f, 0.62f, 0)),
               new Colored(Color.PrettyRed, new Vector3(-0.28f, 0.62f, 0)),
               
               new Colored(Color.PrettyYellow, new Vector3(-0.1f, 0.6f, 0)),
               new Colored(Color.PrettyBlue, new Vector3(-0.08f, 0.95f, 0)),
               new Colored(Color.PrettyRed, new Vector3(-0.1f, 0.95f, 0)),

               new Colored(Color.PrettyYellow, new Vector3(-0.1f, 0.6f, 0)),
               new Colored(Color.PrettyBlue, new Vector3(-0.08f, 0.6f, 0)),
               new Colored(Color.PrettyRed, new Vector3(-0.08f, 0.95f, 0)),

               new Colored(Color.PrettyYellow, new Vector3(-0.08f, 0.6f, 0)),
               new Colored(Color.PrettyBlue, new Vector3(0f, 0.6f, 0)),
               new Colored(Color.PrettyRed, new Vector3(0f, 0.62f, 0)),

               new Colored(Color.PrettyYellow, new Vector3(-0.08f, 0.6f, 0)),
               new Colored(Color.PrettyBlue, new Vector3(0f, 0.62f, 0)),
               new Colored(Color.PrettyRed, new Vector3(-0.08f, 0.62f, 0)),
               
               new Colored(Color.PrettyYellow, new Vector3(0.08f, 0.6f, 0)),
               new Colored(Color.PrettyBlue, new Vector3(0.1f, 0.95f, 0)),
               new Colored(Color.PrettyRed, new Vector3(0.08f, 0.95f, 0)),

               new Colored(Color.PrettyYellow, new Vector3(0.08f, 0.6f, 0)),
               new Colored(Color.PrettyBlue, new Vector3(0.1f, 0.6f, 0)),
               new Colored(Color.PrettyRed, new Vector3(0.1f, 0.95f, 0)),

               new Colored(Color.PrettyYellow, new Vector3(0.1f, 0.6f, 0)),
               new Colored(Color.PrettyBlue, new Vector3(0.18f, 0.6f, 0)),
               new Colored(Color.PrettyRed, new Vector3(0.18f, 0.62f, 0)),

               new Colored(Color.PrettyYellow, new Vector3(0.1f, 0.6f, 0)),
               new Colored(Color.PrettyBlue, new Vector3(0.18f, 0.62f, 0)),
               new Colored(Color.PrettyRed, new Vector3(0.1f, 0.62f, 0)),
               
               new Colored(Color.PrettyYellow, new Vector3(0.26f, 0.6f, 0)),
               new Colored(Color.PrettyBlue, new Vector3(0.28f, 0.95f, 0)),
               new Colored(Color.PrettyRed, new Vector3(0.26f, 0.95f, 0)),

               new Colored(Color.PrettyYellow, new Vector3(0.26f, 0.6f, 0)),
               new Colored(Color.PrettyBlue, new Vector3(0.28f, 0.6f, 0)),
               new Colored(Color.PrettyRed, new Vector3(0.28f, 0.95f, 0)),

               new Colored(Color.PrettyYellow, new Vector3(0.26f, 0.6f, 0)),
               new Colored(Color.PrettyBlue, new Vector3(0.28f, 0.95f, 0)),
               new Colored(Color.PrettyRed, new Vector3(0.26f, 0.95f, 0)),

               new Colored(Color.PrettyYellow, new Vector3(0.26f, 0.6f, 0)),
               new Colored(Color.PrettyBlue, new Vector3(0.28f, 0.6f, 0)),
               new Colored(Color.PrettyRed, new Vector3(0.28f, 0.95f, 0)),

               new Colored(Color.PrettyYellow, new Vector3(0.28f, 0.6f, 0)),
               new Colored(Color.PrettyBlue, new Vector3(0.36f, 0.6f, 0)),
               new Colored(Color.PrettyRed, new Vector3(0.36f, 0.62f, 0)),

               new Colored(Color.PrettyYellow, new Vector3(0.28f, 0.6f, 0)),
               new Colored(Color.PrettyBlue, new Vector3(0.36f, 0.62f, 0)),
               new Colored(Color.PrettyRed, new Vector3(0.28f, 0.62f, 0)),

               new Colored(Color.PrettyYellow, new Vector3(0.28f, 0.93f, 0)),
               new Colored(Color.PrettyBlue, new Vector3(0.36f, 0.93f, 0)),
               new Colored(Color.PrettyRed, new Vector3(0.36f, 0.95f, 0)),

               new Colored(Color.PrettyYellow, new Vector3(0.28f, 0.93f, 0)),
               new Colored(Color.PrettyBlue, new Vector3(0.36f, 0.95f, 0)),
               new Colored(Color.PrettyRed, new Vector3(0.28f, 0.95f, 0)),

               new Colored(Color.PrettyYellow, new Vector3(0.36f, 0.6f, 0)),
               new Colored(Color.PrettyBlue, new Vector3(0.38f, 0.95f, 0)),
               new Colored(Color.PrettyRed, new Vector3(0.36f, 0.95f, 0)),

               new Colored(Color.PrettyYellow, new Vector3(0.36f, 0.6f, 0)),
               new Colored(Color.PrettyBlue, new Vector3(0.38f, 0.6f, 0)),
               new Colored(Color.PrettyRed, new Vector3(0.38f, 0.95f, 0)),

               new Colored(Color.PrettyYellow, new Vector3(0.48f, 0.6f, 0)),
               new Colored(Color.PrettyBlue, new Vector3(0.5f, 0.66f, 0)),
               new Colored(Color.PrettyRed, new Vector3(0.46f, 0.66f, 0)),

               new Colored(Color.PrettyYellow, new Vector3(0.48f, 0.67f, 0)),
               new Colored(Color.PrettyBlue, new Vector3(0.5f, 0.95f, 0)),
               new Colored(Color.PrettyRed, new Vector3(0.46f, 0.95f, 0))

               };
    uint vbo;
    uint vao;


    public override async Task LoadAsync()
	{
		//_editor.EnterEditor();
        
        // can directly initilize the riangles and than just to dram them in the Draw(), so they don't get calculated every frame
        cup = new Cup();
        cup.Position = new Vector3(50, 50, 0);

		lemin = new Lemon();
        lemin.Position = new Vector3(0, 0, 0);

        lemonDrops = new();

        GLThread.ExecuteGLThread(() =>
        {
            vbo = Gl.GenBuffer();
            vao = Gl.GenVertexArray();

            Gl.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            Gl.BindVertexArray(vao);


            Gl.EnableVertexAttribArray(0);
            Gl.VertexAttribPointer(0, 4, VertexAttribType.UnsignedByte, true, sizeof(float) * 3 + sizeof(byte) * 4, 0);

            Gl.EnableVertexAttribArray(1);
            Gl.VertexAttribPointer(1, 3, VertexAttribType.Float, false, sizeof(float) * 3 + sizeof(byte) * 4, sizeof(byte) * 4);


            Gl.BufferData(BufferTarget.ArrayBuffer, (sizeof(float) * 3 + sizeof(byte) * 4) * (uint)_triangle.Length, _triangle, BufferUsage.StaticDraw);
            //Gl.DrawArrays(PrimitiveType.Triangles, 0, _triangle.Length);

        });


        await ChangeMapAsync(new Map2D(new Vector2(0, 0), "vlad"));
        CurrentMap.AddObject(cup);
        CurrentMap.AddObject(lemin);

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

    public float rotation = 0.01f;
    public override void Draw(RenderComposer composer)
	{
		composer.SetUseViewMatrix(false);
		composer.RenderSprite(Vector3.Zero, composer.CurrentTarget.Size, Color.CornflowerBlue);

        {
            composer.SetShader(_triangleShader.Shader);

            Gl.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            Gl.BindVertexArray(vao);

            _triangleShader.Shader.SetUniformMatrix4("modelMatrix", Matrix4x4.CreateRotationZ(rotation));


            rotation = rotation + 0.01f;
            Gl.DrawArrays(PrimitiveType.Triangles, 0, _triangle.Length);

            // DELETING THE BUFFERS AT THE END OF THE FRAME

            composer.SetShader(null);

            VertexBuffer.Bound = 0;
            IndexBuffer.Bound = 0;
            VertexArrayObject.Bound = 0;
        }

        composer.RenderString(new Vector3(0, 0, 0), Color.Black, $"Score: {cup.dropsCount}", FontAsset.GetDefaultBuiltIn().GetAtlas(20f));
		composer.ClearDepth();
		composer.SetUseViewMatrix(true);

		base.Draw(composer);
	}

    public override void Update()
    {
        foreach (var drop in lemonDrops)
        {
            drop.Update(1f);
        }

        base.Update();
    }


}