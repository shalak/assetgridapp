﻿using assetgrid_backend.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetgrid_backend.models.MetaFields
{
    public class MetaFieldValue<T>
    {
        public int Id { get; set; }

        public int FieldId { get; set; }
        public virtual MetaField Field { get; set; } = default!;


        public int ObjectId { get; set; }
        public virtual T Object { get; set; } = default!;
    }

    public class MetaTextLine<T> : MetaFieldValue<T>
    {
        [MaxLength(255)]
        public string Value { get; set; } = null!;
    }

    public class MetaTextLong<T> : MetaFieldValue<T>
    {
        public string Value { get; set; } = null!;
    }

    public class MetaTransaction<T> : MetaFieldValue<T>
    {
        public int ValueId { get; set; }
        public virtual Transaction Value { get; set; } = null!;
    }

    public class MetaAccount<T> : MetaFieldValue<T>
    {
        public int ValueId { get; set; }
        public virtual Account Value { get; set; } = null!;
    }

    public class MetaAttachment<T> : MetaFieldValue<T>
    {
        public string Path { get; set; } = null!;
        public int FileSize { get; set; }
    }

    public class MetaBoolean<T> : MetaFieldValue<T>
    {
        public bool Value { get; set; }
    }

    public class MetaNumber<T> : MetaFieldValue<T>
    {
        public long Value { get; set; }
    }
}
