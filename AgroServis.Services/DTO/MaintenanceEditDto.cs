using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AgroServis.DAL.Enums;
using AgroServis.Services.DTO;

namespace AgroServis.Services.DTOs
{
    public record MaintenanceEditDto
    {
        [Required]
        public int Id { get; set; }

        public int EquipmentId { get; set; }

        [Display(Name = "Equipment")]
        public string EquipmentName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Maintenance date is required")]
        [Display(Name = "Maintenance Date")]
        [DataType(DataType.Date)]
        public DateTime MaintenanceDate { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(
            500,
            MinimumLength = 10,
            ErrorMessage = "Description must be between 10 and 500 characters"
        )]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Maintenance type is required")]
        [Display(Name = "Maintenance Type")]
        public MaintenanceType Type { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [Display(Name = "Status")]
        public MaintenanceStatus Status { get; set; }

        [Display(Name = "Cost")]
        [DataType(DataType.Currency)]
        public decimal? Cost { get; set; }

        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        [Display(Name = "Notes")]
        [DataType(DataType.MultilineText)]
        public string? Notes { get; set; }

        [StringLength(100, ErrorMessage = "Performer name cannot exceed 100 characters")]
        [Display(Name = "Performed By")]
        public string? PerformedBy { get; set; }
    }
}
