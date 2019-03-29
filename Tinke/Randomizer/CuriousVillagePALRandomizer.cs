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
        public CuriousVillagePALRandomizer(sFolder root, string seed, Acciones accion) : base(root, seed, accion) {
            this.puzzlePath = "data/script/puzzletitle/en";
            this.eventScriptFolder = "data/script/event";
            this.roomScriptFolder = "data/script/rooms";
            this.riddleHutScriptFolder = "data/script/nazobaba";
            this.riddleHutScriptName = "babascript.gds";
            this.riddleCheckFlagByte = 0xE7;
            this.puzzleExecuteByte = 0x48;
            this.puzzleSeparatorByte = 0xBA;
        }
    }
}
