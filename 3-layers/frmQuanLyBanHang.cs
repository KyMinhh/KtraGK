using BLL;
using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace _3_layers
{
    public partial class frmQuanLyBanHang : Form
    {
        private readonly SanPhamServices studentService = new SanPhamServices();
        private readonly LoaiSPServices facultyService = new LoaiSPServices();
        private List<Sanpham> students;

        public frmQuanLyBanHang()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                setGridViewStyle(dgvStudent);

                // Fill Faculty Combobox and Bind Grid with Student Data
                var faculties = facultyService.GetAll();
                var students = studentService.GetAll();
                FillFacultyCombobox(faculties);
                BindGrid(students);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            // Enable btnLuu and btnKhongLuu when the form loads
            btnLuu.Enabled = false;
            btnKhongLuu.Enabled = false;
            cmbFaculty.SelectedIndex = -1;
        }

        private void FillFacultyCombobox(List<LoaiSP> faculties)
        {
            cmbFaculty.DataSource = faculties;
            cmbFaculty.DisplayMember = "TenLoai";
            cmbFaculty.ValueMember = "MaLoai";
        }

        private void BindGrid(List<Sanpham> students)
        {
            dgvStudent.Rows.Clear();
            foreach (var student in students)
            {
                int index = dgvStudent.Rows.Add();
                dgvStudent.Rows[index].Cells[0].Value = student.MaSP;
                dgvStudent.Rows[index].Cells[1].Value = student.TenSP;
                dgvStudent.Rows[index].Cells[2].Value = student.NgayNhap.ToString();
                dgvStudent.Rows[index].Cells[3].Value = student.LoaiSP.TenLoai;
            }
        }

        public void setGridViewStyle(DataGridView dgview)
        {
            dgview.BorderStyle = BorderStyle.None;
            dgview.DefaultCellStyle.SelectionBackColor = Color.DarkTurquoise;
            dgview.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgview.BackgroundColor = Color.White;
            dgview.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            // Display a confirmation dialog
            DialogResult result = MessageBox.Show("Are you sure you want to exit?",
                                                  "Exit Confirmation",
                                                  MessageBoxButtons.YesNo,
                                                  MessageBoxIcon.Warning);

            // Check the user's choice
            if (result == DialogResult.Yes)
            {
                // Close the form if the user clicked "Yes"
                this.Close();
            }
            // If "No" is clicked, do nothing and return to the form
        }


        private void btnThem_Click(object sender, EventArgs e)
        {
            try
            {
                // 1. Retrieve user input from the form
                string maSP = txtMa.Text.Trim();
                string tenSP = txtTen.Text.Trim();
                DateTime ngayNhap;
                bool isDateValid = DateTime.TryParse(dateTimePicker1.Text, out ngayNhap);
                string maLoai = cmbFaculty.SelectedValue.ToString();

                // 2. Validate input
                if (string.IsNullOrEmpty(maSP) || string.IsNullOrEmpty(tenSP) || !isDateValid || string.IsNullOrEmpty(maLoai))
                {
                    MessageBox.Show("Vui lòng nhập đầy đủ thông tin!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 3. Create a new Sanpham object
                Sanpham newSanpham = new Sanpham
                {
                    MaSP = maSP,
                    TenSP = tenSP,
                    NgayNhap = ngayNhap,
                    MaLoai = maLoai
                };

                // 4. Insert new product into the database via the service layer
                bool isSuccess = studentService.Add(newSanpham); // Ensure AddSanpham() returns a bool
                if (isSuccess)
                {
                    MessageBox.Show("Thêm sản phẩm thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // 5. Refresh the ListView and clear input fields
                    RefreshListView();
                    ClearInputFields();
                }
                else
                {
                    MessageBox.Show("Thêm sản phẩm thất bại!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            btnLuu.Enabled = true;
            btnKhongLuu.Enabled = true;
        }

        private void ClearInputFields()
        {
            txtMa.Clear();
            txtTen.Clear();
            dateTimePicker1.Value = DateTime.Now;
            cmbFaculty.SelectedIndex = -1; // Reset combobox
        }

        private void RefreshListView()
        {
            var sanPhams = studentService.GetAll(); // Get updated product list
            dgvStudent.Rows.Clear(); // Clear existing rows in DataGridView

            foreach (var sp in sanPhams)
            {
                dgvStudent.Rows.Add(sp.MaSP, sp.TenSP, sp.NgayNhap.ToString(), sp.LoaiSP.TenLoai); // Use Rows.Add instead of Items
            }
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if a product is selected in the DataGridView
                if (dgvStudent.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Please select a product to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Get the selected product's ID from the DataGridView
                string selectedProductId = dgvStudent.SelectedRows[0].Cells["colMa"].Value.ToString();

                // Retrieve the corresponding product from the service by its ID
                Sanpham productToUpdate = studentService.GetById(selectedProductId);

                if (productToUpdate == null)
                {
                    MessageBox.Show("Product not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Update product properties with the new values from the form controls
                productToUpdate.TenSP = txtTen.Text;
                productToUpdate.NgayNhap = dateTimePicker1.Value;
                productToUpdate.MaLoai = cmbFaculty.SelectedValue.ToString();

                // Update the product in the database through the service
                bool isUpdated = studentService.Update(productToUpdate);

                if (isUpdated)
                {
                    MessageBox.Show("Product updated successfully.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Reload the product list into the DataGridView
                    var sanPhams = studentService.GetAll();
                    BindGrid(sanPhams); // Ensure this method correctly binds the DataGridView

                    // Optionally clear the selection
                    dgvStudent.ClearSelection(); // Clear any selected rows in the DataGridView

                    // Clear input fields after update
                    ClearInputFields();
                }
                else
                {
                    MessageBox.Show("Error occurred while updating the product.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Enable the Save and Cancel buttons for the next operation
            btnLuu.Enabled = true;
            btnKhongLuu.Enabled = true;
        }


        private void dgvStudent_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Ensure the user has clicked on a valid cell, not the header or empty space
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                try
                {
                    // Get the selected row from DataGridView
                    DataGridViewRow selectedRow = dgvStudent.Rows[e.RowIndex];

                    // Extract product data from the selected row's cells
                    string productId = selectedRow.Cells["colMa"].Value.ToString();
                    string productName = selectedRow.Cells["colTen"].Value.ToString();
                    DateTime productDate = DateTime.Parse(selectedRow.Cells["colNgay"].Value.ToString());
                    string productCategory = selectedRow.Cells["colLoai"].Value.ToString();

                    // Display the data in the form controls
                    txtMa.Text = productId;
                    txtTen.Text = productName;
                    dateTimePicker1.Value = productDate;

                    // Assuming comboBox holds LoaiSP with ValueMember = MaLoai, find and select the corresponding category
                    foreach (LoaiSP category in cmbFaculty.Items)
                    {
                        if (category.TenLoai == productCategory)
                        {
                            cmbFaculty.SelectedItem = category;
                            break;
                        }
                    }

                    // Enable buttons for edit/delete since we have selected a row
                    btnSua.Enabled = true;
                    btnXoa.Enabled = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnTim_Click(object sender, EventArgs e)
        {
            // Get the search term from the search TextBox
            string searchTerm = txtTimKiem.Text.Trim().ToLower();

            // Clear any existing selection in DataGridView
            dgvStudent.ClearSelection();

            // Iterate through rows of DataGridView to find matching product names
            foreach (DataGridViewRow row in dgvStudent.Rows)
            {
                // Assuming the product name is in the second column (index 1)
                if (row.Cells[1].Value != null &&
                    row.Cells[1].Value.ToString().ToLower().Contains(searchTerm))
                {
                    // If a match is found, select the row and scroll to it
                    row.Selected = true;
                    dgvStudent.FirstDisplayedScrollingRowIndex = row.Index;
                }
                else
                {
                    row.Selected = false; // Deselect rows that do not match
                }
            }

            // Show a message if no match is found
            if (dgvStudent.SelectedRows.Count == 0)
            {
                MessageBox.Show("No matching product found.", "Search Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnKhongLuu_Click(object sender, EventArgs e)
        {
            // Code để hủy bỏ thay đổi, ví dụ như tải lại dữ liệu ban đầu
            MessageBox.Show("Các thay đổi đã được hủy.");

            // Tắt nút Lưu và Không Lưu sau khi hủy
            btnLuu.Enabled = false;
            btnKhongLuu.Enabled = false;
        }

        private void btnLuu_Click(object sender, EventArgs e)
        {
            try
            {
                // Kiểm tra thông tin sản phẩm trước khi lưu
                if (string.IsNullOrWhiteSpace(txtMa.Text) || string.IsNullOrWhiteSpace(txtTen.Text))
                {
                    MessageBox.Show("Vui lòng điền đủ thông tin sản phẩm!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Tạo một đối tượng sản phẩm mới hoặc cập nhật sản phẩm hiện tại
                Sanpham product = new Sanpham
                {
                    MaSP = txtMa.Text,
                    TenSP = txtTen.Text,
                    NgayNhap = DateTime.Parse(dateTimePicker1.Text), // Giả sử format hợp lệ
                    LoaiSP = (LoaiSP)cmbFaculty.SelectedItem // Lấy loại sản phẩm từ combobox
                };

                // Kiểm tra nếu đang thêm mới hay cập nhật
                if (isAdding) // isAdding là biến boolean để xác định thao tác
                {
                    studentService.Add(product); // Gọi phương thức thêm sản phẩm
                }
                else
                {
                    studentService.Update(product); // Gọi phương thức cập nhật sản phẩm
                }

                // Tải lại dữ liệu sau khi lưu thành công
                LoadData(); // Tải lại dữ liệu từ cơ sở dữ liệu hoặc danh sách

                // Tắt nút Lưu và Không Lưu sau khi lưu
                btnLuu.Enabled = false;
                btnKhongLuu.Enabled = false;

                // Thông báo cho người dùng
                MessageBox.Show("Dữ liệu đã được lưu thành công.");
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ và hiển thị thông báo lỗi
                MessageBox.Show($"Lỗi khi lưu dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool isAdding; // Variable to track if we are adding a new product

        private void LoadData()
        {
            var products = studentService.GetAll(); // Lấy danh sách sản phẩm
            BindGrid(products); // Giả sử bạn có phương thức BindGrid() để gán dữ liệu vào DataGridView
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            // Check if a row is selected in the DataGridView
            if (dgvStudent.SelectedRows.Count > 0)
            {
                // Get the selected row
                var selectedRow = dgvStudent.SelectedRows[0];
                var productId = selectedRow.Cells["colMa"].Value.ToString(); // Adjust the cell name if necessary

                // Confirm deletion
                var result = MessageBox.Show("Bạn có chắc chắn muốn xóa sản phẩm này?",
                                              "Xác nhận xóa",
                                              MessageBoxButtons.YesNo,
                                              MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        // Retrieve the product entity from the service
                        var productToDelete = studentService.GetById(productId); // Ensure GetById is implemented

                        if (productToDelete != null)
                        {
                            // Call your service to delete the product
                            studentService.Delete(productToDelete); // Ensure Delete method uses the product entity

                            // Refresh the DataGridView after deletion
                            var updatedProducts = studentService.GetAll(); // Fetch updated list
                            BindGrid(updatedProducts); // Assuming BindGrid updates the DataGridView

                            // Optionally, clear input fields
                            txtMa.Clear();
                            txtTen.Clear();
                            cmbFaculty.SelectedIndex = -1; // Reset combobox selection

                            // Disable Save and Cancel buttons since the action is complete
                            btnLuu.Enabled = false;
                            btnKhongLuu.Enabled = false;

                            MessageBox.Show("Sản phẩm đã được xóa thành công.");
                        }
                        else
                        {
                            MessageBox.Show("Sản phẩm không tồn tại.");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi xóa sản phẩm: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn sản phẩm cần xóa.");
            }
        }

        private bool isEditing = false; 
        private string currentProductId;

        enum OperationType
        {
            None,
            Add,
            Edit,
            Delete
        }

        private OperationType currentOperation = OperationType.None;




    }
}
  
