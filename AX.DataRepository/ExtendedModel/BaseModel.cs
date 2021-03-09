using System;
using System.ComponentModel.DataAnnotations;

namespace AX.DataRepository.ExtendedModel
{
    /// <summary>
    /// 支持主键基类
    /// 适合用于全局基类
    /// </summary>
    public class BaseModel
    {
        [Key]
        [Display(Name = "唯一主键")]
        public string Id { get; set; }

        [Display(Name = "创建时间")]
        public DateTime? BaseCreateTime { get; set; }

        /// <summary>
        /// 使用 Guid 初始化主键
        /// 初始化创建时间
        /// </summary>
        public void Init()
        {
            if (string.IsNullOrWhiteSpace(Id)) { Id = Guid.NewGuid().ToString("N"); }
            if (BaseCreateTime == null) { BaseCreateTime = DateTime.Now; }
        }
    }
}