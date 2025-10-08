# TradeO Project Overview 
---
### Entity Framework Core with Code First Approach
- **Database**: SQL Server
- **ORM**: Entity Framework Core
- **Approach**: Code First
- **Appkication DbContext**

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
- **Index**: (Get all Categories, Live Search with JS , Sort DownList, Create Button , ) underDevelopment 
- **Create**: (Form , Submit & Back To List Button ) Done ✅
- **Edit**: (Form , Update & Back To List Button ) Done ✅

### Category Performance Optimizations:
- Async/Await pattern
- AsNoTracking for read operations
- Efficient query execution



## 🧩 Additional Features

- **Toastr Notifications**:  
  Added success and error notifications using *Toastr.js* for better user feedback on Category actions:
  - **Create Category** – Shows success notification after creating a category.
  - **Edit Category** – Shows success notification after updating a category.
  - **Delete Category** – Shows success notification after successful deletion.


## Future Todo List:
- **Handle NotFound Page**: (Edit Controller , Delete Controller )
- **Custom Sanitization & Input Hardening**
