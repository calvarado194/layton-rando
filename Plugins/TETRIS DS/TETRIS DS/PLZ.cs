﻿/*
 * Copyright (C) 2011  pleoNeX
 *
 *   This program is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.
 *
 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU General Public License for more details.
 *
 *   You should have received a copy of the GNU General Public License
 *   along with this program.  If not, see <http://www.gnu.org/licenses/>. 
 *
 * By: pleoNeX
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using Ekona;
using Ekona.Images;

namespace TETRIS_DS
{
    public class PLZ : PaletteBase
    {
        IPluginHost pluginHost;

        public PLZ(IPluginHost pluginHost, string file, int id) : base(file, id) { this.pluginHost = pluginHost; }

        public override void Read(string file)
        {
            // Decompressed the file
            pluginHost.Decompress(file);
            string dec_file;
            sFolder dec_folder = pluginHost.Get_Files();

            if (dec_folder.files is List<sFile>)
                dec_file = dec_folder.files[0].path;
            else
            {
                string tempFile = Path.GetTempFileName();
                Byte[] compressFile = new Byte[(new FileInfo(file).Length) - 0x08];
                Array.Copy(File.ReadAllBytes(file), 0x08, compressFile, 0, compressFile.Length); ;
                File.WriteAllBytes(tempFile, compressFile);

                pluginHost.Decompress(tempFile);
                dec_file = pluginHost.Get_Files().files[0].path;
            }

            BinaryReader br = new BinaryReader(File.OpenRead(dec_file));

            ColorFormat depth = (br.BaseStream.Length > 0x20) ? ColorFormat.colors256 : ColorFormat.colors16;
            Color[][] colors = new Color[1][];
            colors[0] = Actions.BGR555ToColor(br.ReadBytes((int)br.BaseStream.Length));

            br.Close();
            Set_Palette(colors, depth, false);
            pluginHost.Set_Palette(this);
        }
        public override void Write(string fileOut)
        {
            throw new NotImplementedException();
        }
    }
}
