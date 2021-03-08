using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AX.DataRepository.Test
{
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

    public class ObjectY
    {
        [Key]
        public int ObjectYId { get; set; }
        public string Name { get; set; }
    }

    public class ObjectZ
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}