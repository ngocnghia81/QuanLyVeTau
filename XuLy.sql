﻿CREATE FUNCTION dbo.LaySoThuTuLonNhatTrongThang
(
    @Ngay DATE,
    @Prefix VARCHAR(10) = NULL
)
RETURNS INT
AS
BEGIN
    DECLARE @MaxSoThuTu INT = 0;
    DECLARE @Thang NVARCHAR(2) = RIGHT('0' + CAST(MONTH(@Ngay) AS NVARCHAR), 2);  -- Month as two digits
    DECLARE @Nam NVARCHAR(2) = RIGHT(CAST(YEAR(@Ngay) AS NVARCHAR), 2);            -- Year as last two digits

    -- Check for each prefix case and compute the max sequence number
    IF @Prefix = 'VE'
    BEGIN
        SELECT @MaxSoThuTu = MAX(CAST(SUBSTRING(MaVe, 9, LEN(MaVe) - 8) AS INT))
        FROM Ve
        JOIN NhatKyTau NK ON Ve.MaNhatKy = NK.MaNhatKy -- Adjust this join condition as necessary
        WHERE CONVERT(DATE, NK.NgayGio) = @Ngay;
    END
    ELSE IF @Prefix = 'HD'
    BEGIN
        SELECT @MaxSoThuTu = MAX(CAST(SUBSTRING(MaHoaDon, 9, LEN(MaHoaDon) - 8) AS INT))
        FROM HoaDon
        WHERE CONVERT(DATE, ThoiGianLapHoaDon) = @Ngay;
    END
    ELSE IF @Prefix = 'KH'
    BEGIN
        SELECT @MaxSoThuTu = MAX(CAST(SUBSTRING(MaKhach, LEN(@Prefix + @Thang + @Nam) + 1, LEN(MaKhach) - LEN(@Prefix + @Thang + @Nam)) AS INT))
        FROM KhachHang
        WHERE MaKhach LIKE @Prefix + @Thang + @Nam + '%';
    END
    ELSE IF @Prefix = 'TK'
    BEGIN
        SELECT @MaxSoThuTu = MAX(CAST(SUBSTRING(MaTaiKhoan, LEN(@Prefix + @Thang + @Nam) + 1, LEN(MaTaiKhoan) - LEN(@Prefix + @Thang + @Nam)) AS INT))
        FROM TaiKhoan
        WHERE MaTaiKhoan LIKE @Prefix + @Thang + @Nam + '%';
    END
	BEGIN
        SELECT @MaxSoThuTu = MAX(CAST(SUBSTRING(MaTaiKhoan, LEN(@Prefix + @Nam) + 1, LEN(MaTaiKhoan) - LEN(@Prefix + @Thang + @Nam)) AS INT))
        FROM TAIKHOANNHANVIEN
        WHERE MaTaiKhoan LIKE @Prefix + @Nam + '%';
    END

    -- Set @MaxSoThuTu to 0 if no records were found
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

select dbo.TaoMa('TKNV',GETDATE())

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
	set @MaKhach =  dbo.taoma('KH',getdate())
    -- Chèn dữ liệu vào bảng KhachHang
    INSERT INTO KhachHang (MaKhach, TenKhach, NamSinh, Email, SDT, CCCD, DiaChi)
    VALUES (@MaKhach, @TenKhach, @NamSinh, @Email, @SDT, @CCCD, @DiaChi);
	set @MaTaiKhoan =  dbo.taoma('TK',getdate())
    -- Chèn dữ liệu vào bảng TaiKhoan
    INSERT INTO TaiKhoan (MaTaiKhoan, Email, MatKhau, DaXoa)
    VALUES (@MaTaiKhoan, @Email, @MatKhau, 0);

END;
go

CREATE FUNCTION LayLichTrinhTheoDiemDiDiemDen
(
    @DiemDi NVARCHAR(100),
    @DiemDen NVARCHAR(100)
)
RETURNS TABLE
AS
RETURN
(
    SELECT DiemDi.TenGa AS TenGaDi, DiemDi.Stt_Ga AS SttGaDi, DiemDi.MaLichTrinh,
           DiemDen.TenGa AS TenGaDen, DiemDen.Stt_Ga AS SttGaDen
    FROM (
        SELECT Ga.TenGa, CTLT.Stt_Ga, CTLT.MaLichTrinh
        FROM Ga
        JOIN ChiTietLichTrinh CTLT ON Ga.MaGa = CTLT.MaGa
        WHERE Ga.TenGa = @DiemDi
    ) AS DiemDi
    JOIN (
        SELECT Ga.TenGa, CTLT.Stt_Ga, CTLT.MaLichTrinh
        FROM Ga
        JOIN ChiTietLichTrinh CTLT ON Ga.MaGa = CTLT.MaGa
        WHERE Ga.TenGa = @DiemDen
    ) AS DiemDen
    ON DiemDi.MaLichTrinh = DiemDen.MaLichTrinh
    AND DiemDi.Stt_Ga < DiemDen.Stt_Ga
);
go
SELECT * FROM LayLichTrinhTheoDiemDiDiemDen(N'Sài Gòn', N'Bình Thuận');

CREATE FUNCTION SoLuongToiDaCuaTau
(
	@MaTau Varchar(100)
)
Returns int
as
begin
	DECLARE @SoLuong int;
	select @SoLuong = sum(khoang.SoChoNgoiToiDa)
	from tau join toa on tau.MaTau = toa.MaTau join Khoang on Khoang.MaToa = toa.MaToa
	where tau.MaTau = 'TA1'
	return @soluong
end
go

CREATE FUNCTION TinhTongThoiGianDiChuyen (
    @MaNhatKy VARCHAR(100),
    @ThoiGianDi DATETIME,
    @MaGaDi VARCHAR(100),
    @MaGaDen VARCHAR(100)
)
RETURNS DATETIME
AS
BEGIN
    DECLARE @TongThoiGian DATETIME = @ThoiGianDi;  -- Khởi tạo thời gian ban đầu là thời gian đi
    DECLARE @SoGaDung INT = 0;  -- Số ga dừng
    DECLARE @ThoiGianDiChuyen TIME;  -- Thời gian di chuyển giữa các ga

    -- Xác định các ga dừng trong lộ trình giữa ga đi và ga đến
    WITH GaTram AS (
        SELECT 
            ctlt.Stt_Ga, 
            ctlt.ThoiGianDiChuyenTuTramTruoc
        FROM 
            ChiTietLichTrinh ctlt 
            JOIN NhatKyTau nk ON ctlt.MaLichTrinh = nk.MaLichTrinh
        WHERE 
            nk.MaNhatKy = @MaNhatKy
            AND (ctlt.MaGa = @MaGaDi OR ctlt.MaGa = @MaGaDen)
    ),
    GaKhoangThoiGian AS (
        SELECT 
            ctlt.Stt_Ga, 
            ctlt.ThoiGianDiChuyenTuTramTruoc as tg
        FROM 
            ChiTietLichTrinh ctlt 
            JOIN NhatKyTau nk ON ctlt.MaLichTrinh = nk.MaLichTrinh
        WHERE 
            nk.MaNhatKy = @MaNhatKy
            AND ctlt.Stt_Ga BETWEEN (SELECT MIN(Stt_Ga) FROM GaTram) AND (SELECT MAX(Stt_Ga) FROM GaTram)
    )

    -- Tính tổng thời gian di chuyển giữa các ga
    SELECT 
        @SoGaDung = COUNT(*),
        @TongThoiGian = DATEADD(SECOND, SUM(DATEDIFF(SECOND, '00:00:00', tg)), @TongThoiGian)
    FROM 
        GaKhoangThoiGian;

    -- Cộng thêm thời gian nghỉ 15 phút cho mỗi ga
    SET @TongThoiGian = DATEADD(MINUTE, @SoGaDung * 15, @TongThoiGian);

    -- Nếu không có thời gian di chuyển, trả về thời gian đi ban đầu
    RETURN @TongThoiGian;
END;
GO


CREATE FUNCTION LayTau
(
    @Ngay DATE,
    @DiemDi NVARCHAR(100),
    @DiemDen NVARCHAR(100)
)
RETURNS @Result TABLE
(
    MATAU NVARCHAR(100),
    MaNhatKy NVARCHAR(100),
    ThoiGianDi DATETIME,
    ThoiGianDen DATETIME,
    SLChoTrong INT
)
AS
BEGIN
	DECLARE @MAGADI NVARCHAR(100), @MAGADEN NVARCHAR(100);
	
	SELECT @MAGADI = MaGa
	FROM Ga
	WHERE TenGa = @DiemDi;

	SELECT @MAGADEN = MaGa
	FROM Ga
	WHERE TenGa = @DiemDen;

    INSERT INTO @Result
    SELECT 
        NK.MATAU, 
        NK.MaNhatKy, 
		NK.NgayGio,
        dbo.TinhTongThoiGianDiChuyen(NK.MaNhatKy,NK.NgayGio,@MAGADI,@MAGADEN),
        dbo.SoLuongToiDaCuaTau(NK.MaTau) - COUNT(VE.MANHATKY) AS SLChoTrong
    FROM 
        NhatKyTau NK
    JOIN 
        dbo.LayLichTrinhTheoDiemDiDiemDen(@DiemDi, @DiemDen) AS MaLT
    ON 
        NK.MaLichTrinh = MaLT.MaLichTrinh
    LEFT JOIN 
        VE ON VE.MaNhatKy = NK.MaNhatKy
    WHERE 
        CONVERT(DATE, NK.NgayGio) = @Ngay
    GROUP BY 
        NK.MaNhatKy, NK.MaTau, NK.NgayGio;
	return;
END;
GO


select dbo.TinhTongThoiGianDiChuyen('TA180811241', '2024-11-08 13:00:00.000','SG','HN')
SELECT * FROM LayTau('2024-11-08', N'Hà Nội', N'Sài Gòn');