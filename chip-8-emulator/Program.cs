using SFML.Graphics;
using SFML.System;
using System;
using System.Linq;
using SFML.Window;

namespace chip_8_emulator
{

    public class Game : RenderWindow
    {
        private Time DeltaTime { get; set; }
        Emulator emu = new Emulator();
        public Game(string title) : base(new VideoMode(640, 320), title)
        {
            Closed += Game_Closed;
            KeyPressed += Game_KeyPressed;
            KeyReleased += Game_KeyReleased;
            SetFramerateLimit(480);
            
        }

        private void Game_KeyReleased(object sender, KeyEventArgs e)
        {
            HandleKey(e, false);
        }

        private void Game_KeyPressed(object sender, KeyEventArgs e)
        {
            HandleKey(e, true);
        }

        private void HandleKey(KeyEventArgs e, bool isPressed)
        {
            switch (e.Code)
            {
                case Keyboard.Key.Num1: emu.keys[1] = isPressed; break;
                case Keyboard.Key.Num2: emu.keys[2] = isPressed; break;
                case Keyboard.Key.Num3: emu.keys[3] = isPressed; break;
                case Keyboard.Key.Num4: emu.keys[0xC] = isPressed; break;

                case Keyboard.Key.Q: emu.keys[4] = isPressed; break;
                case Keyboard.Key.W: emu.keys[5] = isPressed; break;
                case Keyboard.Key.E: emu.keys[6] = isPressed; break;
                case Keyboard.Key.R: emu.keys[0xD] = isPressed; break;

                case Keyboard.Key.A: emu.keys[7] = isPressed; break;
                case Keyboard.Key.S: emu.keys[8] = isPressed; break;
                case Keyboard.Key.D: emu.keys[9] = isPressed; break;
                case Keyboard.Key.F: emu.keys[0xE] = isPressed; break;

                case Keyboard.Key.Z: emu.keys[0xA] = isPressed; break;
                case Keyboard.Key.X: emu.keys[0] = isPressed; break;
                case Keyboard.Key.C: emu.keys[0xB] = isPressed; break;
                case Keyboard.Key.V: emu.keys[0xF] = isPressed; break;

                case Keyboard.Key.Escape: Close(); break;
            }
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
            emu.LoadApp("pong.rom");

            while (IsOpen)
            {
                DeltaTime = deltaClock.Restart();
                DispatchEvents();
                Update();
                if (emu.drawFlag)
                {
                    Clear(Color.Black);
                    DrawScreen();
                    emu.drawFlag = false;
                }
                Display();
            }

        }

        private void Update()
        {
            emu.Step();
        }

        private void DrawScreen()
        {
            Color[,] colors = new Color[64, 32];
            for (int x = 0; x < 64; x++)
                for (int y = 0; y < 32; y++)
                    colors[x, y] = emu.screen[y * 64 + x] ? Color.White : Color.Black;
                
            Image img = new Image(colors);
            Texture tex = new Texture(img);
            Sprite s = new Sprite(tex) { Scale = new Vector2f(10, 10) };
            Draw(s);
            s.Dispose();
            tex.Dispose();
            img.Dispose();
        }
    }
    class Program
    {
        static void Main()
        {
            Game gameWindow = new Game("Chip-8 emulator");
            gameWindow.Run();
        }
    }
}
