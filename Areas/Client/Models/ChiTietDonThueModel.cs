using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThueXeDapHoiAn.Models
{
    [Table("ChiTietDonThue")]
    public class ChiTietDonThueModel
    {
        [Key, Column(Order = 0)]
        public int idDonThue { get; set; }

        [Key, Column(Order = 1)]
        public int idXe { get; set; }

        public int soLuong { get; set; }

        public decimal? giaThueTheoGio { get; set; }
        public decimal? giaThueTheoNgay { get; set; }

        [ForeignKey("idDonThue")]
        public virtual DonThueModel DonThue { get; set; }

        [ForeignKey("idXe")]
        public virtual XeModel Xe { get; set; }
    }

}
