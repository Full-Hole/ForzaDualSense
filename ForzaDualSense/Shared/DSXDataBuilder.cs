using CsvHelper;
using ForzaDualSense.Enums;
using ForzaDualSense.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForzaDualSense.Shared
{
    public static class DSXDataBuilder
    {
        static Settings _settings;
        static bool _verbose;
        static bool _logToCsv;
        static int lastThrottleResistance = 1;
        static int lastBrakeResistance = 200;
        static int lastBrakeFreq = 0;

        public static void Config(Settings settings)
        {
            _verbose = _settings.VERBOSE;
            _logToCsv = _settings.LOG_TO_CSV;
            _settings = settings;
        }
        //This prepare data to DualSenseX based on the input parsed data from Forza.
        //See DataPacket.cs for more details about what forza parameters can be accessed.
        //See the Enums at the bottom of this file for details about commands that can be sent to DualSenseX
        //Also see the Test Function below to see examples about those commands
        public static DSXInstructions GetInstructions(DataPacket data, CsvWriter csv)
        {
            DSXInstructions p = new DSXInstructions();
            CsvData csvRecord = new CsvData();
            //Set the controller to do this for
            int controllerIndex = 0;

            //It should probably always be uniformly stiff
            float avgAccel = (float)Math.Sqrt(_settings.TURN_ACCEL_MOD * (data.Sled.AccelerationX * data.Sled.AccelerationX) + _settings.FORWARD_ACCEL_MOD * (data.Sled.AccelerationZ * data.Sled.AccelerationZ));
            int resistance = (int)Math.Floor(Map(avgAccel, 0, _settings.ACCELRATION_LIMIT, _settings.MIN_THROTTLE_RESISTANCE, _settings.MAX_THROTTLE_RESISTANCE));
            int filteredResistance = EWMA(resistance, lastThrottleResistance, _settings.EWMA_ALPHA_THROTTLE);
            //Initialize our array of instructions
            p.instructions = new Instruction[4];

            if (_logToCsv)
            {
                csvRecord.time = data.Sled.TimestampMS;
                csvRecord.AccelerationX = data.Sled.AccelerationX;
                csvRecord.AccelerationY = data.Sled.AccelerationY;
                csvRecord.AccelerationZ = data.Sled.AccelerationZ;
                csvRecord.Brake = data.Dash.Brake;
                csvRecord.TireCombinedSlipFrontLeft = data.Sled.TireCombinedSlipFrontLeft;
                csvRecord.TireCombinedSlipFrontRight = data.Sled.TireCombinedSlipFrontRight;
                csvRecord.TireCombinedSlipRearLeft = data.Sled.TireCombinedSlipRearLeft;
                csvRecord.TireCombinedSlipRearRight = data.Sled.TireCombinedSlipRearRight;
                csvRecord.CurrentEngineRpm = data.Sled.CurrentEngineRpm;
                csvRecord.AverageAcceleration = avgAccel;
                csvRecord.ThrottleResistance = resistance;
                csvRecord.ThrottleResistance_filtered = filteredResistance;
            }

            lastThrottleResistance = filteredResistance;
            //Set the updates for the right Trigger(Throttle)
            p.instructions[2].type = InstructionType.TriggerUpdate;
            p.instructions[2].parameters = new object[] { controllerIndex, Trigger.Right, TriggerMode.Resistance, 0, filteredResistance };

            if (_verbose)
            {
                Console.WriteLine($"Average Acceleration: {avgAccel}; Throttle Resistance: {filteredResistance}");
            }
            //Update the left(Brake) trigger
            p.instructions[0].type = InstructionType.TriggerUpdate;
            float combinedTireSlip = (Math.Abs(data.Sled.TireCombinedSlipFrontLeft) + Math.Abs(data.Sled.TireCombinedSlipFrontRight) + Math.Abs(data.Sled.TireCombinedSlipRearLeft) + Math.Abs(data.Sled.TireCombinedSlipRearRight)) / 4;



            int freq = 0;
            int filteredFreq = 0;
            //All grip lost, trigger should be loose
            // if (combinedTireSlip > 1)
            // {
            //     //Set left trigger to normal mode(i.e no resistance)
            //     p.instructions[0].parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.Normal, 0, 0 };
            //     if (verbose)
            //     {
            //         Console.WriteLine($"Setting Brake to no resistance");
            //     }
            // }
            // //Some grip lost, begin to vibrate according to the amount of grip lost
            // else 
            //if (combinedTireSlip > settings.GRIP_LOSS_VAL && data.Brake > settings.BRAKE_VIBRATION__MODE_START)
            if (combinedTireSlip < _settings.GRIP_LOSS_VAL && data.Dash.Brake < _settings.BRAKE_VIBRATION__MODE_START)
            {
                freq = _settings.MAX_BRAKE_VIBRATION - (int)Math.Floor(Map(combinedTireSlip, _settings.GRIP_LOSS_VAL, 1, 0, _settings.MAX_BRAKE_VIBRATION));
                resistance = _settings.MIN_BRAKE_STIFFNESS - (int)Math.Floor(Map(data.Dash.Brake, 0, 255, _settings.MAX_BRAKE_STIFFNESS, _settings.MIN_BRAKE_STIFFNESS));
                filteredResistance = EWMA(resistance, lastBrakeResistance, _settings.EWMA_ALPHA_BRAKE);
                filteredFreq = EWMA(freq, lastBrakeFreq, _settings.EWMA_ALPHA_BRAKE_FREQ);
                lastBrakeFreq = filteredFreq;
                lastBrakeResistance = filteredResistance;
                if (filteredFreq <= _settings.MIN_BRAKE_VIBRATION)
                {
                    p.instructions[0].parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.Resistance, 0, 0 };

                }
                else
                {
                    p.instructions[0].parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.VibrateResistance, filteredFreq, filteredResistance, _settings.BRAKE_VIBRATION_START, 0, 0, 0, 0 };

                }
                //Set left trigger to the custom mode VibrateResitance with values of Frequency = freq, Stiffness = 104, startPostion = 76. 
                if (_verbose)
                {
                    Console.WriteLine($"Setting Brake to vibration mode with freq: {filteredFreq}, Resistance: {filteredResistance}");
                }

            }
            else
            {
                //By default, Increasingly resistant to force
                resistance = (int)Math.Floor(Map(data.Dash.Brake, 0, 255, _settings.MIN_BRAKE_RESISTANCE, _settings.MAX_BRAKE_RESISTANCE));
                filteredResistance = EWMA(resistance, lastBrakeResistance, _settings.EWMA_ALPHA_BRAKE);
                lastBrakeResistance = filteredResistance;
                p.instructions[0].parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.Resistance, 0, filteredResistance };

                //Get average tire slippage. This value runs from 0.0 upwards with a value of 1.0 or greater meaning total loss of grip.
            }
            if (_verbose)
            {
                Console.WriteLine($"Brake: {data.Dash.Brake}; Brake Resistance: {filteredResistance}; Tire Slip: {combinedTireSlip}");
            }
            if (_logToCsv)
            {
                csvRecord.BrakeResistance = resistance;
                csvRecord.combinedTireSlip = combinedTireSlip;
                csvRecord.BrakeVibrationFrequency = freq;
                csvRecord.BrakeResistance_filtered = filteredResistance;
                csvRecord.BrakeVibrationFrequency_freq = filteredFreq;

            }
            //Update the light bar
            p.instructions[1].type = InstructionType.RGBUpdate;
            //Currently registers intensity on the green channel based on engnine RPM as a percantage of the maxium. 
            p.instructions[1].parameters = new object[] { controllerIndex, 0, (int)Math.Floor(data.Sled.CurrentEngineRpm / data.Sled.EngineMaxRpm * 255), 0 };
            if (_verbose)
            {
                Console.WriteLine($"Engine RPM: {data.Sled.CurrentEngineRpm}");

            }
            if (_logToCsv)
            {
                csv.WriteRecord(csvRecord);
                csv.NextRecord();
            }
            //Send the commands to DualSenseX
            return p;


        }
        //This is the same test method from the UDPExample in DualSenseX. It just provides a basic overview of the different commands that can be used with DualSenseX.
        static void test(string[] args)
        {
            while (true)
            {
                DSXInstructions p = new DSXInstructions();

                int controllerIndex = 0;

                p.instructions = new Instruction[4];

                // ----------------------------------------------------------------------------------------------------------------------------

                //Normal:
                p.instructions[0].type = InstructionType.TriggerUpdate;
                p.instructions[0].parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.Normal };

                //GameCube:
                p.instructions[0].type = InstructionType.TriggerUpdate;
                p.instructions[0].parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.GameCube };

                //VerySoft:
                p.instructions[0].type = InstructionType.TriggerUpdate;
                p.instructions[0].parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.VerySoft };

                //Soft:
                p.instructions[0].type = InstructionType.TriggerUpdate;
                p.instructions[0].parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.Soft };

                //Hard:
                p.instructions[0].type = InstructionType.TriggerUpdate;
                p.instructions[0].parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.Hard };

                //VeryHard:
                p.instructions[0].type = InstructionType.TriggerUpdate;
                p.instructions[0].parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.VeryHard };

                //Hardest:
                p.instructions[0].type = InstructionType.TriggerUpdate;
                p.instructions[0].parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.Hardest };

                //Rigid:
                p.instructions[0].type = InstructionType.TriggerUpdate;
                p.instructions[0].parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.Rigid };

                //VibrateTrigger needs 1 param of value from 0-255:
                p.instructions[0].type = InstructionType.TriggerUpdate;
                p.instructions[0].parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.VibrateTrigger, 10 };

                //Choppy:
                p.instructions[0].type = InstructionType.TriggerUpdate;
                p.instructions[0].parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.Choppy };

                //Medium:
                p.instructions[0].type = InstructionType.TriggerUpdate;
                p.instructions[0].parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.Medium };

                //VibrateTriggerPulse:
                p.instructions[0].type = InstructionType.TriggerUpdate;
                p.instructions[0].parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.VibrateTriggerPulse };

                //CustomTriggerValue with CustomTriggerValueMode:
                p.instructions[0].type = InstructionType.TriggerUpdate;
                p.instructions[0].parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseAB, 0, 101, 255, 255, 0, 0, 0 };

                //Resistance needs 2 params Start: 0-9 Force:0-8:
                p.instructions[0].type = InstructionType.TriggerUpdate;
                p.instructions[0].parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.Resistance, 0, 8 };

                //Bow needs 4 params Start: 0-8 End:0-8 Force:0-8 SnapForce:0-8:
                p.instructions[0].type = InstructionType.TriggerUpdate;
                p.instructions[0].parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.Bow, 0, 8, 2, 5 };

                //Galloping needs 5 params Start: 0-8 End:0-9 FirstFoot:0-6 SecondFoot:0-7 Frequency:0-255:
                p.instructions[0].type = InstructionType.TriggerUpdate;
                p.instructions[0].parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.Galloping, 0, 9, 2, 4, 10 };

                //SemiAutomaticGun needs 3 params Start: 2-7 End:0-8 Force:0-8:
                p.instructions[0].type = InstructionType.TriggerUpdate;
                p.instructions[0].parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.SemiAutomaticGun, 2, 7, 8 };

                //AutomaticGun needs 3 params Start: 0-8 End:0-9 StrengthA:0-7 StrengthB:0-7 Frequency:0-255 Period 0-2:
                p.instructions[0].type = InstructionType.TriggerUpdate;
                p.instructions[0].parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.AutomaticGun, 0, 8, 10 };

                //AutomaticGun needs 6 params Start: 0-9 Strength:0-8 Frequency:0-255:
                p.instructions[0].type = InstructionType.TriggerUpdate;
                p.instructions[0].parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.Machine, 0, 9, 7, 7, 10, 0 };

                // ----------------------------------------------------------------------------------------------------------------------------

                p.instructions[1].type = InstructionType.RGBUpdate;
                p.instructions[1].parameters = new object[] { controllerIndex, 0, 255, 0 };

                // ----------------------------------------------------------------------------------------------------------------------------

                // PLAYER LED 1-5 true/false state
                p.instructions[2].type = InstructionType.PlayerLED;
                p.instructions[2].parameters = new object[] { controllerIndex, true, false, true, false, true };

                // ----------------------------------------------------------------------------------------------------------------------------

                // TriggerThreshold needs 2 params LeftTrigger:0-255 RightTrigger:0-255
                p.instructions[3].type = InstructionType.TriggerThreshold;
                p.instructions[3].parameters = new object[] { controllerIndex, Trigger.Right, 0 };

                // ----------------------------------------------------------------------------------------------------------------------------

                DSXConnector.Send(p);

                Console.WriteLine("Press any key to send again");
                Console.ReadKey();
            }
        }
        //Maps floats from one range to another.
        public static float Map(float x, float in_min, float in_max, float out_min, float out_max)
        {
            if (x > in_max)
            {
                x = in_max;
            }
            else if (x < in_min)
            {
                x = in_min;
            }
            return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
        }
        static float EWMA(float input, float last, float alpha)
        {
            return alpha * input + (1 - alpha) * last;
        }
        static int EWMA(int input, int last, float alpha)
        {
            return (int)Math.Floor(EWMA(input, (float)last, alpha));
        }

    }
}
