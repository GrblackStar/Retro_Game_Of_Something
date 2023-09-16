using Emotion.Game.World3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

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
            for (int y = 0; y < MapSize.Y; y++)
            {
                for (int x = 0; x < MapSize.X; x++)
                {
                    var thisTile = new GroundTile();
                    thisTile.Position = new Vector3(100 * x, 100 * y, 5);

                    AddObject(thisTile);
                }
            }

            return base.InitAsyncInternal();
        }

    }
}
