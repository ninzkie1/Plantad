using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using MoralesFiFthCRUD.ViewModels;
using MoralesFiFthCRUD.Repository;
using MoralesFiFthCRUD.Contracts;
using System.Data.Entity;
using System.Security.Cryptography;


namespace MoralesFiFthCRUD.Controllers
{

    public class HomeController : BaseController
    {

        private readonly database2Entities5 _dbContext;
        private readonly MailManager _mailManager;

        public HomeController()
        {
            _dbContext = new database2Entities5();
            _mailManager = new MailManager();
        }

        public ActionResult Index()
        {
            return View(_userRepo.GetAll());
        }

        [AllowAnonymous]
        public ActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Login");
            return View();
        }
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Login(User u)
        {
            if (User.Identity.IsAuthenticated)
            {
                // Redirect to the Shop page
                return RedirectToAction("Dashboard");
            }
            var user = _userRepo._table.Where(m => m.username == u.username).FirstOrDefault();
            
            if (user != null)
            {
                if (user.password == u.password)
                {
                    FormsAuthentication.SetAuthCookie(u.username, false);
                    return RedirectToAction("Dashboard");
                }
            }
            ModelState.AddModelError("", "User not Exist or Incorrect Password");

            return View(u);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(User u, string SelectedRole)
        {
            _userRepo.Create(u);

            var userAdded = _userRepo._table.FirstOrDefault(m => m.username == u.username);

            if (userAdded == null)
            {
                
                ModelState.AddModelError("", "Failed to create user.");
                return View(u); 
            }

            if (string.IsNullOrEmpty(SelectedRole))
            {
                
                ModelState.AddModelError("", "Role not selected.");
                return View(u); 
            }

            var role = _db.Role.FirstOrDefault(r => r.roleName == SelectedRole);

            if (role == null)
            {
                ModelState.AddModelError("", "Invalid role selected.");
                return View(u); 
            }

            var userRole = new UserRole
            {
                userId = userAdded.id,
                roleId = role.id 
            };

            _userRole.Create(userRole);

            TempData["Msg"] = $"User {u.username} added!";
            return RedirectToAction("LandingPage");
        }


        [Authorize(Roles = "Admin,Buyer,Seller")]
        public ActionResult Edit(int id)
        {

            return View(_userRepo.Get(id));
        }
        [HttpPost]
        public ActionResult Edit(User u)
        {
            _userRepo.Update(u.id, u);
            TempData["Msg"] = $"User {u.username} updated!";

            return RedirectToAction("index");

        }

        public ActionResult Delete(int id)
        {
            _userRepo.Delete(id);
            TempData["Msg"] = $"User deleted!";
            return RedirectToAction("index");
        }
        public ActionResult LandingPage()
        {
            return View();
        }

        public ActionResult Dashboard()
        {
            return View();
        }
        public ActionResult About()
        {
            return View();
        }
        public ActionResult ContactUs()
        {
            return View();
        }


        public ActionResult SignUp()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SignUp(User u, string SelectedRole, string otp)
        {
            var storedOTP = Session["GeneratedOTP"]?.ToString();

            if (string.IsNullOrEmpty(storedOTP))
            {
                ModelState.AddModelError("", "OTP expired or not found. Please try signing up again.");
                return View(u);
            }

            if (otp != storedOTP)
            {
                ModelState.AddModelError("", "Incorrect OTP. Please try again.");
                return View(u);
            }

            //kong  OTP is kay correct, proceed with user creation
            var existingUser = _userRepo._table.FirstOrDefault(m => m.username == u.username);

            if (existingUser != null)
            {
                TempData["ErrorMsg"] = "Username already exists. Please choose a different username.";
                return RedirectToAction("SignUp");
            }

            _userRepo.Create(u);

            var userAdded = _userRepo._table.FirstOrDefault(m => m.username == u.username);

            if (userAdded == null)
            {
                ModelState.AddModelError("", "Failed to create user.");
                return View(u);
            }

            if (string.IsNullOrEmpty(SelectedRole))
            {
                ModelState.AddModelError("", "Role not selected.");
                return View(u);
            }

            var role = _db.Role.FirstOrDefault(r => r.roleName == SelectedRole);

            if (role == null)
            {
                ModelState.AddModelError("", "Invalid role selected.");
                return View(u);
            }

            var userRole = new UserRole
            {
                userId = userAdded.id,
                roleId = role.id
            };

            _userRole.Create(userRole);

            Session.Remove("GeneratedOTP");

            TempData["SuccessMsg"] = $"User {u.username} added!";
            return RedirectToAction("LandingPage");
        }


        [HttpPost]
        public ActionResult GenerateOTP(string email)
        {

            string generatedOTP = "";
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] tokenData = new byte[6];
                rng.GetBytes(tokenData);
                generatedOTP = string.Join("", tokenData.Select(b => (b % 10).ToString()));
            }


            Session["GeneratedOTP"] = generatedOTP;


            string errResponse = "";
            bool emailSent = _mailManager.SendEmail(email, "Your OTP", $"Your sign up OTP is: {generatedOTP}", generatedOTP, ref errResponse);


            if (emailSent)
            {
                return Json(new { success = true, message = "OTP sent successfully!" });
            }
            else
            {
                return Json(new { success = false, message = $"Failed to send OTP: {errResponse}" });
            }
        }

        [Authorize(Roles = "Seller")]
        public ActionResult SellerView()
        {
            string userName = User.Identity.Name;
            var user = _userRepo._table.FirstOrDefault(u => u.username == userName);
            if (user == null)
            {
                ModelState.AddModelError("Shop", "Home");
                return View();
            }
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Seller")]
        public ActionResult SellerView(string productName, int categoryId, HttpPostedFileBase productImage, string productDescription, decimal productPrice, int productQuantity)
        {
            string userName = User.Identity.Name;

            var user = _userRepo._table.FirstOrDefault(u => u.username == userName);

            if (user == null)
            {
                ModelState.AddModelError("Shop", "Home");
                return View();
            }

            var category = _db.Category.FirstOrDefault(c => c.id == categoryId);

            if (category == null)
            {
                ModelState.AddModelError("", "Category not found.");
                return View(); 
            }

            
            if (productImage == null || productImage.ContentLength == 0)
            {
                ModelState.AddModelError("", "Please select a product image.");
                return View(); 
            }

            
            if (!productImage.ContentType.StartsWith("image/"))
            {
                ModelState.AddModelError("", "Please upload a valid image file.");
                return View(); 
            }

            if (productImage.ContentLength > 5 * 1024 * 1024)
            {
                ModelState.AddModelError("", "The image size exceeds the limit (5MB). Please upload a smaller image.");
                return View();
            }

            byte[] imageData;
            using (var binaryReader = new BinaryReader(productImage.InputStream))
            {
                imageData = binaryReader.ReadBytes(productImage.ContentLength);
            }

           
            var product = new Products
            {
                ProductName = productName,
                CategoryId = categoryId,
                UserId = user.id,
                price = productPrice,
                description = productDescription,
                Quantity = productQuantity, 
                ProductImg = imageData 
            };

            
            _productRepo.Create(product);

            TempData["SuccessMsg"] = "Product added successfully!";

            return RedirectToAction("SellerView"); 
        }




        public ActionResult MessageUs()
        {
            return View();

        }
        [Authorize(Roles = "Buyer")]
        public ActionResult Userprofile()

        {
            string userName = User.Identity.Name;

            var user = _dbContext.User.FirstOrDefault(u => u.username == userName);

            if (user == null)
            {
                return View("Shop");
            }

            var userProfile = new SellerViewModel
            {
                UserID = user.id,
                Firstname = user.firstname,
                Lastname = user.lastname,
                email = user.email,
                phonenumber = user.phonenumber ?? 0,
                address = user.address,
                passWord = user.password

                
            };


            return View(userProfile);
        }
        [HttpPost]

        [Authorize(Roles = "Buyer")]
        [ValidateAntiForgeryToken]
        public ActionResult SaveProfileUser(SellerViewModel sellerVM)
        {
            if (ModelState.IsValid)
            {
                var user = _dbContext.User.Find(sellerVM.UserID);

                if (user != null)
                {
                    user.firstname = sellerVM.Firstname;
                    user.lastname = sellerVM.Lastname;
                    user.email = sellerVM.email;
                    user.phonenumber = sellerVM.phonenumber;
                    user.address = sellerVM.address;

                    _dbContext.SaveChanges();

                    TempData["SuccessMsg"] = "Profile updated successfully!";
                }
                else
                {
                    TempData["ErrorMsg"] = "User not found.";
                }
            }

            return RedirectToAction("UserProfile");
        }

        [Authorize(Roles = "Seller")]
        public ActionResult Resellerprofile()
        {
            string userName = User.Identity.Name;

            var user = _dbContext.User.FirstOrDefault(u => u.username == userName);

            if (user == null)
            {
                return View("Error");
            }

            var sellerProfile = new SellerViewModel
            {
                UserID = user.id,
                Firstname = user.firstname,
                Lastname = user.lastname,
                email = user.email,
                phonenumber = user.phonenumber ?? 0,
                address = user.address,
                passWord = user.password,
               
                Products = _dbContext.Products
                    .Where(p => p.UserId == user.id)
                    .Select(p => new ProductViewModel
                    {
                        ProductID = p.ProductID,
                        ProductName = p.ProductName,
                        Category = p.Category != null ? p.Category.CategoryName : "N/A",
                        ProductImg = p.ProductImg,
                        Description = p.description,
                        Quantity = p.Quantity ?? 0,
                        Price = p.price ?? 0,
                        sellerName = p.User.username
                    })
                    .ToList()
            };

            return View(sellerProfile);
        }
        [HttpPost]
        [Authorize(Roles = "Seller")]
        [ValidateAntiForgeryToken]
        public ActionResult SaveProfile(SellerViewModel sellerVM)
        {
            if (ModelState.IsValid)
            {
                var user = _dbContext.User.Find(sellerVM.UserID);

                if (user != null)
                {
                    user.firstname = sellerVM.Firstname;
                    user.lastname = sellerVM.Lastname;
                    user.email = sellerVM.email;
                    user.phonenumber = sellerVM.phonenumber;
                    user.address = sellerVM.address;

                    _dbContext.SaveChanges();

                    TempData["SuccessMsg"] = "Profile updated successfully!";
                }
                else
                {
                    TempData["ErrorMsg"] = "User not found.";
                }
            }

            return RedirectToAction("Resellerprofile");
        }



        [AllowAnonymous]
        public ActionResult test(string username)
        {

            var user = _dbContext.User.FirstOrDefault(u => u.username == username);
            if (user == null)
            {
                return RedirectToAction("Shop", "Home");
            }

            var products = _dbContext.Products
                .Where(p => p.UserId == user.id && p.Category != null)
                .ToList()
                .Select(p => new ProductViewModel
                {
                    ProductID = p.ProductID,
                    ProductName = p.ProductName,
                    Category = p.Category.CategoryName,
                    ProductImg = p.ProductImg,
                    Description = p.description,
                    Quantity = p.Quantity ?? 0,
                    Price = p.price ?? 0,
                    sellerName = _dbContext.Products
                        .Where(pr => pr.ProductID == p.ProductID)
                        .Select(pr => pr.User.username)
                        .FirstOrDefault()
                })
                .ToList();


            return View(products);
        }
        public ActionResult PublicDelete(int id)
        {
            var result = _productRepo.Delete(id);

            if (result == ErrorCode.Success)
            {
                // Product deleted successfully
                TempData["SuccessMsg"] = "Product deleted successfully!";
            }
            else
            {
                // Failed to delete product
                TempData["ErrorMsg"] = "Failed to delete product.";
            }

            return RedirectToAction("Shop");
        }

        [Authorize(Roles = "Seller")]
        public ActionResult DeleteProduct(int id)
        {
            var result = _productRepo.Delete(id);

            if (result == Repository.ErrorCode.Success)
            {
                //kong Product deleted successfully
                TempData["SuccessMsg"] = "Product deleted successfully!";
            }
            else
            {
                //kong Failed to delete product
                TempData["ErrorMsg"] = "Failed to delete product.";
            }

            return RedirectToAction("ResellerProfile");
        }

        [HttpGet]
        [Authorize(Roles = "Seller")]
        public ActionResult EditProduct(int id)
        {
            var product = _dbContext.Products.Find(id);

            if (product == null)
            {
                TempData["ErrorMsg"] = "Product not found.";
                return RedirectToAction("Shop");
            }

            if (product.UserId != GetCurrentUserId())
            {
                TempData["ErrorMsg"] = "You are not authorized to edit this product.";
                return RedirectToAction("Shop");
            }

            var viewModel = new ProductViewModel
            {
                ProductID = product.ProductID,
                ProductName = product.ProductName,
                CategoryId = product.CategoryId ?? 0,
                Description = product.description,
                Quantity = product.Quantity ?? 0,
                Price = product.price ?? 0
            };

            ViewBag.Categories = new SelectList(_dbContext.Category, "id", "CategoryName", viewModel.CategoryId);
            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Seller")]
        [ValidateAntiForgeryToken]
        public ActionResult EditProduct(ProductViewModel productVM)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(_dbContext.Category, "id", "CategoryName", productVM.CategoryId);
                return View(productVM);
            }

            var product = _dbContext.Products.Find(productVM.ProductID);

            if (product == null)
            {
                TempData["ErrorMsg"] = "Product not found.";
                return RedirectToAction("Shop");
            }

            if (product.UserId != GetCurrentUserId())
            {
                TempData["ErrorMsg"] = "You are not authorized to edit this product.";
                return RedirectToAction("Shop");
            }

            product.ProductName = productVM.ProductName;
            product.CategoryId = productVM.CategoryId;
            product.description = productVM.Description;
            product.Quantity = productVM.Quantity;
            product.price = productVM.Price;

            // If butangan og Image
            // if (productVM.ProductImg != null)
            // {
            //     product.ProductImg = productVM.ProductImg;
            // }

            _dbContext.Entry(product).State = EntityState.Modified;

            try
            {
                _dbContext.SaveChanges();
                TempData["SuccessMsg"] = "Product updated successfully!";
                return RedirectToAction("EditProduct");
            }
            catch (Exception ex)
            {
                TempData["ErrorMsg"] = "Failed to update product: " + ex.Message;
                return RedirectToAction("ResellerProfile");
            }
        }



        private int GetCurrentUserId()
        {
            string userName = User.Identity.Name;
            var user = _userRepo._table.FirstOrDefault(u => u.username == userName);
            return user?.id ?? 0; 
        }




        

        public ActionResult Buy(int productId, int quantity)
        {
            string buyerName = User.Identity.Name;

            var buyer = _dbContext.User.FirstOrDefault(u => u.username == buyerName);

            if (buyer == null)
            {
                TempData["ErrorMsg"] = "User not found.";
                return RedirectToAction("Shop");
            }

            var product = _dbContext.Products.FirstOrDefault(p => p.ProductID == productId);

            if (product == null)
            {
                TempData["ErrorMsg"] = "Product not found.";
                return RedirectToAction("Shop");
            }

            if (product.Quantity < quantity)
            {
                TempData["ErrorMsg"] = "Insufficient quantity available.";
                return RedirectToAction("Shop");
            }

            var existingBoughtProduct = _dbContext.Cart.FirstOrDefault(p =>
                p.UserId == buyer.id && p.ProductID == productId);

            if (existingBoughtProduct != null)
            {
                existingBoughtProduct.Quantity += quantity;
                existingBoughtProduct.Price = existingBoughtProduct.Quantity * product.price;
            }
            else
            {
                var boughtProduct = new Cart
                {

                    ProductID = product.ProductID,
                    UserId = buyer.id,
                    BuyerName = buyerName,
                    Quantity = quantity,
                    Price = product.price,
                    CategoryName = product.Category.CategoryName,
                    ProductName = product.ProductName,
                    description = product.description,
                    ProductImg = product.ProductImg

                };

                _dbContext.Cart.Add(boughtProduct);
            }
           

            product.Quantity -= quantity; //kong Update product inventory

            

            _dbContext.SaveChanges();

            TempData["SuccessMsg"] = "Purchase successful!";
            return RedirectToAction("Shop");
        }

        public ActionResult Shop(string searchTerm, string sellerName)
        {

            var products = _dbContext.Products.ToList();


            if (!string.IsNullOrWhiteSpace(searchTerm))
            {

                products = products.Where(p => p.ProductName.Contains(searchTerm)).ToList();
            }
            if (!string.IsNullOrWhiteSpace(sellerName))
            {
                products = products.Where(p => p.User.username == sellerName).ToList();
            }

            var sellerProducts = products.Where(p => p.User.UserRole.Any(r => r.Role.roleName == "Seller"))
                    .Select(p => new ProductViewModel
                    {

                        ProductID = p.ProductID,
                        ProductName = p.ProductName,
                        Category = p.Category != null ? p.Category.CategoryName : "N/A",
                        ProductImg = p.ProductImg,
                        Description = p.description,
                        Quantity = p.Quantity ?? 0,
                        Price = p.price ?? 0,
                        sellerName = p.User.username
                    })
                     .ToList();

           
            return View(sellerProducts);
        }

        [Authorize(Roles = "Buyer")]
        public ActionResult ViewCart()
        {
            string buyerName = User.Identity.Name;

            var buyer = _dbContext.User.FirstOrDefault(u => u.username == buyerName);

            if (buyer == null)
            {
                return View("Error");
            }

           
            var boughtProducts = _dbContext.Cart
                .Where(p => p.UserId == buyer.id)
                .Include(p => p.Products) 
                .ToList()
                .Select(p => new ProductViewModel
                {
                    ProductID = p.Products.ProductID,
                    ProductName = p.Products.ProductName, 
                    Category = p.Products.Category != null ? p.Products.Category.CategoryName : "N/A",
                    ProductImg = p.Products.ProductImg,
                    Description = p.Products.description,
                    Quantity = p.Quantity ?? 0,
                    Price = p.Price ?? 0,
                    sellerName = p.Products.User.username,
                    BuyerName = buyerName 
                })
                .ToList();

            return View(boughtProducts);
        }


        
        [HttpPost]
        public ActionResult DecrementQuantity(int productId)
        {
            string buyerName = User.Identity.Name;
            var buyer = _dbContext.User.FirstOrDefault(u => u.username == buyerName);

            if (buyer == null)
            {
                TempData["ErrorMsg"] = "User not found.";
                return RedirectToAction("ViewCart");
            }

            var productInCart = _dbContext.Cart.FirstOrDefault(p => p.UserId == buyer.id && p.ProductID == productId);
            if (productInCart != null)
            {
                productInCart.Quantity--;

                //mo Calculate the price change
                decimal originalPrice = productInCart.Products.price ?? 0;
                decimal priceChange = originalPrice;

                if (productInCart.Quantity <= 0)
                {
                    //mo Remove from cart entirely
                    _dbContext.Cart.Remove(productInCart);
                }
                else
                {
                    //mo Update price in Cart (optional)
                    productInCart.Price -= priceChange;
                }

                //mo Save changes
                _dbContext.SaveChanges();
            }

            TempData["SuccessMsg"] = "Product quantity decreased successfully.";
            return RedirectToAction("ViewCart");
        }



    }

}


