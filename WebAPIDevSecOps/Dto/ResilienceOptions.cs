namespace WebAPIDevSecOps.Dto
{
    public class ResilienceOptions
    {
        public double FailureRatio { get; set; } = 0.2;
        public int SamplingDurationSeconds { get; set; } = 10;
        public int MinimumThroughput { get; set; } = 8;
        public int BreakDurationSeconds { get; set; } = 15;
    }
}
