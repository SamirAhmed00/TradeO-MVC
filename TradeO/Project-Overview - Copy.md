# TradeO Project Overview 
---
### Entity Framework Core with Code First Approach
- **Database**: SQL Server
- **ORM**: Entity Framework Core
- **Approach**: Code First
- **Appkication DbContext**

### Project Architecture:
- **N-Tier Architecture** with separation of concerns
- **Repository Pattern** with Generic & Specific repositories
- **Unit of Work** for transaction management
- **Areas-based Role Separation** (Admin, Customer)


-------------------------------------------
# Category Management Module
-------------------------------------------
### Category Model:
- **Id**: int (Primary Key, Auto-generated)
- **Name**: string (Required, 3-100 characters) 
- **DisplayOrder**: int (Non-negative)

### Category Validation Rules:
- Name: Required, Unique, Min:3, Max:100
- DisplayOrder: Non-negative integer

### Category Security Implemented:
- AntiForgeryToken in all forms
- Server-side validation
- Client-side validation
- Parameter sanitization

- 
### Category Controller:
- **Actions**: 
  - Index(string SortOrder) :  Done ✅  
  - Create() : GET - Done ✅
  - Create(Category newCategory) : POST - Done ✅
  - Edit() : Get - Done ✅
  - Edit(Category updatedCategory) : Post - Done ✅
  - Delete : Post - Done ✅

### Category Views:
- **Index**: (Get all Categories, Live Search with JS , Sort DownList, Create Button)   Done ✅
- **Create**: (Form , Submit & Back To List Button ) Done ✅
- **Edit**: (Form , Update & Back To List Button ) Done ✅

### Category Performance Optimizations:
- AsNoTracking for read operations
- Async/Await pattern
- Efficient query execution
- Generic Repository for common operations
- Specific Repository for custom business logic

---------------------------

-------------------------------------------
# Prodcut Management Module
-------------------------------------------
### Product Model:
- **Id**: int (Primary Key, Auto-generated)
- **Name**: string (Required, 3-100 characters)
- **Description**: string (Optional, Max 5000 characters)
- **Seller**: string (Required, Max 50 characters)
- **Price**: decimal (Required, > 0)
- **DiscountPrice**: decimal? (Optional, >= 0)
- **ImageUrl**: string (Optional, Valid URL, Max 500 characters)
- **CategoryId**: int (Foreign Key to Category)
- **Category**: Navigation property to Category

### Product Validation Rules:
- Name: Required, Min:3, Max:100
- Description: Optional, Max:5000
- Seller: Required, Max:50
- Price: Required, >0
- DiscountPrice: Optional, >=0
- ImageUrl: Optional, Valid URL, Max:500
- CategoryId: Required, Valid Category
- Custom Validation: DiscountPrice < Price

### Product Security Implemented:
- AntiForgeryToken in all forms
- Server-side validation
- Client-side validation
- Parameter sanitization
- Image URL validation
- CategoryId validation
- Custom validation for business rules

### Product Controller:
- **Actions**: 
  - Index(string SortOrder, int categoryId) :  Done ✅  
  - Create() : GET - Done ✅
  - Create(ProductVM newProduct) : POST - Done ✅
  - Edit(int id) : Get - Done ✅
  - Edit(ProductVM updatedProduct) : Post - Done ✅
  - - Delete(int productId) : Post - Done ✅


### Product Views:
- **Index**: (Get all Products, Live Search with JS , Sort DownList, Category Filter, Create Button ) Done ✅
- **Create**: (Form, ImageUpload , Submit & Back To List Button ) Done ✅
- **Edit**: (Form, ImageUpload , Update & Back To List Button ) Done ✅
- **Details**: (View Product Details, Back To List Button ) UnderDevelopment
- **Delete**: (Confirm Deletion, Back To List Button ) Done ✅

### Product Performance Optimizations:
- AsNoTracking for read operations
- Async/Await pattern
- Efficient query execution
- Generic Repository for common operations
- Specific Repository for custom business logic

### Image Management:
- **Image Upload**: Support for product images
- **Image Preview**: Live preview in create/edit forms  
- **Image Deletion**: Automatic cleanup when deleting products
- **File Validation**: Server-side file type validation

### UI/UX Features:
- **Live Search**: JavaScript-based real-time filtering
- **Bootstrap Modals**: For delete confirmation
- **Responsive Design**: Mobile-friendly layouts
- **Form Validation**: Client-side + server-side
-------------------------------------------


-------------------------------------------
# Microsoft Identity Integration
-------------------------------------------

### User Roles:
- **Admin**: Full access to manage Categories and Products
- **Customer**: View Products only
- **Role Management**: Seeded roles in database

- ### Authentication:
- **ASP.NET Core Identity** for user management
- **Login/Logout** functionality
- **Registration** with email confirmation

### Authorization:
- Role-based access control
- [Authorize] attributes on controllers/actions
- Redirection for unauthorized access

-------------------------------------------




## 🧩 Additional Features
- **Toastr Notifications**:  
  Added success and error notifications using *Toastr.js* for better user feedback on Category actions:
  - **Create Category** – Shows success notification after creating a category.
  - **Edit Category** – Shows success notification after updating a category.
  - **Delete Category** – Shows success notification after successful deletion.


### Data Access Pattern:
- **Repository Pattern** Implementation:
  - `IRepository<T>` - Generic interface for common CRUD operations
  - `ICategoryRepository` - Specific interface with custom Update logic
  - `Repository<T>` - Generic repository implementation
  - `CategoryRepository` - Specific repository for Category business logic
- **Unit of Work Pattern**:
  - `IUnitOfWork` - Centralized interface for repositories
  - `UnitOfWork` - Manages repositories and SaveChanges
  - Injected into Controller instead of individual repositories

## Future Todo List:
- **Handle NotFound Page**: (Edit Controller , Delete Controller )
- **Custom Sanitization & Input Hardening**
- **Product Dynamic Rating System**
- ⚠️ Basic File Upload Security (needs enhancement)








//  From Here I Want any ai help me to write this overview - (Based on Code I will Share With You Later))

-------------------------------------------
# Company Management Module
-------------------------------------------
### Company Model:
-  public string? StreetAddress { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? PhoneNumber { get; set; 


 - ### Company Controller:
 - Crud Operations 


 -------------------------------------------
# Roles Management Module - Utility Only
-------------------------------------------
### Role Model:
public const string Role_Customer = "Customer";
        public const string Role_Company = "Company";
        public const string Role_Admin = "Admin";
        public const string Role_Employee = "Employee";

### Role Management: only get 

 -------------------------------------------
# Shopping cart Management Module 
-------------------------------------------
### Shopping Cart Model:
public int Id { get; set; }

public int ProductId { get; set; }
[ForeignKey("ProductId")]
[ValidateNever] 
public Product Product { get; set; }

[Range(1, maximum: 100, ErrorMessage = "Please select a quantity between 1 and 100")]
public int Count { get; set; }

public string ApplicationUserId { get; set; }
[ForeignKey("ApplicationUserId")]
[ValidateNever]
public ApplicationUser ApplicationUser { get; set; }

[NotMapped]
public decimal Price { get; set; }

### Shopping Cart Controller:
- index 
- delete
- increase
- decrease
