namespace AstralDiaryApi.env
{
    public class MyEnvironment
    {
        private readonly IConfiguration _config;

        public MyEnvironment(IConfiguration config)
        {
            _config = config;
        }
    }
}
