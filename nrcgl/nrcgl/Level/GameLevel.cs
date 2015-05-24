#region License
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
using nrcgl.nrcgl.shapes;


#endregion

using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nrcgl.Level
{
	public class GameLevel : Level
    {

		public GameLevel(int id, string name, GLView gameWindow)
        {
            ID = id;
            Name = name;

            Shapes2Remove = new Stack<Shape3D>();

            CurrentState = Level.State.Loading;

            IsFinishedWithSuccess = false;

			GLView = gameWindow;
        }

        public virtual void LoadShapes() 
        {
        }

        public virtual void LoadAudio()
        {
        }

        public virtual void LoadTextures()
        {
        }

		#region implemented abstract members of Level
		public override void Load()
		{
			LoadTextures();

			LoadAudio();

			LoadShapes();
		}

        public override void Start()
        {
        }

        public override void Run()
        {
        }

        public override void Finish()
        {
        }

        public override void Unload()
        {
        }

        public override void Update()
        {
            while (Shapes2Remove.Count > 0)
                Shapes3D.Remove(Shapes2Remove.Pop().Name);


            //Camera.LevelU2XZ(0.9998470f);

            //Camera.Update();

            switch (CurrentState)
            {
                case State.Loading:
                    return;
                case State.Starting:
                    Start();
                    return;
                case State.Running:
                    Run();
                    return;
                case State.Finishing:
                    Finish();
                    return;
                case State.Unloading:
                    Unload();
                    return;
                default:
                    break;
            }
        }

        public override void Render()
        {
            switch (CurrentState)
            {
                case State.Loading:
                    return;
                case State.Starting:
                    break;
                case State.Running:
                    break;
                case State.Finishing:
                    break;
                case State.Unloading:
                    return;
                default:
                    break;
            }
        }

		public override void OnTouch ()
		{
		}
		#endregion
    }
}
