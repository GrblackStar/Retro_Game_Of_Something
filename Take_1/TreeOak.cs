using Emotion.Game.World3D.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Take_1
{
    public class TreeOak : GenericObject3D
    {
        public TreeOak()
        {
            EntityPath = "NatureKit/tree_oak.fbx";
            Size3D = new System.Numerics.Vector3(10, 10, 10);
            RotationDeg = new System.Numerics.Vector3(90, 0, 0);
        }
    }
}
