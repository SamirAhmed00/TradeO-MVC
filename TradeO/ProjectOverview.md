# TradeO - E-commerce Platform
## ASP.NET Core MVC Project Overview

### 🎯 Project Description
TradeO is a comprehensive e-commerce platform built with ASP.NET Core MVC, following 3-Tier Architecture and Clean Code principles. The application provides a complete online shopping experience with admin management capabilities.

### 🏗️ Architecture & Patterns
- **3-Tier Architecture** (Presentation, Business Logic, Data Access)
- **Clean Code Principles** with proper separation of concerns
- **Repository Pattern** + **Unit of Work Pattern**
- **Dependency Injection** throughout the application
- **Areas Structure** for modular organization

### 📁 Project Structure
TradeO/
├── TradeO/ # MVC Application (Presentation Layer)
│ ├── Areas/
│ │ ├── Admin/ # Admin Management Controllers & Views
│ │ └── Customer/ # Customer-facing Controllers & Views
│ └── Program.cs # Startup configuration
├── TradeO.DataAccess/ # Data Access Layer
│ ├── Data/ # DbContext and Database
│ ├── Repository/ # Repository implementations
│ └── DbInitializer/ # Database seeding
├── TradeO.Models/ # Domain Models & ViewModels
└── TradeO.Utility/ # Utilities & Constants



### 🗄️ Database Design
#### Core Entities:
- **Category** - Product categories with display ordering
- **Product** - Products with pricing, discounts, and images
- **Company** - Business entities for company users
- **ApplicationUser** - Extended Identity user with company relations
- **ShoppingCart** - User shopping cart items
- **OrderHeader** - Order master data with payment info
- **OrderDetail** - Order line items

#### Key Relationships:
- User ↔ Company (Many-to-One)
- Product ↔ Category (Many-to-One)
- OrderHeader ↔ OrderDetail (One-to-Many)
- ShoppingCart ↔ User + Product (Many-to-One)

### 🔐 Authentication & Authorization
#### User Roles:
- **Customer** - Regular shopping users
- **Company** - Business users with delayed payment
- **Employee** - Staff with order management access
- **Admin** - Full system administration

#### Security Features:
- Role-based access control
- User lock/unlock system
- Secure session management
- Anti-forgery token validation

### 💳 Payment System
#### Stripe Integration:
- **Instant Payment** for individual customers
- **Delayed Payment** (30 days) for company users
- **Refund Processing** for cancelled orders
- **Secure Payment Flow** with webhook support

### 🛠️ Technical Features
#### Data Access:
- Entity Framework Core with Code First
- Generic Repository Pattern
- Async/Await operations
- Eager loading with include properties

#### Business Logic:
- Comprehensive validation with Data Annotations
- Image upload and management
- Sorting and filtering capabilities
- Shopping cart session management

#### UI/UX Features:
- Responsive design
- Product categorization
- Advanced sorting options
- Real-time cart updates


### 🔧 Configuration & Setup
#### Key Configurations:
- Database connection with SQL Server
- Stripe payment gateway settings
- Identity system with custom user
- Session management for cart

#### Environment Requirements:
- .NET 9.0
- SQL Server Database Or PostgreSQL Database
- Stripe Account for payments
- File system access for image storage

### 🚀 Deployment Ready
- Database migrations automated
- Seed data initialization
- Environment-based configuration
- Production-ready error handling

### 📈 Scalability Aspects
- Modular architecture for easy extension
- Repository pattern for data abstraction
- Service layer ready for business logic separation
- Area structure for feature organization

---
*Last Updated: 19-10-2025*
*Architecture: 3-Tier with Clean Code Principles*
*Status: Production Ready*