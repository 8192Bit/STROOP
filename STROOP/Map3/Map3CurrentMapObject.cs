﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using OpenTK.Graphics.OpenGL;
using STROOP.Utilities;
using STROOP.Structs.Configurations;
using STROOP.Structs;
using OpenTK;

namespace STROOP.Map3
{
    public class Map3CurrentMapObject : Map3MapObject
    {
        public Map3CurrentMapObject()
            : base()
        {
        }

        public override MapLayout GetMapLayout()
        {
            return Map3Utilities.GetMapLayout();
        }

        public override string GetName()
        {
            return "Current Map";
        }
    }
}
