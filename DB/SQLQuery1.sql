----------------------------------------------------------
--   POS SYSTEM – SCRIPT SQL COMPLET  
--   Compatible : SQL Server 2016+  
----------------------------------------------------------

IF DB_ID('POSSystem') IS NOT NULL
BEGIN
    ALTER DATABASE POSSystem SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE POSSystem;
END
GO

CREATE DATABASE POSSystem;
GO
USE POSSystem;
GO

----------------------------------------------------------
-- TABLE : tblUser
----------------------------------------------------------
CREATE TABLE tblUser (
    id INT IDENTITY PRIMARY KEY,
    username VARCHAR(50) UNIQUE NOT NULL,
    password VARCHAR(255) NOT NULL,
    role VARCHAR(50) NULL,
    isactive BIT DEFAULT 1
);
GO

----------------------------------------------------------
-- TABLE : tblVendor
----------------------------------------------------------
CREATE TABLE tblVendor (
    id INT IDENTITY PRIMARY KEY,
    vender VARCHAR(150) NOT NULL,
    address VARCHAR(200),
    contactperson VARCHAR(150),
    telephone VARCHAR(50),
    email VARCHAR(100),
    fax VARCHAR(50)
);
GO

----------------------------------------------------------
-- TABLE : tblStore
----------------------------------------------------------
CREATE TABLE tblStore (
    id INT IDENTITY PRIMARY KEY,
    store VARCHAR(150) NOT NULL,
    address VARCHAR(200) NULL,
    phone VARCHAR(50) NULL
);
GO

INSERT INTO tblStore(store,address,phone)
VALUES ('Default Store','Address','0000');
GO

----------------------------------------------------------
-- TABLE : BrandTbl
----------------------------------------------------------
CREATE TABLE BrandTbl (
    id INT IDENTITY PRIMARY KEY,
    brand VARCHAR(100) NOT NULL
);
GO

----------------------------------------------------------
-- TABLE : TblCatecory
----------------------------------------------------------
CREATE TABLE TblCatecory (
    id INT IDENTITY PRIMARY KEY,
    category VARCHAR(100) NOT NULL
);
GO

----------------------------------------------------------
-- TABLE : TblProduct1
----------------------------------------------------------
CREATE TABLE TblProduct1 (
    pcode VARCHAR(50) PRIMARY KEY,
    barcode VARCHAR(100),
    pdesc VARCHAR(200) NOT NULL,
    bid INT NOT NULL,
    cid INT NOT NULL,
    price DECIMAL(10,2) NOT NULL,
    qty INT DEFAULT 0,
    reorder INT DEFAULT 0,

    FOREIGN KEY (bid) REFERENCES BrandTbl(id),
    FOREIGN KEY (cid) REFERENCES TblCatecory(id)
);
GO

----------------------------------------------------------
-- TABLE : tblStockin
----------------------------------------------------------
CREATE TABLE tblStockin (
    id INT IDENTITY PRIMARY KEY,
    refno VARCHAR(50),
    pcode VARCHAR(50) NOT NULL,
    sdate DATETIME NOT NULL,
    stockinby VARCHAR(100),
    vendorid INT NULL,
    qty INT DEFAULT 0,
    status VARCHAR(50) DEFAULT 'Pending',

    FOREIGN KEY (pcode) REFERENCES TblProduct1(pcode),
    FOREIGN KEY (vendorid) REFERENCES tblVendor(id)
);
GO

----------------------------------------------------------
-- TABLE : tblCart1
----------------------------------------------------------
CREATE TABLE tblCart1 (
    id INT IDENTITY PRIMARY KEY,
    transno VARCHAR(50),
    pcode VARCHAR(50) NOT NULL,
    price DECIMAL(10,2) NOT NULL,
    qty INT NOT NULL,
    disc DECIMAL(10,2) DEFAULT 0,
    disc_precent DECIMAL(5,2) DEFAULT 0,
    total AS (qty * price - disc) PERSISTED,
    sdate DATETIME NOT NULL DEFAULT GETDATE(),
    status VARCHAR(20) DEFAULT 'Pending',
    cashier VARCHAR(100),

    FOREIGN KEY (pcode) REFERENCES TblProduct1(pcode)
);
GO

----------------------------------------------------------
-- TABLE : tblVat
----------------------------------------------------------
CREATE TABLE tblVat (
    id INT IDENTITY PRIMARY KEY,
    vat DECIMAL(5,2) DEFAULT 0
);
GO

INSERT INTO tblVat(vat) VALUES (0.00);
GO

----------------------------------------------------------
-- TABLE : tblCancel
----------------------------------------------------------
CREATE TABLE tblCancel (
    id INT IDENTITY PRIMARY KEY,
    transno VARCHAR(50),
    pcode VARCHAR(50),
    price DECIMAL(10,2),
    qty INT,
    sdate DATETIME,
    voidby VARCHAR(100),
    cancelledby VARCHAR(100),
    reason VARCHAR(255),
    action VARCHAR(50)
);
GO

----------------------------------------------------------
-- VUE : vwStockin
----------------------------------------------------------
CREATE VIEW vwStockin AS
SELECT s.id, s.refno, s.pcode, p.pdesc, s.qty, s.sdate, s.status
FROM tblStockin s
JOIN TblProduct1 p ON p.pcode = s.pcode;
GO

----------------------------------------------------------
-- VUE : vwSoldItems
----------------------------------------------------------
CREATE VIEW vwSoldItems AS
SELECT c.pcode, p.pdesc, SUM(c.qty) AS qty, SUM(c.total) AS total, c.sdate, c.status
FROM tblCart1 c
JOIN TblProduct1 p ON p.pcode = c.pcode
WHERE c.status = 'Sold'
GROUP BY c.pcode, p.pdesc, c.sdate, c.status;
GO

----------------------------------------------------------
-- VUE : vwCriticalItems
----------------------------------------------------------
CREATE VIEW vwCriticalItems AS
SELECT p.pcode, p.pdesc, p.qty, p.reorder
FROM TblProduct1 p
WHERE p.qty <= p.reorder;
GO

----------------------------------------------------------
-- VUE : vwCancelledOrder
----------------------------------------------------------
CREATE VIEW vwCancelledOrder AS
SELECT * FROM tblCancel;
GO

----------------------------------------------------------
PRINT 'DataBase POSSystem create success';
----------------------------------------------------------

INSERT INTO tblUser (username, password, role, isactive)
VALUES ('admin', 'admin', 'Administrator', 1);
