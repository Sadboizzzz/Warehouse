CREATE DATABASE Warehouse_25DH110382;
GO
USE Warehouse_25DH110382;
GO

CREATE TABLE Products (
    ProductID INT IDENTITY(1,1) PRIMARY KEY,
    ProductName NVARCHAR(100) NOT NULL,
    Category NVARCHAR(100) NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    Quantity INT NOT NULL
);
GO

INSERT INTO Products (ProductName, Category, Price, Quantity) VALUES
(N'Laptop Dell XPS - Nguyễn Duy Hưng', N'Điện tử - Nguyễn Duy Hưng', 25000000, 10),
(N'Chuột không dây Logitech - Nguyễn Duy Hưng', N'Điện tử - Nguyễn Duy Hưng', 450000, 3),
(N'Bàn phím cơ Keychron - Nguyễn Duy Hưng', N'Điện tử - Nguyễn Duy Hưng', 1800000, 0),
(N'Bàn làm việc gỗ - Nguyễn Duy Hưng', N'Nội thất - Nguyễn Duy Hưng', 3200000, 5),
(N'Ghế công thái học - Nguyễn Duy Hưng', N'Nội thất - Nguyễn Duy Hưng', 4500000, 2);
GO