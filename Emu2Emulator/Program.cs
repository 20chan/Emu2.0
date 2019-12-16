using System;
using System.IO;

namespace Emu2Emulator {
    class Program {
        static void Main(string[] args) {
            var code = File.ReadAllBytes("rom");
            var emu = new Emu2(code);
            emu.SerialOut += b => Console.Write($"{(char)b}");

            while (true) {
                emu.Step();
            }
        }
    }
}
