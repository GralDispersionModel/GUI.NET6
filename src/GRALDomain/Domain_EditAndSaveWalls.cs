#region Copyright
///<remarks>
/// <GRAL Graphical User Interface GUI>
/// Copyright (C) [2019]  [Dietmar Oettl, Markus Kuntner]
/// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
/// the Free Software Foundation version 3 of the License
/// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
/// You should have received a copy of the GNU General Public License along with this program.  If not, see <https://www.gnu.org/licenses/>.
///</remarks>
#endregion

using System;
using System.Windows.Forms;
using GralItemData;
using System.IO;

namespace GralDomain
{
    public partial class Domain
    {
        /// <summary>
        /// Show the wall dialog (checkbox25 = checked) 
        /// </summary>
        /// <param name="sender"></param>
        private void ShowWallDialog(object sender, EventArgs e)
        {
            wallsToolStripMenuItem.Checked = checkBox25.Checked;

            if (checkBox25.Checked == true)
            {
                //show editing form for receptors
                HideWindows(25); // Kuntner
                if (ShowFirst.Wa) // set the inital position of the form
                {
                    if (ShowFirst.Ls == false)
                    {
                        EditWall.Location = EditLS.Location;
                    }
                    else if (ShowFirst.As == false)
                    {
                        EditWall.Location = EditAS.Location;
                    }
                    else if (ShowFirst.Bu == false)
                    {
                        EditWall.Location = EditB.Location;
                    }
                    else if (ShowFirst.Ps == false)
                    {
                        EditWall.Location = EditPS.Location;
                    }
                    else if (ShowFirst.Re == false)
                    {
                        EditWall.Location = EditR.Location;
                    }
                    else
                    {
                        EditWall.Location = GetScreenPositionForNewDialog(0);
                    }

                    ShowFirst.Wa = false;
                }
                MouseControl = MouseMode.WallSet; //edit walls
                InfoBoxCloseAllForms(); // close all infoboxes
                EditWall.Show();
                EditWall.ShowForm();
                EditWall.BringToFront();
                Cursor = Cursors.Cross;

                CheckForExistingDrawingObject("WALLS");
            }
            else
            {
                MouseControl = MouseMode.Default;
                EditWall.Hide();
            }
        }

        /// <summary>
        /// Start the wall dialog (checkbox25 = checked) or save the wall data (checkbox25 = unchecked)
        /// </summary>
        /// <param name="sender">if checkbox25.checked == false and sender == null -> EditWall.SaveArray not called</param>
        void EditAndSaveWallData(object sender, EventArgs e)
        {
            checkBox25.Checked = false;
            wallsToolStripMenuItem.Checked = checkBox25.Checked;
            MouseControl = MouseMode.Default;
            Cursor = Cursors.Default;

            EditWall.Hide(); // Kuntner first hide form to save actual sourcedata
            MarkerPoint.X = 0;
            MarkerPoint.Y = 0;

            if (Gral.Main.Project_Locked == true)
            {
                //Gral.Main.Project_Locked_Message(); // Project locked!
                //Picturebox1_Paint();
            }
            else if (MainForm.DeleteGralGffFile() == DialogResult.OK) // Warningmessage if gff Files exist!
            {
                //save buildings input to file
                if (sender != null) // do not use the dialogue data, if data has been changed outisde the EditPortals dialogue
                {
                    EditWall.SaveArray(false);
                }

                string newPath = Path.Combine(Gral.Main.ProjectName, @"Emissions", "Walls.txt");
                WallDataIO _wd = new WallDataIO();
                _wd.SaveWallData(EditWall.ItemData, newPath);
                _wd = null;

                EditWall.CornerWallCount = 0;
                MainForm.ChangeButtonLabel(Gral.ButtonColorEnum.ButtonBuildings, Gral.ButtonColorEnum.RedDot); // Building label red & delete buildings.dat

                if (MainForm.GRALSettings.BuildingMode != Gral.BuildingModeEnum.None)
                {
                    if (EditWall.ItemData.Count > 0)
                    {
                        MainForm.ChangeButtonLabel(Gral.ButtonColorEnum.ButtonBuildings, Gral.ButtonColorEnum.RedDot); // Building label red & delete buildings.dat
                        MainForm.button9.Visible = true;
                    }
                    else
                    {
                        MainForm.ChangeButtonLabel(Gral.ButtonColorEnum.ButtonBuildings, Gral.ButtonColorEnum.Invisible); // Building label - no buildings
                    }
                }
                //add/delete walls in object list
                if (EditWall.ItemData.Count == 0)
                {
                    RemoveItemFromItemOptions("WALLS");
                }
            }

            //show/hide button to select walls
            if (EditWall.ItemData.Count > 0)
            {
                button49.Visible = true;
            }
            else
            {
                button49.Visible = false;
            }

            //enable/disable GRAL simulations
            MainForm.Enable_GRAL();
            //enable/disable GRAMM simulations
            MainForm.Enable_GRAMM();
            Picturebox1_Paint();
        }
    }
}