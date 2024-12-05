let Total = 0;
let MaNK = '';
let MaNKReturned = '';
let MaKM = '';


function saveTau(index,MaNK) {
    const tau = JSON.parse(localStorage.getItem('Tau')) || [];
    tau[index] = MaNK;
    localStorage.setItem('Tau', JSON.stringify(tau));
    console.log(tau);
}
function saveTimVe(from, to, ngaydi, ngayve) {
        const timVe = JSON.parse(localStorage.getItem('TimVe')) || [];
        timVe.push({ from, to, ngaydi, ngayve });
        localStorage.setItem('TimVe', JSON.stringify(timVe));  // Lưu lại giỏ vé vào localStorage
}

function saveToCart(MaKhoang, Stt, Gia,Oneway) {
    const cart = JSON.parse(localStorage.getItem('cart')) || [];  // Lấy giỏ vé từ localStorage
    cart.push({ MaKhoang, Stt, Gia,Oneway });
    localStorage.setItem('cart', JSON.stringify(cart));  // Lưu lại giỏ vé vào localStorage
}

function removeFromCart(maKhoang, seatNumber, Oneway) {
    let cart = JSON.parse(localStorage.getItem('cart')) || []; // Lấy giỏ hàng từ localStorage

    // Tìm và loại bỏ ghế khỏi giỏ
    cart = cart.filter(seat => !(seat.maKhoang === maKhoang && seat.seatNumber === seatNumber && seat.oneway === Oneway));

    // Lưu giỏ hàng đã được cập nhật vào localStorage
    localStorage.setItem('cart', JSON.stringify(cart));
}

// Xóa giỏ vé
function clearCart() {
    localStorage.removeItem('cart');
    Total = 0;
    updateTotal(0); 
}

document.addEventListener("DOMContentLoaded", function () {
    clearCart();
    
    const trains = document.querySelectorAll(".oneway .train__item");

    const trainsReturned = document.querySelectorAll(".returned .train__item");
    Total = 0;
    trains.forEach(item => {
        item.addEventListener("click", function () {
            /*  Xóa class "selected" khỏi tất cả các train__item-item*/
            trains.forEach(el => el.classList.remove("selected"));

            //Thêm class "selected" cho mục được click
            item.classList.add("selected");
            MaNK = item.getAttribute("data-MaNK");
            saveTau(0, MaNK);
            
        });
    });
    trainsReturned.forEach(item => {
        item.addEventListener("click", function () {
            /*  Xóa class "selected" khỏi tất cả các train__item-item*/
            trainsReturned.forEach(el => el.classList.remove("selected"));

            //Thêm class "selected" cho mục được click
            item.classList.add("selected");
            MaNKReturned = item.getAttribute("data-MaNK");
            saveTau(1, MaNKReturned);
        });
    });
});

function clearContent(container) {
    container.querySelector('.trainDetailsContainer').innerHTML = '';

    container.querySelector('.toaDetailsContainer').innerHTML = '';
}

function loadTrainDetails(element, maTau) {
   
    // Tạo một đối tượng XMLHttpRequest để thực hiện yêu cầu AJAX
    var xhr = new XMLHttpRequest();
    var container = element.closest('.container');
    var trainDetail = container.querySelector('.trainDetailsContainer');
    
    clearContent(container);
    // Xác định phương thức và URL yêu cầu
    xhr.open('GET', '/Ve/HienThiToa?maTau=' + maTau, true);

    // Đặt hàm xử lý khi nhận được phản hồi
    xhr.onload = function () {
        if (xhr.status >= 200 && xhr.status < 300) {
            // Nếu yêu cầu thành công, cập nhật nội dung vào trainDetailsContainer
            trainDetail.innerHTML = xhr.responseText;
        } else {
            // Nếu có lỗi, bạn có thể xử lý hoặc thông báo lỗi
            console.error('Có lỗi xảy ra: ' + xhr.statusText);
        }
    };

    // Gửi yêu cầu
    xhr.send();
}
function loadToaDetails(train, maToa) {
    const trains = document.querySelectorAll(".train__toa");
    var container = train.closest('.container');
    var toaDetail = container.querySelector('.toaDetailsContainer');
    // Xóa class "selected" khỏi tất cả các train__toa
    trains.forEach(el => el.classList.remove("selected"));
    // Thêm class "selected" cho phần tử được click
    train.classList.add("selected");

    var index = container.classList.contains('oneway') ? 0 : 1;
    var maNK = JSON.parse(localStorage.getItem("Tau"))[index];
    if (index == 0) {
        var from = JSON.parse(localStorage.getItem("TimVe"))[0].from;;
        var to = JSON.parse(localStorage.getItem("TimVe"))[0].to;
    }
    else {
        var from = JSON.parse(localStorage.getItem("TimVe"))[0].to;;
        var to = JSON.parse(localStorage.getItem("TimVe"))[0].from;
    }
    

    var urlData =  "&from="+from+"&to="+to+"&maNK="+maNK+"&oneway="+index;
    console.log(urlData);
    // Tạo một đối tượng XMLHttpRequest để thực hiện yêu cầu AJAX
    var xhr = new XMLHttpRequest();
   
    // Xác định phương thức và URL yêu cầu
    xhr.open('GET', '/Ve/HienThiKhoang?maToa=' + maToa + urlData, true);

    // Đặt hàm xử lý khi nhận được phản hồi
    xhr.onload = function () {
        if (xhr.status >= 200 && xhr.status < 300) {
            // Nếu yêu cầu thành công, cập nhật nội dung vào khoangDetailsContainer
            toaDetail.innerHTML = xhr.responseText;
            loadSelectedSeats(container);
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
function selectSeat(element, makhoang, stt, giave) {
    var oneway = element.closest('.oneway') != null;

    if (element.classList.contains("reserved")) {
        return; 
    }
    // Kiểm tra xem ghế đã chọn chưa
    if (element.classList.contains("selected")) {
        // Nếu đã chọn, bỏ chọn
        
        element.classList.remove("selected");
        removeFromCart(makhoang, stt, giave,oneway);
        updateTotal(-giave);
        if (oneway)
            updateSL(-1);
        else
            updateSLReturned(-1)
    } else {
        // Nếu chưa chọn, thêm class 'selected'
        element.classList.add("selected");
        saveToCart(makhoang, stt, giave,oneway);
        updateTotal(giave);       
        if (oneway)
            updateSL(1);
        else
            updateSLReturned(1)
    }
    
}

function loadSelectedSeats(container) {
    const selectedSeats = JSON.parse(localStorage.getItem('cart')) || [];
    var Oneway = container.classList.contains('oneway');
    // Lặp qua tất cả các ghế và kiểm tra nếu ghế đã được chọn
    const seats = container.querySelectorAll('.seat');
    seats.forEach(seat => {
        const maKhoang = seat.getAttribute('maKhoang'); // Sử dụng data- attributes
        const seatNumber = seat.getAttribute('stt');     // Sử dụng data- attributes
        selectedSeats.forEach(selectedSeat => {
            if (selectedSeat.MaKhoang == maKhoang && selectedSeat.Stt == seatNumber    ) {
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
    total.textContent = (Total-ApMaKhuyenMai()).toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",") + " VND";
};

function updateSL(value) {
    const slElement = document.getElementById("SL");
    let currentValue = parseInt(slElement.innerText);
    slElement.innerText = currentValue + value;
}

function updateSLReturned(value) {
    const slReturnedElement = document.getElementById("SLReturned");
    let currentValue = parseInt(slReturnedElement.innerText);
    slReturnedElement.innerText = currentValue + value;
}

function ApMaKhuyenMai() {
    var selectedOption = document.getElementById('comboBox').options[document.getElementById('comboBox').selectedIndex];
    MaKM = selectedOption.value;
    var optionValue = JSON.parse(selectedOption.getAttribute('data-value'));  // Parse giá trị JSON từ value của option
    var optionMaxValue = parseInt(selectedOption.getAttribute('data-max')); 

    var amount = Total * optionValue / 100; 

    return  Math.min(amount, optionMaxValue);; 
}
document.getElementById('comboBox').addEventListener('change' ,function () {
    updateTotal(0);
});  // Gọi hàm khi thay đổi lựa chọn
