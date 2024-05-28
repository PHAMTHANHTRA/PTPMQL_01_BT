using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MVC.Data;
using MVC.Models;
using OfficeOpenXml;

namespace MVC.Controllers
{
    public class SinhVienController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SinhVienController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Create()
        {
            List<Book> books = _context.Book.ToList();
            ViewBag.BookList = books;
            return View();
        }

        /// <summary>
        /// Tạo mới sinh viên mượn sách
        /// </summary>
        /// <param name="sinhVien"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdSV,NameSV,Khoa,ClassName,PhoneSV,IdBook,BorrowDate,PayDate")] SinhVien sinhVien)
        {
            // Lấy thông tin sách từ bảng Book dựa trên IdBook
            var book = await _context.Book.FindAsync(sinhVien.IdBook);
            if (ModelState.IsValid)
            {
                // Giảm số lượng sách đi 1
                book.Number--;

                // Cập nhật thông tin sách trong cơ sở dữ liệu
                _context.Update(book);

                // Lưu thay đổi vào cơ sở dữ liệu
                await _context.SaveChangesAsync();

                // Insert thông tin sinh viên mượn sách
                sinhVien.Status = 0;
                _context.Add(sinhVien);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(sinhVien);
        }
        /// <summary>
        /// Xem chi tiết
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Details(int id)
        {
            if (id == 0 || _context.SinhVien == null)
            {
                return NotFound();
            }

            var sinhVien = await _context.SinhVien
                .Where(m => m.Id == id)
                .Join(
                    _context.Book,
                    sv => sv.IdBook,
                    book => book.IdBook,
                    (sv, book) => new SinhVienWithBookViewModel
                    {
                        IdSV = sv.IdSV,
                        NameSV = sv.NameSV,
                        Khoa = sv.Khoa,
                        ClassName = sv.ClassName,
                        PhoneSV = sv.PhoneSV,
                        BorrowDate = sv.BorrowDate,
                        PayDate = sv.PayDate,
                        NameBook = book.NameBook,
                        Status = sv.Status
                    }
                )
                .FirstOrDefaultAsync();

            if (sinhVien == null)
            {
                return NotFound();
            }

            return View(sinhVien);
        }

        // Delete
        public async Task<IActionResult> Delete(int id)
        {
            if (id == 0 || _context.SinhVien == null)
            {
                return NotFound();
            }

            var sinhVien = await _context.SinhVien
                .Where(m => m.Id == id)
                .Join(
                    _context.Book,
                    sv => sv.IdBook,
                    book => book.IdBook,
                    (sv, book) => new SinhVienWithBookViewModel
                    {
                        IdSV = sv.IdSV,
                        NameSV = sv.NameSV,
                        Khoa = sv.Khoa,
                        ClassName = sv.ClassName,
                        PhoneSV = sv.PhoneSV,
                        BorrowDate = sv.BorrowDate,
                        PayDate = sv.PayDate,
                        NameBook = book.NameBook,
                        Status = sv.Status
                    }
                )
                .FirstOrDefaultAsync();

            if (sinhVien == null)
            {
                return NotFound();
            }

            return View(sinhVien);
        }

        // POST: NhanVien/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.SinhVien == null)
            {
                return Problem("Không tìm thấy nhân viên");
            }
            var nhanVien = await _context.SinhVien.FindAsync(id);
            if (nhanVien != null)
            {
                _context.SinhVien.Remove(nhanVien);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Index(SinhVienWithBookViewModel searchModel)
        {
            if (searchModel.ClearFilter)
            {
                // Đặt lại mô hình tìm kiếm về trạng thái ban đầu
                searchModel = new SinhVienWithBookViewModel();
            }
            List<SinhVienWithBookViewModel> result = await (
            from sinhVien in _context.SinhVien
            join Book in _context.Book on sinhVien.IdBook equals Book.IdBook
            select new SinhVienWithBookViewModel
            {
                Id = sinhVien.Id,
                IdSV = sinhVien.IdSV,
                NameSV = sinhVien.NameSV,
                Khoa = sinhVien.Khoa,
                ClassName = sinhVien.ClassName,
                PhoneSV = sinhVien.PhoneSV,
                BorrowDate = sinhVien.BorrowDate,
                PayDate = sinhVien.PayDate,
                NameBook = Book.NameBook,
                Status = sinhVien.Status
            }
            ).ToListAsync();

            // Áp dụng điều kiện tìm kiếm
            if (!string.IsNullOrEmpty(searchModel.NameSV))
            {
                result = result.Where(s => s.NameSV.Contains(searchModel.NameSV)).ToList();
            }

            if (!string.IsNullOrEmpty(searchModel.IdSV))
            {
                result = result.Where(s => s.IdSV == searchModel.IdSV).ToList();
            }
            if (searchModel.Status == 0)
            {

            }
            else if (searchModel.Status == 1)
            {
                result = result.Where(s => s.Status == 0).ToList();
            }
            else if (searchModel.Status == 2)
            {
                result = result.Where(s => s.Status == 1).ToList();
            }

            return result.Count > 0 ? View(result) : View();
        }
        public async Task<IActionResult> ExportToExcel()
        {
            // Lấy danh sách sinh viên từ cơ sở dữ liệu
            List<SinhVienWithBookViewModel> sinhVienList = await (
                from sinhVien in _context.SinhVien
                join Book in _context.Book on sinhVien.IdBook equals Book.IdBook
                select new SinhVienWithBookViewModel
                {
                    IdSV = sinhVien.IdSV,
                    NameSV = sinhVien.NameSV,
                    Khoa = sinhVien.Khoa,
                    ClassName = sinhVien.ClassName,
                    PhoneSV = sinhVien.PhoneSV,
                    PayDate = sinhVien.PayDate,
                    BorrowDate = sinhVien.BorrowDate,
                    NameBook = Book.NameBook,
                    Status = sinhVien.Status
                }
            ).ToListAsync();

            // Tạo một đối tượng ExcelPackage
            using (var package = new ExcelPackage())
            {
                // Tạo một trang tính mới
                var worksheet = package.Workbook.Worksheets.Add("SinhVien");

                // Ghi dữ liệu vào trang tính
                worksheet.Cells["A1"].Value = "Mã Sinh Viên";
                worksheet.Cells["B1"].Value = "Tên Sinh Viên";
                worksheet.Cells["C1"].Value = "Khoa";
                worksheet.Cells["D1"].Value = "Lớp";
                worksheet.Cells["E1"].Value = "Số Điện Thoại";
                worksheet.Cells["F1"].Value = "Ngày Mượn";
                worksheet.Cells["G1"].Value = "Hạn Trả";
                worksheet.Cells["H1"].Value = "Sách Mượn";
                worksheet.Cells["I1"].Value = "Tình trạng";

                var row = 2;
                foreach (var sinhVien in sinhVienList)
                {
                    worksheet.Cells[$"A{row}"].Value = sinhVien.IdSV;
                    worksheet.Cells[$"B{row}"].Value = sinhVien.NameSV;
                    worksheet.Cells[$"C{row}"].Value = sinhVien.Khoa;
                    worksheet.Cells[$"D{row}"].Value = sinhVien.ClassName;
                    worksheet.Cells[$"E{row}"].Value = sinhVien.PhoneSV;
                    worksheet.Cells[$"F{row}"].Value = sinhVien.BorrowDate?.ToString("yyyy-MM-dd"); // Định dạng ngày mượn
                    worksheet.Cells[$"G{row}"].Value = sinhVien.PayDate?.ToString("yyyy-MM-dd"); // Định dạng ngày trả (nếu có)
                    worksheet.Cells[$"H{row}"].Value = sinhVien.NameBook;
                    if (sinhVien.Status == 0)
                    {
                        worksheet.Cells[$"I{row}"].Value = "Đang mượn sách";
                    }
                    else if (sinhVien.Status == 1)
                    {
                        worksheet.Cells[$"I{row}"].Value = "Đã trả sách";
                    }
                    row++;
                }

                // Đặt kích thước cột tự động dựa trên nội dung
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Lưu ExcelPackage vào một mảng byte
                var excelBytes = package.GetAsByteArray();

                // Xác định loại file MIME
                var mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                // Trả về FileResult
                return File(excelBytes, mimeType, "SinhVien.xlsx");
            }
        }

        public async Task<IActionResult> ListSVPayLate(SinhVienWithBookViewModel searchModel)
        {
            if (searchModel.ClearFilter)
            {
                // Đặt lại mô hình tìm kiếm về trạng thái ban đầu
                searchModel = new SinhVienWithBookViewModel();
            }

            // Lấy danh sách sinh viên từ cơ sở dữ liệu
            List<SinhVienWithBookViewModel> result = await (
                from sinhVien in _context.SinhVien
                join Book in _context.Book on sinhVien.IdBook equals Book.IdBook
                select new SinhVienWithBookViewModel
                {
                    IdSV = sinhVien.IdSV,
                    NameSV = sinhVien.NameSV,
                    Khoa = sinhVien.Khoa,
                    ClassName = sinhVien.ClassName,
                    PhoneSV = sinhVien.PhoneSV,
                    BorrowDate = sinhVien.BorrowDate,
                    PayDate = sinhVien.PayDate,
                    NameBook = Book.NameBook,
                    DelayDays = sinhVien.PayDate.HasValue && sinhVien.BorrowDate.HasValue ? (int)(DateTime.Now - sinhVien.PayDate.Value).TotalDays : 0
                }
            ).Where(s => s.PayDate < DateTime.Now).ToListAsync();

            // Áp dụng điều kiện tìm kiếm nếu có
            if (!string.IsNullOrEmpty(searchModel.NameSV))
            {
                result = result.Where(s => s.NameSV.Contains(searchModel.NameSV)).ToList();
            }

            if (!string.IsNullOrEmpty(searchModel.IdSV))
            {
                result = result.Where(s => s.IdSV == searchModel.IdSV).ToList();
            }

            return result.Count > 0 ? View(result) : View();
        }

        public async Task<IActionResult> ThongKe()
        {
            // Lấy tổng số sinh viên mượn sách từ cơ sở dữ liệu
            int totalStudents = await _context.SinhVien.CountAsync();
            // Lấy tổng số sinh viên đang mượn sách từ cơ sở dữ liệu
            int totalBorrowingStudents = await _context.SinhVien.CountAsync(s => s.Status == 0);
            // Lấy tổng số sinh viên đã mượn sách từ cơ sở dữ liệu
            int totalPayingStudents = await _context.SinhVien.CountAsync(s => s.Status == 1);

            // Gán giá trị vào ViewBag để truyền đến view
            ViewBag.TotalStudents = totalStudents;
            ViewBag.TotalBorrowingStudents = totalBorrowingStudents;
            ViewBag.TotalPayingStudents = totalPayingStudents;


            return View();
        }

        // GET: SanPham/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            if (id == 0 || _context.SinhVien == null)
            {
                return NotFound();
            }

            var sinhVien = await _context.SinhVien
                .Where(m => m.Id == id)
                .Join(
                    _context.Book,
                    sv => sv.IdBook,
                    book => book.IdBook,
                    (sv, book) => new SinhVienWithBookViewModel
                    {
                        IdSV = sv.IdSV,
                        NameSV = sv.NameSV,
                        Khoa = sv.Khoa,
                        ClassName = sv.ClassName,
                        PhoneSV = sv.PhoneSV,
                        BorrowDate = sv.BorrowDate,
                        PayDate = sv.PayDate,
                        NameBook = book.NameBook,
                        IdBook = book.IdBook,
                        Status = sv.Status
                    }
                )
                .FirstOrDefaultAsync();

            if (sinhVien == null)
            {
                return NotFound();
            }
            List<Book> books = _context.Book.ToList();
            ViewBag.BookList = books;
            return View(sinhVien);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,IdSV,IdBook,Khoa,NameSV,PhoneSV,ClassName,BorrowDate,PayDate,Status")] SinhVienWithBookViewModel sinhVien)
        {
            if (id != sinhVien.Id)
            {
                return NotFound();
            }
            SinhVien sinhVienResult = ConvertData(sinhVien);
            if (ModelState.IsValid)
            {
                _context.Update(sinhVienResult);
                var newBook = await _context.Book.FindAsync(sinhVienResult.IdBook);
                if (newBook != null)
                {
                    newBook.Number++;
                    _context.Update(newBook);
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(sinhVien);
        }

        public SinhVien ConvertData(SinhVienWithBookViewModel sinhVienWithBookViewModel)
        {
            SinhVien sinhVien = new SinhVien();
            sinhVien.Id = sinhVienWithBookViewModel.Id;
            sinhVien.IdSV = sinhVienWithBookViewModel.IdSV;
            sinhVien.IdBook = sinhVienWithBookViewModel.IdBook;
            sinhVien.NameSV = sinhVienWithBookViewModel.NameSV;
            sinhVien.ClassName = sinhVienWithBookViewModel.ClassName;
            sinhVien.PhoneSV = sinhVienWithBookViewModel.PhoneSV;
            sinhVien.Khoa = sinhVienWithBookViewModel.Khoa;
            sinhVien.BorrowDate = sinhVienWithBookViewModel.BorrowDate;
            sinhVien.PayDate = sinhVienWithBookViewModel.PayDate;
            sinhVien.Status = sinhVienWithBookViewModel.Status;
            return sinhVien;
        }

        public async Task<IActionResult> ExportToExcelSVPayLate()
        {
            // Lấy danh sách sinh viên từ cơ sở dữ liệu
            List<SinhVienWithBookViewModel> result = await (
                from sinhVien in _context.SinhVien
                join Book in _context.Book on sinhVien.IdBook equals Book.IdBook
                select new SinhVienWithBookViewModel
                {
                    IdSV = sinhVien.IdSV,
                    NameSV = sinhVien.NameSV,
                    Khoa = sinhVien.Khoa,
                    ClassName = sinhVien.ClassName,
                    PhoneSV = sinhVien.PhoneSV,
                    BorrowDate = sinhVien.BorrowDate,
                    PayDate = sinhVien.PayDate,
                    NameBook = Book.NameBook,
                    DelayDays = sinhVien.PayDate.HasValue && sinhVien.BorrowDate.HasValue ? (int)(DateTime.Now - sinhVien.PayDate.Value).TotalDays : 0
                }
            ).Where(s => s.PayDate < DateTime.Now).ToListAsync();

            // Tạo một đối tượng ExcelPackage
            using (var package = new ExcelPackage())
            {
                // Tạo một trang tính mới
                var worksheet = package.Workbook.Worksheets.Add("SinhVien");

                // Ghi dữ liệu vào trang tính
                worksheet.Cells["A1"].Value = "Mã Sinh Viên";
                worksheet.Cells["B1"].Value = "Tên Sinh Viên";
                worksheet.Cells["C1"].Value = "Khoa";
                worksheet.Cells["D1"].Value = "Lớp";
                worksheet.Cells["E1"].Value = "Số Điện Thoại";
                worksheet.Cells["F1"].Value = "Ngày Mượn";
                worksheet.Cells["G1"].Value = "Hạn Trả";
                worksheet.Cells["H1"].Value = "Sách Mượn";
                worksheet.Cells["I1"].Value = "Tình trạng";
                worksheet.Cells["J1"].Value = "Số ngày muộn";

                var row = 2;
                foreach (var sinhVien in result)
                {
                    worksheet.Cells[$"A{row}"].Value = sinhVien.IdSV;
                    worksheet.Cells[$"B{row}"].Value = sinhVien.NameSV;
                    worksheet.Cells[$"C{row}"].Value = sinhVien.Khoa;
                    worksheet.Cells[$"D{row}"].Value = sinhVien.ClassName;
                    worksheet.Cells[$"E{row}"].Value = sinhVien.PhoneSV;
                    worksheet.Cells[$"F{row}"].Value = sinhVien.BorrowDate?.ToString("yyyy-MM-dd"); // Định dạng ngày mượn
                    worksheet.Cells[$"G{row}"].Value = sinhVien.PayDate?.ToString("yyyy-MM-dd"); // Định dạng ngày trả (nếu có)
                    worksheet.Cells[$"H{row}"].Value = sinhVien.NameBook;
                    if (sinhVien.Status == 0)
                    {
                        worksheet.Cells[$"I{row}"].Value = "Đang mượn sách";
                    }
                    else if (sinhVien.Status == 1)
                    {
                        worksheet.Cells[$"I{row}"].Value = "Đã trả sách";
                    }
                    worksheet.Cells[$"J{row}"].Value = sinhVien.DelayDays;
                    row++;
                }

                // Đặt kích thước cột tự động dựa trên nội dung
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Lưu ExcelPackage vào một mảng byte
                var excelBytes = package.GetAsByteArray();

                // Xác định loại file MIME
                var mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                // Trả về FileResult
                return File(excelBytes, mimeType, "SinhVien.xlsx");
            }
        }
    }
}
