using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC.Data;
using MVC.Models;
using MVC.Models.Process;   //
using System.Threading.Tasks;
using System.Linq;
using X.PagedList;
using X.PagedList.Extensions; // 

namespace MvcMovie.Controllers
{
    public class PersonController : Controller
    {
        private readonly ApplicationDbContext _context;

        private ExcelProcess _excelProcess = new ExcelProcess();
        private GenCode _gen = new GenCode();


        public PersonController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Person
        public async Task<IActionResult> Index(int? page)
        {
            var model = _context.Person.ToList().ToPagedList(page ?? 1, 5);
            return View(model);
        }

        // GET: Person/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Person/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PersonId,FullName,Address,Ages")] Person person)
        {
            if (ModelState.IsValid)
            {
                // 👉 Sinh mã tự động cho PersonId
                int count = _context.Person.Count() + 1;
                person.PersonId = _gen.GenerateCode("PS", count); // VD: PS001

                _context.Add(person);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(person);
        }

        // GET: Person/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.Person == null)
            {
                return NotFound();
            }

            var person = await _context.Person.FindAsync(id);
            if (person == null)
            {
                return NotFound();
            }
            return View(person);
        }

        // POST: Person/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("PersonId,FullName,Address,Ages")] Person person)
        {
            if (id != person.PersonId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(person);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PersonExists(person.PersonId))
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
            return View(person);
        }

        // GET: Person/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.Person == null)
            {
                return NotFound();
            }

            var person = await _context.Person
                .FirstOrDefaultAsync(m => m.PersonId == id);
            if (person == null)
            {
                return NotFound();
            }

            return View(person);
        }

        // POST: Person/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.Person == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Person' is null");
            }
            var person = await _context.Person.FindAsync(id);
            if (person != null)
            {
                _context.Person.Remove(person);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PersonExists(string id)
        {
            return (_context.Person?.Any(e => e.PersonId == id)).GetValueOrDefault();
        }

        public IActionResult Upload()
        {
            return View();
        }

        // POST: Person/Upload
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Upload(IFormFile file)
{
    if (file != null)
    {
        string fileExtension = Path.GetExtension(file.FileName);
        if (fileExtension != ".xls" && fileExtension != ".xlsx")
        {
            ModelState.AddModelError("", "Please choose excel file to upload!");
        }
        else
        {
            // 1. TẠO ĐƯỜNG DẪN THƯ MỤC AN TOÀN
            // Sử dụng Path.Combine thay vì cộng chuỗi để tránh lỗi đường dẫn
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "Excels");

            // 2. KIỂM TRA VÀ TẠO THƯ MỤC (Khắc phục lỗi DirectoryNotFoundException)
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // 3. ĐẶT TÊN FILE (Khắc phục lỗi dấu hai chấm ':')
            // Sử dụng format "yyyyMMdd_HHmmss" (ví dụ: 20231120_145000) thay vì ToShortTimeString
            var fileName = DateTime.Now.ToString("yyyyMMdd_HHmmss") + fileExtension;
            
            var filePath = Path.Combine(uploadsFolder, fileName);

            // 4. LƯU FILE VÀO SERVER
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            } // Kết thúc block using để đóng file stream lại trước khi đọc

            // 5. ĐỌC DỮ LIỆU TỪ FILE VỪA LƯU
            // Lưu ý: Nên xử lý đọc file SAU KHI đã đóng stream lưu file ở trên để tránh lỗi "File is being used"
            try 
            {
                var dt = _excelProcess.ExcelToDataTable(filePath);

                // Duyệt dữ liệu và lưu vào DB
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var ps = new Person();

                    ps.PersonId = dt.Rows[i][0].ToString() ?? string.Empty;
                    ps.FullName = dt.Rows[i][1].ToString() ?? string.Empty;
                    ps.Address = dt.Rows[i][2].ToString() ?? string.Empty;

                    _context.Add(ps);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                 ModelState.AddModelError("", "Lỗi khi đọc file Excel: " + ex.Message);
            }
        }
    }
    return View();
}

    }
}
