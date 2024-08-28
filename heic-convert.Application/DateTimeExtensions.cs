namespace heic_convert.Application
{
    internal static class DateTimeExtensions
    {
        public static string GetSortableFileSystemName(this DateTime dateTime) => dateTime.ToString("s").Replace(":", "");
    }
}
