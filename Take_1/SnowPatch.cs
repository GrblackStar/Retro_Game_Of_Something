using Emotion.Game.World3D.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Take_1
{
    public class SnowPatch : GenericObject3D
    {
        public SnowPatch()
        {
            EntityPath = "HolidayKit/snowPatch.fbx";
            Size3D = new System.Numerics.Vector3(1, 1, 1);
            RotationDeg = new System.Numerics.Vector3(90, 0, 0);
        }
    }
}
