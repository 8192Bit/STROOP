﻿using SM64_Diagnostic.Structs.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SM64_Diagnostic.Structs
{
    public static class OptionsConfig
    {
        public static bool SlotIndexsFromOne = true;
        public static bool MoveCameraWithPu = true;
        public static bool ScaleDiagonalPositionControllerButtons = false;
        public static bool ExcludeDustForClosestObject = true;
        public static bool NeutralizeTrianglesWith21 = true;
        public static short NeutralizeTriangleValue(bool? use21Nullable = null)
        {
            bool use21 = use21Nullable ?? NeutralizeTrianglesWith21;
            return (short)(use21 ? 21 : 0);
        }
        public static bool UseMisalignmentOffsetForDistanceToLine = true;

        public static bool ShowOverlayHeldObject = true;
        public static bool ShowOverlayStoodOnObject = true;
        public static bool ShowOverlayInteractionObject = true;
        public static bool ShowOverlayUsedObject = true;
        public static bool ShowOverlayCameraObject = true;
        public static bool ShowOverlayCameraHackObject = true;
        public static bool ShowOverlayClosestObject = true;
        public static bool ShowOverlayFloorObject = true;
        public static bool ShowOverlayWallObject = true;
        public static bool ShowOverlayCeilingObject = true;
        public static bool ShowOverlayParentObject = false;
    }
}
