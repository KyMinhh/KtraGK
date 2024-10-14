using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public class LoaiSPServices
    {
        private readonly SamPhamDBContext _context; // Đảm bảo bạn đã khởi tạo DbContext

        public LoaiSPServices()
        {
            _context = new SamPhamDBContext(); // Khởi tạo DbContext ở đây
        }

        // Phương thức GetById để lấy khoa theo ID
        public LoaiSP GetById(string facultyId)
        {
            return _context.LoaiSPs.FirstOrDefault(f => f.MaLoai == facultyId);
        }
        public List<LoaiSP> GetAll()
        {
            return _context.LoaiSPs.ToList();
        }
    }
}
