using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chip_8_emulator
{
    class Emulator
    {
        Random rnd = new Random();
        private static byte[] fontset =
        {
            0xF0, 0x90, 0x90, 0x90, 0xF0, //0
            0x20, 0x60, 0x20, 0x20, 0x70, //1
            0xF0, 0x10, 0xF0, 0x80, 0xF0, //2
            0xF0, 0x10, 0xF0, 0x10, 0xF0, //3
            0x90, 0x90, 0xF0, 0x10, 0x10, //4
            0xF0, 0x80, 0xF0, 0x10, 0xF0, //5
            0xF0, 0x80, 0xF0, 0x90, 0xF0, //6
            0xF0, 0x10, 0x20, 0x40, 0x40, //7
            0xF0, 0x90, 0xF0, 0x90, 0xF0, //8
            0xF0, 0x90, 0xF0, 0x10, 0xF0, //9
            0xF0, 0x90, 0xF0, 0x90, 0x90, //A
            0xE0, 0x90, 0xE0, 0x90, 0xE0, //B
            0xF0, 0x80, 0x80, 0x80, 0xF0, //C
            0xE0, 0x90, 0x90, 0x90, 0xE0, //D
            0xF0, 0x80, 0xF0, 0x80, 0xF0, //E
            0xF0, 0x80, 0xF0, 0x80, 0x80  //F
        };
        // mmu
        byte[] mmu = new byte[0x1000];
        // cpu
        ushort opcode = 0;
        byte[] V = new byte[16]; //registers
        ushort I = 0; //index register
        ushort PC = 0; // program counter
        // gpu
        public byte[] screen = new byte[64 * 32];
        // timers
        byte delay_timer = 0;
        byte sound_timer = 0;
        // stack
        ushort[] stack = new ushort[16];
        ushort SP = 0;
        // keys 
        bool[] keys = new bool[16];
        // flag
        bool drawFlag = true;

        private void PushToStack(ushort value)
        {
            stack[SP] = value;
            SP++;
        }
        private ushort PopFromStack()
        {
            return stack[--SP];
        }

        public void Reset()
        {
            PC = 0x200;
            opcode = I = SP = 0;

            // Clear display
            for (int i = 0; i < 2048; ++i)
                screen[i] = 0;

            for (int i = 0; i < 16; ++i)
            {
                keys[i] = false;
                V[i] = 0;
                stack[i] = 0;
            }

            // Clear memory
            for (int i = 0; i < 4096; ++i)
                mmu[i] = 0;

            // Load fontset
            for (int i = 0; i < 80; ++i)
                mmu[i] = fontset[i];

            // Reset timers
            delay_timer = 0;
            sound_timer = 0;

            // Clear screen once
            drawFlag = true;
        }
        public void Draw()
        {

        }
        public void SetKeys(Keys keys)
        {

        }
        public void LoadApp(string filepath)
        {

        }
        public void Step()
        {
            // Fetch Opcode
            opcode = (ushort)(mmu[PC] << 8 | mmu[PC + 1]);
            // Decode Opcode
            // as is in https://en.wikipedia.org/wiki/CHIP-8#Opcode_table
            switch (opcode & 0xF000)
            {
                case 0x0000:
                    switch (opcode)
                    {
                        case 0x00E0:
                            screen = new byte[64 * 32];
                            drawFlag = true;
                            PC += 2;
                            break;
                        case 0x00EE:
                            PC = PopFromStack();
                            PC += 2;
                            break;
                    }
                    break;
                case 0x1000:
                    PC = (ushort)(opcode & 0x0FFF);
                    break;
                case 0x2000:
                    PushToStack(PC);
                    PC = (ushort)(opcode & 0x0FFF);
                    break;
                case 0x3000:
                    PC += (ushort)(V[(opcode & 0x0F00) >> 8] == (opcode & 0x00FF) ? 4 : 2);
                    break;
                case 0x4000:
                    PC += (ushort)(V[(opcode & 0x0F00) >> 8] != (opcode & 0x00FF) ? 4 : 2);
                    break;
                case 0x5000:
                    PC += (ushort)(V[(opcode & 0x0F00) >> 8] == V[(opcode & 0x00F0)>>4] ? 4 : 2);
                    break;
                case 0x6000:
                    V[(opcode & 0x0F00) >> 8] = (byte)(opcode & 0x00FF);
                    PC += 2;
                    break;
                case 0x7000:
                    V[(opcode & 0x0F00) >> 8] += (byte)(opcode & 0x00FF);
                    PC += 2;
                    break;
                case 0x8000:

                    break;
                case 0x9000:
                    PC += (ushort)(V[(opcode & 0x0F00) >> 8] != V[(opcode & 0x00F0)>>4] ? 4 : 2);
                    break;
                case 0xA000:
                    I = (ushort)(opcode & 0x0FFF);
                    PC += 2;
                    break;
                case 0xB000:
                    PC = (ushort)((opcode & 0x0FFF) + V[0]);
                    break;
                case 0xC000:
                    V[(opcode & 0x0F00) >> 8] = (byte)(rnd.Next(0xFF) & (opcode & 0x00FF));
                    break;
                case 0xD000:

                    break;
                case 0xE000:

                    break;
                case 0xF000:

                    break;
            }
            // Execute Opcode

            // Update timers
            if (delay_timer > 0) delay_timer--;
            if (sound_timer == 1) Console.WriteLine("\a"); // beep
            if (sound_timer > 0) sound_timer--; 
        }
    }
}
