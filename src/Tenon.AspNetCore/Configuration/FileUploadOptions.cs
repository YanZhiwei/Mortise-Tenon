using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tenon.AspNetCore.Configuration
{
    /// <summary>
    /// 文件上传配置选项
    /// </summary>
    public sealed class FileUploadOptions
    {
        /// <summary>
        /// 最大请求体大小（默认100MB）
        /// </summary>
        public long MaxRequestBodySize { get; set; } = 100 * 1024 * 1024;

        /// <summary>
        /// 单个文件最大大小（默认100MB）
        /// </summary>
        public long MaxFileSize { get; set; } = 100 * 1024 * 1024;

        /// <summary>
        /// 允许的文件扩展名
        /// </summary>
        public HashSet<string> AllowedExtensions { get; set; } = [];

        /// <summary>
        /// 允许的文件内容类型
        /// </summary>
        public HashSet<string> AllowedContentTypes { get; set; } = [];

        /// <summary>
        /// 是否验证文件名（默认为true）
        /// </summary>
        public bool ValidateFileName { get; set; } = true;

        internal void Validate()
        {
            if (MaxRequestBodySize <= 0)
                throw new InvalidOperationException($"{nameof(MaxRequestBodySize)} must be greater than 0");

            if (MaxFileSize <= 0)
                throw new InvalidOperationException($"{nameof(MaxFileSize)} must be greater than 0");

            if (MaxFileSize > MaxRequestBodySize)
                throw new InvalidOperationException($"{nameof(MaxFileSize)} cannot be greater than {nameof(MaxRequestBodySize)}");
        }
    }
}
