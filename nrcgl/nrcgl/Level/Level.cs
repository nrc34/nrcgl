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
using nrcgl;
using nrcgl.nrcgl.shapes;


#endregion

using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nrcgl.Level
{
	public abstract class Level : ILevel
    {
        public enum State
        {
            Loading,
            Starting,
            Running,
            Finishing,
            Unloading
        }
        
        
        public int ID { get; set; }

        public string Name { get; set; }

        public State CurrentState  { get; set; }

        public bool IsFinishedWithSuccess { get; set; }

		public GLView GLView { get; set; }

        public Dictionary<string, Shape3D> Shapes3D { get; set; }

        public Stack<Shape3D> Shapes2Remove { get; set; }

        public Dictionary<string, int> Textures { get; set; }

        public Camera Camera { get; set; }


        #region ILevel implementation

		public abstract void Load();

        public abstract void Unload();

        public abstract void Start();

        public abstract void Run();

		public abstract void Update();

		public abstract void Render();

        public abstract void Finish();

		public abstract void OnTouch();

        #endregion

    }
}
