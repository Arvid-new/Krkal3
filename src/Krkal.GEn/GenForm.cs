using System;
using System.Collections.Generic;
using System.Text;
using SlimDX;
using Krkal.GEn.Base;
using System.Drawing;
using System.Windows.Forms;
using SlimDX.Direct3D9;
using System.Globalization;

namespace Krkal.GEn
{
	class GenForm : Game
	{
        const int InitialWidth = 800;
        const int InitialHeight = 600;

		TextConsole _console;

		KrkalScene _scene;

		bool[] _keyState = new bool[256];
		bool[] _keyStateThisFrame = new bool[256];
		bool[] _keyStateNotReleased = new bool[256];

		Vector3 _myPos;

        public Device Device
        {
            get { return GraphicsDeviceManager.Direct3D9.Device; }
        }


		public GenForm()
        {

            Window.ClientSize = new Size(InitialWidth, InitialHeight);
            Window.Text = "SlimDX - Simple Triangle Sample";
            Window.KeyDown += Window_KeyDown;
			Window.KeyUp += Window_KeyUp;


			DeviceSettings settings = new DeviceSettings();
			settings.BackBufferWidth = InitialWidth;
			settings.BackBufferHeight = InitialHeight;
			settings.DeviceVersion = DeviceVersion.Direct3D9;
			settings.Windowed = true;
			settings.EnableVSync = false;
			settings.BackBufferFormat = Format.A8R8G8B8;
			settings.DepthStencilFormat = Format.D24S8;


            GraphicsDeviceManager.ChangeDevice(settings);

			this.IsFixedTimeStep = false;
		}

		void Window_KeyUp(object sender, KeyEventArgs e) {
			_keyState[e.KeyValue] = false;
			_keyStateNotReleased[e.KeyValue] = false;
		}

        void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // F1 toggles between full screen and windowed mode
            // Escape quits the application
            if (e.KeyCode == Keys.F1)
                GraphicsDeviceManager.ToggleFullScreen();
            else if (e.KeyCode == Keys.Escape)
                Exit();

			_keyState[e.KeyValue] = true;
			if (!_keyStateNotReleased[e.KeyValue]) {
				// avoid multiple presses from key repeat
				_keyStateThisFrame[e.KeyValue] = true;
				_keyStateNotReleased[e.KeyValue] = true;
			}
		}


		protected override void Initialize() {
			_console = new TextConsole();
			Resources.Add(_console);

			_scene = new KrkalScene(this);

			base.Initialize();
		}



        protected override void LoadContent()
        {
			base.LoadContent();

        }

        protected override void UnloadContent()
        {
			base.UnloadContent();

        }

        protected override void Draw(GameTime gameTime)
        {
			base.Draw(gameTime);

            Device.BeginScene();

			_scene.Draw();

			_console.Begin();
			_console.Location = new Point(5, 5);
			_console.ForegroundColor = new Color4(1.0f, 1.0f, 1.0f, 0.0f);
			_console.WriteLine(GraphicsDeviceManager.DeviceInformation);
			_console.WriteLine(GraphicsDeviceManager.DeviceStatistics);
			_console.WriteLine(gameTime.FramesPerSecond.ToString(".00", CultureInfo.CurrentCulture));
			_console.WriteLine(String.Format(CultureInfo.CurrentCulture, "My Position is: {0}", _myPos));
			_console.End();

            Device.EndScene();
        }


		protected override void Update(GameTime gameTime) {
			base.Update(gameTime);

			float speed = gameTime.ElapsedRealTime * 80;
			if (IsKeyPressed(Keys.W)) _myPos.Y = _myPos.Y - speed;
			if (IsKeyPressed(Keys.S)) _myPos.Y = _myPos.Y + speed;
			if (IsKeyPressed(Keys.A)) _myPos.X = _myPos.X - speed;
			if (IsKeyPressed(Keys.D)) _myPos.X = _myPos.X + speed;
			if (IsKeyPressed(Keys.Q)) _myPos.Z = _myPos.Z + speed;
			if (IsKeyPressed(Keys.E)) _myPos.Z = _myPos.Z - speed;

			_scene.ChangeLightPos(_myPos);

			_scene.ChangeRotation(gameTime.TotalRealTime);

			// reset the key frame list
			Array.Clear(_keyStateThisFrame, 0, 256);
		}


		public bool IsKeyPressed(Keys key) {
			return _keyState[(int)key];
		}

		public bool IsKeyPressedThisFrame(Keys key) {
			return _keyStateThisFrame[(int)key];
		}
	}
}
