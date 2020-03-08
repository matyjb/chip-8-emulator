using System;
using System.Collections.Generic;
using System.IO;
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
        //ushort[] stack = new ushort[16];
        Stack<ushort> stack = new Stack<ushort>(16);
        //ushort SP = 0;
        // keys 
        public bool[] keys = new bool[16];
        // flag
        public bool drawFlag = true;

        public void Reset()
        {
            PC = 0x200;
            opcode = I = 0;
            stack.Clear();

            // Clear display
            screen = new bool[64,32];
                screen[i] = 0;

            for (int i = 0; i < 16; ++i)
            {
                keys[i] = false;
                V[i] = 0;
            }

            // Clear memory
            mmu = new byte[0x1000];
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
        public void Draw(ushort opcode)
        {
            ushort x = V[opcode.gb(2)];
            ushort y = V[opcode.gb(1)];
            ushort height = opcode.gb(0);
            ushort pixel;

            V[0xF] = 0;
            for (int yline = 0; yline < height; yline++)
            {
                pixel = mmu[I + yline];
                for (int xline = 0; xline < 8; xline++)
                {
                    if ((pixel & (0x80 >> xline)) != 0)
                    {
                        if (screen[(x + xline + ((y + yline) * 64))] == 1)
                        {
                            V[0xF] = 1;
                        }
                        screen[x + xline + ((y + yline) * 64)] ^= 1;
                    }
                }
            }
        }
        public void LoadApp(string filepath)
        {
            FileStream sr = new FileStream(filepath, FileMode.Open);
            int b, i = 0;
            while ((b = sr.ReadByte()) != -1)
            {
                mmu[i + 512] = Convert.ToByte(b);
                i++;
            }
            sr.Close();
        }
        public void Step()
        {
            // Fetch Opcode
            opcode = (ushort)(mmu[PC] << 8 | mmu[PC + 1]);
            // Decode Opcode
            // Execute Opcode
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
                            PC = stack.Pop();
                            PC += 2;
                            break;
                    }
                    break;
                case 0x1000:
                    PC = (ushort)(opcode & 0x0FFF);
                    break;
                case 0x2000:
                    stack.Push(PC);
                    PC = (ushort)(opcode & 0x0FFF);
                    break;
                case 0x3000:
                    PC += (ushort)(V[opcode.gb(2)] == (opcode & 0x00FF) ? 4 : 2);
                    break;
                case 0x4000:
                    PC += (ushort)(V[opcode.gb(2)] != (opcode & 0x00FF) ? 4 : 2);
                    break;
                case 0x5000:
                    PC += (ushort)(V[opcode.gb(2)] == V[opcode.gb(1)] ? 4 : 2);
                    break;
                case 0x6000:
                    V[opcode.gb(2)] = (byte)(opcode & 0x00FF);
                    PC += 2;
                    break;
                case 0x7000:
                    V[opcode.gb(2)] += (byte)(opcode & 0x00FF);
                    PC += 2;
                    break;
                case 0x8000:
                    switch (opcode & 0x000F)
                    {
                        case 0x0000:
                            V[opcode.gb(2)] = V[opcode.gb(1)];
                            PC += 2;
                            break;
                        case 0x0001:
                            V[opcode.gb(2)] |= V[opcode.gb(1)];
                            PC += 2;
                            break;
                        case 0x0002:
                            V[opcode.gb(2)] &= V[opcode.gb(1)];
                            PC += 2;
                            break;
                        case 0x0003:
                            V[opcode.gb(2)] ^= V[opcode.gb(1)];
                            PC += 2;
                            break;
                        case 0x0004:
                            int result = V[opcode.gb(2)] + V[opcode.gb(1)];
                            V[opcode.gb(2)] = (byte)result;
                            V[0xF] = (byte)(result > 0xFF ? 1 : 0);
                            PC += 2;
                            break;
                        case 0x0005:
                            int result2 = V[opcode.gb(2)] - V[opcode.gb(1)];
                            V[opcode.gb(2)] = (byte)result2;
                            V[0xF] = (byte)(result2 < 0 ? 1 : 0);
                            PC += 2;
                            break;
                        case 0x0006:
                            V[0xF] = (byte)(V[opcode.gb(2)] & 0x1);
                            V[opcode.gb(2)] >>= 1;
                            PC += 2;
                            break;
                        case 0x0007:
                            int result3 = V[opcode.gb(1)] - V[opcode.gb(2)];
                            V[opcode.gb(2)] = (byte)result3;
                            V[0xF] = (byte)(result3 < 0 ? 0 : 1);
                            PC += 2;
                            break;
                        case 0x000E:
                            V[0xF] = (byte)(V[opcode.gb(2)] >> 7);
                            V[opcode.gb(2)] <<= 1;
                            PC += 2;
                            break;
                    }
                    break;
                case 0x9000:
                    PC += (ushort)(V[opcode.gb(2)] != V[opcode.gb(1)] ? 4 : 2);
                    break;
                case 0xA000:
                    I = (ushort)(opcode & 0x0FFF);
                    PC += 2;
                    break;
                case 0xB000:
                    PC = (ushort)((opcode & 0x0FFF) + V[0]);
                    break;
                case 0xC000:
                    V[opcode.gb(2)] = (byte)(rnd.Next(0xFF) & (opcode & 0x00FF));
                    PC += 2;
                    break;
                case 0xD000:
                    Draw(opcode);
                    drawFlag = true;
                    PC += 2;
                    break;
                case 0xE000:
                    switch (opcode & 0x00FF)
                    {
                        case 0x009E:
                            PC += (ushort)(keys[V[opcode.gb(2)]] ? 4 : 2);
                            break;
                        case 0x00A1:
                            PC += (ushort)(keys[V[opcode.gb(2)]] ? 2 : 4);
                            break;
                    }
                    break;
                case 0xF000:
                    switch (opcode & 0x00FF)
                    {
                        case 0x0007:
                            V[opcode.gb(2)] = delay_timer;
                            PC += 2;
                            break;
                        case 0x000A:

                            bool isAnyKeyPressed = false;
                            for (int i = 0; i < 16; i++)
                            {
                                if (keys[i])
                                {
                                    V[opcode.gb(2)] = (byte)i;
                                    isAnyKeyPressed = true;
                                    break;
                                }
                            }
                            if (!isAnyKeyPressed)
                                return;

                            PC += 2;
                            break;
                        case 0x0015:
                            delay_timer = V[opcode.gb(2)];
                            PC += 2;
                            break;
                        case 0x0018:
                            sound_timer = V[opcode.gb(2)];
                            PC += 2;
                            break;
                        case 0x001E:
                            if (I + V[opcode.gb(2)] > 0xFFF)
                                V[0xF] = 1;
                            else
                                V[0xF] = 0;
                            I += V[opcode.gb(2)];
                            PC += 2;
                            break;
                        case 0x0029:
                            I = (ushort)(V[opcode.gb(2)] * 0x5);
                            PC += 2;
                            break;
                        case 0x0033:
                            mmu[I] = (byte)(V[opcode.gb(2)] / 100);
                            mmu[I + 1] = (byte)(V[opcode.gb(2)] / 10 % 10);
                            mmu[I + 2] = (byte)(V[opcode.gb(2)] % 100 % 10);
                            PC += 2;
                            break;
                        case 0x0055:
                            for (int i = 0; i <= opcode.gb(2); i++)
                                mmu[I + i] = V[i];

                            // On the original interpreter, when the operation is done, I = I + X + 1.
                            I += (ushort)(opcode.gb(2) + 1);
                            PC += 2;
                            break;
                        case 0x0065:
                            for (int i = 0; i <= opcode.gb(2); i++)
                                V[i] = mmu[I + i];

                            // On the original interpreter, when the operation is done, I = I + X + 1.
                            I += (ushort)(opcode.gb(2) + 1);
                            PC += 2;
                            break;
                    }
                    break;
            }

            // Update timers
            if (delay_timer > 0) delay_timer--;
            if (sound_timer == 1) Console.Beep();
            if (sound_timer > 0) sound_timer--;
        }
    }
    public static class Extensions
    {
        public static byte gb(this ushort value, int pos)
        {
            int mask = 0x000F << (pos * 4);
            return (byte)((value & mask) >> (pos * 4));
        }
    }

}
