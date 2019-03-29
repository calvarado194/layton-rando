/*
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
 * Programador: pleoNeX
 * Programa utilizado: Microsoft Visual C# 2010 Express
 * Fecha: 18/02/2011
 * 
 */

namespace Tinke
{
    partial class Sistema
    {
        /// <summary>
        /// Variable del diseñador requerida.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén utilizando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben eliminar; false en caso contrario, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido del método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Sistema));
            this.iconos = new System.Windows.Forms.ImageList(this.components);
            this.panelObj = new System.Windows.Forms.Panel();
            this.toolStrip2 = new System.Windows.Forms.ToolStrip();
            this.toolTipSearch = new System.Windows.Forms.ToolTip(this.components);
            this.btnSaveROM = new System.Windows.Forms.Button();
            this.seedTxt = new System.Windows.Forms.TextBox();
            this.seed_txt = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // iconos
            // 
            this.iconos.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("iconos.ImageStream")));
            this.iconos.TransparentColor = System.Drawing.Color.Transparent;
            this.iconos.Images.SetKeyName(0, "folder.png");
            this.iconos.Images.SetKeyName(1, "page_white.png");
            this.iconos.Images.SetKeyName(2, "palette.png");
            this.iconos.Images.SetKeyName(3, "picture.png");
            this.iconos.Images.SetKeyName(4, "page_white_text.png");
            this.iconos.Images.SetKeyName(5, "compress.png");
            this.iconos.Images.SetKeyName(6, "package.png");
            this.iconos.Images.SetKeyName(7, "folder_go.png");
            this.iconos.Images.SetKeyName(8, "pictures.png");
            this.iconos.Images.SetKeyName(9, "picture_link.png");
            this.iconos.Images.SetKeyName(10, "photo.png");
            this.iconos.Images.SetKeyName(11, "picture_save.png");
            this.iconos.Images.SetKeyName(12, "picture_delete.png");
            this.iconos.Images.SetKeyName(13, "film.png");
            this.iconos.Images.SetKeyName(14, "music.png");
            this.iconos.Images.SetKeyName(15, "picture_go.png");
            this.iconos.Images.SetKeyName(16, "font.png");
            this.iconos.Images.SetKeyName(17, "script.png");
            this.iconos.Images.SetKeyName(18, "folder_add.png");
            this.iconos.Images.SetKeyName(19, "disk.png");
            this.iconos.Images.SetKeyName(20, "page_gear.png");
            this.iconos.Images.SetKeyName(21, "image.png");
            this.iconos.Images.SetKeyName(22, "map.png");
            this.iconos.Images.SetKeyName(23, "package_go.png");
            this.iconos.Images.SetKeyName(24, "package_add.png");
            // 
            // panelObj
            // 
            this.panelObj.AutoScroll = true;
            this.panelObj.BackColor = System.Drawing.Color.Transparent;
            this.panelObj.Location = new System.Drawing.Point(649, 25);
            this.panelObj.Name = "panelObj";
            this.panelObj.Size = new System.Drawing.Size(515, 515);
            this.panelObj.TabIndex = 10;
            // 
            // toolStrip2
            // 
            this.toolStrip2.BackColor = System.Drawing.Color.Transparent;
            this.toolStrip2.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip2.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip2.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Table;
            this.toolStrip2.Location = new System.Drawing.Point(409, 319);
            this.toolStrip2.Name = "toolStrip2";
            this.toolStrip2.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip2.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.toolStrip2.Size = new System.Drawing.Size(1, 0);
            this.toolStrip2.TabIndex = 11;
            this.toolStrip2.Text = "toolStrip2";
            // 
            // toolTipSearch
            // 
            this.toolTipSearch.AutoPopDelay = 10000;
            this.toolTipSearch.InitialDelay = 500;
            this.toolTipSearch.IsBalloon = true;
            this.toolTipSearch.ReshowDelay = 100;
            this.toolTipSearch.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            // 
            // btnSaveROM
            // 
            this.btnSaveROM.ImageIndex = 19;
            this.btnSaveROM.ImageList = this.iconos;
            this.btnSaveROM.Location = new System.Drawing.Point(229, 73);
            this.btnSaveROM.Name = "btnSaveROM";
            this.btnSaveROM.Size = new System.Drawing.Size(110, 40);
            this.btnSaveROM.TabIndex = 19;
            this.btnSaveROM.Text = "Seed and Save ROM...";
            this.btnSaveROM.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnSaveROM.UseVisualStyleBackColor = true;
            this.btnSaveROM.Click += new System.EventHandler(this.btnSaveROM_Click);
            // 
            // seedTxt
            // 
            this.seedTxt.Location = new System.Drawing.Point(101, 23);
            this.seedTxt.MaxLength = 8;
            this.seedTxt.Name = "seedTxt";
            this.seedTxt.Size = new System.Drawing.Size(238, 20);
            this.seedTxt.TabIndex = 20;
            // 
            // seed_txt
            // 
            this.seed_txt.AutoSize = true;
            this.seed_txt.Location = new System.Drawing.Point(12, 26);
            this.seed_txt.Name = "seed_txt";
            this.seed_txt.Size = new System.Drawing.Size(82, 13);
            this.seed_txt.TabIndex = 21;
            this.seed_txt.Text = "Seed parameter";
            // 
            // Sistema
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.ClientSize = new System.Drawing.Size(350, 125);
            this.Controls.Add(this.seed_txt);
            this.Controls.Add(this.seedTxt);
            this.Controls.Add(this.btnSaveROM);
            this.Controls.Add(this.toolStrip2);
            this.Controls.Add(this.panelObj);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.Name = "Sistema";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Sistema_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ImageList iconos;
        private System.Windows.Forms.Panel panelObj;
        private System.Windows.Forms.ToolStrip toolStrip2;
        private System.Windows.Forms.ToolTip toolTipSearch;
        private System.Windows.Forms.Button btnSaveROM;
        private System.Windows.Forms.TextBox seedTxt;
        private System.Windows.Forms.Label seed_txt;

    }
}

