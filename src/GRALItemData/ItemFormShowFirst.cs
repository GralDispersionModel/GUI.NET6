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

namespace GralItemData
{
    /// <summary>
    /// This class holds the data about the first visible Item Form
    /// </summary>
    public class ShowFirstItem
	{
    	public bool Ps  { get; set; } 
        public bool Ls  { get; set; } 
        public bool As  { get; set; } 
        public bool Ts  { get; set; } 
        public bool Bu  { get; set; } 
        public bool Re  { get; set; } 
        public bool Wa  { get; set; } 
        public bool Veg  { get; set; } 
    	
        public ShowFirstItem()
        {
            Reset();
        }

        /// <summary>
        /// Reset the first visible item data
        /// </summary>
        public void Reset()
        {
            Ps = true;
            Ls = true;
            As = true;
            Ts = true;
            Bu = true;
            Re = true;
            Wa = true;
            Veg = true;
        }
	}
}