using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ekona;
using Tinke.Randomizer;

namespace Tinke.Randomizer
{
    class CuriousVillagePALRandomizer : GameRandomizer
    {
        private int finalPuzzleCheckAmount = 75;
        private int maxPuzzleCheckAmount = 113;
        public CuriousVillagePALRandomizer(sFolder root, string seed, Acciones accion)
            : base(root, seed, accion)
        {
            this.puzzlePath = "data/script/puzzletitle/en";
            this.eventScriptFolder = "data/script/event";
            this.roomScriptFolder = "data/script/rooms";
            this.riddleHutScriptFolder = "data/script/nazobaba";
            this.riddleHutScriptName = "babascript.gds";
            this.riddleCheckFlagByte = 0xE7;
            this.puzzleExecuteByte = 0x48;
            this.puzzleSeparatorByte = 0xBA;
        }

        protected override byte[] applyGameSpecificSettings(byte[] fileBytes, string filename)
        {
            byte[] puzzleCheckMask = { 0x77, 0x00, 0x01, 0x00 };

            if (filename.StartsWith("e"))
            {
                for (int b = 0; b < fileBytes.Length - puzzleCheckMask.Length; b++)
                {
                    bool foundSequence = true;

                    for (int c = 0; c < puzzleCheckMask.Length; c++)
                    {
                        if (fileBytes[b + c] != puzzleCheckMask[c])
                        {
                            foundSequence = false;
                        }
                    }

                    if (!foundSequence) continue;

                    if (this.removePuzzleCounterChecks)
                    {
                        if (fileBytes[b + puzzleCheckMask.Length] != (byte)(this.finalPuzzleCheckAmount))
                        {
                            fileBytes[b + puzzleCheckMask.Length] = 0x01;
                        }
                    }
                    if (this.enforceMaxPuzzles) {
                        if (fileBytes[b + puzzleCheckMask.Length] == (byte)(this.finalPuzzleCheckAmount))
                        {
                            fileBytes[b + puzzleCheckMask.Length] = (byte)(this.maxPuzzleCheckAmount);
                        }
                    }
                }
            }

            return fileBytes;
        }
    }
}
