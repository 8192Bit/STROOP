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
using OpenTK.Graphics;
using STROOP.Models;
using System.Xml.Linq;

namespace STROOP.Map
{
    public abstract class MapObject
    {
        public float Size = 25;
        public double Opacity = 1;
        public byte OpacityByte
        {
            get => (byte)(Opacity * 255);
            set => Opacity = value / 255f;
        }
        public int OpacityPercent
        {
            get => (int)(Opacity * 100);
            set => Opacity = value / 100.0;
        }
        public float OutlineWidth = 1;
        public Color Color = SystemColors.Control;
        public Color4 Color4 { get => new Color4(Color.R, Color.G, Color.B, OpacityByte); }
        public Color OutlineColor = Color.Black;

        public bool? CustomRotates = null;
        public bool InternalRotates = false;
        public bool Rotates
        {
            get => CustomRotates ?? InternalRotates;
        }

        private BehaviorCriteria? _behaviorCriteriaToDisplay = null;

        public bool ShowTriUnits = false;

        protected ContextMenuStrip _contextMenuStrip = null;

        private MapObjectSettingsAccumulator _accumulator = new MapObjectSettingsAccumulator();

        public MapObject()
        {
        }

        public void DrawOn2DControl()
        {
            if (Config.MapGui.checkBoxMapOptionsEnableOrthographicView.Checked)
            {
                DrawOn2DControlOrthographicView();
            }
            else
            {
                DrawOn2DControlTopDownView();
            }
        }

        public abstract void DrawOn2DControlTopDownView();

        public abstract void DrawOn2DControlOrthographicView();

        public abstract void DrawOn3DControl();

        public virtual Matrix4 GetModelMatrix()
        {
            return Matrix4.Identity;
        }

        public abstract string GetName();

        protected Image _customImage = null;
        public abstract Image GetInternalImage();
        public Image GetImage() { return _customImage ?? GetInternalImage(); }

        protected MapTrackerIconType _iconType = MapTrackerIconType.TopDownImage;
        public virtual void SetIconType(MapTrackerIconType iconType, Image image = null)
        {
            if ((iconType == MapTrackerIconType.CustomImage) != (image != null))
                throw new ArgumentOutOfRangeException();

            _iconType = iconType;
            _customImage = image;
        }

        public abstract MapDrawType GetDrawType();

        public virtual float GetY()
        {
            PositionAngle posAngle = GetPositionAngle();
            if (posAngle == null) return float.PositiveInfinity;
            return (float)posAngle.Y;
        }

        public void NotifyStoreBehaviorCritera()
        {
            ObjectDataModel obj = GetObject();
            if (obj == null) return;
            obj.Update();
            _behaviorCriteriaToDisplay = obj.BehaviorCriteria;
        }

        public bool ShouldDisplay(MapTrackerVisibilityType visiblityType)
        {
            ObjectDataModel obj = GetObject();
            if (obj == null) return true;
            obj.Update();
            switch (visiblityType)
            {
                case MapTrackerVisibilityType.VisibleAlways:
                    return true;
                case MapTrackerVisibilityType.VisibleWhenLoaded:
                    return obj.IsActive;
                case MapTrackerVisibilityType.VisibleWhenThisBhvrIsLoaded:
                    return obj.IsActive && BehaviorCriteria.HasSameAssociation(obj.BehaviorCriteria, _behaviorCriteriaToDisplay);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public virtual PositionAngle GetPositionAngle()
        {
            return null;
        }

        public virtual ObjectDataModel GetObject()
        {
            PositionAngle posAngle = GetPositionAngle();
            if (posAngle == null) return null;
            if (!posAngle.IsObjectDependent()) return null;
            uint objAddress = posAngle.GetObjectAddressIfObjectDependent().Value;
            return new ObjectDataModel(objAddress, true);
        }

        public override string ToString()
        {
            return GetName();
        }

        public virtual ContextMenuStrip GetContextMenuStrip()
        {
            if (_contextMenuStrip == null)
            {
                ToolStripMenuItem item = new ToolStripMenuItem("There are no additional options");
                item.Enabled = false;
                _contextMenuStrip = new ContextMenuStrip();
                _contextMenuStrip.Items.Add(item);
            }

            return _contextMenuStrip;
        }

        public virtual void Update()
        {
        }

        public virtual bool ParticipatesInGlobalIconSize()
        {
            return false;
        }

        public virtual void ApplySettings(MapObjectSettings settings)
        {
            GetContextMenuStrip();

            _accumulator.ApplySettings(settings);
        }

        protected MapTracker GetParentMapTracker()
        {
            foreach (MapTracker mapTracker in Config.MapGui.flowLayoutPanelMapTrackers.Controls)
            {
                if (mapTracker.ContainsMapObject(this)) return mapTracker;
            }
            return null;
        }

        public virtual void NotifyMouseEvent(MouseEvent mouseEvent, bool isLeftButton, int mouseX, int mouseY)
        {
        }

        public virtual void CleanUp()
        {
        }

        public virtual List<XAttribute> GetXAttributes()
        {
            return new List<XAttribute>();
        }

        public XElement ToXElement()
        {
            XElement xElement = new XElement("MapObject");
            xElement.Add(new XAttribute("type", GetType().Name));
            List<XAttribute> xAttributes = GetXAttributes();
            foreach (XAttribute xAttribute in xAttributes)
            {
                xElement.Add(xAttribute);
            }
            xElement.Add(_accumulator.ToXElement());
            return xElement;
        }

        public static MapObject FromXElement(XElement xElement)
        {
            string type = xElement.Attribute(XName.Get("type")).Value;
            MapObject mapObject;
            switch (type)
            {
                case "MapObjectAggregatedPath":
                    mapObject = new MapObjectAggregatedPath();
                    break;
                case "MapObjectAllObjectCeiling":
                    mapObject = new MapObjectAllObjectCeiling();
                    break;
                case "MapObjectAllObjectFloor":
                    mapObject = new MapObjectAllObjectFloor();
                    break;
                case "MapObjectAllObjectsWithName":
                    mapObject = MapObjectAllObjectsWithName.Create(xElement.Attribute(XName.Get("objectName")).Value);
                    break;
                case "MapObjectAllObjectWall":
                    mapObject = new MapObjectAllObjectWall();
                    break;
                case "MapObjectAngleRange":
                    mapObject = new MapObjectAngleRange(PositionAngle.FromString(xElement.Attribute(XName.Get("positionAngle")).Value));
                    break;
                case "MapObjectBranchPath":
                    mapObject = new MapObjectBranchPath(PositionAngle.FromString(xElement.Attribute(XName.Get("positionAngle")).Value));
                    break;
                case "MapObjectCamera":
                    mapObject = new MapObjectCamera();
                    break;
                case "MapObjectCellGridlines":
                    mapObject = new MapObjectCellGridlines();
                    break;
                case "MapObjectCoffinBox":
                    mapObject = new MapObjectCoffinBox(ParsingUtilities.ParseHex(xElement.Attribute(XName.Get("objectAddress")).Value));
                    break;
                //case "MapObjectCompass":
                //    mapObject = new MapObjectCompass();
                //    break;
                //case "MapObjectCoordinateLabels":
                //    mapObject = new MapObjectCoordinateLabels();
                //    break;
                //case "MapObjectCUpFloor":
                //    mapObject = new MapObjectCUpFloor();
                //    break;
                //case "MapObjectCurrentBackground":
                //    mapObject = new MapObjectCurrentBackground();
                //    break;
                //case "MapObjectCurrentCell":
                //    mapObject = new MapObjectCurrentCell();
                //    break;
                //case "MapObjectCurrentMap":
                //    mapObject = new MapObjectCurrentMap();
                //    break;
                //case "MapObjectCurrentUnit":
                //    mapObject = new MapObjectCurrentUnit();
                //    break;
                //case "MapObjectCustomBackground":
                //    mapObject = new MapObjectCustomBackground();
                //    break;
                //case "MapObjectCustomCeiling":
                //    mapObject = new MapObjectCustomCeiling();
                //    break;
                //case "MapObjectCustomCylinder":
                //    mapObject = new MapObjectCustomCylinder();
                //    break;
                //case "MapObjectCustomCylinderPoints":
                //    mapObject = new MapObjectCustomCylinderPoints();
                //    break;
                //case "MapObjectCustomFloor":
                //    mapObject = new MapObjectCustomFloor();
                //    break;
                //case "MapObjectCustomGridlines":
                //    mapObject = new MapObjectCustomGridlines();
                //    break;
                //case "MapObjectCustomMap":
                //    mapObject = new MapObjectCustomMap();
                //    break;
                //case "MapObjectCustomPositionAngle":
                //    mapObject = new MapObjectCustomPositionAngle();
                //    break;
                //case "MapObjectCustomPositionAngleArrow":
                //    mapObject = new MapObjectCustomPositionAngleArrow();
                //    break;
                //case "MapObjectCustomSphere":
                //    mapObject = new MapObjectCustomSphere();
                //    break;
                //case "MapObjectCustomSpherePoints":
                //    mapObject = new MapObjectCustomSpherePoints();
                //    break;
                //case "MapObjectCustomUnitPoints":
                //    mapObject = new MapObjectCustomUnitPoints();
                //    break;
                //case "MapObjectCustomWall":
                //    mapObject = new MapObjectCustomWall();
                //    break;
                //case "MapObjectDrawDistanceSphere":
                //    mapObject = new MapObjectDrawDistanceSphere();
                //    break;
                //case "MapObjectDrawing":
                //    mapObject = new MapObjectDrawing();
                //    break;
                //case "MapObjectEffectiveHitboxCylinder":
                //    mapObject = new MapObjectEffectiveHitboxCylinder();
                //    break;
                //case "MapObjectEffectiveHurtboxCylinder":
                //    mapObject = new MapObjectEffectiveHurtboxCylinder();
                //    break;
                //case "MapObjectFacingDivider":
                //    mapObject = new MapObjectFacingDivider();
                //    break;
                //case "MapObjectFloatGridlines":
                //    mapObject = new MapObjectFloatGridlines();
                //    break;
                //case "MapObjectGhost":
                //    mapObject = new MapObjectGhost();
                //    break;
                //case "MapObjectHitboxCylinder":
                //    mapObject = new MapObjectHitboxCylinder();
                //    break;
                //case "MapObjectHitboxHackTriangle":
                //    mapObject = new MapObjectHitboxHackTriangle();
                //    break;
                //case "MapObjectHolp":
                //    mapObject = new MapObjectHolp();
                //    break;
                //case "MapObjectHome":
                //    mapObject = new MapObjectHome();
                //    break;
                //case "MapObjectHomeLine":
                //    mapObject = new MapObjectHomeLine();
                //    break;
                //case "MapObjectHurtboxCylinder":
                //    mapObject = new MapObjectHurtboxCylinder();
                //    break;
                //case "MapObjectIwerlipses":
                //    mapObject = new MapObjectIwerlipses();
                //    break;
                //case "MapObjectLedgeGrabChecker":
                //    mapObject = new MapObjectLedgeGrabChecker();
                //    break;
                //case "MapObjectLevelCeiling":
                //    mapObject = new MapObjectLevelCeiling();
                //    break;
                //case "MapObjectLevelFloor":
                //    mapObject = new MapObjectLevelFloor();
                //    break;
                //case "MapObjectLevelWall":
                //    mapObject = new MapObjectLevelWall();
                //    break;
                //case "MapObjectLineSegment":
                //    mapObject = new MapObjectLineSegment();
                //    break;
                //case "MapObjectMario":
                //    mapObject = new MapObjectMario();
                //    break;
                //case "MapObjectMarioCeiling":
                //    mapObject = new MapObjectMarioCeiling();
                //    break;
                //case "MapObjectMarioFacingArrow":
                //    mapObject = new MapObjectMarioFacingArrow();
                //    break;
                //case "MapObjectMarioFloor":
                //    mapObject = new MapObjectMarioFloor();
                //    break;
                //case "MapObjectMarioFloorArrow":
                //    mapObject = new MapObjectMarioFloorArrow();
                //    break;
                //case "MapObjectMarioIntendedArrow":
                //    mapObject = new MapObjectMarioIntendedArrow();
                //    break;
                //case "MapObjectMarioMovingArrow":
                //    mapObject = new MapObjectMarioMovingArrow();
                //    break;
                //case "MapObjectMarioSlidingArrow":
                //    mapObject = new MapObjectMarioSlidingArrow();
                //    break;
                //case "MapObjectMarioTwirlArrow":
                //    mapObject = new MapObjectMarioTwirlArrow();
                //    break;
                //case "MapObjectMarioWall":
                //    mapObject = new MapObjectMarioWall();
                //    break;
                //case "MapObjectNextPositions":
                //    mapObject = new MapObjectNextPositions();
                //    break;
                //case "MapObjectObject":
                //    mapObject = new MapObjectObject();
                //    break;
                //case "MapObjectObjectAngleToMarioArrow":
                //    mapObject = new MapObjectObjectAngleToMarioArrow();
                //    break;
                //case "MapObjectObjectCeiling":
                //    mapObject = new MapObjectObjectCeiling();
                //    break;
                //case "MapObjectObjectCustomArrow":
                //    mapObject = new MapObjectObjectCustomArrow();
                //    break;
                //case "MapObjectObjectFacingArrow":
                //    mapObject = new MapObjectObjectFacingArrow();
                //    break;
                //case "MapObjectObjectFloor":
                //    mapObject = new MapObjectObjectFloor();
                //    break;
                //case "MapObjectObjectGraphicsArrow":
                //    mapObject = new MapObjectObjectGraphicsArrow();
                //    break;
                //case "MapObjectObjectMovingArrow":
                //    mapObject = new MapObjectObjectMovingArrow();
                //    break;
                //case "MapObjectObjectWall":
                //    mapObject = new MapObjectObjectWall();
                //    break;
                //case "MapObjectPath":
                //    mapObject = new MapObjectPath();
                //    break;
                //case "MapObjectPoint":
                //    mapObject = new MapObjectPoint();
                //    break;
                //case "MapObjectPreviousPositions":
                //    mapObject = new MapObjectPreviousPositions();
                //    break;
                //case "MapObjectPuGridlines":
                //    mapObject = new MapObjectPuGridlines();
                //    break;
                //case "MapObjectPunchDetector":
                //    mapObject = new MapObjectPunchDetector();
                //    break;
                //case "MapObjectPunchFloor":
                //    mapObject = new MapObjectPunchFloor();
                //    break;
                //case "MapObjectPushHitboxCylinder":
                //    mapObject = new MapObjectPushHitboxCylinder();
                //    break;
                //case "MapObjectSector":
                //    mapObject = new MapObjectSector();
                //    break;
                //case "MapObjectSelf":
                //    mapObject = new MapObjectSelf();
                //    break;
                //case "MapObjectSwooperEffectiveTargetArrow":
                //    mapObject = new MapObjectSwooperEffectiveTargetArrow();
                //    break;
                //case "MapObjectTangibilitySphere":
                //    mapObject = new MapObjectTangibilitySphere();
                //    break;
                //case "MapObjectUnitGridlines":
                //    mapObject = new MapObjectUnitGridlines();
                //    break;
                //case "MapObjectWaters":
                //    mapObject = new MapObjectWaters();
                //    break;
                default:
                    throw new ArgumentOutOfRangeException("Unknown MapObject type: " + type);
            }

            XElement settingsXElement = xElement.Element(XName.Get("Settings"));
            MapObjectSettings settings = MapObjectSettings.FromXElement(settingsXElement);
            mapObject.ApplySettings(settings);

            return mapObject;
        }
    }
}
