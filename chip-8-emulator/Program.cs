using SFML.Graphics;
using SFML.System;
using System;

namespace chip_8_emulator
{

    public class Game : RenderWindow
    {
        private Time DeltaTime { get; set; }
        Emulator emu = new Emulator();
        public Game(string title) : base(new SFML.Window.VideoMode(160, 144), title)
        {
            Closed += Game_Closed;
            SetFramerateLimit(60);
        }

        private void Game_Closed(object sender, EventArgs e)
        {
            Close();
        }

        public void Run()
        {
            Clock deltaClock = new Clock();
            DeltaTime = deltaClock.Restart();
            emu.Reset();
            emu.LoadApp("test");

            while (IsOpen)
            {
                DeltaTime = deltaClock.Restart();
                DispatchEvents();
                HandleKeys();
                Update();
                DrawScreen();
                Display();
            }

        }

        private void Update()
        {
            emu.Step();
        }

        private void DrawScreen()
        {
            Image img = new Image(emu.screen);
            Texture tex = new Texture(img);
            Sprite s = new Sprite(tex);
            Draw(s);
            s.Dispose();
            tex.Dispose();
            img.Dispose();
        }

        private void HandleKeys()
        {
            //emu.SetKeys();
        }
    }
    class Program
    {
        static void Main()
        {
            Game gameWindow = new Game("Gameboy emulator");
            gameWindow.Run();
        }
    }
}
