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
using System.Drawing.Imaging;

namespace STROOP.Map
{
    public class MapObjectPunchDetector : MapObjectCylinder
    {
        public MapObjectPunchDetector()
            : base()
        {
        }

        protected override List<(float centerX, float centerZ, float radius, float minY, float maxY)> Get3DDimensions()
        {
            PositionAngle mario = PositionAngle.Mario;
            (double centerX, double centerZ) = MoreMath.AddVectorToPoint(50, mario.Angle, mario.X, mario.Z);
            double minY = mario.Y;
            double maxY = minY + 80;
            float radius = 5;
            return new List<(float centerX, float centerZ, float radius, float minY, float maxY)>()
            {
                ((float)centerX, (float)centerZ, radius, (float)minY, (float)maxY)
            };
        }

        public override Image GetInternalImage()
        {
            return Config.ObjectAssociations.CylinderImage;
        }

        public override string GetName()
        {
            return "Punch Detector";
        }

        public override float GetY()
        {
            return (float)PositionAngle.Mario.Y;
        }
    }
}
