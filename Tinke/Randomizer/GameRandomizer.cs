using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ekona;
using System.IO;
using Tinke;

namespace Tinke.Randomizer
{
    abstract class GameRandomizer
    {
        //general parameters, shared by all roms
        sFolder gameRoot;
        Random rng;
        string seed;
        Acciones accion;

        //these parameters shall be defined per rom
        byte[] puzzleIDs;
        protected string puzzlePath = "";
        protected byte puzzleSeparatorByte = 0xBA;
        protected string eventScriptFolder = "";
        protected string roomScriptFolder = "";
        protected string riddleHutScriptFolder = "";
        protected string riddleHutScriptName = "";
        protected byte riddleCheckFlagByte = 0xE7;

        public GameRandomizer(sFolder root, string seed, Acciones accion)
        {
            this.seed = seed;
            this.rng = new Random(seed.GetHashCode());
            this.gameRoot = root;
            this.accion = accion;
        }

        public void seedPuzzles(){
            sFolder puzzleFolder = this.findFolder(this.puzzlePath);
            sFile qinfo = this.findFile(puzzleFolder, "qtitle.gds");

            int puzzleCounter = 0;

            string hexFile = Path.GetTempFileName();
            BinaryReader binr = new BinaryReader(File.OpenRead(qinfo.path));
            binr.BaseStream.Position = qinfo.offset;

            byte[] fileBytes = binr.ReadBytes((int)qinfo.size);

            for (int b = 0; b < fileBytes.Length; b++) {
                if (fileBytes[b] == this.puzzleSeparatorByte) {
                    puzzleCounter++;
                }
            }

            this.puzzleIDs = new byte[puzzleCounter];
            for (int index = 0; index < puzzleCounter; index++) {
                puzzleIDs[index] = (byte)(index + 1);
            }

            this.puzzleIDs = this.Shuffle(puzzleIDs);
            binr.Close();
        }

        protected sFile findFile(sFolder folder, string name)
        {
            foreach (sFile tmpFile in folder.files) {
                if (tmpFile.name == name) {
                    return tmpFile;
                }
            }

            return new sFile();
        }

        /*
         * Given an absolute folder path, from root, return pointer to said folder.
         * Throws RandomizerException if not found
         */
        protected sFolder findFolder(string path) {
            sFolder tmp = this.gameRoot;

            if(path.StartsWith("/")){
                path = path.Substring(1);
            }
            string[] pathArray = path.Split('/');

            int folderPathIndex = 0;
            while (folderPathIndex < pathArray.Length)
            {
                bool found = false;
                foreach (sFolder folder in tmp.folders)
                {
                    if (folder.name == pathArray[folderPathIndex])
                    {
                        tmp = folder;
                        folderPathIndex++;
                        found = true;
                        break;
                    }
                }

                if (!found) return new sFolder();
            }

            return tmp;
        }

        protected T[] Shuffle<T>(T[] array)
        {
            int n = array.Length;
            while (n > 1)
            {
                int k = this.rng.Next(n--);
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }

            return array;
        }

        /**
         * Writes rando data to file system
         */
        public void Write()
        {
            //write event scripts
            sFolder eventRoot = this.findFolder(this.eventScriptFolder);
            for (int fileIndex = 0; fileIndex < eventRoot.files.Count; fileIndex++){
                sFile file = eventRoot.files[fileIndex];
                byte[] puzzleMask = { 0x48, 0x00, 0x01, 0x00 };
                byte[] riddleCheckMask = { this.riddleCheckFlagByte, 0x00, 0x01, 0x00 };

                string hexFile = Path.GetTempFileName();
                BinaryReader binr = new BinaryReader(File.OpenRead(file.path));
                binr.BaseStream.Position = file.offset;

                byte[] fileBytes = binr.ReadBytes((int)file.size);

                for (int b = 0; b < fileBytes.Length - puzzleMask.Length; b++)
                {
                    bool foundSequence = true;

                    for (int c = 0; c < puzzleMask.Length; c++)
                    {
                        if (fileBytes[b + c] != puzzleMask[c])
                        {
                            foundSequence = false;
                        }
                    }

                    if (!foundSequence) continue;

                    int id_to_replace = fileBytes[b + puzzleMask.Length] - 1;

                    //prevent puzzle 001
                    if (this.puzzleIDs[id_to_replace] == 0x24) continue;

                    fileBytes[b + puzzleMask.Length] = this.puzzleIDs[id_to_replace];
                }

                for (int b = 0; b < fileBytes.Length - puzzleMask.Length; b++)
                {
                    bool foundSequence = true;

                    for (int c = 0; c < riddleCheckMask.Length; c++)
                    {
                        if (fileBytes[b + c] != riddleCheckMask[c])
                        {
                            foundSequence = false;
                        }
                    }

                    if (!foundSequence) continue;

                    int id_to_replace = fileBytes[b + riddleCheckMask.Length] - 1;

                    //prevent puzzle 001
                    if (this.puzzleIDs[id_to_replace] == 0x24) continue;

                    fileBytes[b + puzzleMask.Length] = this.puzzleIDs[id_to_replace];
                }

                File.WriteAllBytes(hexFile, fileBytes);
                binr.Close();

                this.accion.Change_File(file.id, hexFile);
            }

            //write room scripts
            sFolder roomsRoot = this.findFolder(this.roomScriptFolder);
            for (int fileIndex = 0; fileIndex < roomsRoot.files.Count; fileIndex++)
            {
                sFile file = roomsRoot.files[fileIndex];

                byte[] puzzleMask = { 0x48, 0x00, 0x01, 0x00 };

                string hexFile = Path.GetTempFileName();
                BinaryReader binr = new BinaryReader(File.OpenRead(file.path));
                binr.BaseStream.Position = file.offset;

                byte[] fileBytes = binr.ReadBytes((int)file.size);

                for (int b = 0; b < fileBytes.Length - puzzleMask.Length; b++)
                {
                    bool foundSequence = true;

                    for (int c = 0; c < puzzleMask.Length; c++)
                    {
                        if (fileBytes[b + c] != puzzleMask[c])
                        {
                            foundSequence = false;
                        }
                    }

                    if (!foundSequence) continue;

                    int id_to_replace = fileBytes[b + puzzleMask.Length] - 1;

                    //prevent puzzle 001
                    if (this.puzzleIDs[id_to_replace] == 0x24) continue;

                    fileBytes[b + puzzleMask.Length] = this.puzzleIDs[id_to_replace];
                }

                File.WriteAllBytes(hexFile, fileBytes);
                binr.Close();

                this.accion.Change_File(file.id, hexFile);
            }

            return;
        }
    }
}
