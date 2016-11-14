﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SM64_Diagnostic.Structs;
using System.Windows.Forms;
using SM64_Diagnostic.Utilities;
using SM64_Diagnostic.Controls;

namespace SM64_Diagnostic.ManagerClasses
{
    public class CameraManager : DataManager
    {
        public CameraManager(ProcessStream stream, List<WatchVariable> cameraData, FlowLayoutPanel variableTable)
            : base(stream, cameraData, variableTable)
        {
        }
    }
}
