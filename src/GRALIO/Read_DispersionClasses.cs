#region Copyright
///<remarks>
/// <GRAL Graphical User Interface GUI>
/// Copyright (C) [2019-2020]  [Dietmar Oettl, Markus Kuntner]
/// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
/// the Free Software Foundation version 3 of the License
/// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
/// You should have received a copy of the GNU General Public License along with this program.  If not, see <https://www.gnu.org/licenses/>.
///</remarks>
#endregion

using System;
using System.IO;
using System.Windows.Forms;
using System.IO.Compression;
using System.Globalization;

namespace GralIO
{
    /// <summary>
    /// Read GRAMM dispersion classes from "*.scl" files
    /// </summary>
    public class ReadSclUstOblClasses
    {
        private readonly string decsep = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
        private string _filename;
        public string FileName { get { return _filename; } set { _filename = value; } } // Filename
        private float _GRAMMhorgridsize;
        public float GRAMMhorgridsize { set { _GRAMMhorgridsize = value; } get { return _GRAMMhorgridsize; } }

        private int NI;
        private int NJ;

        private int _NX;
        public int NX { set { _NX = value; } }
        private int _NY;
        public int NY { set { _NY = value; } }
        private int _NZ;
        public int NZ { set { _NZ = value; } }
        private int _Y0 = 0;
        public int Y0 { set { _Y0 = value; } }
        private int _X0 = 0;
        public int X0 { set { _X0 = value; } }

        private double[,] _Stabclasses;
        public double[,] Stabclasses
        {
            set { _Stabclasses = value; }
            get
            {
                if (_stabClassesInt16 != null)
                {
                    CreateDoubleArray(_stabClassesInt16);
                }
                return _Stabclasses;
            }
        }
        private Int16[,] _MOlength;
        public Int16[,] MOlength { set { _MOlength = value; } get { return _MOlength; } }
        private Int16[,] _Ustar;
        public Int16[,] Ustar { set { _Ustar = value; } get { return _Ustar; } }
        private Int16[,] _stabClassesInt16;

        /// <summary>
        /// Read GRAMM dispersion classes from "*.scl" files
        /// </summary>
        public bool ReadSclFile() // read complete file to _Stabclasses, _MO_Lenght and _Ustar
        {
            try
            {
                if (File.Exists(_filename))
                {
                    if (ReadFlowFieldFiles.CheckIfFileIsZipped(_filename)) // file zipped?
                    {
                        using (FileStream fs = new FileStream(_filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            using (BufferedStream bs = new BufferedStream(fs, 32768))
                            {
                                using (ZipArchive archive = new ZipArchive(bs, ZipArchiveMode.Read, false)) //open Zip archive
                                {
                                    foreach (ZipArchiveEntry entry in archive.Entries)  // search for a scl file
                                    {
                                        if (entry.FullName.Contains("scl"))
                                        {
                                            using (BinaryReader stability = new BinaryReader(entry.Open())) //Open zip entry
                                            {
                                                ReadValues(stability, ref _stabClassesInt16);
                                            }
                                        }
                                        if (entry.FullName.Contains("ust"))
                                        {
                                            using (BinaryReader stability = new BinaryReader(entry.Open())) //Open zip entry
                                            {
                                                ReadValues(stability, ref _Ustar);
                                            }
                                        }
                                        if (entry.FullName.Contains("obl"))
                                        {
                                            using (BinaryReader stability = new BinaryReader(entry.Open())) //Open zip entry
                                            {
                                                ReadValues(stability, ref _MOlength);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else // not zipped
                    {
                        using (BinaryReader stability = new BinaryReader(File.Open(_filename, FileMode.Open)))
                        {
                            ReadValues(stability, ref _stabClassesInt16);
                        }
                    }
                }
                else
                {
                    throw new FileNotFoundException(_filename + @"not found");
                }

                return true; // Reading OK
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Create and return a double[,] array, created and copied from an int16[,] array - reduce array allocations
        /// </summary>
        private void CreateDoubleArray(Int16[,] _int16Array)
        {
            if (_Stabclasses == null || _Stabclasses.GetLength(0) != _int16Array.GetLength(0) || _Stabclasses.GetLength(1) != _int16Array.GetLength(1))
            {
                _Stabclasses = new double[_int16Array.GetLength(0), _int16Array.GetLength(1)];
            }
            for (int i = 0; i < _int16Array.GetLength(0); i++)
            {
                for (int j = 0; j < _int16Array.GetLength(1); j++)
                {
                    _Stabclasses[i, j] = (double)_int16Array[i, j];
                }
            }
            //return _Stabclasses;
        }

        /// <summary>
        /// Read entire GRAMM dispersion classes/MO Lenght or USt from "*.scl" files
        /// </summary>
        private bool ReadValues(BinaryReader stability, ref Int16[,] scl_Array)
        {
            try
            {
                // read the header
                stability.ReadInt32();
                NI = stability.ReadInt32();
                NJ = stability.ReadInt32();
                int NK = stability.ReadInt32();

                _GRAMMhorgridsize = stability.ReadSingle();
                if (scl_Array == null || scl_Array.GetLength(0) != NI || scl_Array.GetLength(1) != NJ)
                {
                    scl_Array = new Int16[NI, NJ]; // create new array
                }
                else
                {
#if NET6_0_OR_GREATER
                    Array.Clear(scl_Array);
#else
                    Array.Clear(scl_Array, 0, NI * NJ);
#endif
                }

                byte[] _row = new byte[NJ * 2];
                for (int i = 0; i < NI; i++)
                {
                    _row = stability.ReadBytes(NJ * 2);
                    for (int j = 0; j < NJ; j++)
                    {
                        scl_Array[i, j] = BitConverter.ToInt16(_row, j * 2);
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Read one cell of GRAMM dispersion classes from "*.scl" files
        /// </summary>
        public int ReadSclFile(int x, int y) // read one value from *.scl
        {
            try
            {
                short temp = 0;
                if (File.Exists(_filename) && x >= 0 && y >= 0)
                {
                    if (ReadFlowFieldFiles.CheckIfFileIsZipped(_filename)) // file zipped?
                    {
                        using (FileStream fs = new FileStream(_filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            using (BufferedStream bs = new BufferedStream(fs, 32768))
                            {
                                using (ZipArchive archive = new ZipArchive(bs, ZipArchiveMode.Read, false)) //open Zip archive
                                {
                                    foreach (ZipArchiveEntry entry in archive.Entries) // search for a scl file
                                    {
                                        if (entry.FullName.Contains("scl"))
                                        {
                                            using (BinaryReader stability = new BinaryReader(entry.Open())) //Open Zip entry
                                            {
                                                // read the header
                                                stability.ReadInt32();
                                                int NI = stability.ReadInt32();
                                                int NJ = stability.ReadInt32();
                                                int NK = stability.ReadInt32();
                                                _GRAMMhorgridsize = stability.ReadSingle();

                                                long position = (x * NJ + y); // Position in bytes 20 Bytes = Header

                                                if (x < NI && y < NJ)
                                                {
                                                    // Seek doesn't work in zipped files
                                                    // stability.BaseStream.Seek(position, SeekOrigin.Begin);
                                                    for (int i = 0; i < position; i++) // seek manually
                                                    {
                                                        stability.ReadInt16();
                                                    }

                                                    temp = stability.ReadInt16(); // read this value
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else // not zipped
                    {
                        using (BinaryReader stability = new BinaryReader(File.Open(_filename, FileMode.Open)))
                        {
                            // read the header
                            stability.ReadInt32();
                            int NI = stability.ReadInt32();
                            int NJ = stability.ReadInt32();
                            int NK = stability.ReadInt32();
                            _GRAMMhorgridsize = stability.ReadSingle();

                            long position = (x * NJ + y) * 2 + 20; // Position in bytes 20 Bytes = Header

                            long lenght = stability.BaseStream.Length; // data set lenght
                            if (position < lenght && x < NI && y < NJ)
                            {
                                stability.BaseStream.Seek(position, SeekOrigin.Begin);
                                temp = stability.ReadInt16(); // read this value
                            }
                        }

                    }
                }
                else
                {
                    throw new FileNotFoundException(_filename + @"not found");
                }

                return temp; // Reading OK
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString(), "GRAL GUI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 0;
            }
        }

        /// <summary>
        /// Read a mean value of 3x3 cells of GRAMM dispersion classes from "*.scl" files
        /// </summary>
        public int ReadSclMean(int x, int y) // read a mean (3x3) value from *.scl file and return the stability class
                                             // define Filename!
        {
            int SCL = 0;
            if (ReadSclFile()) // read complete file
            {
                SCL = SclMean(x, y);
            }

            return SCL; // return mean stability class
        }

        /// <summary>
        /// Read a mean value of 3x3 cells of GRAMM dispersion classes from "*.scl" files
        /// </summary>
        public int SclMean(int x, int y)
        {
            int counter = 0;
            double sum = 0;
            try
            {
                for (int i = x - 1; i < x + 2; i++)
                {
                    for (int j = y - 1; j < y + 2; j++)
                    {
                        if (i >= 0 && j >= 0 && i < NI && j < NJ) // inside _Stabclassesay
                        {
                            sum += _stabClassesInt16[i, j];
                            counter++;
                            if (i == x && j == y) // double weighting of center
                            {
                                sum += _stabClassesInt16[i, j];
                                counter++;
                            }

                        }
                    }
                }
            }
            catch { }

            if (counter > 0)
            {
                return (int)Math.Round(sum / counter); // compute nearest value
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Export the stability classes, friction velocity, and Obukhov length for a GRAMM sub domain
        /// </summary>
        public bool ExportSclFile() //output, export for stability classes, friction velocity, and Obukhov length
        {
            try
            {
                // write a Zip file
                int header = -1;
                Int16 dummy;
                string stabclassfilename = (_filename + ".scl");
                try
                {
                    using (BinaryWriter writer = new BinaryWriter(File.Open(stabclassfilename, FileMode.Create)))
                    {; }

                    using (FileStream zipToOpen = new FileStream(stabclassfilename, FileMode.Create))
                    {
                        using (BufferedStream bufZipFile = new BufferedStream(zipToOpen, 32768))
                        {
                            using (ZipArchive archive = new ZipArchive(bufZipFile, ZipArchiveMode.Update))
                            {
                                string ustarfilename = (Path.GetFileNameWithoutExtension(_filename) + ".ust");
                                ZipArchiveEntry write_entry1 = archive.CreateEntry(ustarfilename);
                                using (BinaryWriter writer = new BinaryWriter(write_entry1.Open()))
                                {
                                    writer.Write(header);
                                    writer.Write(_NX - _X0);
                                    writer.Write(_NY - _Y0);
                                    writer.Write(_NZ);
                                    writer.Write(_GRAMMhorgridsize);
                                    for (int i = _X0; i < _NX; i++)
                                    {
                                        for (int j = _Y0; j < _NY; j++)
                                        {
                                            dummy = _Ustar[i, j];
                                            writer.Write(dummy);
                                        }
                                    }
                                }

                                string obukhovfilename = (Path.GetFileNameWithoutExtension(_filename) + ".obl");
                                ZipArchiveEntry write_entry2 = archive.CreateEntry(obukhovfilename);
                                using (BinaryWriter writer = new BinaryWriter(write_entry2.Open()))
                                {
                                    writer.Write(header);
                                    writer.Write(_NX - _X0);
                                    writer.Write(_NY - _Y0);
                                    writer.Write(_NZ);
                                    writer.Write(_GRAMMhorgridsize);
                                    for (int i = _X0; i < _NX; i++)
                                    {
                                        for (int j = _Y0; j < _NY; j++)
                                        {
                                            dummy = _MOlength[i, j];
                                            writer.Write(dummy);
                                        }
                                    }
                                }

                                //computation and ouput of stability classes
                                string stabilityfile = (Path.GetFileNameWithoutExtension(_filename) + ".scl");
                                ZipArchiveEntry write_entry3 = archive.CreateEntry(stabilityfile);
                                using (BinaryWriter writer = new BinaryWriter(write_entry3.Open()))
                                {
                                    writer.Write(header);
                                    writer.Write(_NX - _X0);
                                    writer.Write(_NY - _Y0);
                                    writer.Write(_NZ);
                                    writer.Write(_GRAMMhorgridsize);
                                    for (int i = _X0; i < _NX; i++)
                                    {
                                        for (int j = _Y0; j < _NY; j++)
                                        {
                                            dummy = _stabClassesInt16[i, j];
                                            writer.Write(dummy);
                                        }
                                    }
                                }
                            }
                        } // archive
                    } // Zip File
                } // catch
                catch { }

                return true; // Reading OK
            }
            catch
            {
                return false;
            }
        }

        public bool close()
        {
            _filename = null;
            return true;
        }

    }
}
