using System;
using System.Collections.Generic;
using System.Configuration;

namespace MrCMS.Settings
{
    public static class FileTypeUploadSettings
    {
        private const string DefaultAllowedFileTypes =
            ".gif,.jpeg,.jpg,.png,.webp,.rar,.zip,.pdf,.mp3,.mp4,.wmv,.doc,.docx,.xls,.xlsx,.ppt,.pptx,.avi,.mpg,.wav,.mov,.wma,.webm,.ogv,.mpeg,.7z,.txt,.csv,.svg";

        public static string AllowedFileTypes => ConfigurationManager.AppSettings["allowed-file-types"] ?? DefaultAllowedFileTypes;

        public static IEnumerable<string> AllowedFileTypeList
        {
            get { return Array.ConvertAll(AllowedFileTypes.Split(','), p => p.Trim());  }
        }
    }
}