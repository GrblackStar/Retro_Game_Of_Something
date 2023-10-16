#region Using

using System.Numerics;
using System.Runtime.InteropServices.JavaScript;
using Emotion.Common;
using Emotion.Game.World;
using Emotion.Game.World2D;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.Game.World3D;
using Emotion.Game.World3D.Objects;
using Emotion.Game.World3D.SceneControl;
using Emotion.Graphics;
using Emotion.Graphics.Camera;
using Emotion.Graphics.ThreeDee;
using Emotion.Platform.Input;
using Emotion.Primitives;
using Emotion.Testing;
using Emotion.UI;
using Take_1;

#endregion
namespace Emotion.ExecTest;

public class Program
{
    private static void Main(string[] args)
    {
        var config = new Configurator
        {
            DebugMode = true,
        };

        Engine.Setup(config);

        Engine.SceneManager.SetScene(new TakeOneGame());

        Engine.Run();
    }
}


public class TakeOneGame : World3DBaseScene<Take1Map>
{
    public UIController UI;

    public Type ObjectTypeToPlace;

    public BaseTakeOneObject GhostObject;

    public Type QuadObjectsForType;
    public List<Quad3D> QuadObjects;

    public override async Task LoadAsync()
    {
        var cam3D = new Camera3D(new Vector3(100));
        cam3D.LookAtPoint(Vector3.Zero);
        Engine.Renderer.Camera = cam3D;

        await ChangeMapAsync(new Take1Map(new Vector2(10, 10)));

        UI = new UIController();
        AddPlaceableObjectMenu(UI);

        Engine.Host.OnKey.AddListener(KeyHandler, KeyListenerType.Game);

        QuadObjects = new List<Quad3D>();
    }

    public override void Draw(RenderComposer composer)
    {
        composer.SetUseViewMatrix(false);
        composer.RenderSprite(Vector3.Zero, composer.CurrentTarget.Size, Color.CornflowerBlue);
        composer.ClearDepth();
        composer.SetUseViewMatrix(true);

        base.Draw(composer);

        composer.SetUseViewMatrix(false);
        UI.Render(composer);
    }

    private bool KeyHandler(Key key, KeyStatus status)
    {
        if (key == Key.MouseKeyLeft && ObjectTypeToPlace != null)
        {
            if (status == KeyStatus.Down)
            {
                var thisTreeObject = (GameObject3D)Activator.CreateInstance(ObjectTypeToPlace);
                thisTreeObject.Position = GhostObject.Position;
                CurrentMap.AddObject(thisTreeObject);

                return false;
            }
        }
        return true;
    }

    private void UpdateGhost(GameObject3D ghost)
    {
        if (CurrentMap.EditorMode) return;

        //currentObject.Position = Engine.Renderer.Camera.Position;  ------>>>>> moves with the camera
        Ray3D ray3D = (Engine.Renderer.Camera as Camera3D).GetCameraMouseRay();
        Vector3 position = new Vector3(0, 0, 0);

        var enumerator = CurrentMap.GetObjectsByType<GroundTile>();
        while (enumerator.MoveNext())
        {
            var obj = enumerator.Current;
            if (ray3D.IntersectWithObject(obj, out Mesh _, out position, out Vector3 _, out int _))
            {
                ghost.Position = CurrentMap.SnapToGrid(position);
                if (CurrentMap.IsValidPosition(ghost))
                {
                    // change color
                    ghost.Tint = Color.White.SetAlpha(150);
                }
                else
                {
                    ghost.Tint = Color.Red.SetAlpha(150);
                }
            }

        }
        UpdatePlanesPosition(ghost);
    }

    private void UpdatePlanesPosition(GameObject3D gameObject)
    {
        Rectangle rectangle = CurrentMap.CalcBoundingRectangle(gameObject);

        var firstGrid = CurrentMap.WorldToGrid(new Vector3(rectangle.Position.X, rectangle.Position.Y, 5));
        var lastGrid = CurrentMap.WorldToGrid(new Vector3(rectangle.Size.X + rectangle.Position.X, rectangle.Size.Y + rectangle.Position.Y, 5));

        int i = 0;
        for (int x = (int)firstGrid.X; x < (int)lastGrid.X; x++)
        {
            for (int y = (int)firstGrid.Y; y < (int)lastGrid.Y; y++)
            {
                if (QuadObjects.Count > 0)
                {
                    QuadObjects.ElementAt(i).Position = CurrentMap.GridToWorld(new Vector2(x, y));
                    QuadObjects.ElementAt(i).Z = 5.1f;

                    if (CurrentMap.IsValidPosition(QuadObjects.ElementAt(i)))
                    {
                        QuadObjects.ElementAt(i).Tint = Color.White.SetAlpha(80);
                    }
                    else
                    {
                        QuadObjects.ElementAt(i).Tint = Color.Red.SetAlpha(255);
                    }

                    i++;
                }
            }
        }
    }
    
    // check if the ghostObject is loded and it has bounds
    public void UpdatePlanesForGhost()
    {
        var ghostObjectType = GhostObject?.GetType();
        if (ghostObjectType != QuadObjectsForType && GhostObject.ObjectState == ObjectState.Alive)
        {
            QuadObjects?.Clear();
            AddPlanesToList(GhostObject);
            foreach (var item in QuadObjects)
            {
                CurrentMap.AddObject(item);
            }
            QuadObjectsForType = ghostObjectType;
        }
    }

    public void AddPlanesToList(GameObject3D gameObject)
    {
        Rectangle rectangle = CurrentMap.CalcBoundingRectangle(gameObject);

        var firstGrid = CurrentMap.WorldToGrid(new Vector3(rectangle.Position.X, rectangle.Position.Y, 5));
        var lastGrid = CurrentMap.WorldToGrid(new Vector3(rectangle.Size.X + rectangle.Position.X, rectangle.Size.Y + rectangle.Position.Y, 5));

        for (int x = (int)firstGrid.X; x < (int)lastGrid.X; x++)
        {
            for (int y = (int)firstGrid.Y; y < (int)lastGrid.Y; y++)
            {
                Quad3D quadPiece = new Quad3D();
                quadPiece.Width = 25;
                quadPiece.Depth = 25;
                quadPiece.Position = CurrentMap.GridToWorld(new Vector2(x, y));
                quadPiece.Z = 5.1f;

                QuadObjects.Add(quadPiece);
            }
        }
    }


    public override void Update()
    {
        base.Update();
        UI.Update();
        if (ObjectTypeToPlace != null) UpdateGhost(GhostObject);
        UpdatePlanesForGhost();
    }

    protected void AddPlaceableObjectMenu(UIController ui)
    {
        UISolidColor barContainer = new UISolidColor();
        barContainer.StretchX = true;
        barContainer.StretchY = true;
        barContainer.WindowColor = Color.Black * 0.75f;
        barContainer.Paddings = new Rectangle(2, 2, 2, 2);
        barContainer.Anchor = UIAnchor.BottomCenter;
        barContainer.ParentAnchor = UIAnchor.BottomCenter;
        barContainer.Margins = new Rectangle(0, 0, 0, 5);

        var bar = new UICallbackListNavigator();
        bar.StretchX = true;
        bar.StretchY = true;
        bar.IgnoreParentColor = true;
        bar.LayoutMode = LayoutMode.HorizontalList;
        barContainer.AddChild(bar);

        var types = EditorUtility.GetTypesWhichInherit<GameObject3D>();
        for (int i = 0; i < types.Count; i++)
        {
            if (types[i].GetType() == typeof(BaseTakeOneObject)) continue;
            var type = types[i];
            if (type.Assembly == typeof(Take1Map).Assembly)
            {
                UIObject3DWindow thisObjectWin = new UIObject3DWindow(type);
                thisObjectWin.MaxSize = new Vector2(32);
                thisObjectWin.MinSize = new Vector2(32);
                
                bar.AddChild(thisObjectWin);
            }
        }
        bar.SetupMouseSelection();

        UIObject3DWindow? oldSel = null;
        bar.OnChoiceConfirmed = (nu, _) =>
        {
            if (oldSel != null) oldSel.Selected = false;

            var nuAs = nu as UIObject3DWindow;
            Assert.NotNull(nuAs);

            nuAs.Selected = true;
            ObjectTypeToPlace = nuAs.Type;
            oldSel = nuAs;

            if (GhostObject != null)
            {
                CurrentMap.RemoveObject(GhostObject);
                foreach(var item in QuadObjects)
                {
                    CurrentMap.RemoveObject(item);
                }
                QuadObjects.Clear();
            }
            
            GhostObject = (BaseTakeOneObject)Activator.CreateInstance(ObjectTypeToPlace);
            GhostObject.ShouldApplyToGrid = false;
            GhostObject.Tint = GhostObject.Tint.SetAlpha(150);
            GhostObject.ObjectFlags = ObjectFlags.Map3DDontThrowShadow;
            CurrentMap.AddObject(GhostObject);
        };

        ui.AddChild(barContainer);
    }
}