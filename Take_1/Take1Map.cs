
﻿using Emotion.Game.World3D;
using Emotion.Graphics;
using Emotion.Primitives;
using Emotion.Utility;
﻿using Emotion.Common;
using Emotion.Graphics.Camera;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Emotion.Game.World3D.Objects;
using Emotion.Game.World2D;
using Emotion.ExecTest;
using Emotion.Common.Serialization;
using Emotion.IO;

namespace Take_1
{
    public class Take1Map : Map3D
    {
        public Vector2 TileGrid = new Vector2(25, 25);

        public struct TileData
        {
            public bool IsOccupied;
        }

        [DontSerialize]
        public TileData[][] gridData;

        private InfiniteGrid _gridObject;
        private Vector2 _gridOffset;

        public Take1Map(Vector2 mapSize)
        {
            MapSize = mapSize;
        }

        protected Take1Map()
        {

        }

        public Rectangle CalcBoundingRectangle(GameObject3D gameObject)
        {
            Cube cube = gameObject.Bounds3D;
            Rectangle rectangle = new Rectangle();
            rectangle.Width = 2 * cube.HalfExtents.X;
            rectangle.Height = 2 * cube.HalfExtents.Y;
            rectangle.X = cube.Origin.X - cube.HalfExtents.X;
            rectangle.Y = cube.Origin.Y - cube.HalfExtents.Y;

            rectangle.SnapToGrid(TileGrid);

            Vector3 first = new Vector3(rectangle.Position.X, rectangle.Position.Y, 5);
            Vector3 second = new Vector3(rectangle.Size.X + rectangle.Position.X, rectangle.Size.Y + rectangle.Position.Y, 5);

            return new Rectangle
            {
                Position = new Vector2(first.X, first.Y),
                Size = new Vector2(second.X - first.X, second.Y - first.Y)
            };
        }

        public void ProcessGridTiles(GameObject3D gameObject, Action<int, int> action, bool clamped = true)
        {
            Rectangle rectangle = CalcBoundingRectangle(gameObject);

            var firstGrid = WorldToGrid(new Vector3(rectangle.Position.X, rectangle.Position.Y, 5));
            var lastGrid = WorldToGrid(new Vector3(rectangle.Size.X + rectangle.Position.X, rectangle.Size.Y + rectangle.Position.Y, 5));

            for (int x = (int)firstGrid.X; x < (int)lastGrid.X; x++)
            {
                for (int y = (int)firstGrid.Y; y < (int)lastGrid.Y; y++)
                {
                    if (clamped)
                    {
                        if (x < 0 || x > gridData.Length - 1) continue;
                        if (y < 0 || y > gridData[x].Length - 1) continue;
                    }

                    action(x, y);
                }
            }
        }

        // after placing them (before addtomap), where it's initializing the object, it sets the tiles under it as occupied
        public void ApplyObjectToGrid(GameObject3D gameObject3D)
        {
            ProcessGridTiles(gameObject3D, (x, y) =>
            {
                gridData[x][y].IsOccupied = true;
            });
            visualized = false;
        }

        public bool IsValidPosition(GameObject3D gameObject3D)
        {
            //get the position of the object and see if every tile underneath it is free
            bool isValid = true;

            ProcessGridTiles(gameObject3D, (x, y) =>
            {
                if (gridData[x][y].IsOccupied) isValid = false;
            });

            return isValid;
        }

        protected override Task InitAsyncInternal()
        {
            RenderShadowMap = true;

            // do not delete for now
            _gridObject = new InfiniteGrid();
            _gridObject.Z = 6;
            _gridObject.TileSize = TileGrid.X;
            _gridObject.Offset = TileGrid / 2f;
            //AddObject(_gridObject);
            
            Vector2 groundTileBounds = new Vector2(100, 100);

            for (int y = 0; y < MapSize.Y; y++)
            {
                for (int x = 0; x < MapSize.X; x++)
                {
                    var thisTile = new GroundTile();
                    thisTile.Position = (groundTileBounds * new Vector2(x, y)).ToVec3(5);
                    thisTile.ObjectName = $"Tile ({x}x{y})";

                    AddObject(thisTile);
                }
            }

            Vector2 gridRatio = groundTileBounds / TileGrid;
            int gridWidth = (int) (MapSize.X * gridRatio.X);
            var gridHeight = (int)(MapSize.Y * gridRatio.Y);

            gridData = new TileData[gridWidth][];
            for (int x = 0; x < gridWidth; x++)
            {
                gridData[x] = new TileData[gridHeight];
                for (int y = 0; y < gridHeight; y++)
                {
                    TileData tile = new TileData();
                    tile.IsOccupied = false;

                    // x -> index of the column
                    gridData[x][y] = tile;
                }
            }

            var patchesToPlace = 0;
            var mapBounds = new Rectangle(
                -groundTileBounds.X / 2f,
                -groundTileBounds.Y / 2f,
                MapSize.X * groundTileBounds.X + groundTileBounds.X / 2f,
                MapSize.Y * groundTileBounds.Y + groundTileBounds.Y / 2f
               );
            _gridOffset = mapBounds.Position;

            /*
            OnObjectAdded += (obj) =>
            {
                ApplyObjectToGrid(obj as GameObject3D);
            };
            */

            // Trash
            List<Rectangle> patchesPlacedAt = new List<Rectangle>();
            var loopCounter = 0;
            while(patchesPlacedAt.Count < patchesToPlace)
            {
                Vector2 snowPatchSize = new Vector2(100, 100); // todo: entity bounds

                float x = Helpers.GenerateRandomNumber((int) (mapBounds.X + snowPatchSize.X / 2f), (int) (mapBounds.Right - snowPatchSize.X / 2f));
                float y = Helpers.GenerateRandomNumber((int) (mapBounds.Y + snowPatchSize.Y / 2f), (int) (mapBounds.Bottom - snowPatchSize.Y / 2f));
               
                Rectangle newPatchBound = new Rectangle(0, 0, snowPatchSize)
                {
                    Center = new Vector2(x, y)
                };

                bool collidesWithAnother = false;
                //for (int i = 0; i < patchesPlacedAt.Count; i++)
                //{
                //    var alreadyPlacedBound = patchesPlacedAt[i];
                //    if (alreadyPlacedBound.Intersects(newPatchBound))
                //    {
                //        collidesWithAnother = true;
                //        break;
                //    }
                //}

                if (!collidesWithAnother)
                {
                    var snowPatch = new SnowPatch();
                    snowPatch.ObjectName = "Snow";
                    snowPatch.Position = newPatchBound.Center.ToVec3(6);
                    snowPatch.Size3D *= Helpers.GenerateRandomNumber(1, 25) / 10f;
                    var rot = Helpers.GenerateRandomNumber(1, 360);
                    snowPatch.RotationDeg = new Vector3(0, 0, rot);
                    AddObject(snowPatch);
                    patchesPlacedAt.Add(newPatchBound);
                }

                // Infinite loop prevention
                loopCounter++;
                if (loopCounter > patchesToPlace + 100) break;
            }

            return base.InitAsyncInternal();
        }

        // Transform one coordinate system to the other:
        // Position of an object
        public Vector2 WorldToGrid(Vector3 position)
        {
            position -= _gridOffset.ToVec3();
            return new Vector2(position.X / TileGrid.X, position.Y / TileGrid.Y).Floor();
        }

        public Vector3 GridToWorld(Vector2 position)
        {
            return new Vector3(position.X * TileGrid.X + TileGrid.X / 2f, position.Y * TileGrid.Y + TileGrid.Y / 2f, 5) + _gridOffset.ToVec3();
        }

        // gets the position of the object
        public Vector3 SnapToGrid(Vector3 position)
        {
            return GridToWorld(WorldToGrid(position));
        }

        bool visualized = false;

        public override void Render(RenderComposer c)
        {
            base.Render(c);

            //c.SetDepthTest(false);
            var sofiaGuide = Engine.AssetLoader.Get<TextureAsset>("Maps/sofia_guide.png");
            //c.RenderSprite(Vector3.Zero + _gridOffset.ToVec3(5.5f), sofiaGuide.Texture.Size, Color.White, sofiaGuide.Texture) ;
            //c.SetDepthTest(true);
            
            if(!visualized)
            {
                //c.DbgClear();
                //for (int x = 0; x < gridData.Length; x++)
                //{
                //    var column = gridData[x];
                //    for (int y = 0; y < column.Length; y++)
                //    {
                //        Vector3 worldPos = GridToWorld(new Vector2(x, y));
                //        var data = gridData[x][y];
                //        c.DbgAddPoint(worldPos, 3f, data.IsOccupied ? Color.PrettyRed : Color.PrettyGreen);
                //    }
                //}
                visualized = true;
            }
           

            //c.PushModelMatrix(Matrix4x4.CreateTranslation(0, 0, 10));
            //Vector2 groundTileBounds = new Vector2(100, 100);
            //var mapBounds = new Rectangle(
            //   -groundTileBounds.X / 2f,
            //   -groundTileBounds.Y / 2f,
            //   MapSize.X * groundTileBounds.X,
            //   MapSize.Y * groundTileBounds.Y
            //  );
            //c.RenderOutline(mapBounds, Color.Red);

            //Vector2 snowPatchSize = new Vector2(100, 100);
            //var snowBounds = new Rectangle(
            //   mapBounds.X + snowPatchSize.X / 2f,
            //   mapBounds.Y + snowPatchSize.Y / 2f,
            //   mapBounds.Right - snowPatchSize.X / 2f,
            //   mapBounds.Bottom - snowPatchSize.Y / 2f
            //  );
            //c.RenderOutline(snowBounds, Color.White);

            //c.PopModelMatrix();
        }
    }
}
