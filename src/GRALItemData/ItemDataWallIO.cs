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

using System.Collections.Generic;
using System.Drawing;
using System.IO;
using GralData;

namespace GralItemData
{
    /// <summary>
    /// This class represents supports reading/writing the building data
    /// </summary>
    public class WallDataIO
	{
		public bool LoadWallData(List<WallData> _data, string _filename)
		{
			return LoadWallData(_data, _filename, false, new RectangleF());
		}
		
		public bool LoadWallData(List<WallData> _data, string _filename, bool _filterData, RectangleF _domainRect)
		{
			bool reading_ok = false;
			try
			{
				if (File.Exists(_filename) && _data != null)
			    {
					using(StreamReader myReader = new StreamReader(_filename))
					{
						string text; // read header
						int version = 0;
						text = myReader.ReadLine(); // header 1st line
						if (text == "Version_19")
						{
							version = 1;
							text = myReader.ReadLine(); // header 2nd line
							text = myReader.ReadLine(); // header 3rd line
						}
						else
						{
							text = myReader.ReadLine(); // header 2nd line
						}
						
						while (myReader.EndOfStream == false) // read until EOF
						{
							text = version.ToString() + "," + myReader.ReadLine(); // read data and add version number
							
							if (_filterData == false)
							{
								_data.Add(new WallData(text));
							}
							else  // filter data -> import data inside domain area
							{
								WallData _dta = new WallData(text);
								bool inside = false;
								foreach(PointD_3d _pti in _dta.Pt)
								{
									PointF _pttest = new PointF((float) (_pti.X), (float) (_pti.Y));
									if (_domainRect.Contains(_pttest))
									{
										inside = true;
										break;
									}
								}
								if (inside)
								{
									_data.Add(_dta);
								}
							}
						}
					}
					reading_ok = true;
				}				
			}
			catch
			{
				reading_ok = false;
			}
			
			return reading_ok;
		}
		
		public bool SaveWallData(List<WallData> _data, string _projectPath)
		{
			bool writing_ok = false;
			try
			{
				using (StreamWriter myWriter = File.CreateText(_projectPath))
				{
					myWriter.WriteLine("Version_19");
					myWriter.WriteLine("List of all walls within the model domain");
					myWriter.Write("Name, number of vertices, lenght, corner points (x[m],y[m],z[m])");
					myWriter.WriteLine();
					foreach (WallData _dta in _data)
					{
						myWriter.WriteLine(_dta.ToString());
					}
				}
				writing_ok = true;
			}
			catch
			{	
			}
			return writing_ok;
		}
	}
}

