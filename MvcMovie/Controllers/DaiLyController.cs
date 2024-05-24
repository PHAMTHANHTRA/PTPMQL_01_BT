using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MvcMovie.Data;
using MvcMovie.Models;
using MvcMovie.Models.Process;
using OfficeOpenXml;

namespace MvcMovie.Controllers
{
    public class DaiLyController : Controller
    {
        private readonly ApplicationDbcontext _context;
        private ExcelProcess _excelPro = new ExcelProcess();

        public DaiLyController(ApplicationDbcontext context)
        {
            _context = context;
        }

        // GET: DaiLy
        public async Task<IActionResult> Index()
        {
            var applicationDbcontext = _context.DaiLy.Include(d => d.HeThongPhanPhoi);
            return View(await applicationDbcontext.ToListAsync());
        }

        // GET: DaiLy/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var daiLy = await _context.DaiLy
                .Include(d => d.HeThongPhanPhoi)
                .FirstOrDefaultAsync(m => m.MaDaiLy == id);
            if (daiLy == null)
            {
                return NotFound();
            }

            return View(daiLy);
        }

        // GET: DaiLy/Create
        public IActionResult Create()
        {
            ViewData["MaHTPP"] = new SelectList(_context.HeThongPhanPhoi, "MaHTPP", "MaHTPP");
            return View();
        }

        // POST: DaiLy/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaDaiLy,TenDaiLy,DiaChi,NguoiDaiDien,DienThoai,MaHTPP")] DaiLy daiLy)
        {
            if (ModelState.IsValid)
            {
                _context.Add(daiLy);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaHTPP"] = new SelectList(_context.HeThongPhanPhoi, "MaHTPP", "MaHTPP", daiLy.MaHTPP);
            return View(daiLy);
        }

        // GET: DaiLy/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var daiLy = await _context.DaiLy.FindAsync(id);
            if (daiLy == null)
            {
                return NotFound();
            }
            ViewData["MaHTPP"] = new SelectList(_context.HeThongPhanPhoi, "MaHTPP", "MaHTPP", daiLy.MaHTPP);
            return View(daiLy);
        }

        // POST: DaiLy/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("MaDaiLy,TenDaiLy,DiaChi,NguoiDaiDien,DienThoai,MaHTPP")] DaiLy daiLy)
        {
            if (id != daiLy.MaDaiLy)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(daiLy);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DaiLyExists(daiLy.MaDaiLy))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaHTPP"] = new SelectList(_context.HeThongPhanPhoi, "MaHTPP", "MaHTPP", daiLy.MaHTPP);
            return View(daiLy);
        }

        // GET: DaiLy/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var daiLy = await _context.DaiLy
                .Include(d => d.HeThongPhanPhoi)
                .FirstOrDefaultAsync(m => m.MaDaiLy == id);
            if (daiLy == null)
            {
                return NotFound();
            }

            return View(daiLy);
        }

        // POST: DaiLy/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var daiLy = await _context.DaiLy.FindAsync(id);
            if (daiLy != null)
            {
                _context.DaiLy.Remove(daiLy);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DaiLyExists(string id)
        {
            return _context.DaiLy.Any(e => e.MaDaiLy == id);
        }
        public async Task<IActionResult> Upload()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file!=null)
                {
                    string fileExtension = Path.GetExtension(file.FileName);
                    if (fileExtension != ".xls" && fileExtension != ".xlsx")
                    {
                        ModelState.AddModelError("", "Please choose excel file to upload!");
                    }
                    else
                    {
                        //rename file when upload to server
                        var fileName = DateTime.Now.ToShortTimeString() + fileExtension;
                        var filePath = Path.Combine(Directory.GetCurrentDirectory() + "/Upload/Excels", fileName );
                        var fileLocation = new FileInfo(filePath).ToString();
                        using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                //save file to server
                                await file.CopyToAsync(stream);
                                //read data from file and write to database
                                var dt = _excelPro.ExcelToDataTable(fileLocation);
                                for(int i = 0; i < dt.Rows.Count; i++)
                                {
                                    var dl = new DaiLy();
                                    dl.MaDaiLy = dt.Rows[i][0].ToString();
                                    dl.TenDaiLy = dt.Rows[i][1].ToString();
                                    dl.DiaChi = dt.Rows[i][2].ToString();
                                    dl.NguoiDaiDien = dt.Rows[i][3].ToString();
                                    dl.DienThoai = dt.Rows[i][4].ToString();
                                    dl.MaHTPP = dt.Rows[i][5].ToString();


                                    _context.Add(dl);
                                }
                                await _context.SaveChangesAsync();
                                return RedirectToAction(nameof(Index));
                            }
                        
                    }
                }
            return View ();
        }
        public IActionResult Download()
        {
            var fileName = "DaiLyList.xlsx";
            using(ExcelPackage excelPackage = new ExcelPackage())
            {
                ExcelWorksheet excelWorksheet = excelPackage.Workbook.Worksheets.Add("Sheet 2");
                excelWorksheet.Cells["A1"].Value = "MaDaiLy";
                excelWorksheet.Cells["B1"].Value = "TenDaiLy";
                excelWorksheet.Cells["C1"].Value = "DiaChi";
                excelWorksheet.Cells["D1"].Value = "NguoiDaiDien";
                excelWorksheet.Cells["E1"].Value = "DienThoai";
                excelWorksheet.Cells["F1"].Value = "MaHTPP";
                var dlList = _context.DaiLy.ToList();
                excelWorksheet.Cells["A2"].LoadFromCollection(dlList);
                var stream = new MemoryStream(excelPackage.GetAsByteArray());
                return File(stream,"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",fileName);
            }          
        }  
       
    }
}