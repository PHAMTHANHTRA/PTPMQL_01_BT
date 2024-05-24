using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace MvcMovie.Models
{
[Table("DaiLy")]
    public class DaiLy
    {
        [Key]
        public string? MaDaiLy {get;set;}
        public string? TenDaiLy { get; set; }
        public string DiaChi { get; set; }
        public string NguoiDaiDien { get; set; }
        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Phone Number")]
        [Required(ErrorMessage = "Phone Number Required!")]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$",
                       ErrorMessage = "Entered phone format is not valid.")]
        public string DienThoai { get; set; }
        public string? MaHTPP {get;set;}
        [ForeignKey("MaHTPP")]
        public HeThongPhanPhoi? HeThongPhanPhoi { get; set; }
   
    }
}
