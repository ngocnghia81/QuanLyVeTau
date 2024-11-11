CREATE FUNCTION dbo.LaySoThuTuLonNhatTrongThang
(
    @Ngay date,
	@Prefix varchar(10) = NULL
)
RETURNS INT
AS
BEGIN
    DECLARE @MaxSoThuTu INT;
	DECLARE @Thang NVARCHAR(2) = RIGHT('0' + CAST(MONTH(@Ngay) AS NVARCHAR), 2);  -- Tháng dưới dạng 2 chữ số
	DECLARE @Nam NVARCHAR(2) = RIGHT(CAST(YEAR(@Ngay) AS NVARCHAR), 2);  -- Năm dưới dạng 2 chữ số (ví dụ, 2012 -> 12)
	IF @Prefix = 'VE'
	BEGIN
		SELECT @MaxSoThuTu = MAX(SUBSTRING(MaVe, 9, LEN(MaVe) - 8))
		FROM Ve, NhatKyTau NK 
		WHERE CONVERT(DATE, NK.NgayGio) = @Ngay;
	END
	IF @Prefix = 'HD' 
	BEGIN
		SELECT @MaxSoThuTu = MAX(SUBSTRING(MaHoaDon, 9, LEN(MaHoaDon) - 8))
		FROM HoaDon 
		WHERE CONVERT(DATE, ThoiGianLapHoaDon) = @Ngay;
	END
	IF @Prefix = 'KH'
	BEGIN
		SELECT @MaxSoThuTu = MAX(CAST(SUBSTRING(MaKhach, LEN(@Prefix + @Thang + @Nam) + 1, LEN(MaKhach)) AS INT))
		FROM KhachHang
		WHERE MaKhach LIKE @Prefix + @Thang + @Nam + '%';
	
	IF @Prefix = 'TK'
	BEGIN
		SELECT @MaxSoThuTu = MAX(CAST(SUBSTRING(MaTaiKhoan, LEN(@Prefix + @Thang + @Nam) + 1, LEN(MaTaiKhoan)) AS INT))
		FROM TaiKhoan
		WHERE MaTaiKhoan LIKE @Prefix + @Thang + @Nam + '%';
	END

	END
    IF @MaxSoThuTu IS NULL
        SET @MaxSoThuTu = 0;
    RETURN @MaxSoThuTu;
END;
go

CREATE FUNCTION dbo.TaoMa
(
    @Prefix CHAR(2) = NULL,
    @Ngay DATE
)
RETURNS NVARCHAR(19)
AS
BEGIN
    DECLARE @Ma NVARCHAR(19);
    
    -- Chuyển ngày và tháng thành chuỗi có 2 ký tự
   
    DECLARE @ThangStr NVARCHAR(2) = RIGHT('0' + CAST(MONTH(@Ngay) AS NVARCHAR), 2);
    
    -- Lấy 2 chữ số cuối của năm
    DECLARE @NamStr NVARCHAR(2) = RIGHT(CAST(YEAR(@Ngay) AS NVARCHAR), 2);
    
    -- Lấy số thứ tự lớn nhất hiện có trong ngày
    DECLARE @SoThuTu INT = dbo.LaySoThuTuLonNhatTrongThang(@Ngay, @Prefix);
    
    -- Tạo mã
        SET @Ma = @Prefix + @ThangStr + @NamStr + CAST(@SoThuTu + 1 AS NVARCHAR);  -- Không cần RIGHT() ở đây

    RETURN @Ma;
END;
go
CREATE PROCEDURE DangKy  
    @TenKhach NVARCHAR(100),
    @NamSinh DATE,
    @Email VARCHAR(100),
    @SDT VARCHAR(10),
    @CCCD VARCHAR(12),
    @DiaChi NVARCHAR(MAX),
    @MatKhau VARCHAR(255)
AS
BEGIN 
	DECLARE  @MaKhach VARCHAR(10);
	DECLARE @MaTaiKhoan VARCHAR(10);
	set @MaKhach = select dbo.taoma('KH',getdate())
    -- Chèn dữ liệu vào bảng KhachHang
    INSERT INTO KhachHang (MaKhach, TenKhach, NamSinh, Email, SDT, CCCD, DiaChi)
    VALUES (@MaKhach, @TenKhach, @NamSinh, @Email, @SDT, @CCCD, @DiaChi);
	set @MaTaiKhoan =  dbo.taoma('TK',getdate())
    -- Chèn dữ liệu vào bảng TaiKhoan
    INSERT INTO TaiKhoan (MaTaiKhoan, Email, MatKhau, DaXoa)
    VALUES (@MaTaiKhoan, @Email, @MatKhau, 0);

END;
go
EXEC DangKy
    @TenKhach = N'Nguyen Van A',
    @NamSinh = '1990-01-01',
    @Email = 'nguyenvana@example.com',
    @SDT = '0123456789',
    @CCCD = '123456789012',
    @DiaChi = N'123 Đường ABC, Phường XYZ, Quận 1, TP.HCM',
    @MatKhau = 'password123';

	EXEC DangKy @TenKhach = 'Khanh', @NamSinh = '2004', @Email = 'khanhsoai6@gmail.com', @SDT = '0944144560', @CCCD = '56789165789', @DiaChi = '156', @MatKhau = '1'
	EXEC DangKy @TenKhach = 'Khanh', @NamSinh = '1212', @Email = 'khanhsoai68@gmail.com', @SDT = '0949006301', @CCCD = '312321', @DiaChi = '1', @MatKhau = '1'