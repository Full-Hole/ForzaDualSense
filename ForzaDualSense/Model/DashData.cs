﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForzaDSX.Model
{
    public sealed class DashData
    {
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float PositionZ { get; set; }
        public float Speed { get; set; }
        public float Power { get; set; }
        public float Torque { get; set; }
        public float TireTempFl { get; set; }
        public float TireTempFr { get; set; }
        public float TireTempRl { get; set; }
        public float TireTempRr { get; set; }
        public float Boost { get; set; }
        public float Fuel { get; set; }
        public float Distance { get; set; }
        public float BestLapTime { get; set; }
        public float LastLapTime { get; set; }
        public float CurrentLapTime { get; set; }
        public float CurrentRaceTime { get; set; }
        public uint Lap { get; set; }
        public uint RacePosition { get; set; }
        public uint Accelerator { get; set; }
        public uint Brake { get; set; }
        public uint Clutch { get; set; }
        public uint Handbrake { get; set; }
        public uint Gear { get; set; }
        public int Steer { get; set; }
        public uint NormalDrivingLine { get; set; }
        public uint NormalAiBrakeDifference { get; set; }
    }
}