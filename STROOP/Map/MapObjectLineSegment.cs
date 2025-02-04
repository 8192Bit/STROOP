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
using System.Windows.Forms;
using System.Xml.Linq;
using STROOP.Map.Map3D;

namespace STROOP.Map
{
    public class MapObjectLineSegment : MapObjectLine
    {
        private PositionAngle _posAngle1;
        private PositionAngle _posAngle2;
        private bool _useFixedSize;
        private float _backwardsSize;
        private float _iconSize;

        private ToolStripMenuItem _itemUseFixedSize;
        private ToolStripMenuItem _itemSetBackwardsSize;
        private ToolStripMenuItem _itemSetIconSize;

        private static readonly string SET_BACKWARDS_SIZE_TEXT = "Set Backwards Size";
        private static readonly string SET_ICON_SIZE_TEXT = "Set Icon Size";

        public MapObjectLineSegment(PositionAngle posAngle1, PositionAngle posAngle2)
            : base()
        {
            _posAngle1 = posAngle1;
            _posAngle2 = posAngle2;
            _useFixedSize = false;
            _backwardsSize = 0;
            _iconSize = 10;

            Size = 0;
            LineWidth = 3;
            LineColor = Color.Red;
        }

        public static MapObject Create(string text1, string text2)
        {
            PositionAngle posAngle1 = PositionAngle.FromString(text1);
            PositionAngle posAngle2 = PositionAngle.FromString(text2);
            if (posAngle1 == null || posAngle2 == null) return null;
            return new MapObjectLineSegment(posAngle1, posAngle2);
        }

        protected override List<(float x, float y, float z)> GetVerticesTopDownView()
        {
            (double x1, double y1, double z1, double angle1) = _posAngle1.GetValues();
            (double x2, double y2, double z2, double angle2) = _posAngle2.GetValues();
            double dist = PositionAngle.GetHDistance(_posAngle1, _posAngle2);
            (double startX, double startZ) = MoreMath.ExtrapolateLine2D(x2, z2, x1, z1, dist + _backwardsSize);
            (double endX, double endZ) = MoreMath.ExtrapolateLine2D(x1, z1, x2, z2, (_useFixedSize ? 0 : dist) + Size);

            List<(float x, float y, float z)> vertices = new List<(float x, float y, float z)>();
            vertices.Add(((float)startX, 0, (float)startZ));
            vertices.Add(((float)endX, 0, (float)endZ));
            return vertices;
        }

        protected override List<(float x, float y, float z)> GetVerticesOrthographicView()
        {
            (double x1, double y1, double z1, double angle1) = _posAngle1.GetValues();
            (double x2, double y2, double z2, double angle2) = _posAngle2.GetValues();
            double dist = PositionAngle.GetDistance(_posAngle1, _posAngle2);
            (double startX, double startY, double startZ) = MoreMath.ExtrapolateLine3D(x2, y2, z2, x1, y1, z1, dist + _backwardsSize);
            (double endX, double endY, double endZ) = MoreMath.ExtrapolateLine3D(x1, y1, z1, x2, y2, z2, (_useFixedSize ? 0 : dist) + Size);

            List<(float x, float y, float z)> vertices = new List<(float x, float y, float z)>();
            vertices.Add(((float)startX, (float)startY, (float)startZ));
            vertices.Add(((float)endX, (float)endY, (float)endZ));
            return vertices;
        }

        protected override List<(float x, float y, float z)> GetVertices3D()
        {
            return GetVerticesOrthographicView();
        }

        public override void DrawOn2DControlTopDownView()
        {
            base.DrawOn2DControlTopDownView();

            if (_customImage != null)
            {
                (float x, float y, float z) = ((float, float, float))PositionAngle.GetMidPoint(_posAngle1, _posAngle2);
                (float controlX, float controlZ) = MapUtilities.ConvertCoordsForControlTopDownView(x, z);
                PointF point = new PointF(controlX, controlZ);
                SizeF size = MapUtilities.ScaleImageSizeForControl(_customImage.Size, _iconSize, Scales);   
                MapUtilities.DrawTexture(_customImageTex.Value, point, size, 0, 1);
            }
        }

        public override void DrawOn2DControlOrthographicView()
        {
            base.DrawOn2DControlOrthographicView();

            if (_customImage != null)
            {
                (float x, float y, float z) = ((float, float, float))PositionAngle.GetMidPoint(_posAngle1, _posAngle2);
                (float controlX, float controlZ) = MapUtilities.ConvertCoordsForControlOrthographicView(x, y, z);
                PointF point = new PointF(controlX, controlZ);
                SizeF size = MapUtilities.ScaleImageSizeForControl(_customImage.Size, _iconSize, Scales);
                MapUtilities.DrawTexture(_customImageTex.Value, point, size, 0, 1);
            }
        }

        public override void DrawOn3DControl()
        {
            base.DrawOn3DControl();

            if (_customImage != null)
            {
                (float x, float y, float z) = ((float, float, float))PositionAngle.GetMidPoint(_posAngle1, _posAngle2);
                Matrix4 viewMatrix = GetModelMatrix(x, y, z, 0);
                GL.UniformMatrix4(Config.Map3DGraphics.GLUniformView, false, ref viewMatrix);

                Map3DVertex[] vertices2 = GetVertices();
                int vertexBuffer = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices2.Length * Map3DVertex.Size),
                    vertices2, BufferUsageHint.StaticDraw);
                GL.BindTexture(TextureTarget.Texture2D, _customImageTex.Value);
                GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
                Config.Map3DGraphics.BindVertices();
                GL.DrawArrays(PrimitiveType.Triangles, 0, vertices2.Length);
                GL.DeleteBuffer(vertexBuffer);
            }
        }

        public Matrix4 GetModelMatrix(float x, float y, float z, float ang)
        {
            SizeF _imageNormalizedSize = new SizeF(
                _customImage.Width >= _customImage.Height ? 1.0f : (float)_customImage.Width / _customImage.Height,
                _customImage.Width <= _customImage.Height ? 1.0f : (float)_customImage.Height / _customImage.Width);

            Vector3 pos = new Vector3(x, y, z);

            float size = _iconSize / 200;
            return Matrix4.CreateScale(size * _imageNormalizedSize.Width, size * _imageNormalizedSize.Height, 1)
                * Matrix4.CreateRotationZ(0)
                * Matrix4.CreateScale(1.0f / Config.Map3DGraphics.NormalizedWidth, 1.0f / Config.Map3DGraphics.NormalizedHeight, 1)
                * Matrix4.CreateTranslation(MapUtilities.GetPositionOnViewFromCoordinate(pos));
        }

        private Map3DVertex[] GetVertices()
        {
            return new Map3DVertex[]
            {
                new Map3DVertex(new Vector3(-1, -1, 0), Color.White, new Vector2(0, 1)),
                new Map3DVertex(new Vector3(1, -1, 0), Color.White, new Vector2(1, 1)),
                new Map3DVertex(new Vector3(-1, 1, 0), Color.White, new Vector2(0, 0)),
                new Map3DVertex(new Vector3(1, 1, 0), Color.White, new Vector2(1, 0)),
                new Map3DVertex(new Vector3(-1, 1, 0), Color.White,  new Vector2(0, 0)),
                new Map3DVertex(new Vector3(1, -1, 0), Color.White, new Vector2(1, 1)),
            };
        }

        public override ContextMenuStrip GetContextMenuStrip()
        {
            if (_contextMenuStrip == null)
            {
                _itemUseFixedSize = new ToolStripMenuItem("Use Fixed Size");
                _itemUseFixedSize.Click += (sender, e) =>
                {
                    MapObjectSettings settings = new MapObjectSettings(
                        changeLineSegmentUseFixedSize: true, newLineSegmentUseFixedSize: !_useFixedSize);
                    GetParentMapTracker().ApplySettings(settings);
                };

                string suffix1 = string.Format(" ({0})", _backwardsSize);
                _itemSetBackwardsSize = new ToolStripMenuItem(SET_BACKWARDS_SIZE_TEXT + suffix1);
                _itemSetBackwardsSize.Click += (sender, e) =>
                {
                    string text = DialogUtilities.GetStringFromDialog(labelText: "Enter backwards size.");
                    float? backwardsSizeNullable = ParsingUtilities.ParseFloatNullable(text);
                    if (!backwardsSizeNullable.HasValue) return;
                    float backwardsSize = backwardsSizeNullable.Value;
                    MapObjectSettings settings = new MapObjectSettings(
                        changeLineSegmentBackwardsSize: true, newLineSegmentBackwardsSize: backwardsSize);
                    GetParentMapTracker().ApplySettings(settings);
                };

                string suffix2 = string.Format(" ({0})", _iconSize);
                _itemSetIconSize = new ToolStripMenuItem(SET_ICON_SIZE_TEXT + suffix2);
                _itemSetIconSize.Click += (sender, e) =>
                {
                    string text = DialogUtilities.GetStringFromDialog(labelText: "Enter icon size.");
                    float? iconSizeNullable = ParsingUtilities.ParseFloatNullable(text);
                    if (!iconSizeNullable.HasValue) return;
                    float iconSize = iconSizeNullable.Value;
                    MapObjectSettings settings = new MapObjectSettings(
                        changeIconSize: true, newIconSize: iconSize);
                    GetParentMapTracker().ApplySettings(settings);
                };

                _contextMenuStrip = new ContextMenuStrip();
                _contextMenuStrip.Items.Add(_itemUseFixedSize);
                _contextMenuStrip.Items.Add(_itemSetBackwardsSize);
                _contextMenuStrip.Items.Add(_itemSetIconSize);
            }

            return _contextMenuStrip;
        }

        public override void ApplySettings(MapObjectSettings settings)
        {
            base.ApplySettings(settings);

            if (settings.ChangeLineSegmentUseFixedSize)
            {
                _useFixedSize = settings.NewLineSegmentUseFixedSize;
                _itemUseFixedSize.Checked = settings.NewLineSegmentUseFixedSize;
            }

            if (settings.ChangeLineSegmentBackwardsSize)
            {
                _backwardsSize = settings.NewLineSegmentBackwardsSize;
                string suffix = string.Format(" ({0})", settings.NewLineSegmentBackwardsSize);
                _itemSetBackwardsSize.Text = SET_BACKWARDS_SIZE_TEXT + suffix;
            }

            if (settings.ChangeIconSize)
            {
                _iconSize = settings.NewIconSize;
                string suffix = string.Format(" ({0})", settings.NewIconSize);
                _itemSetIconSize.Text = SET_ICON_SIZE_TEXT + suffix;
            }
        }

        public override string GetName()
        {
            return "Line Segment";
        }

        public override Image GetInternalImage()
        {
            return Config.ObjectAssociations.LineSegmentImage;
        }

        public override List<XAttribute> GetXAttributes()
        {
            return new List<XAttribute>()
            {
                new XAttribute("positionAngle1", _posAngle1),
                new XAttribute("positionAngle2", _posAngle2),
            };
        }
    }
}
