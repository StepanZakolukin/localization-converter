namespace LocalizationConverter.Core.Constants;

internal static class ResxConstants
{
    // Имена XML-элементов и атрибутов (тегов)
    public static class Nodes
    {
        public const string Root = "root";
        public const string Header = "resheader";
        public const string Data = "data";
        public const string Value = "value";
        public const string NameAttribute = "name";
        public const string SpaceAttribute = "xml:space";
        public const string PreserveValue = "preserve";
    }

    // Имена ключей заголовков
    public static class Keys
    {
        public const string MimeType = "resmimetype";
        public const string Version = "version";
        public const string Reader = "reader";
        public const string Writer = "writer";
    }

    // Значения для заголовков Microsoft
    public static class Values
    {
        public const string MimeType = "text/microsoft-resx";
        public const string Version = "2.0";
        public const string ReaderType = "System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
        public const string WriterType = "System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
    }
}
