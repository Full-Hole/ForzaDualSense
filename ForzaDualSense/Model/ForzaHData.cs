using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForzaDualSense.Model
{
    public class ForzaHData
    {
        public ForzaHData(byte[] rawTelemetryPacket)
        {
           
            using MemoryStream inputStream = new MemoryStream(rawTelemetryPacket);
            using BinaryReader reader = new BinaryReader(inputStream);
            IsRaceOn = reader.ReadUInt32(); 

            TimestampMS = reader.ReadUInt32();

            EngineMaxRpm = reader.ReadSingle();
            EngineIdleRpm = reader.ReadSingle();
            CurrentEngineRpm = reader.ReadSingle();

            AccelerationX = reader.ReadSingle();
            AccelerationY = reader.ReadSingle();
            AccelerationZ = reader.ReadSingle();

            VelocityX = reader.ReadSingle();
            VelocityY = reader.ReadSingle();
            VelocityZ = reader.ReadSingle();

            AngularVelocityX = reader.ReadSingle();
            AngularVelocityY = reader.ReadSingle();
            AngularVelocityZ = reader.ReadSingle();

            Yaw = reader.ReadSingle();
            Pitch = reader.ReadSingle();
            Roll = reader.ReadSingle();

            NormalizedSuspensionTravelFrontLeft = reader.ReadSingle();
            NormalizedSuspensionTravelFrontRight = reader.ReadSingle();
            NormalizedSuspensionTravelRearLeft = reader.ReadSingle();
            NormalizedSuspensionTravelRearRight = reader.ReadSingle();

            TireSlipRatioFrontLeft = reader.ReadSingle();
            TireSlipRatioFrontRight = reader.ReadSingle();
            TireSlipRatioRearLeft = reader.ReadSingle();
            TireSlipRatioRearRight = reader.ReadSingle();

            WheelRotationSpeedFrontLeft = reader.ReadSingle();
            WheelRotationSpeedFrontRight = reader.ReadSingle();
            WheelRotationSpeedRearLeft = reader.ReadSingle();
            WheelRotationSpeedRearRight = reader.ReadSingle();

            WheelOnRumbleStripFrontLeft = reader.ReadInt32();
            WheelOnRumbleStripFrontRight = reader.ReadInt32();
            WheelOnRumbleStripRearLeft = reader.ReadInt32();
            WheelOnRumbleStripRearRight = reader.ReadInt32();

            WheelInPuddleDepthFrontLeft = reader.ReadSingle();
            WheelInPuddleDepthFrontRight = reader.ReadSingle();
            WheelInPuddleDepthRearLeft = reader.ReadSingle();
            WheelInPuddleDepthRearRight = reader.ReadSingle();

            SurfaceRumbleFrontLeft = reader.ReadSingle();
            SurfaceRumbleFrontRight = reader.ReadSingle();
            SurfaceRumbleRearLeft = reader.ReadSingle();
            SurfaceRumbleRearRight = reader.ReadSingle();

            TireSlipAngleFrontLeft = reader.ReadSingle();
            TireSlipAngleFrontRight = reader.ReadSingle();
            TireSlipAngleRearLeft = reader.ReadSingle();
            TireSlipAngleRearRight = reader.ReadSingle();

            TireCombinedSlipFrontLeft = reader.ReadSingle();
            TireCombinedSlipFrontRight = reader.ReadSingle();
            TireCombinedSlipRearLeft = reader.ReadSingle();
            TireCombinedSlipRearRight = reader.ReadSingle();

            SuspensionTravelMetersFrontLeft = reader.ReadSingle();
            SuspensionTravelMetersFrontRight = reader.ReadSingle();
            SuspensionTravelMetersRearLeft = reader.ReadSingle();
            SuspensionTravelMetersRearRight = reader.ReadSingle();

            CarOrdinal = reader.ReadUInt32();
            CarClass = reader.ReadUInt32();
            CarPerformanceIndex = reader.ReadUInt32();
            DrivetrainType = reader.ReadUInt32();
            NumCylinders = reader.ReadUInt32();

            CarType = reader.ReadUInt32();
            unownX = reader.ReadUInt32();
            unownY = reader.ReadUInt32();            

            PositionX = reader.ReadSingle();
            PositionY = reader.ReadSingle();
            PositionZ = reader.ReadSingle();

            Speed = reader.ReadSingle();
            Power = reader.ReadSingle();
            Torque = reader.ReadSingle();

            TireTempFrontLeft = reader.ReadSingle();
            TireTempFrontRight = reader.ReadSingle();
            TireTempRearLeft = reader.ReadSingle();
            TireTempRearRight = reader.ReadSingle();

            Boost = reader.ReadSingle();
            Fuel = reader.ReadSingle();
            DistanceTraveled = reader.ReadSingle();
            BestLapTime = reader.ReadSingle();
            LastLapTime = reader.ReadSingle();
            CurrentLapTime = reader.ReadSingle();
            CurrentRaceTime = reader.ReadSingle();

            LapNumber = reader.ReadUInt16();
            RacePosition = reader.ReadByte();

            Accelerator = reader.ReadByte();
            Brake = reader.ReadByte();
            Clutch = reader.ReadByte();
            Handbrake = reader.ReadByte();
            Gear = reader.ReadByte();
            Steer = reader.ReadSByte();

            NormalizedDrivingLine = reader.ReadSByte();
            NormalizedAIBrakeDifference = reader.ReadSByte();
        }


        #region Parsed telemetry values
        public uint IsRaceOn { get; set; } // = 1 when race is on. = 0 when in menus/race stopped …
        public uint TimestampMS { get; set; } // Can overflow to 0 eventually
        public float EngineMaxRpm { get; set; }
        public float EngineIdleRpm { get; set; }
        public float CurrentEngineRpm { get; set; }
        public float AccelerationX { get; set; } // In the car's local space; X = right, Y = up, Z = forward
        public float AccelerationY { get; set; }
        public float AccelerationZ { get; set; }
        public float VelocityX { get; set; } // In the car's local space; X = right, Y = up, Z = forward
        public float VelocityY { get; set; }
        public float VelocityZ { get; set; }
        public float AngularVelocityX { get; set; } // In the car's local space; X = pitch, Y = yaw, Z = roll
        public float AngularVelocityY { get; set; }
        public float AngularVelocityZ { get; set; }
        public float Yaw { get; set; }
        public float Pitch { get; set; }
        public float Roll { get; set; }
        public float NormalizedSuspensionTravelFrontLeft { get; set; } // Suspension travel normalized: 0.0f = max stretch; 1.0 = max compression
        public float NormalizedSuspensionTravelFrontRight { get; set; }
        public float NormalizedSuspensionTravelRearLeft { get; set; }
        public float NormalizedSuspensionTravelRearRight { get; set; }
        public float TireSlipRatioFrontLeft { get; set; } // Tire normalized slip ratio, = 0 means 100% grip and |ratio| > 1.0 means loss of grip.
        public float TireSlipRatioFrontRight { get; set; }
        public float TireSlipRatioRearLeft { get; set; }
        public float TireSlipRatioRearRight { get; set; }
        public float WheelRotationSpeedFrontLeft { get; set; } // Wheel rotation speed radians/sec.
        public float WheelRotationSpeedFrontRight { get; set; }
        public float WheelRotationSpeedRearLeft { get; set; }
        public float WheelRotationSpeedRearRight { get; set; }
        public float WheelOnRumbleStripFrontLeft { get; set; } // = 1 when wheel is on rumble strip, = 0 when off.
        public float WheelOnRumbleStripFrontRight { get; set; }
        public float WheelOnRumbleStripRearLeft { get; set; }
        public float WheelOnRumbleStripRearRight { get; set; }
        public float WheelInPuddleDepthFrontLeft { get; set; } // = from 0 to 1, where 1 is the deepest puddle
        public float WheelInPuddleDepthFrontRight { get; set; }
        public float WheelInPuddleDepthRearLeft { get; set; }
        public float WheelInPuddleDepthRearRight { get; set; }
        public float SurfaceRumbleFrontLeft { get; set; } // Non-dimensional surface rumble values passed to controller force feedback
        public float SurfaceRumbleFrontRight { get; set; }
        public float SurfaceRumbleRearLeft { get; set; }
        public float SurfaceRumbleRearRight { get; set; }
        public float TireSlipAngleFrontLeft { get; set; } // Tire normalized slip angle, = 0 means 100% grip and |angle| > 1.0 means loss of grip.
        public float TireSlipAngleFrontRight { get; set; }
        public float TireSlipAngleRearLeft { get; set; }
        public float TireSlipAngleRearRight { get; set; }
        public float TireCombinedSlipFrontLeft { get; set; } // Tire normalized combined slip, = 0 means 100% grip and |slip| > 1.0 means loss of grip.
        public float TireCombinedSlipFrontRight { get; set; }
        public float TireCombinedSlipRearLeft { get; set; }
        public float TireCombinedSlipRearRight { get; set; }
        public float SuspensionTravelMetersFrontLeft { get; set; } // Actual suspension travel in meters
        public float SuspensionTravelMetersFrontRight { get; set; }
        public float SuspensionTravelMetersRearLeft { get; set; }
        public float SuspensionTravelMetersRearRight { get; set; }
        public uint CarOrdinal { get; set; } // Unique ID of the car make/model
        public uint CarClass { get; set; } // Between 0 (D -- worst cars) and 7 (X class -- best cars) inclusive
        public uint CarPerformanceIndex { get; set; } // Between 100 (slowest car) and 999 (fastest car) inclusive
        public uint DrivetrainType { get; set; } // Corresponds to EDrivetrainType; 0 = FWD, 1 = RWD, 2 = AWD
        public uint NumCylinders { get; set; } // Number of cylinders in the engine
        //Horizon Data
        public uint CarType { get; set; }
        public uint unownX { get; set; } //Unown data posibly damage to objects
        public uint unownY { get; set; } //Unown data
        //Dash
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float PositionZ { get; set; }
        public float Speed { get; set; } // meters per second
        public float Power { get; set; } // watts
        public float Torque { get; set; } // newton meter
        public float TireTempFrontLeft { get; set; }
        public float TireTempFrontRight { get; set; }
        public float TireTempRearLeft { get; set; }
        public float TireTempRearRight { get; set; }
        public float Boost { get; set; }
        public float Fuel { get; set; }
        public float DistanceTraveled { get; set; }
        public float BestLapTime { get; set; }
        public float LastLapTime { get; set; }
        public float CurrentLapTime { get; set; }
        public float CurrentRaceTime { get; set; }
        public ushort LapNumber { get; set; }
        public byte RacePosition { get; set; }
        public byte Accelerator { get; set; }
        public byte Brake { get; set; }
        public byte Clutch { get; set; }
        public byte Handbrake { get; set; }
        public byte Gear { get; set; }
        public sbyte Steer { get; set; }
        public sbyte NormalizedDrivingLine { get; set; }
        public sbyte NormalizedAIBrakeDifference { get; set; }
        #endregion
    }
}
