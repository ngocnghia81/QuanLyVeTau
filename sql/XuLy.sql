
CREATE FUNCtION LayTenGa
(
	@MaChiTiet varchar(100)
)
returns nvarchar(100)
as
begin
	Declare @TenGa nvarchar(100);

	select @TenGa = ga.TenGa
	from ChiTietLichTrinh ctlt 
		join ga on ga.MaGa = ctlt.MaGa
	where ctlt.MaChiTiet = @MaChiTiet
	return @TenGa
end
go

CREATE FUNCTION dbo.LaySoThuTuLonNhatTrongThang
(
    @Ngay DATE,
    @Prefix VARCHAR(10) = NULL
)
RETURNS INT
AS
BEGIN
    DECLARE @MaxSoThuTu INT = 0;
	DECLARE @Day varchar(2) = RIGHT('0' + CAST(Day(@Ngay) AS NVARCHAR), 2)
    DECLARE @Thang NVARCHAR(2) = RIGHT('0' + CAST(MONTH(@Ngay) AS NVARCHAR), 2);  -- Month as two digits
    DECLARE @Nam NVARCHAR(2) = RIGHT(CAST(YEAR(@Ngay) AS NVARCHAR), 2);            -- Year as last two digits

    -- Check for each prefix case and compute the max sequence number
    IF @Prefix = 'VE'
    BEGIN
        SELECT @MaxSoThuTu = MAX(CAST(SUBSTRING(Ve.MaVe, 9, LEN(Ve.MaVe) - 8) AS INT))
        FROM Ve
        JOIN NhatKyTau NK ON Ve.MaNhatKy = NK.MaNhatKy -- Adjust this join condition as necessary
        WHERE CAST(SUBSTRING(Ve.MaVe, 3, 6) AS INT) = @Day +@Thang +@Nam
    END
    ELSE IF @Prefix = 'HD'
    BEGIN
        SELECT @MaxSoThuTu = MAX(CAST(SUBSTRING(MaHoaDon, 9, LEN(MaHoaDon) - 8) AS INT))
        FROM HoaDon
        WHERE CONVERT(DATE, ThoiGianLapHoaDon) = @Ngay;
    END
	ELSE IF @Prefix Like 'TA%'
    BEGIN
        SELECT @MaxSoThuTu = MAX(CAST(SUBSTRING(MaNhatKy, LEN(@Prefix+@Day+@Thang+@Nam)+1, LEN(@Prefix+@Day+@Thang+@Nam)) AS INT))
        FROM NhatKyTau
        WHERE CONVERT(DATE, NgayGio) = @Ngay;
    END
    ELSE IF (@Prefix = 'KH' or @Prefix = 'TK')
    BEGIN
        SELECT @MaxSoThuTu = MAX(CAST(SUBSTRING(MaKhach, LEN('KH' + @Thang + @Nam) + 1, LEN(MaKhach) - LEN('KH' + @Thang + @Nam)) AS INT))
        FROM KhachHang
        WHERE MaKhach LIKE 'KH' + @Thang + @Nam + '%';
    END
    ELSE IF (@Prefix = 'TKNV' or @Prefix = 'NV') -- For the last case, it should be `NV` instead of repeating the `TK` condition
    BEGIN
        SELECT @MaxSoThuTu = MAX(CAST(SUBSTRING(MaTaiKhoan, LEN('TKNV' + @Nam) + 1, LEN(MaTaiKhoan) - LEN('TKNV' + @Nam)) AS INT))
        FROM TAIKHOANNHANVIEN
        WHERE MaTaiKhoan LIKE 'TKNV' + @Nam + '%';
    END
	ELSE IF (@PREFIX = 'HL')
	BEGIN
		SELECT @MaxSoThuTu = MAX(CAST(SUBSTRING(HanhLy.MaHanhLy, 9, LEN(HanhLy.MaHanhLy) - 8) AS INT))
        FROM HanhLy       
		WHERE MaHanhLy LIKE 'HL'+@Day+@Thang+@Nam+'%'
	END

    -- Set @MaxSoThuTu to 0 if no records were found
    IF @MaxSoThuTu IS NULL
        SET @MaxSoThuTu = 0;

    RETURN @MaxSoThuTu;
END;
GO


CREATE FUNCTION dbo.TaoMa
(
    @Prefix VARCHAR(10) = NULL,
    @Ngay DATE
)
RETURNS NVARCHAR(19)
AS
BEGIN
    DECLARE @Ma NVARCHAR(19);
    
    -- Chuyển ngày và tháng thành chuỗi có 2 ký tự
    DECLARE @NgayStr NVARCHAR(2) = RIGHT('0' + CAST(Day(@Ngay) AS NVARCHAR), 2);
    DECLARE @ThangStr NVARCHAR(2) = RIGHT('0' + CAST(MONTH(@Ngay) AS NVARCHAR), 2);
    
    -- Lấy 2 chữ số cuối của năm
    DECLARE @NamStr NVARCHAR(2) = RIGHT(CAST(YEAR(@Ngay) AS NVARCHAR), 2);
    
    -- Lấy số thứ tự lớn nhất hiện có trong ngày
    DECLARE @SoThuTu INT = dbo.LaySoThuTuLonNhatTrongThang(@Ngay, @Prefix);
    if (@Prefix = 'HD' or @Prefix = 'Ve' or @Prefix like 'TA%' OR @Prefix = 'HL')
		SET @Ma = @Prefix+ @NgayStr + @ThangStr + @NamStr + CAST(@SoThuTu + 1 AS NVARCHAR);  -- Không cần RIGHT() ở đây
	else if(@Prefix = 'NV' or @Prefix ='TKNV')
		set @Ma= @Prefix + @NamStr + CAST(@SoThuTu +1 as nvarchar);
    -- Tạo mã
     else  SET @Ma = @Prefix + @ThangStr + @NamStr + CAST(@SoThuTu + 1 AS NVARCHAR);  -- Không cần RIGHT() ở đây

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
	set @MaKhach =  dbo.taoma('KH',getdate())
    -- Chèn dữ liệu vào bảng KhachHang
    INSERT INTO KhachHang (MaKhach, TenKhach, NamSinh, Email, SDT, CCCD, DiaChi)
    VALUES (@MaKhach, @TenKhach, @NamSinh, @Email, @SDT, @CCCD, @DiaChi);
	SET @MaTaiKhoan = 'TK' + SUBSTRING(@MaKhach, 3, LEN(@MaKhach) - 2);
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
            AND ctlt.Stt_Ga BETWEEN (SELECT MIN(Stt_Ga) FROM GaTram)+1 AND (SELECT MAX(Stt_Ga) FROM GaTram)
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

select dbo.TinhTongThoiGianDiChuyen('TA180811241','2024-11-08 13:00:00.000','sg','lk')

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
	DECLARE @MaGaDau Varchar(100), @MaGaCuoi Varchar(100), @MAGADI NVARCHAR(100), @MAGADEN NVARCHAR(100);
	select @MaGaDau = CTLT.MaGa
	from ChiTietLichTrinh CTLT join dbo.LayLichTrinhTheoDiemDiDiemDen(@DiemDi,@DiemDen) as MaLT on MaLT.MaLichTrinh = CTLT.MaLichTrinh
	where CTLT.Stt_Ga = 1

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
		Case
			when @MaGaDau = @MAGADI then NK.NgayGio
			else dbo.TinhTongThoiGianDiChuyen(NK.MaNhatKy,NK.NgayGio,@MaGaDau,@MAGADI)
		end,
        DATEADD(MINUTE, -15, dbo.TinhTongThoiGianDiChuyen(NK.MaNhatKy, NK.NgayGio, @MaGaDau, @MAGADEN)),
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

CREATE FUNCTION LAYTOA
(
	@MaTau varchar(100)
)
Returns Table
as
return
(
	select * from toa where MaTau = @MaTau
)
go

CREATE FUNCTION GiaVe
(
	@MaLoaiToa varchar(100)
)
returns decimal(10,2)
as
begin
	Declare @giave decimal(10,2);
	select @giave = GiaMacDinh+(GiaMacDinh*CoDieuHoa*0.1)
	from LoaiToa
	where MaLoaiToa = @MaLoaiToa
	return @giave;
end
go

CREATE FUNCTION LayKhoang
(
	@MaToa varchar(100)
)
Returns @Result Table
(
	SoToa int,
	MaKhoang varchar(100),
	SoKhoang int,
	LoaiToa nvarchar(100),
	SLChoNgoi int,
	GiaVe decimal(10,2)
)
AS
BEGIN
	
	INSERT INTO @Result
	select Toa.SoToa,K.MaKhoang,K.SoKhoang, Toa.MaLoaiToa,K.SoChoNgoiToiDa, dbo.GiaVe(Toa.MaLoaiToa)
	from Khoang K join Toa on Toa.MaToa = K.MaToa
	where K.MaToa = @MaToa


	RETURN
END
go

CREATE FUNCTION LayKhuyenMai()
returns table
as
return
(
select * 
from KhuyenMai
where SoLuongConLai > 0 and GETDATE() < NgayKetThuc and GETDATE() > NgayBatDau
)
go


select * from LayKhuyenMai()

CREATE PROCEDURE TAOHOADON
(
    @Email varchar(100),
    @MaKhuyenMai varchar(100),
    @ThanhTien decimal(19,2),
    @ThoiGian datetime
)
AS
BEGIN
    DECLARE @MaKhach varchar(100), @MaHoaDon varchar(100);

    -- Nếu không có thời gian, gán giá trị mặc định
    IF @ThoiGian IS NULL
    BEGIN
        SET @ThoiGian = GETDATE();  -- Nếu không có thời gian, gán giá trị mặc định là thời gian hiện tại
    END

    -- Tạo mã hóa đơn
    SET @MaHoaDon = dbo.TaoMa('HD', @ThoiGian);  -- Gọi hàm để tạo mã hóa đơn

    -- Lấy mã khách từ bảng KhachHang dựa trên email
    SELECT @MaKhach = MaKhach
    FROM KhachHang
    WHERE Email = @Email;

    -- Thực hiện thao tác INSERT vào bảng HoaDon
    INSERT INTO HoaDon (MaHoaDon, MaKhach, MaKhuyenMai, ThanhTien, ThoiGianLapHoaDon)
    VALUES (@MaHoaDon, @MaKhach, @MaKhuyenMai, @ThanhTien, @ThoiGian);

    -- Trả về mã hóa đơn (có thể dùng để lấy về sau này nếu cần)
    SELECT @MaHoaDon AS MaHoaDon;
END;
GO

exec dbo.TAOHOADON @email = 'khanhsoai6@gmail.com', @MaKhuyenMai = null, @ThanhTien = '35200', @ThoiGian = null
exec dbo.TAOHOADON @email = 'khanhsoai6@gmail.com', @MaKhuyenMai = null, @ThanhTien = '70400', @ThoiGian = null


CREATE PROCEDURE TAOVE
(
	@MaNhatKy varchar(100),
	@MaHoaDon varchar(100),
	@GiaVe int,
	@MaKhoang varchar(100),
	@stt int,
	@DiemDi nvarchar(100),
	@DiemDen nvarchar(100)
)
as 
begin
	Declare @MaVe varchar(100) = dbo.TaoMa('Ve',Getdate());
	Declare @MaGaDi varchar(100), @MaGaDen varchar(100);
	select @MaGaDi = CTLT.MaChiTiet 
		from ChiTietLichTrinh CTLT 
			join ga on CTLT.MaGa = ga.MaGa 
			join NhatKyTau NK on NK.MaLichTrinh = CTLT.MaLichTrinh
		where ga.TenGa = @DiemDi and NK.MaNhatKy = @MaNhatKy;
	select @MaGaDen = CTLT.MaChiTiet 
		from ChiTietLichTrinh CTLT 
			join ga on CTLT.MaGa = ga.MaGa 
			join NhatKyTau NK on NK.MaLichTrinh = CTLT.MaLichTrinh
		where ga.TenGa = @DiemDen and NK.MaNhatKy = @MaNhatKy;

	Insert into Ve
	values (@MaVe,@MaNhatKy,@MaHoaDon,@GiaVe,@MaKhoang,@stt,@MaGaDi,@MaGaDen,0)
end
go												

EXEC dbo.TAOVE @MaNhatKy = 'TA180811241', @MaHoaDon = 'HD16112416', @GiaVe = 104400, @MaKhoang = 'K449', @stt = 2, @DiemDi = N'Sài Gòn', @DiemDen = 'Hà Nội'	 

select * from HoaDon where MaHoaDon= 'HD1611241'

CREATE FUNCTION dbo.LaySttGaFromMaChiTiet (@MaChiTiet VARCHAR(100))
RETURNS INT
AS
BEGIN
    DECLARE @SttGa INT;

    -- Lấy giá trị Stt_Ga từ bảng ChiTietLichTrinh
    SELECT @SttGa = ctlt.Stt_Ga
    FROM ChiTietLichTrinh ctlt
    WHERE ctlt.MaChiTiet = @MaChiTiet;

    -- Trả về kết quả
    RETURN @SttGa;
END;
GO

CREATE FUNCTION dbo.LayVeTheoGaDiDen (
	@MaKhoang varchar(100),
	@MaNK varchar(100),
    @DiemDi nvarchar(100), -- Tham số SttGaDi
    @DiemDen nvarchar(100) -- Tham số SttGaDen
	
)
RETURNS TABLE
AS
RETURN
    WITH STTDi as
	(
		select ctlt.Stt_Ga
		from ChiTietLichTrinh ctlt
		join NhatKyTau NK on NK.MaLichTrinh = ctlt.MaLichTrinh
		join Ga on ga.MaGa = ctlt.MaGa
		where NK.MaNhatKy = @MaNK and ga.TenGa = @DiemDi
	),
	STTDen as
	(
		select ctlt.Stt_Ga
		from ChiTietLichTrinh ctlt
		join NhatKyTau NK on NK.MaLichTrinh = ctlt.MaLichTrinh
		join Ga on ga.MaGa = ctlt.MaGa
		where NK.MaNhatKy = @MaNK and ga.TenGa = @DiemDen
	),
	BangVeDaBan AS (
        SELECT DISTINCT
            VE.MaVe,
			Ve.STT_Ghe,
            dbo.GetSttGaFromMaChiTiet(ve.DiemDi) AS SttGaDi,
            dbo.GetSttGaFromMaChiTiet(ve.DiemDen) AS SttGaDen,
            dbo.LayTenGa(Ve.DiemDi) AS TenGaDi,
            dbo.LayTenGa(Ve.DiemDen) AS TenGaDen
        FROM VE
        JOIN NhatKyTau nk ON ve.MaNhatKy = nk.MaNhatKy
        JOIN ChiTietLichTrinh ctlt ON nk.MaLichTrinh = ctlt.MaLichTrinh
        WHERE ve.MaKhoang = @MaKhoang and ve.MaNhatKy = @MaNK and ve.DaThuHoi = 0
    )
    SELECT BangVeDaBan.*
    FROM BangVeDaBan,STTDi,STTDen
    WHERE (
        STTDi.Stt_Ga BETWEEN SttGaDi AND SttGaDen - 1
        OR STTDen.Stt_Ga BETWEEN SttGaDi + 1 AND SttGaDen
    );

--select * from dbo.LayVeTheoGaDiDen('K449','TA180811241',N'Bình Thuận',N'Hà Nội')

CREATE TRIGGER Trigger_Insert_TRAVE_VE on LICHSUDOITRAVE
AFTER INSERT 
as
BEGIN
    
    UPDATE VE
    SET VE.DaThuHoi = 1      
    FROM VE,inserted
    WHERE VE.MaVe = inserted.MaVe
END;

Insert into LichSuDoiTraVe
values ('Ve1911249',N'Tra',GETDATE(),10)


CREATE PROCEDURE TraVe
    @MaVe VARCHAR(15),
	@LePhi decimal(10,2) =0
AS
BEGIN
    DECLARE @ThoiGianChay DATETIME;
    DECLARE @ThoiGianTra DATETIME = GETDATE();
    DECLARE @GiaVe DECIMAL(10, 2);


    -- Lấy giá vé và thời gian chạy của tàu từ vé
    SELECT @GiaVe = V.GiaVe, @ThoiGianChay = Nht.NgayGio -- Cập nhật tên cột nếu cần
    FROM Ve V
    JOIN NhatKyTau Nht ON V.MaNhatKy = Nht.MaNhatKy
    WHERE V.MaVe = @MaVe;

    -- Tính phí hoàn vé
    IF DATEDIFF(HOUR, @ThoiGianTra, @ThoiGianChay) >= 72
    BEGIN
        SET @LePhi = @GiaVe * 0.5;
    END
    ELSE IF DATEDIFF(HOUR, @ThoiGianTra, @ThoiGianChay) >= 24
    BEGIN
        SET @LePhi = @GiaVe * 0.2; 
    END
    ELSE
    BEGIN
        SET @LePhi = @GiaVe; 
    END

    -- Thực hiện trả vé
    INSERT INTO LichSuDoiTraVe
    VALUES ( @MaVe, 'Trả',GETDATE(), @LePhi);
    
    -- Cập nhật trạng thái vé
    UPDATE Ve SET DaThuHoi = 1 WHERE MaVe = @MaVe;

    PRINT 'Trả vé thành công, lệ phí: ' + CAST(@LePhi AS NVARCHAR(15));
END;

exec dbo.TraVe 'Ve1911248'