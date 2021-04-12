﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bravo.Accessors.FileIO
{
    public class ModelSettings
    {
        public FileFormat FileFormat { get; set; }

        public int Drape { get; set; }

        public double LocalZ { get; set; }

        public double TimeOffset { get; set; }

        /// <summary>
        /// In degrees
        /// </summary>
        public double ParticleRadius { get; set; }

        public int RowCount { get; set; }

        public int ColumnCount { get; set; }

        public ColorRange[] ColorRanges { get; set; }
    }

    public class ColorRange
    {
        public double Min { get; set; }

        public double Max { get; set; }

        public string Color { get; set; }
    }
}
