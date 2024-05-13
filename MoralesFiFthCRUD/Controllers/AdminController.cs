using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MoralesFiFthCRUD.ViewModels;
using MoralesFiFthCRUD.Repository;
using System.Text;

namespace MoralesFiFthCRUD.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly GenericUnitOfWork _dbContext = new GenericUnitOfWork();

        
        public List<SelectListItem> GetMembers()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            var members = _dbContext.GetRepositoryInstance<User>().GetAllRecords();
            foreach (var member in members)
            {
                list.Add(new SelectListItem { Value = member.id.ToString(), Text = member.username });
            }
            return list;
        }

        public List<SelectListItem> GetCategories()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            var Category = _dbContext.GetRepositoryInstance<Category>().GetAllRecords().Where(c => !c.IsDelete).ToList();
            foreach (var category in Category)
            {
                list.Add(new SelectListItem { Value = category.id.ToString(), Text = category.CategoryName });
            }
            return list;
        }

        public ActionResult AdminDashboard()
        {
            var allUsers = _dbContext.GetRepositoryInstance<User>().GetAllRecords();

            var allProducts = _dbContext.GetRepositoryInstance<Products>().GetAllRecords();

            var dashboardViewModel = new AdminDashboardViewModel
            {
                Users = allUsers.ToList(),
                Products = allProducts.ToList()
            };

            return View(dashboardViewModel);
        }

        public new ActionResult ListUsers()
        {
            return View(_dbContext.GetRepositoryInstance<User>().GetAllRecords());
        }

        public ActionResult UserEdit(int memberId)
        {
            ViewBag.MembersList = GetMembers();
            var user = _dbContext.GetRepositoryInstance<User>().GetFirstorDefaultByParameter(u => u.id == memberId);
            if (user == null)
            {
                return HttpNotFound(); 
            }
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UserEdit(User user)
        {
            if (ModelState.IsValid)
            {
                
                var existingUser = _dbContext.GetRepositoryInstance<User>().GetFirstorDefaultByParameter(u => u.id == user.id);

                if (existingUser == null)
                {
                    return HttpNotFound();
                }

               
                existingUser.email = user.email;
                existingUser.password = user.password; 
                existingUser.IsDelete = user.IsDelete;

                _dbContext.GetRepositoryInstance<User>().Update(existingUser);
                return RedirectToAction("ListUsers");
            }

            return View(user);
        }


        public ActionResult Category()
        {
            var categories = _dbContext.GetRepositoryInstance<Category>().GetAllRecords().Where(c => !c.IsDelete).ToList();
            return View(categories);
        }

        
        public ActionResult Products()
        {
            var products = _dbContext.GetRepositoryInstance<Products>().GetAllRecords()
                .Select(p => new ProductViewModel
                {
                    ProductID = p.ProductID,
                    Name = p.Name,
                   
                })
                .ToList();

            ViewBag.CategoryList = GetCategories(); 

            return View(products);
        }

        public ActionResult ProductEdit(int productId)
        {
            ViewBag.CategoryList = GetCategories();
            var product = _dbContext.GetRepositoryInstance<Products>().GetFirstorDefaultByParameter(p => p.ProductID == productId);
            if (product == null)
            {
                return HttpNotFound(); 
            }
            return View(product);
        }

        [HttpPost]
        public ActionResult ProductEdit(Products product, HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                string filename = Path.GetFileName(file.FileName);
                string path = Path.Combine(Server.MapPath("~/ProductImg/"), filename);
                file.SaveAs(path);

                
                byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(filename);
                product.ProductImg = fileBytes;
            }

            _dbContext.GetRepositoryInstance<Products>().Update(product);
            return RedirectToAction("Products");
        }

        public ActionResult ProductAdd()
        {
            ViewBag.CategoryList = GetCategories();
            return View();
        }

        [HttpPost]
        public ActionResult ProductAdd(Products product, HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                string filename = Path.GetFileName(file.FileName);
                string path = Path.Combine(Server.MapPath("~/ProductImg/"), filename);
                file.SaveAs(path);

               
                byte[] filenameBytes = Encoding.UTF8.GetBytes(filename);

               
                product.ProductImg = filenameBytes;
            }

            _dbContext.GetRepositoryInstance<Products>().Add(product);
            return RedirectToAction("Products");
        }

       

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _dbContext.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult User()
        {
            var users = _dbContext.GetRepositoryInstance<User>().GetAllRecords();
            return View(users);
        }

        public ActionResult Categories()
        {
            var categories = _dbContext.GetRepositoryInstance<Category>().GetAllRecords();
            return View(categories);
        }
    }
}
