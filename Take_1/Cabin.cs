using Emotion.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Take_1
{
    public class Cabin : BaseTakeOneObject
    {
        public Cabin()
        {
            this.ShouldApplyToGrid = true;
            EntityPath = "Custom/cabin.fbx";
            Size3D = new System.Numerics.Vector3(50, 50, 50);
        }

        public override void Init()
        {
            base.Init();
            Entity.LocalTransform = System.Numerics.Matrix4x4.Identity;
            Entity.BackFaceCulling = false;
        }
    }
}
