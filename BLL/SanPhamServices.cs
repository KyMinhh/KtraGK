using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public class SanPhamServices
    {
        private readonly SamPhamDBContext _context;
        public List<Sanpham> GetAll()
        {
            return _context.Sanphams.ToList();
        }

        public SanPhamServices()
        {
            // Khởi tạo DbContext, thay thế "DbContext" bằng lớp context thực tế của bạn.
            _context = new SamPhamDBContext(); // Chắc chắn rằng bạn khởi tạo đúng tên DbContext của dự án
        }

        // Phương thức lấy sinh viên theo ID
        public Sanpham GetById(string id)
        {
            using (var context = new SamPhamDBContext())
            {
                return context.Sanphams.FirstOrDefault(sp => sp.MaSP == id);
            }
        }


        // Phương thức thêm mới sinh viên
        public bool Add(Sanpham student)
        {
            _context.Sanphams.Add(student);
            _context.SaveChanges();
            return true;
        }

        // Phương thức cập nhật sinh viên
        public bool Update(Sanpham student)
        {
            var existingStudent = _context.Sanphams.Find(student.MaSP);
            if (existingStudent != null)
            {
                _context.Entry(existingStudent).CurrentValues.SetValues(student);
                _context.SaveChanges();
                return true;
            }
            return false;
        }
        public void Delete(Sanpham product)
        {
            using (var context = new SamPhamDBContext())
            {
                context.Sanphams.Attach(product); // Attach the product to the context
                context.Sanphams.Remove(product); // Remove the product
                context.SaveChanges(); // Save changes to the database
            }
        }
    }
}
