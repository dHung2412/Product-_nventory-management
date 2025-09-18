// Application/DTOs/WarehouseDto.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class WarehouseDto
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [Required]
        [StringLength(500)]
        public string Address { get; set; }
    }

    public class CreateWarehouseDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [Required]
        [StringLength(500)]
        public string Address { get; set; }
    }

    public class UpdateWarehouseDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [Required]
        [StringLength(500)]
        public string Address { get; set; }
    }
}