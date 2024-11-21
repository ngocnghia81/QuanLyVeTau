
-------------Tau------------------------
DROP TRIGGER trg_ThemTau

-- Tạo trigger INSTEAD OF để tự động sinh mã tàu khi thêm mới tàu
CREATE TRIGGER trg_ThemTau
ON Tau
INSTEAD OF INSERT
AS
BEGIN
    DECLARE @MaxMaTau INT;
    DECLARE @NewMaTau VARCHAR(10);
    DECLARE @Prefix VARCHAR(2) = 'TA';
    DECLARE @Suffix INT;

    SELECT @MaxMaTau = MAX(CAST(SUBSTRING(MaTau, 3, LEN(MaTau)) AS INT))
    FROM Tau
    WHERE MaTau LIKE 'TA%' 

    IF @MaxMaTau IS NULL
    BEGIN
        SET @MaxMaTau = 0;
    END

    -- Tạo mã tàu mới
    SET @Suffix = @MaxMaTau + 1;
    SET @NewMaTau = @Prefix + RIGHT('00' + CAST(@Suffix AS VARCHAR(2)), 2);

    INSERT INTO Tau (MaTau, TenTau, DaXoa)
    SELECT @NewMaTau, TenTau, DaXoa
    FROM inserted;
END;


-- Trigger đánh dấu tàu là đã xóa thay vì xóa hoàn toàn
DROP TRIGGER trg_SetDaXoa_Tau

CREATE TRIGGER trg_SetDaXoa_Tau
ON Tau
INSTEAD OF DELETE
AS
BEGIN
    IF EXISTS (
        SELECT 1
        FROM NhatKyTau
        JOIN LichTrinhTau ON NhatKyTau.MaLichTrinh = LichTrinhTau.MaLichTrinh
        WHERE NhatKyTau.MaTau IN (SELECT MaTau FROM DELETED)
        AND LichTrinhTau.TrangThai = N'Đang hoạt động'
        AND NhatKyTau.NgayGio > GETDATE()
    )
    BEGIN
		RAISERROR('Tàu có lịch trình trong tương lai và không thể xoá.', 16, 2);
        RETURN;    
	END

    ELSE IF EXISTS (
        SELECT 1
        FROM Ve
        JOIN NhatKyTau ON Ve.MaNhatKy = NhatKyTau.MaNhatKy
        WHERE NhatKyTau.MaTau IN (SELECT MaTau FROM DELETED)
        AND Ve.DaThuHoi = 0
    )
    BEGIN
        RAISERROR('Tàu có vé đã được bán và không thể xoá.', 16, 2);
        RETURN;
    END
    ELSE
    BEGIN
        UPDATE Tau
        SET DaXoa = 1
        WHERE MaTau IN (SELECT MaTau FROM DELETED);
    END
END;

-- Tạo Trigger INSTEAD OF INSERT để tự động sinh mã toa mà không lọc theo mã tàu
DROP TRIGGER trg_ThemToa;
CREATE TRIGGER trg_ThemToa
ON Toa
INSTEAD OF INSERT
AS
BEGIN
    DECLARE @MaTau VARCHAR(10);
    DECLARE @MaxToa INT;
    DECLARE @NewMaToa VARCHAR(10);
    DECLARE @Suffix INT;
    DECLARE @Prefix VARCHAR(2) = 'TO';

    -- Lấy mã tàu từ bảng inserted
    SELECT @MaTau = MaTau FROM inserted;

    -- Kiểm tra số lượng toa hiện có trong bảng Toa và lấy số toa lớn nhất không phân biệt tàu
    SELECT @MaxToa = MAX(CAST(SUBSTRING(MaToa, 3, LEN(MaToa) - 2) AS INT))
    FROM Toa;

    -- Nếu không có toa nào, khởi tạo số toa là 0
    IF @MaxToa IS NULL
    BEGIN
        SET @MaxToa = 0;
    END

    -- Tính số toa tiếp theo
    SET @Suffix = @MaxToa + 1;

    -- Tạo mã toa mới với số tiếp theo. Dùng `CAST` mà không cố định chiều dài
    SET @NewMaToa = @Prefix + CAST(@Suffix AS VARCHAR(10)); -- Tạo mã toa như TO1, TO10, TO100, TO1000, v.v.

    -- Kiểm tra số toa trong khoảng 7 đến 12
    -- Chèn toa mới vào bảng Toa
    INSERT INTO Toa (MaToa, MaTau, SoToa, MaLoaiToa)
    SELECT @NewMaToa, MaTau, SoToa, MaLoaiToa
    FROM inserted;
END;


-- Tạo Trigger kiểm tra khi xoá toa
DROP TRIGGER IF EXISTS trg_KiemTraXoaToa;
CREATE TRIGGER trg_KiemTraXoaToa
ON Toa
INSTEAD OF DELETE
AS
BEGIN
    DECLARE @MaToa VARCHAR(10);
    DECLARE @MaTau VARCHAR(10);
    DECLARE @HasFutureSchedule INT;
    DECLARE @HasTicket INT;

    -- Lấy mã toa từ bảng deleted (chứa các bản ghi bị xóa)
    SELECT @MaToa = MaToa FROM deleted;

    -- Lấy mã tàu từ bảng Toa tương ứng với mã toa bị xóa
    SELECT @MaTau = MaTau 
    FROM Toa
    WHERE MaToa = @MaToa;

    -- Kiểm tra xem toa có lịch trình trong tương lai không (LichTrinh.ThoiGianKhoiHanh > GETDATE())
    SELECT @HasFutureSchedule = COUNT(*) 
    FROM NhatKyTau 
    JOIN LichTrinhTau ON NhatKyTau.MaLichTrinh = LichTrinhTau.MaLichTrinh
    WHERE NhatKyTau.MaTau = @MaTau
      AND NhatKyTau.NgayGio > GETDATE();

    -- Kiểm tra xem toa có vé nào chưa được sử dụng không
    SELECT @HasTicket = COUNT(*) 
    FROM Ve 
    JOIN NhatKyTau ON Ve.MaNhatKy = NhatKyTau.MaNhatKy
    WHERE NhatKyTau.MaTau = @MaTau
      AND NhatKyTau.NgayGio > GETDATE()
      AND Ve.DaThuHoi = 0;

    -- Nếu có lịch trình trong tương lai hoặc có vé chưa sử dụng, không cho phép xóa
    IF @HasFutureSchedule > 0 AND @HasTicket > 0
    BEGIN
        RAISERROR('Không thể xóa toa này vì có vé chưa sử dụng!', 16, 1);
        RETURN;
    END
    
    DELETE FROM Toa
    WHERE MaToa = @MaToa;
END;


DROP TRIGGER trg_ThemKhoang
CREATE TRIGGER trg_ThemKhoang
ON Khoang
INSTEAD OF INSERT
AS
BEGIN
    DECLARE @MaToa VARCHAR(10);
    DECLARE @MaxKhoang INT;
    DECLARE @NewMaKhoang VARCHAR(10);
    DECLARE @Suffix INT;
    DECLARE @Prefix VARCHAR(1) = 'K'; 

    -- Lấy mã toa từ bảng inserted
    SELECT @MaToa = MaToa FROM inserted;

    -- Kiểm tra số lượng khoang hiện có trong bảng Khoang và lấy số khoang lớn nhất
    SELECT @MaxKhoang = MAX(CAST(SUBSTRING(MaKhoang, 2, LEN(MaKhoang) - 1) AS INT))
    FROM Khoang

    -- Nếu không có khoang nào, khởi tạo số khoang là 0
    IF @MaxKhoang IS NULL
    BEGIN
        SET @MaxKhoang = 0;
    END

    -- Tính số khoang tiếp theo
    SET @Suffix = @MaxKhoang + 1;

    -- Tạo mã khoang mới với số tiếp theo. Dùng `CAST` mà không cố định chiều dài
    SET @NewMaKhoang = @Prefix + CAST(@Suffix AS VARCHAR(10)); -- Tạo mã khoang như K1, K10, K100, v.v.

    -- Chèn khoang mới vào bảng Khoang
    INSERT INTO Khoang (MaKhoang, MaToa, SoKhoang, SoChoNgoiToiDa)
    SELECT @NewMaKhoang, MaToa, SoKhoang, SoChoNgoiToiDa
    FROM inserted;
END;


CREATE TRIGGER trg_KiemTraXoaKhoang
ON Khoang
INSTEAD OF DELETE
AS
BEGIN
    DECLARE @MaKhoang VARCHAR(10);
    DECLARE @MaToa VARCHAR(10);
    DECLARE @MaTau VARCHAR(10);
    DECLARE @HasFutureSchedule INT;
    DECLARE @HasTicket INT;

    -- Lấy thông tin từ bảng deleted
    SELECT @MaKhoang = MaKhoang, @MaToa = MaToa 
    FROM deleted;

    -- Lấy mã tàu từ bảng Toa tương ứng với mã toa bị xóa
    SELECT @MaTau = MaTau 
    FROM Toa
    WHERE MaToa = @MaToa;

    -- Kiểm tra xem khoang có liên quan đến lịch trình trong tương lai không
    SELECT @HasFutureSchedule = COUNT(*) 
    FROM NhatKyTau 
    JOIN LichTrinhTau ON NhatKyTau.MaLichTrinh = LichTrinhTau.MaLichTrinh
    WHERE NhatKyTau.MaTau = @MaTau
      AND NhatKyTau.NgayGio > GETDATE();

    -- Kiểm tra xem khoang có vé nào chưa được sử dụng không
    SELECT @HasTicket = COUNT(*) 
    FROM Ve 
    JOIN NhatKyTau ON Ve.MaNhatKy = NhatKyTau.MaNhatKy
    WHERE NhatKyTau.MaTau = @MaTau
      AND NhatKyTau.NgayGio > GETDATE()
      AND Ve.DaThuHoi = 0
      AND Ve.MaKhoang = @MaKhoang;

    -- Nếu khoang có lịch trình tương lai hoặc vé chưa sử dụng, không cho phép xóa
    IF @HasFutureSchedule > 0
    BEGIN
        IF @HasTicket > 0
        BEGIN
            RAISERROR('Không thể xóa khoang này vì có vé chưa sử dụng!', 16, 1);
            RETURN;
        END
    END

    -- Nếu không có ràng buộc nào, tiến hành xóa khoang
    DELETE FROM Khoang
    WHERE MaKhoang = @MaKhoang;
END;
-------------end Tau------------------------
------------ LichTrinh------------------------
CREATE FUNCTION SinhMaLichTrinh (@TienTo CHAR(2))
RETURNS VARCHAR(10)
AS
BEGIN
    DECLARE @MaxSo INT = 0;
    DECLARE @MaMoi VARCHAR(10);

    -- Lấy số thứ tự lớn nhất của tiền tố truyền vào
    SELECT @MaxSo = ISNULL(MAX(CAST(SUBSTRING(MaLichTrinh, 3, LEN(MaLichTrinh) - 2) AS INT)), 0)
    FROM LichTrinhTau
    WHERE LEFT(MaLichTrinh, 2) = @TienTo;

    -- Tăng số thứ tự lên 1 và tạo mã mới
    SET @MaMoi = @TienTo + RIGHT('00' + CAST(@MaxSo + 1 AS VARCHAR), 2);

    RETURN @MaMoi;
END;

SELECT dbo.SinhMaLichTrinh('SE')


CREATE PROCEDURE ThemLichTrinh
    @TienTo CHAR(2),
    @TenLichTrinh NVARCHAR(100)
AS
BEGIN
    DECLARE @MaLichTrinh VARCHAR(10);

    -- Gọi function để sinh mã mới
    SET @MaLichTrinh = dbo.SinhMaLichTrinh(@TienTo);

    -- Thực hiện chèn dữ liệu
    INSERT INTO LichTrinhTau (MaLichTrinh, TenLichTrinh)
    VALUES (@MaLichTrinh, @TenLichTrinh);
END;


DROP TRIGGER trg_CapNhatTrangThaiLichTrinh
CREATE TRIGGER trg_CapNhatTrangThaiLichTrinh
ON LichTrinhTau
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Kiểm tra nếu trạng thái được cập nhật là 'Đang hoạt động'
    IF EXISTS (
        SELECT 1
        FROM inserted i
        JOIN deleted d ON i.MaLichTrinh = d.MaLichTrinh
        WHERE d.TrangThai = N'Đang hoạt động' AND i.TrangThai <> d.TrangThai
    )
    BEGIN
        -- Kiểm tra nhật ký tàu của lịch trình
        IF EXISTS (
            SELECT 1
            FROM inserted i
            JOIN NhatKyTau nk ON i.MaLichTrinh = nk.MaLichTrinh
            WHERE nk.TrangThai = N'Chưa hoàn thành'
              AND nk.NgayGio > GETDATE()
        )
        BEGIN
            -- Gây lỗi và ngăn chặn thay đổi
            RAISERROR (N'Không thể cập nhật trạng thái "Đang hoạt động" khi có nhật ký trong tương lai đang hoạt động.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
    END
END;


CREATE FUNCTION dbo.KiemTraThoiGianDenCuaGa (
    @MaLichTrinh NVARCHAR(10),
    @ThoiGianKhoiHanh DATETIME,
    @MaGa VARCHAR(10),
    @ThoiGianDen DATETIME
)
RETURNS BIT
AS
BEGIN
    DECLARE @Result BIT = 0;
    
    -- Kiểm tra xem có tàu đến cùng ga trong khoảng thời gian nhỏ hơn 20 phút không
    IF EXISTS (
        SELECT 1
        FROM ChiTietLichTrinh ctlt 
        JOIN NhatKyTau nk ON nk.MaLichTrinh = ctlt.MaLichTrinh 
        WHERE CAST(nk.NgayGio AS DATE) = CAST(@ThoiGianKhoiHanh AS DATE) 
        AND nk.TrangThai = N'Chưa hoàn thành' 
        AND nk.MaNhatKy <> @MaLichTrinh
        AND ctlt.MaGa = @MaGa
        AND ctlt.MaLichTrinh <> @MaLichTrinh
        AND ABS(DATEDIFF(MINUTE, 
            dbo.TinhTongThoiGianDiChuyen(nk.MaNhatKy, nk.NgayGio, 
            (SELECT TOP 1 MaGa FROM ChiTietLichTrinh WHERE MaLichTrinh = ctlt.MaLichTrinh AND ctlt.Stt_Ga = 1), ctlt.MaGa),
            @ThoiGianDen
        )) < 20
    )
    BEGIN
        SET @Result = 1;  -- Trả về 1 nếu có tàu đến cùng ga trong khoảng thời gian < 20 phút
    END

    RETURN @Result;
END;

DROP TRIGGER trg_ThemSuaChiTietLichTrinh
CREATE TRIGGER trg_ThemChiTietLichTrinh
ON ChiTietLichTrinh
INSTEAD OF INSERT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @MaChiTiet VARCHAR(10);
    DECLARE @MaxStt INT;
    DECLARE @MaLichTrinh VARCHAR(10);
    DECLARE @MaGa VARCHAR(10);
    DECLARE @Stt_Ga INT;
    DECLARE @ThoiGian TIME;
    DECLARE @MaNhatKy VARCHAR(11);
    DECLARE @ThoiGianKhoiHanh DATETIME;
    DECLARE @ThoiGianDen DATETIME;
    DECLARE @MaGaDauTien VARCHAR(10);
    DECLARE @Stt_Ga_Truoc INT;
    DECLARE @MaGaTruoc VARCHAR(10);

    DECLARE @ThoiGianDenCuaTramDaTonTai DATETIME;

    -- Lấy thông tin từ bảng inserted
    SELECT
        @MaLichTrinh = i.MaLichTrinh,
        @Stt_Ga = i.Stt_Ga,
        @ThoiGian = i.ThoiGianDiChuyenTuTramTruoc,
        @MaNhatKy = nk.MaNhatKy,
        @MaGa = i.MaGa,
        @ThoiGianKhoiHanh = nk.NgayGio
    FROM inserted i 
    JOIN NhatKyTau nk ON i.MaLichTrinh = nk.MaLichTrinh
    WHERE nk.TrangThai = N'Chưa hoàn thành';

    -- Kiểm tra nếu có số thứ tự ga đã tồn tại cho mã lịch trình
    IF EXISTS (SELECT 1 FROM ChiTietLichTrinh WHERE MaLichTrinh = @MaLichTrinh AND Stt_Ga = @Stt_Ga)
    BEGIN
        -- Nếu có, trả lỗi
        RAISERROR('Số thứ tự Ga đã tồn tại cho mã lịch trình này!', 16, 1);
        RETURN;
    END

    -- Lấy thông tin ga trước đó nếu có
    SELECT @Stt_Ga_Truoc = ctlt.Stt_Ga, @MaGaTruoc = ctlt.MaGa
    FROM ChiTietLichTrinh ctlt 
    JOIN NhatKyTau nk ON ctlt.MaLichTrinh = nk.MaLichTrinh
    WHERE nk.MaNhatKy = @MaNhatKy AND ctlt.Stt_Ga = @Stt_Ga - 1;

    -- Lấy ga đầu tiên trong chuyến đi
    SELECT @MaGaDauTien = ctlt.MaGa 
    FROM NhatKyTau nk 
    JOIN ChiTietLichTrinh ctlt ON nk.MaLichTrinh = ctlt.MaLichTrinh
    WHERE nk.TrangThai = N'Chưa hoàn thành' AND ctlt.Stt_Ga = 1 AND MaNhatKy = @MaNhatKy;

    -- Tính thời gian đến của ga
    SELECT @ThoiGianDen = dbo.TinhTongThoiGianDiChuyen(@MaNhatKy, @ThoiGianKhoiHanh, @MaGaDauTien, @MaGaTruoc);

    IF @ThoiGianDen IS NULL
    BEGIN
        SET @ThoiGianDen = @ThoiGianKhoiHanh;
    END;

    -- Cập nhật thời gian đến
    SET @ThoiGianDen = DATEADD(MINUTE, DATEDIFF(MINUTE, '00:00:00', @ThoiGian), @ThoiGianDen)

    -- Kiểm tra thời gian đến cùng ga có nhỏ hơn 20 phút hay không bằng hàm
    IF dbo.KiemTraThoiGianDenCuaGa(@MaLichTrinh, @ThoiGianKhoiHanh, @MaGa, @ThoiGianDen) = 1
    BEGIN        
        RAISERROR('Lỗi: Tàu đến cùng ga trong khoảng thời gian nhỏ hơn 20 phút!', 16, 1);
        RETURN;
    END

    -- Xử lý tạo mã chi tiết lịch trình cho INSERT
    IF EXISTS (SELECT * FROM inserted)
    BEGIN
        -- Tạo mã chi tiết lịch trình mới khi INSERT
        SELECT @MaxStt = MAX(CAST(SUBSTRING(MaChiTiet, CHARINDEX('-', MaChiTiet) + 1, LEN(MaChiTiet)) AS INT))
        FROM ChiTietLichTrinh
        WHERE MaLichTrinh = (SELECT MaLichTrinh FROM inserted);

        IF @MaxStt IS NULL
        BEGIN
            SET @MaChiTiet = (SELECT MaLichTrinh FROM inserted) + '-1';
        END
        ELSE
        BEGIN
            SET @MaChiTiet = (SELECT MaLichTrinh FROM inserted) + '-' + CAST(@MaxStt + 1 AS VARCHAR);
        END

        -- Chèn dữ liệu vào bảng ChiTietLichTrinh
        INSERT INTO ChiTietLichTrinh (MaChiTiet, MaLichTrinh, MaGa, Stt_Ga, ThoiGianDiChuyenTuTramTruoc, KhoangCachTuTramTruoc)
        SELECT @MaChiTiet, MaLichTrinh, MaGa, Stt_Ga, ThoiGianDiChuyenTuTramTruoc, KhoangCachTuTramTruoc
        FROM inserted;
    END
    ELSE
    BEGIN
        -- Cập nhật dữ liệu nếu là thao tác UPDATE
        UPDATE ctlt
        SET 
            ctlt.MaGa = i.MaGa,
            ctlt.ThoiGianDiChuyenTuTramTruoc = i.ThoiGianDiChuyenTuTramTruoc,
            ctlt.KhoangCachTuTramTruoc = i.KhoangCachTuTramTruoc
        FROM ChiTietLichTrinh ctlt
        JOIN inserted i ON ctlt.MaChiTiet = i.MaChiTiet;
    END
END;

DROP TRIGGER trg_KiemTraThoiGianKhiThemNhatKy

CREATE TRIGGER trg_CheckTimeAndFutureDate
ON NhatKyTau
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    -- Kiểm tra nếu có bất kỳ bản ghi nào vi phạm thời gian cách nhau dưới 20 phút
    IF EXISTS (
        SELECT 1
        FROM NhatKyTau nk1
        INNER JOIN inserted i
        ON nk1.MaLichTrinh = i.MaLichTrinh
        WHERE nk1.MaNhatKy <> i.MaNhatKy -- Loại trừ bản ghi vừa được thêm
        AND ABS(DATEDIFF(MINUTE, nk1.NgayGio, i.NgayGio)) < 20
    )
    BEGIN
        -- Hủy thao tác chèn và đưa ra thông báo lỗi
        ROLLBACK TRANSACTION;
        RAISERROR ('Thời gian giữa các nhật ký của cùng một lịch trình phải cách nhau ít nhất 20 phút.', 16, 1);
        RETURN;
    END

    -- Kiểm tra nếu có bất kỳ bản ghi nào không lớn hơn ngày hiện tại ít nhất 1 ngày
    IF EXISTS (
        SELECT 1
        FROM inserted i
        WHERE i.NgayGio < DATEADD(DAY, 1, CAST(GETDATE() AS DATE)) -- Thời gian phải lớn hơn ngày hiện tại + 1
    )
    BEGIN
        -- Hủy thao tác chèn và đưa ra thông báo lỗi
        ROLLBACK TRANSACTION;
        RAISERROR ('Thời gian của nhật ký phải lớn hơn ngày hiện tại ít nhất 1 ngày.', 16, 1);
        RETURN;
    END
END;

DROP TRIGGER trg_KiemTraCapNhatNhatKy;
CREATE TRIGGER trg_KiemTraCapNhatNhatKy
ON NhatKyTau
AFTER UPDATE
AS
BEGIN
    DECLARE @MaNhatKy VARCHAR(11), @TrangThaiOld NVARCHAR(50), @TrangThaiNew NVARCHAR(50), @MaLichTrinh VARCHAR(15), @NgayGio DATETIME;
	DECLARE @MaGaDau VARCHAR(10), @MaGaCuoi VARCHAR(10);

    -- Lấy thông tin từ bảng inserted và deleted
    SELECT @MaNhatKy = inserted.MaNhatKy, 
           @TrangThaiOld = deleted.TrangThai,
           @TrangThaiNew = inserted.TrangThai,
           @MaLichTrinh = inserted.MaLichTrinh,
           @NgayGio = inserted.NgayGio
    FROM inserted
    JOIN deleted ON inserted.MaNhatKy = deleted.MaNhatKy;
	IF(@TrangThaiOld = N'Huỷ' OR @TrangThaiOld = N'Hủy')
	BEGIN
		PRINT @TrangThaiOld;
		RAISERROR(N'Không thể thay đổi trạng thái vì đã huỷ nhật ký.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
	END;

	SELECT 
		@MaGaDau = (SELECT TOP 1 MaGa 
					FROM ChiTietLichTrinh 
					WHERE MaLichTrinh = nk.MaLichTrinh 
					ORDER BY Stt_Ga ASC),
		@MaGaCuoi = (SELECT TOP 1 MaGa 
						FROM ChiTietLichTrinh 
						WHERE MaLichTrinh = nk.MaLichTrinh 
						ORDER BY Stt_Ga DESC) 
	FROM 
		NhatKyTau nk
	WHERE 
		nk.MaNhatKy = @MaNhatKy;

    -- Chỉ cho phép cập nhật thành "Hoàn thành" nếu thời gian hiện tại >= thời gian kết thúc của nhật ký
    IF (@TrangThaiNew = N'Hoàn thành')
    BEGIN
        DECLARE @ThoiGianKetThuc DATETIME;

        SELECT @ThoiGianKetThuc = dbo.TinhTongThoiGianDiChuyen(@MaNhatKy,@NgayGio, @MaGaDau, @MaGaCuoi);

        IF GETDATE() < @ThoiGianKetThuc
        BEGIN
            RAISERROR(N'Không thể cập nhật thành "Hoàn thành" vì chưa đến thời gian kết thúc nhật ký.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
    END

    -- Chỉ cho phép cập nhật thành "Hủy" nếu không có vé của lịch trình được bán
    IF (@TrangThaiNew = N'Hủy')
    BEGIN
        IF EXISTS (
            SELECT 1
            FROM Ve v JOIN NhatKyTau nk ON v.MaNhatKy = nk.MaNhatKy
            WHERE v.MaNhatKy = @MaNhatKy 
					AND v.DaThuHoi = 0	
					AND nk.NgayGio >= DATEADD(DAY, 1, GETDATE()) 
        )
        BEGIN
            RAISERROR(N'Không thể cập nhật thành "Hủy" vì đã có vé được bán cho lịch trình này.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
    END

    -- Không cho phép thay đổi trạng thái nếu có tàu chạy vào ngày mai
    IF EXISTS (
        SELECT 1
        FROM NhatKyTau nk
        WHERE nk.MaLichTrinh = @MaLichTrinh
          AND CAST(nk.NgayGio AS DATE) = CAST(DATEADD(DAY, 1, GETDATE()) AS DATE)
    )
    BEGIN
        RAISERROR(N'Không thể thay đổi trạng thái vì có tàu đang chạy vào ngày mai.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END;


UPDATE NhatKyTau SET TrangThai = N'Hoàn thành' WHERE MaNhatKy = 'TA111111241'

UPDATE NhatKyTau SET TrangThai = N'Chưa hoàn thành' WHERE MaNhatKy = 'TA41911241'

------------end LichTrinh------------------------
SELECT * FROM ChiTietLichTrinh WHERE MaLichTrinh = 'BC4'

SELECT * FROM NhatKyTau WHERE TrangThai = N'Hủy'

SELECT * FROM NhatKyTau WHERE MaNhatKy = 'TA111111241'