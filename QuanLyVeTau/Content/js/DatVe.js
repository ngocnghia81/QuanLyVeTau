// Lưu trữ giỏ vé trong localStorage
function saveToCart(maKhoang, seatNumber, price) {
    const cart = JSON.parse(localStorage.getItem('cart')) || [];  // Lấy giỏ vé từ localStorage
    cart.push({ maKhoang, seatNumber, price });
    localStorage.setItem('cart', JSON.stringify(cart));  // Lưu lại giỏ vé vào localStorage
}

function removeFromCart(maKhoang, seatNumber) {
    let cart = JSON.parse(localStorage.getItem('cart')) || []; // Lấy giỏ hàng từ localStorage

    // Tìm và loại bỏ ghế khỏi giỏ
    cart = cart.filter(seat => !(seat.maKhoang === maKhoang && seat.seatNumber === seatNumber));

    // Lưu giỏ hàng đã được cập nhật vào localStorage
    localStorage.setItem('cart', JSON.stringify(cart));
}

// Xóa giỏ vé
function clearCart() {
    localStorage.removeItem('cart');
}

let Total = 0;
document.addEventListener("DOMContentLoaded", function () {
    const trains = document.querySelectorAll(".train__item");
    Total = 0;
    trains.forEach(item => {
        item.addEventListener("click", function () {
            /*  Xóa class "selected" khỏi tất cả các train__item-item*/
            trains.forEach(el => el.classList.remove("selected"));

            //Thêm class "selected" cho mục được click
            item.classList.add("selected");
            clearContent();
            clearCart();
        });
    });
});

function clearContent() {
    document.getElementById('trainDetailsContainer').innerHTML = '';
    document.getElementById('khoangDetailsContainer').innerHTML = '';
}

function loadTrainDetails(maTau) {
    // Tạo một đối tượng XMLHttpRequest để thực hiện yêu cầu AJAX
    var xhr = new XMLHttpRequest();

    // Xác định phương thức và URL yêu cầu
    xhr.open('GET', '/Ve/HienThiToa?maTau=' + maTau, true);

    // Đặt hàm xử lý khi nhận được phản hồi
    xhr.onload = function () {
        if (xhr.status >= 200 && xhr.status < 300) {
            // Nếu yêu cầu thành công, cập nhật nội dung vào trainDetailsContainer
            document.getElementById('trainDetailsContainer').innerHTML = xhr.responseText;
        } else {
            // Nếu có lỗi, bạn có thể xử lý hoặc thông báo lỗi
            console.error('Có lỗi xảy ra: ' + xhr.statusText);
        }
    };

    // Gửi yêu cầu
    xhr.send();
}
function loadToaDetails(train,maToa) {
    const trains = document.querySelectorAll(".train__toa");

    // Xóa class "selected" khỏi tất cả các train__toa
    trains.forEach(el => el.classList.remove("selected"));

    // Thêm class "selected" cho phần tử được click
    train.classList.add("selected");

    // Tạo một đối tượng XMLHttpRequest để thực hiện yêu cầu AJAX
    var xhr = new XMLHttpRequest();

    // Xác định phương thức và URL yêu cầu
    xhr.open('GET', '/Ve/HienThiKhoang?maToa=' + maToa, true);

    // Đặt hàm xử lý khi nhận được phản hồi
    xhr.onload = function () {
        if (xhr.status >= 200 && xhr.status < 300) {
            // Nếu yêu cầu thành công, cập nhật nội dung vào khoangDetailsContainer
            document.getElementById('khoangDetailsContainer').innerHTML = xhr.responseText;
            loadSelectedSeats();
        } else {
            // Nếu có lỗi, bạn có thể xử lý hoặc thông báo lỗi
            console.error('Có lỗi xảy ra: ' + xhr.statusText);
        }
    }

    // Gửi yêu cầu
    xhr.send();
}

// Lưu trữ ghế đã chọn
var selectedSeats = [];

// Hàm xử lý chọn ghế
// Hàm xử lý khi nhấp vào ghế
function selectSeat(element,makhoang,stt,giave) {
    // Kiểm tra xem ghế đã bị "reserved" chưa
    if (element.classList.contains("reserved")) {
        return;  // Dừng hàm nếu ghế đã bị "reserved"
    }
    // Kiểm tra xem ghế đã chọn chưa
    if (element.classList.contains("selected")) {
        // Nếu đã chọn, bỏ chọn
        element.classList.remove("selected");
        removeFromCart(makhoang, stt, giave);
        updateTotal(-giave);
    } else {
        // Nếu chưa chọn, thêm class 'selected'
        element.classList.add("selected");
        saveToCart(makhoang, stt, giave);
        updateTotal(giave);       
    }
    
}

function loadSelectedSeats() {
    const selectedSeats = JSON.parse(localStorage.getItem('cart')) || [];

    // Lặp qua tất cả các ghế và kiểm tra nếu ghế đã được chọn
    const seats = document.querySelectorAll('.seat');

    seats.forEach(seat => {
        const maKhoang = seat.getAttribute('maKhoang'); // Sử dụng data- attributes
        const seatNumber = seat.getAttribute('stt');     // Sử dụng data- attributes
        selectedSeats.forEach(selectedSeat => {
            if (selectedSeat.maKhoang == maKhoang && selectedSeat.seatNumber == seatNumber) {
                console.log("Ghế đã được chọn:", seat);
                // Ở đây, bạn có thể thêm bất kỳ hành động nào, ví dụ thay đổi màu ghế hoặc thêm lớp CSS
                seat.classList.add('selected');
            }
        });
    });
}


function showPrice(element, price) {

    tooltip = document.createElement('div');
    tooltip.className = 'seat__tooltip';

    if (element.classList.contains('reserved')) {
        tooltip.textContent = "Đã bán";
    }
    else
        tooltip.textContent = `Giá: ${price.toLocaleString()}đ`; // Thay đổi nội dung với giá động
    tooltip.style.display = 'block';

    element.appendChild(tooltip);


}

function hidePrice(element) {
    const tooltip = element.querySelector('.seat__tooltip');

    if (tooltip) {
        tooltip.remove();
    }

}

function updateTotal(amount) {
    Total += amount;
    total = document.getElementById("totalAmount");
    total.textContent = Total.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",") + " VND";
    }
