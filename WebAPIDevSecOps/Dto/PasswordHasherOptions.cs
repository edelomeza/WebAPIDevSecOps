namespace WebAPIDevSecOps.Dto
{
    public class PasswordHasherOptions
    {
        public int MemorySize { get; set; } = 65536;
        public int Iterations { get; set; } = 3;
        public int DegreeOfParallelism { get; set; } = 1;
        public int SaltSize { get; set; } = 16;
        public int HashSize { get; set; } = 32;
    }
}
