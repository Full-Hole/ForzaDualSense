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
        public static DSXInstructions GetInstructions(TelemetryData data, CsvWriter csv)
        {
            DSXInstructions p = new DSXInstructions();
            CsvData csvRecord = new CsvData();
            //Set the controller to do this for
            int controllerIndex = 0;

            //It should probably always be uniformly stiff
            float avgAccel = (float)Math.Sqrt(_settings.TURN_ACCEL_MOD * (data.AccelerationX * data.AccelerationX) + _settings.FORWARD_ACCEL_MOD * (data.AccelerationZ * data.AccelerationZ));
            int resistance = (int)Math.Floor(Map(avgAccel, 0, _settings.ACCELRATION_LIMIT, _settings.MIN_THROTTLE_RESISTANCE, _settings.MAX_THROTTLE_RESISTANCE));
            int filteredResistance = EWMA(resistance, lastThrottleResistance, _settings.EWMA_ALPHA_THROTTLE);
            //Initialize our array of instructions
            p.instructions = new Instruction[4];

            if (_logToCsv)
            {
                csvRecord.time = data.TimestampMS;
                csvRecord.AccelerationX = data.AccelerationX;
                csvRecord.AccelerationY = data.AccelerationY;
                csvRecord.AccelerationZ = data.AccelerationZ;
                csvRecord.Brake = data.Brake;
                csvRecord.TireCombinedSlipFrontLeft = data.TireCombinedSlipFrontLeft;
                csvRecord.TireCombinedSlipFrontRight = data.TireCombinedSlipFrontRight;
                csvRecord.TireCombinedSlipRearLeft = data.TireCombinedSlipRearLeft;
                csvRecord.TireCombinedSlipRearRight = data.TireCombinedSlipRearRight;
                csvRecord.CurrentEngineRpm = data.CurrentEngineRpm;
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
            float combinedTireSlip = (Math.Abs(data.TireCombinedSlipFrontLeft) + Math.Abs(data.TireCombinedSlipFrontRight) + Math.Abs(data.TireCombinedSlipRearLeft) + Math.Abs(data.TireCombinedSlipRearRight)) / 4;



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
            if (combinedTireSlip < _settings.GRIP_LOSS_VAL && data.Brake < _settings.BRAKE_VIBRATION__MODE_START)
            {
                freq = _settings.MAX_BRAKE_VIBRATION - (int)Math.Floor(Map(combinedTireSlip, _settings.GRIP_LOSS_VAL, 1, 0, _settings.MAX_BRAKE_VIBRATION));
                resistance = _settings.MIN_BRAKE_STIFFNESS - (int)Math.Floor(Map(data.Brake, 0, 255, _settings.MAX_BRAKE_STIFFNESS, _settings.MIN_BRAKE_STIFFNESS));
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
                resistance = (int)Math.Floor(Map(data.Brake, 0, 255, _settings.MIN_BRAKE_RESISTANCE, _settings.MAX_BRAKE_RESISTANCE));
                filteredResistance = EWMA(resistance, lastBrakeResistance, _settings.EWMA_ALPHA_BRAKE);
                lastBrakeResistance = filteredResistance;
                p.instructions[0].parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.Resistance, 0, filteredResistance };

                //Get average tire slippage. This value runs from 0.0 upwards with a value of 1.0 or greater meaning total loss of grip.
            }
            if (_verbose)
            {
                Console.WriteLine($"Brake: {data.Brake}; Brake Resistance: {filteredResistance}; Tire Slip: {combinedTireSlip}");
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
            p.instructions[1].parameters = new object[] { controllerIndex, 0, (int)Math.Floor(data.CurrentEngineRpm / data.EngineMaxRpm * 255), 0 };
            if (_verbose)
            {
                Console.WriteLine($"Engine RPM: {data.CurrentEngineRpm}");

            }
            if (_logToCsv)
            {
                csv.WriteRecord(csvRecord);
                csv.NextRecord();
            }
            //Send the commands to DualSenseX
            return p;


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
