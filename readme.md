# AX.DataRepository

从 AX.Core 中独立出的数据库操作封装层，基于 Net 5.0 / Dapper 2.0.78

### ⚠ 提醒

* ⚠ 代码未经过详尽测试，不建议使用本项目代码用于商业环境，建议仅用于学习参考。
* ⚠ 目前仅支持 Mysql。
* ⚠ 目前不支持代码多主键。
* ⚠ 目前不支持自增主键。
* ⚠ 目前不支持字段名与数据库列名映射。
* ⚠ 目前不支持忽略某些字段。

### 特色功能

支持从实体类生成建表语句
支持 DisplayAttribute 标注字段注释名称
支持 字符串类型标注 StringLengthAttribute 超过4000长度则使用 text数据库类型

支持单表分页通用查询

### 快速起步

⚠ 具体详细用法请参考测试用例

实体类：

```c#
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


    [Table("ObjectX")]
    public class ObjectX
    {
        [Key]
        public string ObjectXId { get; set; }

        public string Name { get; set; }

        public int Order { get; set; }

        public DateTime dateTime { get; set; }

        public DateTime? NulldateTime { get; set; }

        public decimal Money { get; set; }

        public decimal? NullMoney { get; set; }
    }

``` 

使用：

```c#
IDataRepository DB = new DataRepository(new MySqlConnection("Server=localhost;Database=test;Uid=root;Pwd=root;"));

//生成更新当前数据库结构SQL，传参指定是否执行。
var sql1 = DB.UpdateSchema<ObjectX>(true);

DB.DeleteAll<ObjectX>();

DB.Insert(new ObjectX() { ObjectXId = Guid.NewGuid().ToString(), NulldateTime = DateTime.Now, NullMoney = 45.23M }); 

```