﻿using STROOP.Managers;
using STROOP.Map3;
using STROOP.Map3.Map;
using STROOP.Map3.Map.Graphics;
using STROOP.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace STROOP.Structs.Configurations
{
    public static class Config
    {
        public static uint RamSize;

        public static List<Emulator> Emulators = new List<Emulator>();
        public static ProcessStream Stream;
        public static ObjectAssociations ObjectAssociations;
        public static MapAssociations MapAssociations;
        public static StroopMainForm StroopMainForm;
        public static TabControlEx TabControlMain;
        public static Label DebugText;
        public static MapGraphics Map3Graphics;
        public static MapGui Map3Gui;
        public static Map4Graphics Map4Graphics;
        public static Map4Camera Map4Camera;

        public static CameraManager CameraManager;
        public static DebugManager DebugManager;
        public static DisassemblyManager DisassemblyManager;
        public static HackManager HackManager;
        public static HudManager HudManager;
        public static MapManager Map3Manager;
        public static ModelManager ModelManager;
        public static MarioManager MarioManager;
        public static MiscManager MiscManager;
        public static ObjectManager ObjectManager;
        public static ObjectSlotsManager ObjectSlotsManager;
        public static OptionsManager OptionsManager;
        public static TestingManager TestingManager;
        public static InjectionManager InjectionManager;
        public static TriangleManager TriangleManager;
        public static WaterManager WaterManager;
        public static SnowManager SnowManager;
        public static InputManager InputManager;
        public static ActionsManager ActionsManager;
        public static PuManager PuManager;
        public static TasManager TasManager;
        public static FileManager FileManager;
        public static MainSaveManager MainSaveManager;
        public static AreaManager AreaManager;
        public static DataManager QuarterFrameManager;
        public static DataManager CustomManager;
        public static VarHackManager VarHackManager;
        public static CamHackManager CamHackManager;
        public static MemoryManager MemoryManager;
        public static SearchManager SearchManager;
        public static CellsManager CellsManager;
        public static CoinManager CoinManager;
        public static GfxManager GfxManager;
        public static PaintingManager PaintingManager;
        public static MusicManager MusicManager;
        public static SoundManager SoundManager;
        public static M64Manager M64Manager;

        public static List<VariableAdder> GetVariableAdders()
        {
            List<VariableAdder> variableAdderList =
                ControlUtilities.GetFieldsOfType<VariableAdder>(typeof(Config), null);
            variableAdderList.Sort((d1, d2) => d1.TabIndex - d2.TabIndex);
            return variableAdderList;
        }

        public static void Print(object formatNullable = null, params object[] args)
        {
            object format = formatNullable ?? "";
            string formatted = String.Format(format.ToString(), args);
            System.Diagnostics.Trace.WriteLine(formatted);
        }

        public static void SetDebugText(object obj)
        {
            DebugText.Visible = true;
            DebugText.Text = obj.ToString();
        }
    }
}
