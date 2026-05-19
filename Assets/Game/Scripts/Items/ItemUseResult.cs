namespace NekogamiRanch.Items
{
    public readonly struct ItemUseResult
    {
        private ItemUseResult(bool success, string message)
        {
            Success = success;
            Message = message;
        }

        public bool Success { get; }
        public string Message { get; }

        public static ItemUseResult Failed(string message = null)
        {
            return new ItemUseResult(false, message);
        }

        public static ItemUseResult Succeeded(string message = null)
        {
            return new ItemUseResult(true, message);
        }
    }
}
