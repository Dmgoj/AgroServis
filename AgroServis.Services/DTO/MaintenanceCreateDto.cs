using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AgroServis.DAL.Enums;
using AgroServis.Services.DTO;

namespace AgroServis.Services.DTOs
{
    public record MaintenanceCreateDto
    {
        [Required(ErrorMessage = "Please select equipment")]
        [Display(Name = "Equipment")]
        public int EquipmentId { get; set; }

        [Required(ErrorMessage = "Maintenance date is required")]
        [Display(Name = "Maintenance Date")]
        [DataType(DataType.Date)]
        public DateTime MaintenanceDate { get; set; } = DateTime.Now;

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
        public MaintenanceType Type { get; set; } = MaintenanceType.Regular;

        [Required(ErrorMessage = "Status is required")]
        [Display(Name = "Status")]
        public MaintenanceStatus Status { get; set; } = MaintenanceStatus.Scheduled;

        [Display(Name = "Cost")]
        [DataType(DataType.Currency)]
        public decimal? Cost { get; set; }

        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        [Display(Name = "Notes")]
        [DataType(DataType.MultilineText)]
        public string? Notes { get; set; }

        public List<EquipmentDto>? AvailableEquipment { get; set; }
    }
}
