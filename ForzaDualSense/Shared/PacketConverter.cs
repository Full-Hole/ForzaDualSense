using System.Net;
using Newtonsoft.Json;
using ForzaDualSense.Extensions;
using ForzaDualSense.Model;

namespace ForzaDualSense.Shared
{
    //Needed to communicate with DualSenseX
    public static class PacketConverter
    {
        //Parses data from Forza into a DataPacket
        public static DataPacket ParseData(byte[] packet)
        {
            return new DataPacket()
            {
                Sled = new SledData()
                {
                    IsRaceOn = packet.IsRaceOn(),
                    TimestampMS = packet.TimestampMs(),
                    EngineMaxRpm = packet.EngineMaxRpm(),
                    EngineIdleRpm = packet.EngineIdleRpm(),
                    CurrentEngineRpm = packet.CurrentEngineRpm(),
                    AccelerationX = packet.AccelerationX(),
                    AccelerationY = packet.AccelerationY(),
                    AccelerationZ = packet.AccelerationZ(),
                    VelocityX = packet.VelocityX(),
                    VelocityY = packet.VelocityY(),
                    VelocityZ = packet.VelocityZ(),
                    AngularVelocityX = packet.AngularVelocityX(),
                    AngularVelocityY = packet.AngularVelocityY(),
                    AngularVelocityZ = packet.AngularVelocityZ(),
                    Yaw = packet.Yaw(),
                    Pitch = packet.Pitch(),
                    Roll = packet.Roll(),
                    NormalizedSuspensionTravelFrontLeft = packet.NormSuspensionTravelFl(),
                    NormalizedSuspensionTravelFrontRight = packet.NormSuspensionTravelFr(),
                    NormalizedSuspensionTravelRearLeft = packet.NormSuspensionTravelRl(),
                    NormalizedSuspensionTravelRearRight = packet.NormSuspensionTravelRr(),
                    TireSlipRatioFrontLeft = packet.TireSlipRatioFl(),
                    TireSlipRatioFrontRight = packet.TireSlipRatioFr(),
                    TireSlipRatioRearLeft = packet.TireSlipRatioRl(),
                    TireSlipRatioRearRight = packet.TireSlipRatioRr(),
                    WheelRotationSpeedFrontLeft = packet.WheelRotationSpeedFl(),
                    WheelRotationSpeedFrontRight = packet.WheelRotationSpeedFr(),
                    WheelRotationSpeedRearLeft = packet.WheelRotationSpeedRl(),
                    WheelRotationSpeedRearRight = packet.WheelRotationSpeedRr(),
                    WheelOnRumbleStripFrontLeft = packet.WheelOnRumbleStripFl(),
                    WheelOnRumbleStripFrontRight = packet.WheelOnRumbleStripFr(),
                    WheelOnRumbleStripRearLeft = packet.WheelOnRumbleStripRl(),
                    WheelOnRumbleStripRearRight = packet.WheelOnRumbleStripRr(),
                    WheelInPuddleDepthFrontLeft = packet.WheelInPuddleFl(),
                    WheelInPuddleDepthFrontRight = packet.WheelInPuddleFr(),
                    WheelInPuddleDepthRearLeft = packet.WheelInPuddleRl(),
                    WheelInPuddleDepthRearRight = packet.WheelInPuddleRr(),
                    SurfaceRumbleFrontLeft = packet.SurfaceRumbleFl(),
                    SurfaceRumbleFrontRight = packet.SurfaceRumbleFr(),
                    SurfaceRumbleRearLeft = packet.SurfaceRumbleRl(),
                    SurfaceRumbleRearRight = packet.SurfaceRumbleRr(),
                    TireSlipAngleFrontLeft = packet.TireSlipAngleFl(),
                    TireSlipAngleFrontRight = packet.TireSlipAngleFr(),
                    TireSlipAngleRearLeft = packet.TireSlipAngleRl(),
                    TireSlipAngleRearRight = packet.TireSlipAngleRr(),
                    TireCombinedSlipFrontLeft = packet.TireCombinedSlipFl(),
                    TireCombinedSlipFrontRight = packet.TireCombinedSlipFr(),
                    TireCombinedSlipRearLeft = packet.TireCombinedSlipRl(),
                    TireCombinedSlipRearRight = packet.TireCombinedSlipRr(),
                    SuspensionTravelMetersFrontLeft = packet.SuspensionTravelMetersFl(),
                    SuspensionTravelMetersFrontRight = packet.SuspensionTravelMetersFr(),
                    SuspensionTravelMetersRearLeft = packet.SuspensionTravelMetersRl(),
                    SuspensionTravelMetersRearRight = packet.SuspensionTravelMetersRr(),
                    CarOrdinal = packet.CarOrdinal(),
                    CarClass = packet.CarClass(),
                    CarPerformanceIndex = packet.CarPerformanceIndex(),
                    DrivetrainType = packet.DriveTrain(),
                    NumCylinders = packet.NumCylinders(),

                },
                Dash = new DashData()
                {
                    PositionX = packet.PositionX(),
                    PositionY = packet.PositionY(),
                    PositionZ = packet.PositionZ(),
                    Speed = packet.Speed(),
                    Power = packet.Power(),
                    Torque = packet.Torque(),
                    TireTempFl = packet.TireTempFl(),
                    TireTempFr = packet.TireTempFr(),
                    TireTempRl = packet.TireTempRl(),
                    TireTempRr = packet.TireTempRr(),
                    Boost = packet.Boost(),
                    Fuel = packet.Fuel(),
                    Distance = packet.Distance(),
                    BestLapTime = packet.BestLapTime(),
                    LastLapTime = packet.LastLapTime(),
                    CurrentLapTime = packet.CurrentLapTime(),
                    CurrentRaceTime = packet.CurrentRaceTime(),
                    Lap = packet.Lap(),
                    RacePosition = packet.RacePosition(),
                    Accelerator = packet.Accelerator(),
                    Brake = packet.Brake(),
                    Clutch = packet.Clutch(),
                    Handbrake = packet.Handbrake(),
                    Gear = packet.Gear(),
                    Steer = packet.Steer(),
                    NormalDrivingLine = packet.NormalDrivingLine(),
                    NormalAiBrakeDifference = packet.NormalAiBrakeDifference(),
                }
            };
        }
        
        public static string PacketToJson(Packet packet)
        {
            return JsonConvert.SerializeObject(packet);
        }

        public static Packet JsonToPacket(string json)
        {
            return JsonConvert.DeserializeObject<Packet>(json);
        }
    }
}
