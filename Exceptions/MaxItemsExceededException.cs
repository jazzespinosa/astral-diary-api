namespace AstralDiaryApi.Exceptions
{
    public class MaxItemsExceededException : Exception
    {
        public MaxItemsExceededException(string message)
            : base(message) { }
    }
}
