namespace heic_convert.Application
{
    public class InvalidDirectoryConfigurationException : ApplicationException
    {
        public override string Message => "The directory configuration is invalid. WatchDirectory and OutDirectory have to point to different folders.";
    }
}
