namespace ForzaDualSense
{
    public class Settings
    {
        public float GRIP_LOSS_VAL { get; set; } = 0.5f; //The point at which the brake will begin to become choppy
        public int MAX_BRAKE_VIBRATION { get; set; } = 35; //The maximum brake frequency in Hz (avoid over 40). COrrelates to better grip
        public int MIN_BRAKE_VIBRATION { get; set; } = 3; //The Minimum brake frequency in Hz (avoid over 40). Helps avoid clicking in controller
        public float TURN_ACCEL_MOD { get; set; } = 0.5f; //How to scale turning acceleration in determining throttle stiffness.
        public float EWMA_ALPHA_THROTTLE { get; set; } = 0.01f; //Smoothing for Throttle Resistance output. Lower = smoother. Must be greater than 0
        public float EWMA_ALPHA_BRAKE { get; set; } = 1.0f; //Smoothing for Brake Resistance output. Lower = smoother. Must be greater than 0
        public float EWMA_ALPHA_BRAKE_FREQ { get; set; } = 1.0f; //Smoothing for Brake Resistance output. Lower = smoother. Must be greater than 0
        public float FORWARD_ACCEL_MOD { get; set; } = 1.0f;//How to scale Forward acceleration in determining throttle stiffness.
        public int MIN_BRAKE_STIFFNESS { get; set; } = 200; //On a scale of 1-200 with 1 being most stiff
        public int MAX_BRAKE_STIFFNESS { get; set; } = 1; //On a scale of 1-200 with 1 being most stiff
        public int BRAKE_VIBRATION_START { get; set; } = 20; //The position (0-255) at which the brake should feel engaged with low grip surfaces
        public int BRAKE_VIBRATION__MODE_START { get; set; } = 10; //The depression of the brake lever at which the program should switch to vibration mode rather than smooth resistance. This helps to avoid clicking as vibration mode clicks when no force is applied. 
        public int MAX_THROTTLE_RESISTANCE { get; set; } = 6; //The Maximum resistance on the throttle (0-7)
        public int MAX_BRAKE_RESISTANCE { get; set; } = 6;//The Maximum resistance on the Brake (0-7)
        public int MIN_THROTTLE_RESISTANCE { get; set; } = 1;//The Minimum resistance on the throttle (0-7)
        public int MIN_BRAKE_RESISTANCE { get; set; } = 1;//The Minimum resistance on the Brake (0-7)
        public int ACCELRATION_LIMIT { get; set; } = 10; //The upper end acceleration when calculating the throttle resistance. Any acceleration above this will be counted as this value when determining the throttle resistance.
        public bool DISABLE_APP_CHECK { get; set; } = false; //Should we disable the check for running applications?
        public int DSX_PORT { get; set; } = 6969; //Port for DSX Port Listener
        public int FORZA_PORT { get; set; } = 5300; //Port for Forza UDP server
        public bool VERBOSE = false; //Extended console output
        public bool LOG_TO_CSV = false; // enable log to csv
        public string CSV_PATH = @"C:\Temp\fz_data_log.csv"; //path to csv file
        public int CSV_BUFFER_LENGTH = 120;
    }
}