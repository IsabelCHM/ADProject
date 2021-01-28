﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace ADProject.Models
{
    [Table("Group")]
    public partial class Group
    {
        [Key]
        public int GroupId { get; set; }
        [Required]
        [Column("groupName")]
        [StringLength(50)]
        public string GroupName { get; set; }
        [Column("groupPhoto")]
        [StringLength(500)]
        public string GroupPhoto { get; set; }
        [Column("description")]
        [StringLength(500)]
        public string Description { get; set; }
        [Column("dateCreated", TypeName = "datetime")]
        public DateTime DateCreated { get; set; }
        [Column("isPublished")]
        public bool IsPublished { get; set; }
    }
}