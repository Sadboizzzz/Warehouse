using System.Data.Entity;
using System.Net;
using System.Linq;
using System.Web.Mvc;
using Warehouse_25DH110382.Models;
using System;

namespace Warehouse_25DH110382.Controllers
{
    public class ProductController : Controller
    {
        private WarehouseDbContext db = new WarehouseDbContext();

        // (Đã có ở Câu 2a) Danh sách sản phẩm
        public ActionResult Index()
        {
            var products = db.Products.ToList();
            return View(products);
        }

        // ==================== CÂU 2b: SỬA THÔNG TIN SẢN PHẨM ====================

        // GET: Product/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Product product)
        {
            // Kiểm tra ràng buộc theo đề bài: tên và danh mục không rỗng, giá > 0
            if (string.IsNullOrWhiteSpace(product.ProductName))
            {
                ModelState.AddModelError("ProductName", "Tên sản phẩm không được để trống.");
            }
            if (string.IsNullOrWhiteSpace(product.Category))
            {
                ModelState.AddModelError("Category", "Danh mục không được để trống.");
            }
            if (product.Price <= 0)
            {
                ModelState.AddModelError("Price", "Đơn giá phải lớn hơn 0.");
            }

            if (ModelState.IsValid)
            {
                // Lấy sản phẩm gốc từ CSDL để đảm bảo không bị thay đổi ProductID hoặc Quantity trái phép
                var existingProduct = db.Products.Find(product.ProductID);
                if (existingProduct != null)
                {
                    // Chỉ cho phép cập nhật Tên, Danh mục, Đơn giá
                    existingProduct.ProductName = product.ProductName.Trim();
                    existingProduct.Category = product.Category.Trim();
                    existingProduct.Price = product.Price;

                    // Giữ nguyên Quantity và ProductID, lưu thay đổi bằng Entity Framework
                    db.Entry(existingProduct).State = EntityState.Modified;
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
            }
            return View(product);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        // GET: Product/Search (Câu 3: Tra cứu hàng hóa)
        public ActionResult Search(string searchString, int? stockStatus, string sortOrder)
        {
            var products = db.Products.AsQueryable();

            // 1. Tìm theo một phần ProductName (không phân biệt chữ hoa/thường)
            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.ProductName.Contains(searchString));
            }

            // 2. Lọc theo mức tồn kho: 0 (Hết hàng), 1 (Sắp hết: 1-4), 2 (Đủ hàng: >=5)
            if (stockStatus.HasValue)
            {
                if (stockStatus.Value == 0)
                {
                    products = products.Where(p => p.Quantity == 0);
                }
                else if (stockStatus.Value == 1)
                {
                    products = products.Where(p => p.Quantity >= 1 && p.Quantity <= 4);
                }
                else if (stockStatus.Value == 2)
                {
                    products = products.Where(p => p.Quantity >= 5);
                }
            }

            // 3. Sắp xếp theo Price tăng hoặc giảm bằng LINQ
            ViewBag.CurrentSort = sortOrder;
            if (sortOrder == "price_asc")
            {
                products = products.OrderBy(p => p.Price);
            }
            else if (sortOrder == "price_desc")
            {
                products = products.OrderByDescending(p => p.Price);
            }
            else
            {
                products = products.OrderBy(p => p.ProductID); // Mặc định theo ID
            }

            // Giữ lại giá trị trên form tìm kiếm
            ViewBag.SearchString = searchString;
            ViewBag.StockStatus = stockStatus;

            return View(products.ToList());
        }
        // ==================== CÂU 4: KIỂM KÊ 1 SẢN PHẨM ====================

        // GET: Product/InventoryCheck1
        public ActionResult InventoryCheck1()
        {
            ViewBag.ProductList = new SelectList(db.Products.ToList(), "ProductID", "ProductName");
            return View();
        }

        // POST: Product/InventoryCheck1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult InventoryCheck1(int? productId, int? actualQuantity)
        {
            ViewBag.ProductList = new SelectList(db.Products.ToList(), "ProductID", "ProductName", productId);

            if (!productId.HasValue)
            {
                ViewBag.ErrorMessage = "Vui lòng chọn sản phẩm cần kiểm kê.";
                return View();
            }

            // Kiểm tra số nguyên không âm
            if (!actualQuantity.HasValue || actualQuantity.Value < 0)
            {
                ViewBag.ErrorMessage = "Số lượng thực tế phải là số nguyên không âm (>= 0).";
                return View();
            }

            var product = db.Products.Find(productId.Value);
            if (product == null)
            {
                ViewBag.ErrorMessage = "Không tìm thấy sản phẩm trong hệ thống.";
                return View();
            }

            int oldQuantity = product.Quantity;
            int newQuantity = actualQuantity.Value;
            int diff = newQuantity - oldQuantity;

            // Xác định trạng thái chênh lệch theo yêu cầu đề bài
            if (newQuantity > oldQuantity)
            {
                ViewBag.DiffStatus = $"Chênh lệch tăng (+{diff} sản phẩm)";
                ViewBag.DiffType = "increase";
            }
            else if (newQuantity < oldQuantity)
            {
                ViewBag.DiffStatus = $"Chênh lệch giảm ({diff} sản phẩm)";
                ViewBag.DiffType = "decrease";
            }
            else
            {
                ViewBag.DiffStatus = "Không chênh lệch (0 sản phẩm)";
                ViewBag.DiffType = "equal";
            }

            // Cập nhật số lượng mới vào CSDL qua Entity Framework
            product.Quantity = newQuantity;
            db.Entry(product).State = EntityState.Modified;
            db.SaveChanges();

            ViewBag.OldQuantity = oldQuantity;
            ViewBag.NewQuantity = newQuantity;
            ViewBag.ProductName = product.ProductName;
            ViewBag.SuccessMessage = "Đã cập nhật số lượng kiểm kê vào cơ sở dữ liệu thành công!";

            return View();
        }

        // ==================== CÂU 5: KIỂM KÊ ĐỒNG THỜI 2 SẢN PHẨM ====================

        // GET: Product/InventoryCheck2
        public ActionResult InventoryCheck2()
        {
            var productList = db.Products.ToList();
            ViewBag.ProductList1 = new SelectList(productList, "ProductID", "ProductName");
            ViewBag.ProductList2 = new SelectList(productList, "ProductID", "ProductName");
            return View();
        }

        // POST: Product/InventoryCheck2
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult InventoryCheck2(int? productId1, int? qty1, int? productId2, int? qty2)
        {
            var productList = db.Products.ToList();
            ViewBag.ProductList1 = new SelectList(productList, "ProductID", "ProductName", productId1);
            ViewBag.ProductList2 = new SelectList(productList, "ProductID", "ProductName", productId2);

            // 1. Kiểm tra chọn đủ sản phẩm và khác nhau
            if (!productId1.HasValue || !productId2.HasValue)
            {
                ViewBag.ErrorMessage = "Vui lòng chọn đầy đủ cả hai sản phẩm.";
                return View();
            }

            if (productId1.Value == productId2.Value)
            {
                ViewBag.ErrorMessage = "Hai sản phẩm được chọn phải khác nhau.";
                return View();
            }

            // 2. Kiểm tra số lượng hợp lệ (số nguyên không âm >= 0)
            if (!qty1.HasValue || qty1.Value < 0 || !qty2.HasValue || qty2.Value < 0)
            {
                ViewBag.ErrorMessage = "Số lượng thực tế của cả hai sản phẩm phải là số nguyên không âm (>= 0).";
                return View();
            }

            // 3. Tìm 2 sản phẩm trong CSDL
            var p1 = db.Products.Find(productId1.Value);
            var p2 = db.Products.Find(productId2.Value);

            if (p1 == null || p2 == null)
            {
                ViewBag.ErrorMessage = "Một trong các sản phẩm được chọn không tồn tại trong hệ thống.";
                return View();
            }

            // Lưu lại thông tin cũ để hiển thị thông báo kết quả
            int oldQty1 = p1.Quantity;
            int oldQty2 = p2.Quantity;

            // 4. Cập nhật trong cùng một Transaction để đảm bảo tính toàn vẹn (cả hai cùng thành công hoặc giữ nguyên)
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    p1.Quantity = qty1.Value;
                    db.Entry(p1).State = EntityState.Modified;

                    p2.Quantity = qty2.Value;
                    db.Entry(p2).State = EntityState.Modified;

                    db.SaveChanges();
                    transaction.Commit(); // Xác nhận lưu giao dịch

                    ViewBag.SuccessMessage = "Đã cập nhật đồng thời thành công cho cả hai sản phẩm!";
                    ViewBag.Result1 = $"{p1.ProductName}: Cũ = {oldQty1} -> Mới = {p1.Quantity}";
                    ViewBag.Result2 = $"{p2.ProductName}: Cũ = {oldQty2} -> Mới = {p2.Quantity}";
                }
                catch (Exception ex)
                {
                    transaction.Rollback(); // Hoàn tác nếu có lỗi xảy ra
                    ViewBag.ErrorMessage = "Lỗi khi cập nhật giao dịch: " + ex.Message;
                    return View();
                }
            }

            return View();
        }

        // ==================== CÂU 6: THÊM SẢN PHẨM MỚI ====================

        // GET: Product/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Product product)
        {
            // 1. Chuẩn hóa dữ liệu đầu vào: cắt khoảng trắng thừa hai đầu nếu có
            if (!string.IsNullOrEmpty(product.ProductName))
            {
                product.ProductName = product.ProductName.Trim();
            }
            if (!string.IsNullOrEmpty(product.Category))
            {
                product.Category = product.Category.Trim();
            }

            // 2. Kiểm tra ràng buộc hợp lệ
            if (string.IsNullOrWhiteSpace(product.ProductName))
            {
                ModelState.AddModelError("ProductName", "Tên sản phẩm không được để trống.");
            }
            if (string.IsNullOrWhiteSpace(product.Category))
            {
                ModelState.AddModelError("Category", "Danh mục không được để trống.");
            }
            if (product.Price <= 0)
            {
                ModelState.AddModelError("Price", "Đơn giá phải lớn hơn 0.");
            }
            if (product.Quantity < 0)
            {
                ModelState.AddModelError("Quantity", "Số lượng tồn kho phải là số nguyên không âm (>= 0).");
            }

            // 3. Kiểm tra trùng tên sản phẩm (không phân biệt hoa/thường và bỏ khoảng trắng)
            if (!string.IsNullOrEmpty(product.ProductName))
            {
                string normalizedNewName = product.ProductName.ToLower();
                bool isDuplicate = db.Products.Any(p => p.ProductName.Trim().ToLower() == normalizedNewName);
                if (isDuplicate)
                {
                    ModelState.AddModelError("ProductName", "Tên sản phẩm này đã tồn tại trong kho hàng.");
                }
            }

            if (ModelState.IsValid)
            {
                // Lưu vào CSDL bằng Entity Framework
                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(product);
        }

        // ==================== CÂU 7: THỐNG KÊ KẾT QUẢ KIỂM KÊ ====================

        // GET: Product/Statistics
        public ActionResult Statistics()
        {
            var products = db.Products.ToList();

            // 1. Tổng số lượng sản phẩm (số lượng bản ghi hoặc tổng số lượng tồn)
            int totalProductTypes = products.Count;

            // 2. Số sản phẩm hết hàng (Quantity = 0)
            int outOfStockCount = products.Count(p => p.Quantity == 0);

            // 3. Tổng giá trị hàng tồn = SUM(Price * Quantity)
            decimal totalInventoryValue = products.Sum(p => p.Price * p.Quantity);

            // Truyền dữ liệu sang View bằng ViewBag
            ViewBag.TotalProductTypes = totalProductTypes;
            ViewBag.OutOfStockCount = outOfStockCount;
            ViewBag.TotalInventoryValue = totalInventoryValue;

            return View(products);
        }
    }
}