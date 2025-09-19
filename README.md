# 🏢 Warehouse Management System API

A comprehensive warehouse management system built with .NET 8, Entity Framework Core, and JWT authentication.

## 🚀 Features

### 📦 **Product Management**
- Create, read, update, delete products
- Search products by name, description, or category
- Category management
- Product statistics

### 🏢 **Warehouse Management**
- Multi-warehouse support
- Warehouse stock tracking
- Warehouse statistics
- Deletion validation

### 📊 **Stock Management**
- Real-time stock tracking
- Stock transactions (Import/Export/Adjust)
- Low stock alerts
- Stock analytics and reports
- Multi-warehouse stock management

### 👥 **User Management**
- User registration and authentication
- Role-based access control (Admin, Manager, Employee)
- Password management
- User statistics

### 🔐 **Security**
- JWT-based authentication
- Role-based authorization
- Password hashing with BCrypt
- CORS configuration

## 🛠️ Technology Stack

- **.NET 8** - Web API framework
- **Entity Framework Core 8** - ORM
- **SQL Server** - Database
- **JWT Bearer** - Authentication
- **Swagger/OpenAPI** - API documentation
- **BCrypt** - Password hashing
- **Clean Architecture** - Project structure

## 📋 Prerequisites

- .NET 8 SDK
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 or VS Code

## 🚀 Getting Started

### 1. Clone the Repository
```bash
git clone <repository-url>
cd warehouse-management
```

### 2. Update Connection String
Edit `appsettings.json` and update the connection string:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=WarehouseManagementDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### 3. Run the Application
```bash
dotnet restore
dotnet build
dotnet run
```

### 4. Access the API
- **Swagger UI**: `https://localhost:5001` or `http://localhost:5000`
- **Health Checks**: `https://localhost:5001/health`
- **API Info**: `https://localhost:5001/api/info`

## 🔑 Default Credentials

The system creates a default admin user on first run:
- **Username**: `admin`
- **Password**: `admin123`
- **Role**: `Admin`

## 📚 API Endpoints

### 🔐 Authentication
- `POST /api/auth/login` - User login
- `POST /api/auth/register` - User registration
- `POST /api/auth/refresh` - Refresh token
- `POST /api/auth/logout` - User logout
- `GET /api/auth/validate` - Validate token
- `GET /api/auth/me` - Get current user

### 👥 Users
- `GET /api/user` - Get all users (Admin/Manager)
- `GET /api/user/{id}` - Get user by ID
- `POST /api/user` - Create user (Admin)
- `PUT /api/user/{id}` - Update user (Admin)
- `DELETE /api/user/{id}` - Delete user (Admin)
- `PUT /api/user/{id}/change-password` - Change password
- `PUT /api/user/{id}/activate` - Activate user (Admin)
- `PUT /api/user/{id}/deactivate` - Deactivate user (Admin)

### 📦 Products
- `GET /api/product` - Get all products
- `GET /api/product/{id}` - Get product by ID
- `GET /api/product/category/{category}` - Get products by category
- `GET /api/product/search?searchTerm=...` - Search products
- `GET /api/product/categories` - Get all categories
- `POST /api/product` - Create product (Admin/Manager)
- `PUT /api/product/{id}` - Update product (Admin/Manager)
- `DELETE /api/product/{id}` - Delete product (Admin)

### 🏢 Warehouses
- `GET /api/warehouse` - Get all warehouses
- `GET /api/warehouse/{id}` - Get warehouse by ID
- `GET /api/warehouse/{id}/stock` - Get warehouse stock
- `POST /api/warehouse` - Create warehouse (Admin/Manager)
- `PUT /api/warehouse/{id}` - Update warehouse (Admin/Manager)
- `DELETE /api/warehouse/{id}` - Delete warehouse (Admin)

### 📊 Stock Management
- `GET /api/stock/items` - Get all stock items
- `GET /api/stock/items/{id}` - Get stock item by ID
- `POST /api/stock/items` - Create stock item (Admin/Manager)
- `PUT /api/stock/items/{id}/quantity` - Update stock quantity
- `POST /api/stock/items/{id}/add` - Add stock
- `POST /api/stock/items/{id}/remove` - Remove stock
- `DELETE /api/stock/items/{id}` - Delete stock item (Admin/Manager)

### 📈 Stock Transactions
- `GET /api/stock/transactions` - Get all transactions
- `GET /api/stock/transactions/{id}` - Get transaction by ID
- `POST /api/stock/import` - Import stock
- `POST /api/stock/export` - Export stock
- `POST /api/stock/adjust` - Adjust stock (Admin/Manager)

### 📊 Analytics & Reports
- `GET /api/stock/low-stock` - Get low stock items
- `GET /api/stock/over-stock` - Get over stock items
- `GET /api/stock/statistics` - Get stock statistics
- `GET /api/stock/available?productId=...&warehouseId=...` - Check available stock

## 🔒 Authorization

The API uses role-based authorization:

- **Admin**: Full access to all endpoints
- **Manager**: Access to most endpoints except user deletion
- **Employee**: Limited access to stock operations

## 🏗️ Project Structure

```
├── API/
│   └── Controllers/          # API Controllers
├── Application/
│   ├── DTOs/                 # Data Transfer Objects
│   ├── Interfaces/           # Service Interfaces
│   └── Services/             # Business Logic Services
├── Domain/
│   ├── Entities/             # Domain Entities
│   └── RepositoryInterface/  # Repository Interfaces
├── Infrastructure/
│   ├── Data/                 # Database Context & Configurations
│   ├── Repositories/         # Repository Implementations
│   ├── Security/             # Security Services
│   └── Services/             # Infrastructure Services
├── Program.cs                # Application Entry Point
├── appsettings.json         # Configuration
└── README.md                # This file
```

## 🧪 Testing the API

### 1. Login to get JWT token
```bash
curl -X POST "https://localhost:5001/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "usernameOrEmail": "admin",
    "password": "admin123"
  }'
```

### 2. Use the token in subsequent requests
```bash
curl -X GET "https://localhost:5001/api/product" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

## 🔧 Configuration

### JWT Settings
```json
{
  "JwtSettings": {
    "SecretKey": "your-super-secret-key-that-is-at-least-32-characters-long",
    "Issuer": "WarehouseManagement",
    "Audience": "WarehouseManagementClient",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  }
}
```

### CORS Settings
```json
{
  "CorsSettings": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://localhost:4200"
    ],
    "AllowCredentials": true
  }
}
```

## 🚀 Deployment

### 1. Update Connection String
Update the connection string in `appsettings.json` for your production database.

### 2. Update JWT Secret
Generate a strong secret key for JWT tokens in production.

### 3. Configure CORS
Update CORS settings to allow your frontend domain.

### 4. Deploy
```bash
dotnet publish -c Release -o ./publish
```

## 📝 License

This project is licensed under the MIT License.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## 📞 Support

For support, email support@warehouse.com or create an issue in the repository.

---

**Happy Coding! 🚀**
