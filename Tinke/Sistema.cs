/*
 * Copyright (C) 2011  pleoNeX, ikuyo
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
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using Ekona;
using Tinke.Randomizer;

namespace Tinke
{
    public partial class Sistema : Form
    {
        RomInfo romInfo;
        Debug debug;

        Acciones accion;
        StringBuilder sb;
        int filesSupported;
        int nFiles;
        bool isMono;
        Keys keyDown;
        bool stop;
        private string[] allowedGameCodes = {
            "A5FP" // Curious Village - PAL
        };

        public Sistema()
        {
            InitializeComponent();
            this.Text = "Professor Layton Randomizer - ";

            // The IE control of the Debug windows doesn't work in Mono
            isMono = (Type.GetType("Mono.Runtime") != null) ? true : false;

            sb = new StringBuilder();
            TextWriter tw = new StringWriter(sb);
            tw.NewLine = "<br>";
            if (!isMono)
                Console.SetOut(tw);

            #region Language
            if (!File.Exists(Application.StartupPath + Path.DirectorySeparatorChar + "Tinke.xml"))
            {
                File.WriteAllText(Application.StartupPath + Path.DirectorySeparatorChar + "Tinke.xml", "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                                 "\n<Tinke>\n  <Options>" +
                                 "\n    <Language>English</Language>" +
                                 "\n    <InstantSearch>True</InstantSearch>" +
                                 "\n    <WindowDebug>True</WindowDebug>" +
                                 "\n    <WindowInformation>True</WindowInformation>" +
                                 "\n    <ModeWindow>False</ModeWindow>" +
                                 "\n  </Options>\n</Tinke>",
                                 Encoding.UTF8);
            }

            foreach (string langFile in Directory.GetFiles(Application.StartupPath + Path.DirectorySeparatorChar + "langs"))
            {
                if (!langFile.EndsWith(".xml"))
                    continue; ;

                string flag = Application.StartupPath + Path.DirectorySeparatorChar + "langs" + Path.DirectorySeparatorChar + langFile.Substring(langFile.Length - 9, 5) + ".png";
                Image iFlag;
                if (File.Exists(flag))
                    iFlag = Image.FromFile(flag);
                else
                    iFlag = iconos.Images[1];

                XElement xLang = XElement.Load(langFile);
                if (xLang.Name != "Language")
                    continue;

            }

            #endregion
            this.Load += new EventHandler(Sistema_Load);
            keyDown = Keys.Escape;
        }
        void Sistema_Load(object sender, EventArgs e)
        {
            string[] filesToRead = new string[1];
            if (Environment.GetCommandLineArgs().Length == 1)
            {
                OpenFileDialog o = new OpenFileDialog();
                o.CheckFileExists = true;
                o.Multiselect = true;

                if (o.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    Application.Exit();
                    return;
                }
                filesToRead = o.FileNames;
                o.Dispose();
            }
            else if (Environment.GetCommandLineArgs().Length == 2)
            {
                if (Environment.GetCommandLineArgs()[1] == "-fld")
                {
                    FolderBrowserDialog o = new FolderBrowserDialog();
                    o.ShowNewFolderButton = false;
                    if (o.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    {
                        Application.Exit();
                        return;
                    }
                    filesToRead[0] = o.SelectedPath;
                    o.Dispose();
                }
                else
                    filesToRead[0] = Environment.GetCommandLineArgs()[1];
            }
            else if (Environment.GetCommandLineArgs().Length >= 3)
            {
                filesToRead = new String[Environment.GetCommandLineArgs().Length - 1];
                Array.Copy(Environment.GetCommandLineArgs(), 1, filesToRead, 0, filesToRead.Length);
            }

            Thread espera = new System.Threading.Thread(ThreadEspera);
            if (!isMono)
                espera.Start("S02");

            if (filesToRead.Length == 1 &&
                (Path.GetFileName(filesToRead[0]).ToUpper().EndsWith(".NDS") || Path.GetFileName(filesToRead[0]).ToUpper().EndsWith(".SRL")))
                ReadGame(filesToRead[0]);
            else if (filesToRead.Length == 1 && Directory.Exists(filesToRead[0]))
                ReadFolder(filesToRead[0]);
            else
                ReadFiles(filesToRead);

            if (!isMono)
            {
                espera.Abort();

            }
            sb.Length = 0;

            if(Array.IndexOf(allowedGameCodes, new string(romInfo.Cabecera.gameCode)) < 0){
                MessageBox.Show("Your ROM is not a valid Layton game ROM or it's not supported yet", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }

            this.Show();
            this.Activate();
        }
        private void Sistema_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                XElement xml = XElement.Load(Application.StartupPath + Path.DirectorySeparatorChar + "Tinke.xml").Element("Options");


                xml = xml.Parent;
                xml.Save(Application.StartupPath + Path.DirectorySeparatorChar + "Tinke.xml");
            }
            catch { MessageBox.Show(Tools.Helper.GetTranslation("Sistema", "S37"), Tools.Helper.GetTranslation("Sistema", "S3A")); }

            if (accion is Acciones)
            {
                if (accion.IsNewRom & accion.ROMFile != "")
                {
                    if (MessageBox.Show(Tools.Helper.GetTranslation("Sistema", "S39"), Tools.Helper.GetTranslation("Sistema", "S3A"),
                        MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.Cancel)
                    {
                        e.Cancel = true;
                        return;
                    }
                }
                accion.Dispose();
            }
        }

        private void ReadGame(string file)
        {
            DateTime startTime = DateTime.Now;

            romInfo = new RomInfo(file);  // Read the header and banner
            DateTime t1 = DateTime.Now;
            accion = new Acciones(file, new String(romInfo.Cabecera.gameCode));
            DateTime t2 = DateTime.Now;

            // Read File Allocation Table (offset and size)
            Nitro.Estructuras.sFAT[] fat = Nitro.FAT.ReadFAT(file, romInfo.Cabecera.FAToffset, romInfo.Cabecera.FATsize);
            DateTime t3 = DateTime.Now;

            // Read the File Name Table and get the directory hierarchy
            sFolder root = Nitro.FNT.ReadFNT(file, romInfo.Cabecera.fileNameTableOffset, fat, accion);
            DateTime t4 = DateTime.Now;

            accion.LastFileID = fat.Length;
            accion.LastFolderID = root.id + 0xF000;
            root.id = 0xF000;

            // Add system files (fnt.bin, banner.bin, overlays, arm9 and arm7)
            if (!(root.folders is List<sFolder>))
                root.folders = new List<sFolder>();
            root.folders.Add(Add_SystemFiles(fat));
            DateTime t5 = DateTime.Now;

            accion.Root = root;
            accion.SortedIDs = Nitro.FAT.SortByOffset(fat);
            DateTime t6 = DateTime.Now;

            Stream stream = File.OpenRead(file);
            stream.Close();
            stream.Dispose();

            DateTime t7 = DateTime.Now;

            Get_SupportedFiles();
            DateTime t8 = DateTime.Now;

            XElement xml = Tools.Helper.GetTranslation("Messages");
            Console.Write("<br><u>" + xml.Element("S0F").Value + "</u><font size=\"2\" face=\"consolas\"><ul>");
            Console.WriteLine("<li>" + xml.Element("S10").Value + (t8 - startTime).ToString() + "</li>");
            Console.WriteLine("<li>" + xml.Element("S11").Value + (t1 - startTime).ToString() + "</li>");
            Console.WriteLine("<li>" + xml.Element("S12").Value + (t2 - t1).ToString() + "</li>");
            Console.WriteLine("<li>" + xml.Element("S15").Value + (t3 - t2).ToString() + "</li>");
            Console.WriteLine("<li>" + xml.Element("S13").Value + (t4 - t3).ToString() + "</li>");
            Console.WriteLine("<li>" + xml.Element("S14").Value + (t5 - t4).ToString() + "</li>");
            Console.WriteLine("<li>" + xml.Element("S16").Value + (t6 - t5).ToString() + "</li>");
            Console.WriteLine("<li>" + xml.Element("S18").Value + (t7 - t6).ToString() + "</li>");
            Console.WriteLine("<li>" + xml.Element("S19").Value + (t8 - t7).ToString() + "</li>");
            Console.WriteLine("</ul>");
            Console.WriteLine("Number of directories: {0}", accion.LastFolderID - 0xF000 - 1);
            Console.WriteLine("Number of files: {0}</font>", fat.Length);

            this.Text += new String(romInfo.Cabecera.gameTitle).Replace("\0", "") +
                " (" + new String(romInfo.Cabecera.gameCode) + ')';
        }
        private sFolder Add_SystemFiles(Nitro.Estructuras.sFAT[] fatTable)
        {
            sFolder ftc = new sFolder();
            ftc.name = "ftc";
            ftc.id = (ushort)accion.LastFolderID;
            accion.LastFolderID++;
            ftc.files = new List<sFile>();
            ftc.files.AddRange(Nitro.Overlay.ReadBasicOverlays(
                accion.ROMFile, romInfo.Cabecera.ARM9overlayOffset, romInfo.Cabecera.ARM9overlaySize, true, fatTable));
            ftc.files.AddRange(Nitro.Overlay.ReadBasicOverlays(
                accion.ROMFile, romInfo.Cabecera.ARM7overlayOffset, romInfo.Cabecera.ARM7overlaySize, false, fatTable));

            sFile rom = new sFile();
            rom.name = "rom.nds";
            rom.offset = 0x00;
            rom.size = (uint)new FileInfo(accion.ROMFile).Length;
            rom.path = accion.ROMFile;
            rom.id = (ushort)accion.LastFileID;
            accion.LastFileID++;
            ftc.files.Add(rom);

            sFile fnt = new sFile();
            fnt.name = "fnt.bin";
            fnt.offset = romInfo.Cabecera.fileNameTableOffset;
            fnt.size = romInfo.Cabecera.fileNameTableSize;
            fnt.path = accion.ROMFile;
            fnt.id = (ushort)accion.LastFileID;
            accion.LastFileID++;
            ftc.files.Add(fnt);

            sFile fat = new sFile();
            fat.name = "fat.bin";
            fat.offset = romInfo.Cabecera.FAToffset;
            fat.size = romInfo.Cabecera.FATsize;
            fat.path = accion.ROMFile;
            fat.id = (ushort)accion.LastFileID;
            accion.LastFileID++;
            ftc.files.Add(fat);

            sFile banner = new sFile();
            banner.name = "banner.bin";
            banner.offset = romInfo.Cabecera.bannerOffset;
            banner.size = 0x840;
            banner.path = accion.ROMFile;
            banner.id = (ushort)accion.LastFileID;
            accion.LastFileID++;
            ftc.files.Add(banner);

            sFile arm9 = new sFile();
            arm9.name = "arm9.bin";
            arm9.offset = romInfo.Cabecera.ARM9romOffset;
            arm9.size = romInfo.Cabecera.ARM9size;
            arm9.path = accion.ROMFile;
            arm9.id = (ushort)accion.LastFileID;
            accion.LastFileID++;
            ftc.files.Add(arm9);

            sFile arm7 = new sFile();
            arm7.name = "arm7.bin";
            arm7.offset = romInfo.Cabecera.ARM7romOffset;
            arm7.size = romInfo.Cabecera.ARM7size;
            arm7.path = accion.ROMFile;
            arm7.id = (ushort)accion.LastFileID;
            accion.LastFileID++;
            ftc.files.Add(arm7);

            if (romInfo.Cabecera.ARM9overlaySize != 0)
            {
                sFile y9 = new sFile();
                y9.name = "y9.bin";
                y9.offset = romInfo.Cabecera.ARM9overlayOffset;
                y9.size = romInfo.Cabecera.ARM9overlaySize;
                y9.path = accion.ROMFile;
                y9.id = (ushort)accion.LastFileID;
                accion.LastFileID++;
                ftc.files.Add(y9);
            }

            if (romInfo.Cabecera.ARM7overlaySize != 0)
            {
                sFile y7 = new sFile();
                y7.name = "y7.bin";
                y7.offset = romInfo.Cabecera.ARM7overlayOffset;
                y7.size = romInfo.Cabecera.ARM7overlaySize;
                y7.path = accion.ROMFile;
                y7.id = (ushort)accion.LastFileID;
                accion.LastFileID++;
                ftc.files.Add(y7);
            }

            Set_Format(ftc);
            return ftc;
        }
        private void ReadFiles(string[] files)
        {
            btnSaveROM.Enabled = false;

            romInfo = new RomInfo(); // Para que no se formen errores...
            DateTime startTime = DateTime.Now;

            accion = new Acciones("", "NO GAME");
            DateTime t1 = DateTime.Now;

            accion.LastFileID = files.Length;
            accion.LastFolderID = 0xF000;

            // Obtenemos el sistema de archivos
            sFolder root = new sFolder();
            root.name = "root";
            root.id = 0xF000;
            root.files = new List<sFile>();
            for (int i = 0; i < files.Length; i++)
            {
                sFile currFile = new sFile();
                currFile.id = (ushort)i;
                currFile.name = Path.GetFileName(files[i]);
                currFile.offset = 0x00;
                currFile.path = files[i];

                currFile.size = (uint)new FileInfo(files[i]).Length;
                root.files.Add(currFile);
            }
            DateTime t2 = DateTime.Now;

            accion.Root = root;
            DateTime t3 = DateTime.Now;

            Set_Format(root);
            DateTime t4 = DateTime.Now;
            DateTime t5 = DateTime.Now;

            Get_SupportedFiles();
            DateTime t6 = DateTime.Now;

            XElement xml = Tools.Helper.GetTranslation("Messages");
            Console.Write("<br><u>" + xml.Element("S0F").Value + "</u><ul><font size=\"2\" face=\"consolas\">");
            Console.WriteLine("<li>" + xml.Element("S10").Value + (t6 - startTime).ToString() + "</li>");
            Console.WriteLine("<li>" + xml.Element("S12").Value + (t1 - startTime).ToString() + "</li>");
            Console.WriteLine("<li>" + xml.Element("S21").Value + (t2 - t1).ToString() + "</li>");
            Console.WriteLine("<li>" + xml.Element("S16").Value + (t3 - t2).ToString() + "</li>");
            Console.WriteLine("<li>" + xml.Element("S17").Value + (t4 - t3).ToString() + "</li>");
            Console.WriteLine("<li>" + xml.Element("S18").Value + (t5 - t4).ToString() + "</li>");
            Console.WriteLine("<li>" + xml.Element("S19").Value + (t6 - t5).ToString() + "</li>");
            Console.Write("</font></ul><br>");
        }
        private void ReadFolder(string folder)
        {
            btnSaveROM.Enabled = false;

            romInfo = new RomInfo(); // Para que no se formen errores...
            DateTime startTime = DateTime.Now;

            accion = new Acciones("", "NO GAME");
            DateTime t1 = DateTime.Now;

            accion.LastFileID = 0;
            accion.LastFolderID = 0xF000;

            // Obtenemos el sistema de archivos
            sFolder root = new sFolder();
            root.name = "root";
            root.id = 0xF000;
            accion.LastFileID = 0x00;
            accion.LastFolderID = 0xF000;
            root = accion.Recursive_GetExternalDirectories(folder, root);
            DateTime t2 = DateTime.Now;

            accion.Root = root;
            DateTime t3 = DateTime.Now;

            Set_Format(root);
            DateTime t4 = DateTime.Now;
            DateTime t5 = DateTime.Now;

            Get_SupportedFiles();
            DateTime t6 = DateTime.Now;

            XElement xml = Tools.Helper.GetTranslation("Messages");
            Console.Write("<br><u>" + xml.Element("S0F").Value + "</u><ul><font size=\"2\" face=\"consolas\">");
            Console.WriteLine("<li>" + xml.Element("S10").Value + (t6 - startTime).ToString() + "</li>");
            Console.WriteLine("<li>" + xml.Element("S12").Value + (t1 - startTime).ToString() + "</li>");
            Console.WriteLine("<li>" + xml.Element("S21").Value + (t2 - t1).ToString() + "</li>");
            Console.WriteLine("<li>" + xml.Element("S16").Value + (t3 - t2).ToString() + "</li>");
            Console.WriteLine("<li>" + xml.Element("S17").Value + (t4 - t3).ToString() + "</li>");
            Console.WriteLine("<li>" + xml.Element("S18").Value + (t5 - t4).ToString() + "</li>");
            Console.WriteLine("<li>" + xml.Element("S19").Value + (t6 - t5).ToString() + "</li>");
            Console.Write("</font></ul><br>");
        }

        private void LoadPreferences()
        {
            try
            {
                XElement xml = XElement.Load(Application.StartupPath + Path.DirectorySeparatorChar + "Tinke.xml").Element("Options");

                if (!isMono && xml.Element("WindowDebug").Value == "True")
                {
                    debug.Show();
                    debug.Activate();
                }
                if (xml.Element("WindowInformation").Value == "True" && accion.ROMFile != "") // En caso de que se haya abierto una ROM, no archivos sueltos
                {
                    romInfo.Show();
                    romInfo.Activate();
                }
            }
            catch { MessageBox.Show(Tools.Helper.GetTranslation("Sistema", "S38"), Tools.Helper.GetTranslation("Sistema", "S3A")); }
        }


        private TreeNode Create_Nodes(sFolder currFolder)
        {
            TreeNode currNode = new TreeNode();

            if (currFolder.id < 0xF000) // Archivo descomprimido
            {
                int imageIndex = accion.ImageFormatFile(accion.Get_Format(currFolder.id));
                currNode = new TreeNode(currFolder.name, imageIndex, imageIndex);
            }
            else
                currNode = new TreeNode(currFolder.name, 0, 0);
            currNode.Tag = currFolder.id;
            currNode.Name = currFolder.name;


            if (currFolder.folders is List<sFolder>)
                foreach (sFolder subFolder in currFolder.folders)
                    currNode.Nodes.Add(Create_Nodes(subFolder));


            if (currFolder.files is List<sFile>)
            {
                foreach (sFile archivo in currFolder.files)
                {
                    int nImage = accion.ImageFormatFile(archivo.format);
                    string ext = "";

                    if (archivo.format == Format.Unknown)
                    {
                        ext = accion.Get_MagicIDS(archivo);
                        if (ext != "")
                            ext = " [" + ext + ']';
                    }
                    TreeNode fileNode = new TreeNode(archivo.name + ext, nImage, nImage);
                    fileNode.Name = archivo.name;
                    fileNode.Tag = archivo.id;
                    currNode.Nodes.Add(fileNode);
                }
            }

            return currNode;
        }
        private TreeNode Create_Nodes(sFolder currFolder, Stream stream)
        {
            TreeNode currNode = new TreeNode();

            if (currFolder.id < 0xF000) // Archivo descomprimido
            {
                int imageIndex = accion.ImageFormatFile(accion.Get_Format(currFolder.id));
                currNode = new TreeNode(currFolder.name, imageIndex, imageIndex);
            }
            else
                currNode = new TreeNode(currFolder.name, 0, 0);
            currNode.Tag = currFolder.id;
            currNode.Name = currFolder.name;


            if (currFolder.folders is List<sFolder>)
                foreach (sFolder subFolder in currFolder.folders)
                    currNode.Nodes.Add(Create_Nodes(subFolder, stream));


            if (currFolder.files is List<sFile>)
            {
                foreach (sFile archivo in currFolder.files)
                {
                    int nImage = accion.ImageFormatFile(archivo.format);
                    string ext = "";

                    if (archivo.format == Format.Unknown)
                    {
                        stream.Position = archivo.offset;
                        ext = accion.Get_MagicIDS(stream, archivo.size);
                        if (ext != "")
                            ext = " [" + ext + ']';
                    }
                    TreeNode fileNode = new TreeNode(archivo.name + ext, nImage, nImage);
                    fileNode.Name = archivo.name;
                    fileNode.Tag = archivo.id;
                    currNode.Nodes.Add(fileNode);
                }
            }

            return currNode;
        }
        private void FolderToNode(sFolder folder, ref TreeNode node)
        {
            if (folder.id < 0xF000)
            {
                node.ImageIndex = accion.ImageFormatFile(accion.Get_Format(folder.id));
                node.SelectedImageIndex = node.ImageIndex;
            }
            else
            {
                node.ImageIndex = 0;
                node.SelectedImageIndex = 0;
            }
            node.Tag = folder.id;
            node.Name = folder.name;

            if (folder.folders is List<sFolder>)
            {
                foreach (sFolder subFolder in folder.folders)
                {
                    TreeNode newNodo = new TreeNode(subFolder.name);
                    FolderToNode(subFolder, ref newNodo);
                    node.Nodes.Add(newNodo);
                }
            }


            if (folder.files is List<sFile>)
            {
                foreach (sFile archivo in folder.files)
                {
                    int nImage = accion.ImageFormatFile(archivo.format);
                    string ext = "";
                    if (archivo.format == Format.Unknown)
                    {
                        ext = accion.Get_MagicIDS(archivo);
                        if (ext != "")
                            ext = " [" + ext + ']'; // Previene extensiones vacías
                    }
                    TreeNode fileNode = new TreeNode(archivo.name + ext, nImage, nImage);
                    fileNode.Name = archivo.name;
                    fileNode.Tag = archivo.id;
                    node.Nodes.Add(fileNode);
                }
            }


        }
        private TreeNode[] FilesToNodes(sFile[] files)
        {
            TreeNode[] nodos = new TreeNode[files.Length];

            for (int i = 0; i < files.Length; i++)
            {
                int nImage = accion.ImageFormatFile(files[i].format);
                string ext = "";
                if (files[i].format == Format.Unknown)
                {
                    ext = new String(Encoding.ASCII.GetChars(accion.Get_MagicID(files[i].path)));
                    if (ext != "")
                        ext = " [" + ext + ']';
                }
                nodos[i] = new TreeNode(files[i].name + ext, nImage, nImage);
                nodos[i].Name = files[i].name;
                nodos[i].Tag = files[i].id;
            }

            return nodos;
        }

        private void Set_Format(sFolder folder)
        {
            if (folder.files is List<sFile>)
            {
                for (int i = 0; i < folder.files.Count; i++)
                {
                    sFile newFile = folder.files[i];
                    newFile.format = accion.Get_Format(newFile);
                    folder.files[i] = newFile;
                }
            }


            if (folder.folders is List<sFolder>)
                foreach (sFolder subFolder in folder.folders)
                    Set_Format(subFolder);
        }
        private void Get_SupportedFiles()
        {
            filesSupported = nFiles = 0; // Reiniciamos el contador

            Recursive_SupportedFiles(accion.Root);
            if (nFiles == 0)
                nFiles = 1;

        }

        private void Sistema_KeyDown() { }

        private void Sistema_KeyUp() { }

        private void Recursive_SupportedFiles(sFolder folder)
        {
            if (folder.files is List<sFile>)
            {
                foreach (sFile archivo in folder.files)
                {
                    if (archivo.format == Format.System || archivo.size == 0x00)
                        continue;

                    if (archivo.format != Format.Unknown)
                        filesSupported++;
                    nFiles++;
                }
            }

            if (folder.folders is List<sFolder>)
                foreach (sFolder subFolder in folder.folders)
                    Recursive_SupportedFiles(subFolder);
        }

        private void ThreadEspera(Object name)
        {
            Espera espera = new Espera((string)name, false);

            try
            {
                espera.ShowDialog();
            }
            catch
            {
            }
        }

        private void treeSystem_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (stop)
                return;
        }

        #region Buttons
        private void btnHex_Click(object sender, EventArgs e)
        {
            sFile file = accion.Selected_File();
            string filePath = accion.Save_File(file);
            Form hex;

            if (!isMono) {
                hex = new VisorHex(filePath, file.id, file.name != "rom.nds");
                hex.FormClosed += hex_FormClosed;
            } else {
                hex = new VisorHexBasic(filePath, 0, file.size);
            }

            hex.Text += " - " + file.name;
            hex.Show();
        }
        void hex_FormClosed(object sender, FormClosedEventArgs e)
        {
            VisorHex hex = sender as VisorHex;
            if (sender != null && hex.Edited)
                accion.Change_File(hex.FileID, hex.NewFile);
        }

        private void treeSystem_MouseDoubleClick(object sender, MouseEventArgs e)
        {

            if (accion.IDSelect < 0xF000)   // Comprobación de que la selección no sea un directorio
            {
                accion.Read_File();

                if (!isMono)
                    debug.Add_Text(sb.ToString());
                sb.Length = 0;
            }
        }

        

        private void btnSaveROM_Click(object sender, EventArgs e)
        {
            /* ROM sections:
             * 
             * Header (0x0000-0x4000)
             * ARM9 Binary
             *   |_ARM9
             *   |_ARM9 Overlays Tables
             *   |_ARM9 Overlays
             * ARM7 Binary
             *   |_ARM7
             *   |_ARM7 Overlays Tables
             *   |_ARM7 Overlays
             * FNT (File Name Table)
             *   |_Main tables
             *   |_Subtables (names)
             * FAT (File Allocation Table)
             *   |_Files offset
             *     |_Start offset
             *     |_End offset
             * Banner
             *   |_Header 0x20
             *   |_Icon (Bitmap + palette) 0x200 + 0x20
             *   |_Game titles (Japanese, English, French, German, Italian, Spanish) 6 * 0x100
             * Files...
            */

            Thread espera = new Thread(ThreadEspera);
            if (!isMono)
                espera.Start("S05");

            // Get special files
            sFolder ftc = accion.Search_Folder("ftc");

            sFile fnt = ftc.files.Find(sFile => sFile.name == "fnt.bin");
            sFile fat = ftc.files.Find(sFile => sFile.name == "fat.bin");
            sFile arm9 = ftc.files.Find(sFile => sFile.name == "arm9.bin");
            sFile arm7 = ftc.files.Find(sFile => sFile.name == "arm7.bin");

            int index = ftc.files.FindIndex(sFile => sFile.name == "y9.bin");
            sFile y9 = new sFile();
            List<sFile> ov9 = new List<sFile>();
            if (index != -1)
            {
                y9 = ftc.files[index];
                ov9 = ftc.files.FindAll(sFile => sFile.name.StartsWith("overlay9_"));
            }

            index = ftc.files.FindIndex(sFile => sFile.name == "y7.bin");
            List<sFile> ov7 = new List<sFile>();
            sFile y7 = new sFile();
            if (index != -1)
            {
                y7 = ftc.files[index];
                ov7 = ftc.files.FindAll(sFile => sFile.name.StartsWith("overlay7_"));
            }

            #region Get ROM sections
            BinaryReader br;
            Console.WriteLine(Tools.Helper.GetTranslation("Messages", "S08"));
            Nitro.Estructuras.ROMHeader header = romInfo.Cabecera;
            uint currPos = header.headerSize;


            // Write ARM9
            string arm9Binary = Path.GetTempFileName();
            string overlays9 = Path.GetTempFileName();
            Console.Write("\tARM9 Binary...");

            br = new BinaryReader(File.OpenRead(arm9.path));
            br.BaseStream.Position = arm9.offset;
            BinaryWriter bw = new BinaryWriter(File.OpenWrite(arm9Binary));
            bw.Write(br.ReadBytes((int)arm9.size));
            bw.Flush();
            br.Close();

            header.ARM9romOffset = currPos;
            header.ARM9size = arm9.size;
            header.ARM9overlayOffset = 0;
            uint arm9overlayOffset = 0;

            currPos += arm9.size;

            // Write the Nitrocode
            br = new BinaryReader(File.OpenRead(accion.ROMFile));
            br.BaseStream.Position = romInfo.Cabecera.ARM9romOffset + romInfo.Cabecera.ARM9size;
            if (br.ReadUInt32() == 0xDEC00621)
            {
                // Nitrocode found
                bw.Write(0xDEC00621);
                bw.Write(br.ReadUInt32());
                bw.Write(br.ReadUInt32());
                currPos += 0x0C;
                bw.Flush();
            }
            br.Close();

            uint rem = currPos % 0x200;
            if (rem != 0)
            {
                while (rem < 0x200)
                {
                    bw.Write((byte)0xFF);
                    rem++;
                    currPos++;
                }
            }

            if (header.ARM9overlaySize != 0)
            {
                // ARM9 Overlays Tables
                br = new BinaryReader(File.OpenRead(y9.path));
                br.BaseStream.Position = y9.offset;
                Nitro.Overlay.Write_Y9(bw, br, ov9.ToArray());
                bw.Flush();
                br.Close();
                header.ARM9overlayOffset = currPos;
                header.ARM9overlaySize = y9.size;

                currPos += y9.size;
                rem = currPos % 0x200;
                if (rem != 0)
                {
                    while (rem < 0x200)
                    {
                        bw.Write((byte)0xFF);
                        rem++;
                        currPos++;
                    }
                }

                Nitro.Overlay.EscribirOverlays(overlays9, ov9, accion.ROMFile);
                bw.Write(File.ReadAllBytes(overlays9)); // ARM9 Overlays
                arm9overlayOffset = currPos;
                currPos += (uint)new FileInfo(overlays9).Length;
            }
            bw.Flush();
            bw.Close();

            Console.WriteLine(Tools.Helper.GetTranslation("Messages", "S09"), new FileInfo(arm9Binary).Length);

            string seedStr = seedTxt.Text;
            if (seedStr == "")
            {
                seedStr = DateTime.Now.ToString("s");
            }
            String seed = "MiUSlRDz" + seedStr;
            GameRandomizer rando = null;
            string gameCode = new string(romInfo.Cabecera.gameCode);
            if(gameCode == "A5FP"){
                rando = new CuriousVillagePALRandomizer(accion.Root, seed, accion);
            }

            if (rando != null)
            {
                rando.seedPuzzles();
                rando.Write();
            }

            // Escribismo el ARM7 Binary
            string arm7Binary = Path.GetTempFileName();
            string overlays7 = Path.GetTempFileName();
            Console.Write("\tARM7 Binary...");

            br = new BinaryReader(File.OpenRead(arm7.path));
            br.BaseStream.Position = arm7.offset;
            bw = new BinaryWriter(File.OpenWrite(arm7Binary));
            bw.Write(br.ReadBytes((int)arm7.size));
            bw.Flush();
            br.Close();

            header.ARM7romOffset = currPos;
            header.ARM7size = arm7.size;
            header.ARM7overlayOffset = 0x00;
            uint arm7overlayOffset = 0x00;

            currPos += arm7.size;
            rem = currPos % 0x200;
            if (rem != 0)
            {
                while (rem < 0x200)
                {
                    bw.Write((byte)0xFF);
                    rem++;
                    currPos++;
                }
            }

            if (romInfo.Cabecera.ARM7overlaySize != 0x00)
            {
                // ARM7 Overlays Tables
                br = new BinaryReader(File.OpenRead(y7.path));
                br.BaseStream.Position = y7.offset;
                bw.Write(br.ReadBytes((int)y7.size));
                bw.Flush();
                br.Close();
                header.ARM7overlayOffset = currPos;
                header.ARM7overlaySize = y7.size;

                currPos += y7.size;
                rem = currPos % 0x200;
                if (rem != 0)
                {
                    while (rem < 0x200)
                    {
                        bw.Write((byte)0xFF);
                        rem++;
                        currPos++;
                    }
                }

                Nitro.Overlay.EscribirOverlays(overlays7, ov7, accion.ROMFile);
                bw.Write(File.ReadAllBytes(overlays7)); // ARM7 Overlays

                arm7overlayOffset = currPos;
                currPos += (uint)new FileInfo(overlays7).Length;
            }
            bw.Flush();
            bw.Close();
            Console.WriteLine(Tools.Helper.GetTranslation("Messages", "S09"), new FileInfo(arm7Binary).Length);


            // Escribimos el FNT (File Name Table)
            string fileFNT = Path.GetTempFileName();
            Console.Write("\tFile Name Table (FNT)...");

            bw = new BinaryWriter(File.OpenWrite(fileFNT));
            br = new BinaryReader(File.OpenRead(fnt.path));
            br.BaseStream.Position = fnt.offset;
            bw.Write(br.ReadBytes((int)fnt.size));
            bw.Flush();
            br.Close();
            header.fileNameTableSize = fnt.size;
            header.fileNameTableOffset = currPos;

            currPos += fnt.size;
            rem = currPos % 0x200;
            if (rem != 0)
            {
                while (rem < 0x200)
                {
                    bw.Write((byte)0xFF);
                    rem++;
                    currPos++;
                }
            }
            bw.Flush();
            bw.Close();

            Console.WriteLine(Tools.Helper.GetTranslation("Messages", "S09"), new FileInfo(fileFNT).Length);


            // Escribimos el FAT (File Allocation Table)
            string fileFAT = Path.GetTempFileName();
            header.FAToffset = currPos;
            Nitro.FAT.Write(fileFAT, accion.Root, header.FAToffset, accion.SortedIDs, arm9overlayOffset, arm7overlayOffset);
            currPos += (uint)new FileInfo(fileFAT).Length;

            // Escribimos el banner
            string banner = Path.GetTempFileName();
            Nitro.NDS.EscribirBanner(banner, romInfo.Banner);
            header.bannerOffset = currPos;
            currPos += (uint)new FileInfo(banner).Length;

            // Escribimos los archivos
            string files = Path.GetTempFileName();
            Nitro.NDS.Write_Files(files, accion.ROMFile, accion.Root, accion.SortedIDs);
            currPos += (uint)new FileInfo(files).Length;

            // Update the ROM size values of the header
            header.ROMsize = currPos;
            header.tamaño = (uint)Math.Ceiling(Math.Log(currPos, 2));
            header.tamaño = (uint)Math.Pow(2, header.tamaño);

            // Get Header CRC
            string tempHeader = Path.GetTempFileName();
            Nitro.NDS.EscribirCabecera(tempHeader, header, accion.ROMFile);
            BinaryReader brHeader = new BinaryReader(File.OpenRead(tempHeader));
            header.headerCRC16 = (ushort)Ekona.Helper.CRC16.Calculate(brHeader.ReadBytes(0x15E));
            brHeader.Close();
            File.Delete(tempHeader);

            // Write header
            string header_file = Path.GetTempFileName();
            Nitro.NDS.EscribirCabecera(header_file, header, accion.ROMFile);


            Console.Write("<br>");
            #endregion

            if (!isMono)
                espera.Abort();


            // Obtenemos el nuevo archivo para guardar
            SaveFileDialog o = new SaveFileDialog();
            o.AddExtension = true;
            o.DefaultExt = ".nds";
            o.Filter = "Nintendo DS ROM (*.nds)|*.nds";
            o.OverwritePrompt = true;
        Open_Dialog:
            if (o.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (o.FileName == accion.ROMFile)
                {
                    MessageBox.Show(Tools.Helper.GetTranslation("Sistema", "S44"));
                    goto Open_Dialog;
                }

                espera = new Thread(ThreadEspera);
                if (!isMono)
                    espera.Start("S06");

                Console.WriteLine(Tools.Helper.GetTranslation("Messages", "S0D"), o.FileName);
                bw = new BinaryWriter(new FileStream(o.FileName, FileMode.Create));

                Ekona.Helper.IOutil.Append(ref bw, header_file);
                Ekona.Helper.IOutil.Append(ref bw, arm9Binary);
                Ekona.Helper.IOutil.Append(ref bw, arm7Binary);
                Ekona.Helper.IOutil.Append(ref bw, fileFNT);
                Ekona.Helper.IOutil.Append(ref bw, fileFAT);
                Ekona.Helper.IOutil.Append(ref bw, banner);
                Ekona.Helper.IOutil.Append(ref bw, files);

                rem = header.tamaño - (uint)bw.BaseStream.Position;
                while (rem > 0)
                {
                    bw.Write((byte)0xFF);
                    rem--;
                }
                bw.Flush();
                bw.Close();

                Console.WriteLine("<b>" + Tools.Helper.GetTranslation("Messages", "S09") + "</b>", new FileInfo(o.FileName).Length);
                accion.IsNewRom = false;
            }

            // Borramos archivos ya innecesarios
            File.Delete(header_file);
            File.Delete(arm9Binary);
            File.Delete(overlays9);
            File.Delete(arm7Binary);
            File.Delete(overlays7);
            File.Delete(fileFNT);
            File.Delete(fileFAT);
            File.Delete(banner);
            File.Delete(files);

            if (!isMono)
            {
                espera.Abort();
            }
            sb.Length = 0;
        }
        private void btnImport_Click(object sender, EventArgs e)
        {
            OpenFileDialog o = new OpenFileDialog();
            o.CheckFileExists = true;
            o.CheckPathExists = true;
            o.Multiselect = true;
            if (o.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            // The current selected file will be changed if they only select one file
            if (o.FileNames.Length == 1 && accion.IDSelect < 0xF000)
            {
                accion.Change_File(accion.IDSelect, o.FileNames[0]);
                return;
            }

            // If more than one file is selected, they will be changed by name
            foreach (string currFile in o.FileNames)
            {
                sFolder filesWithSameName = new sFolder();
                if (accion.IDSelect > 0xF000)
                    filesWithSameName = accion.Search_FileName(Path.GetFileName(currFile), accion.Selected_Folder());
                else
                    filesWithSameName = accion.Search_FileName(Path.GetFileName(currFile));

                sFile fileToBeChanged;
                if (filesWithSameName.files.Count == 0)
                    continue;
                else if (filesWithSameName.files.Count == 1)
                    fileToBeChanged = filesWithSameName.files[0];
                else
                {
                    // Get relative path
                    for (int i = 0; i < filesWithSameName.files.Count; i++)
                    {
                        sFile file = filesWithSameName.files[i];
                        file.tag = accion.Get_RelativePath(filesWithSameName.files[i].id, "", accion.Root);
                        filesWithSameName.files[i] = file;
                    }

                    Dialog.SelectFile dialog = new Dialog.SelectFile(filesWithSameName.files.ToArray());
                    if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                        continue;

                    fileToBeChanged = dialog.SelectedFile;
                }

                accion.Change_File(fileToBeChanged.id, currFile);
            }
        }
        #endregion

        

    }

}
