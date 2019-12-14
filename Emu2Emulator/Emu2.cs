using System;
using System.Collections.Generic;
using System.Text;

namespace Emu2Emulator {
    public class Emu2 {
        public byte A;
        public short PC;

        public byte[] Memory;
        public bool[] Writables;

        public readonly byte[] Code;

        public event Action<byte> SerialOut;

        public byte C0 => Code[PC];
        public byte C1 => Code[PC + 1];

        public short C0111 => (short)((C0 & 0xf) * 0x100 + C1);
         
        public Emu2(byte[] code) {
            A = 0;
            PC = 0x100;
            Memory = new byte[0x1000];
            Writables = new bool[0x1000];
            for (var i = 0; i <Writables.Length; i++) {
                Writables[i] = true;
            }

            Code = code;
        }

        public void Step() {
            var manualPC = false;
            if (C0 == 0x00) {
                A += C1;
            }
            else if (C0 == 0x01) {
                A = C1;
            }
            else if (C0 == 0x02) {
                A ^= C1;
            }
            else if (C0 == 0x03) {
                A |= C1;
            }
            else if (C0 == 0x04) {
                A &= C1;
            }
            else if ((C0 & 0xf0) == 0x80) {
                A = Memory[C0111];
            }
            else if ((C0 & 0xf0) == 0xd0) {
                if (Writables[C0111]) {
                    Memory[C0111] ^= A;
                }
            }
            else if ((C0 & 0xf0) == 0xf0) {
                if (Writables[C0111]) {
                    Memory[C0111] = A;
                }
            }
            else if (C0 == 0x13 && C1 == 0x37) {
                SerialOut?.Invoke(A);
            }
            else if ((C0 & 0xf0) == 0x20) {
                PC = C0111;
                manualPC = true;
            }
            else if ((C0 & 0xf0) == 0x30) {
                if (A == 0) {
                    PC = C0111;
                    manualPC = true;
                }
            }
            else if ((C0 & 0xf0) == 0x40) {
                if (A == 1) {
                    PC = C0111;
                    manualPC = true;
                }
            }
            else if ((C0 & 0xf0) == 0x50) {
                if (A == 255) {
                    PC = C0111;
                    manualPC = true;
                }
            }
            else if (C0 == 0x60) {
                A = Compare(A, C1);
            }
            else if ((C0 & 0xf0) == 0x70) {
                A = Compare(A, Memory[C0111]);
            }
            else if (C0 == 0xbe && C1 == 0xef) {
                PC = 0x100;
                A = 0x42;
                manualPC = true;
            }
            else if ((C0 & 0xf0) == 0x90) {
                Writables[C0111] = false;
            }
            else if ((C0 & 0xf0) == 0xa0) {
                Writables[C0111] = true;
            }
            else if ((C0 & 0xf0) == 0xc0) {
                if (Writables[C0111]) {
                    Memory[C0111] ^= 0x42;
                }
            }
            else if (C0 == 0xee && C1 == 0xee) {
                // nop
            }
            else {
                A--;
            }

            if (!manualPC) {
                PC += 2;
            }
        }

        private static byte Compare(int a, int b) {
            if (a == b) return 0;
            if (a < b) return 1;
            else return 255;
        }
    }
}
