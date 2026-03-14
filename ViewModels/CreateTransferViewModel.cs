using System.ComponentModel.DataAnnotations;

namespace ISDN.ViewModels
{
    public class CreateTransferViewModel
    {
        [Required]
        [Display(Name = "Source RDC")]
        public int FromRdcId { get; set; }

        [Required]
        [Display(Name = "Destination RDC")]
        public int ToRdcId { get; set; }

        [Required]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
    }
}