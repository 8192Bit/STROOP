﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using STROOP.Structs;
using STROOP.Controls.Map;

namespace STROOP.Map3
{
    public class Map3Graphics
    {
        int _mapTex = -1;
        int _mapBackgroundTex = -1;
        Size _mapImageSize;
        List<Map3Object> _mapObjects = new List<Map3Object>();

        public RectangleF MapView;
        public GLControl Control;

        public Map3Graphics(GLControl control)
        {
            Control = control;
        }

        public void Load()
        {
            Control.MakeCurrent();
            Control.Context.LoadAll();

            Control.Paint += OnPaint;
            Control.Resize += OnResize;

            GL.ClearColor(Color.FromKnownColor(KnownColor.Control));
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            SetupViewport();
        }

        public void OnPaint(object sender, EventArgs e)
        {
            Control.MakeCurrent();

            // Set default background color (clear drawing area)
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.MatrixMode(MatrixMode.Modelview);

            if (_mapBackgroundTex != -1)
            {
                DrawTexture(_mapBackgroundTex, new PointF(Control.Width / 2, Control.Height / 2), Control.Size);
            }

            // Don't draw if no map is loaded
            if (_mapTex == -1)
            {
                Control.SwapBuffers();
                return;
            }

            // Draw map image
            //DrawTexture(_mapTex, new PointF(MapView.X + MapView.Width / 2, MapView.Y + MapView.Height / 2), MapView.Size);

            // Loop through and draw all map objects
            foreach (Map3Object mapObj in _mapObjects)
            {
                // Draw the map object
                mapObj.DrawOnControl();
            }

            Control.SwapBuffers();
        }

        public void OnResize(object sender, EventArgs e)
        {
            Control.MakeCurrent();

            SetupViewport();
            SetMapView();
        }

        public SizeF ScaleImageSize(Size imageSize, float desiredSize)
        {
            float scale = Math.Max(imageSize.Height / desiredSize, imageSize.Width / desiredSize);
            return new SizeF(imageSize.Width / scale, imageSize.Height / scale);
        }

        private void SetupViewport()
        {
            int w = Control.Width;
            int h = Control.Height;
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            GL.Ortho(0, w, h, 0, -1, 1); // Bottom-left corner pixel has coordinate (0, 0)
            GL.Viewport(0, 0, w, h); // Use all of the glControl painting area
        }

        private void SetMapView()
        {
            // Calculate scale of "zoom" view (make sure image fits fully within the region, 
            // it is at a maximum size, and the aspect ration is maintained 
            float hScale = ((float)Control.Width) / _mapImageSize.Width;
            float vScale = ((float)Control.Height) / _mapImageSize.Height;
            float scale = Math.Min(hScale, vScale);

            float marginV = 0;
            float marginH = 0;
            if (hScale > vScale)
                marginH = (Control.Width - scale * _mapImageSize.Width);
            else
                marginV = (Control.Height - scale * _mapImageSize.Height);

            // Calculate where the map image should be drawn
            MapView = new RectangleF(marginH / 2, marginV / 2, Control.Width - marginH, Control.Height - marginV);
        }

        public void SetBackground(Image background)
        {
            int oldTex = _mapBackgroundTex;

            if (background != null)
            {
                _mapBackgroundTex = LoadTexture(background as Bitmap);
            }
            else
            {
                _mapBackgroundTex = -1;
            }

            // Delete old map image
            if (oldTex != -1)
            {
                GL.DeleteTexture(oldTex);
            }
        }

        public void SetMap(Image map)
        {
            int oldTex = _mapTex;

            _mapTex = LoadTexture(map as Bitmap);
            _mapImageSize = map.Size;
            SetMapView();

            // Delete old map image
            if (oldTex != -1)
            {
                GL.DeleteTexture(oldTex);
            }
        }

        public void DrawTexture(int texId, PointF loc, SizeF size, float angle = 0, float alpha = 1)
        {
            // Place and rotate texture to correct location on control
            GL.LoadIdentity();
            GL.Translate(new Vector3(loc.X, loc.Y, 0));
            GL.Rotate(360 - angle, Vector3.UnitZ);
            GL.Color4(1.0, 1.0, 1.0, alpha);

            // Start drawing texture
            GL.BindTexture(TextureTarget.Texture2D, texId);
            GL.Begin(PrimitiveType.Quads);

            // Set drawing coordinates
            GL.TexCoord2(0.0f, 1.0f); GL.Vertex2(-size.Width / 2, size.Height / 2);
            GL.TexCoord2(1.0f, 1.0f); GL.Vertex2(size.Width / 2, size.Height / 2);
            GL.TexCoord2(1.0f, 0.0f); GL.Vertex2(size.Width / 2, -size.Height / 2);
            GL.TexCoord2(0.0f, 0.0f); GL.Vertex2(-size.Width / 2, -size.Height / 2);

            GL.End();
        }

        public int LoadTexture(Bitmap bmp)
        {
            // Create texture and id
            int id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);

            // Set Bi-Linear Texture Filtering
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapNearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            // Get data from bitmap image
            BitmapData bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            // Store bitmap data as OpenGl texture
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, bmp.Width, bmp.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);

            bmp.UnlockBits(bmp_data);

            // Generate mipmaps for texture filtering
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            return id;
        }

        public void AddMapObject(Map3Object mapObj)
        {
            _mapObjects.Add(mapObj);
        }

        public void RemoveMapObject(Map3Object mapObj)
        {
            _mapObjects.Remove(mapObj);
            mapObj?.Dispose();
        }
    }
}
