using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DAL
{
    public class BookImage
    {
        [Key]
        public Guid BookImageId { get; set; }
        public byte[] Content { get; set; }

        public Book Book { get; set; }

    }
}
