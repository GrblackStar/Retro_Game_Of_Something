
﻿using Emotion.Game.World3D;
using Emotion.Graphics;
using Emotion.Primitives;
using Emotion.Utility;
﻿using Emotion.Common;
using Emotion.Game.World3D;
using Emotion.Graphics.Camera;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Take_1
{
    public class Take1Map : Map3D
    {
        public Take1Map(Vector2 mapSize)
        {
            MapSize = mapSize;
        }

        protected override Task InitAsyncInternal()
        {
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

            var patchesToPlace = 60;
            var mapBounds = new Rectangle(
                -groundTileBounds.X / 2f,
                -groundTileBounds.Y / 2f,
                MapSize.X * groundTileBounds.X + groundTileBounds.X / 2f,
                MapSize.Y * groundTileBounds.Y + groundTileBounds.Y / 2f
               );

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
                    snowPatch.RotationDeg = new Vector3(rot, 90, 90); // due to gimbal lock, correct would be Vector3(90, 0, rot)
                    AddObject(snowPatch);
                    patchesPlacedAt.Add(newPatchBound);
                }

                // Infinite loop prevention
                loopCounter++;
                if (loopCounter > patchesToPlace + 100) break;
            }

            return base.InitAsyncInternal();
        }

        public override void Render(RenderComposer c)
        {
            base.Render(c);

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
