namespace NekogamiRanch.Toys
{
    public readonly struct ToyUseResult
    {
        private ToyUseResult(bool success, string message)
        {
            Success = success;
            Message = message;
        }

        public bool Success { get; }
        public string Message { get; }

        public static ToyUseResult Failed(string message = null)
        {
            return new ToyUseResult(false, message);
        }

        public static ToyUseResult Succeeded(string message = null)
        {
            return new ToyUseResult(true, message);
        }
    }
}
