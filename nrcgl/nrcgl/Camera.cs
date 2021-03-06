﻿#region License
//
// The NRCGL License.
//
// The MIT License (MIT)
//
// Copyright (c) 2015 Nuno Ramalho da Costa
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//
#endregion

using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nrcgl
{
    public class Camera
    {
        private Vector3 position;
        private Quaternion quaternion;
        private Matrix4 cameraUVW;
        private Matrix4 view;


        public Matrix4 View
        {
            get { return view; }
            set { view = value; }
        }

        public Matrix4 CameraUVW
        {
            get { return cameraUVW; }
            set { cameraUVW = value; }
        }

        public virtual Vector3 Position
        {
            get { return position; }
            set
            {
                Vector3 deltaPosition = value - position;

                TranslateWC(deltaPosition.X, deltaPosition.Y, deltaPosition.Z);

                position = value;
            }
        }

        public Quaternion Quaternion
        {
            get { return quaternion; }
            set { quaternion = value; }
        }
        
        
        public Camera()
        {
            View = Matrix4.Identity;

            Position = Vector3.Zero;

            Quaternion = Quaternion.Identity;

            CameraUVW = Matrix4.Identity;



        }

        public virtual void TranslateWC(float x, float y, float z)
        {
            position.X += x;
            position.Y += y;
            position.Z += z;
        }

        public void TranslateLC(float u, float v, float w)
        {

            Vector4 wcTranslation = Vector4.Transform(new Vector4(u, v, w, 0), CameraUVW);

            TranslateWC(wcTranslation.X, wcTranslation.Y, wcTranslation.Z);
        }

        // not tested
        public void LookAt(Vector3 target, Vector3 up)
        {
            Matrix4 lookAt = Matrix4.LookAt(position, target, up);

            View = Matrix4.Mult(View, lookAt);

            CameraUVW = Matrix4.Mult(View, lookAt);

        }

        public virtual void Rotate(Vector3 axis, float angle)
        {
            Quaternion q = Quaternion.FromAxisAngle(axis, angle);

            Rotate(q);
        }

        public virtual void Rotate(Quaternion q)
        {
            Quaternion =  Quaternion * q;

            //rotate CameraUVW

			var tempQ = q;

			tempQ.W *= -1;

			CameraUVW = CameraUVW * Matrix4.Rotate(tempQ);
        }

        public void RotateU(float angle)
        {
            Rotate(new Vector3(CameraUVW.Row0.X,
                               CameraUVW.Row0.Y,
                               CameraUVW.Row0.Z), angle);
        }

        public void RotateV(float angle)
        {
            Rotate(new Vector3(CameraUVW.Row1.X,
                               CameraUVW.Row1.Y,
                               CameraUVW.Row1.Z), angle);
        }

        public void RotateW(float angle)
        {
            Rotate(new Vector3(CameraUVW.Row2.X,
                               CameraUVW.Row2.Y,
                               CameraUVW.Row2.Z), angle);
        }

        public void LevelU2XZ(float minDot)
        {
            Vector3 v1 = new Vector3(CameraUVW.Row0);
            Vector3 v2 = new Vector3(CameraUVW.Row0.X, 0f, CameraUVW.Row0.Z);

            v2.Normalize();

            float cameraUangle = Vector3.Dot(v2, v1);

            Vector3 axis = new Vector3(CameraUVW.Row2.X,
                                       CameraUVW.Row2.Y,
                                       CameraUVW.Row2.Z);
            float angle = (float)Math.Acos(Convert.ToDouble(cameraUangle));

            if (CameraUVW.Row0.Y > 0 && cameraUangle < minDot)
                Rotate(axis, angle);
            else if (cameraUangle < minDot)
                Rotate(axis, -angle);
        }

        public void Update()
        {
			Matrix4 RM = Matrix4.Rotate(Quaternion);

            Matrix4 TM = Matrix4.CreateTranslation(Position);

            View = RM;

            View = TM * View;
        }

    }
}
