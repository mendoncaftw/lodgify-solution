namespace ApiApplication
{
    public class AppSettings
    {
        public MoviesApiSettings MoviesApi { get; set; }
    }

    public class MoviesApiSettings
    {
        public string ApiKey { get; set; }
        public string ApiHeaderName { get; set; }
        public string BaseAddress { get; set; }
        public CacheSettings Cache { get; set; }

        public class CacheSettings
        {
            public bool Enabled { get; set; }
            public int TtlInSeconds { get; set; }
        }
    }
}
