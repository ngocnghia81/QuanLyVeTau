let AllTRAINS;

(function () {
    let userData = JSON.parse(localStorage.getItem("userData")) ?? [];
    for (const data of userData) {
        if (data.flag) {
            loginCheck(data.userName);
        }
    }
})();

function getAllUserData() {
    let userData = JSON.parse(localStorage.getItem("userData")) ?? [];
    return userData;
}
}

function createPData() {
    let passengerList = JSON.parse(localStorage.getItem("passengerList")) ?? [];
    return passengerList;
}

function dateFun() {
    let date = new Date();
    document.getElementById("showCurrentTime").innerText = `${date.getDate().toString().padStart(2, '0')}/${(date.getMonth() + 1).toString().padStart(2, '0')
        }/${date.getFullYear()} [${date.getHours().toString().padStart(2, '0')
        }:${date.getMinutes().toString().padStart(2, '0')}:${date.getSeconds().toString().padStart(2, '0')
        }]`;
}


setInterval(dateFun, 100);

function checkFlag() {
    let userData = JSON.parse(localStorage.getItem("userData")) ?? null;
    if (userData == null) {
        alert("You have don't have any data please register your accout");
    } else {
        for (const data of userData) {
            console.log(data.flag);
            if (data.flag) {
                // console.log(data);
                return true;
                break;
            }
            return false;
        }
    }
}

function loginCheck(user) {
    let showUserName = document.getElementById("showUserName");
    let pLogSwitch = document.getElementById("pLogSwitch");
    let pLogSwitch2 = document.getElementById("pLogSwitch2");
    let welcomeNote = document.getElementById("welcomeNote");
    // console.log(welcomeNote);
    if (checkFlag()) {
        welcomeNote.textContent = "Welcome";
        showUserName.textContent = user;
        pLogSwitch.classList.remove("pLogActive");
        pLogSwitch2.classList.add("pLogActive");
        showUserName.textContent = `${user}`;
        pLogSwitch.textContent = "Log Out";
        pLogSwitch.style.color = "red";
        pLogSwitch2.addEventListener("click", () => {
            let conform = confirm("Are you sure!");
            if (conform) {
                let userData = getAllUserData();
                for (const data of userData) {
                    if (data.userName == user) {
                        console.log(userData);
                        data.flag = false;
                        console.log(userData);
                        localStorage.setItem(
                            "userData",
                            JSON.stringify(userData)
                        );
                        window.location.reload();
                        break;
                    }
                }
            }
        });
    } else {
        welcomeNote.textContent = "";
        pLogSwitch.classList.add("pLogActive");
        pLogSwitch2.classList.remove("pLogActive");
    }
}

function pEmail() {
    let userData = getAllUserData();
    let pEmail = document.getElementById("pEmail");
    let showUserName = document.getElementById("showUserName");
    for (const data of userData) {
        if (data.userName == showUserName.textContent) {
            pEmail.innerText = data.email;
            return;
        }
    }
}

let plog = document.getElementById("pLog");

plog.addEventListener("submit", (ev) => {
    let user = ev.target.pUsername;
    let pass = ev.target.pPassword;
    let userData = JSON.parse(localStorage.getItem("userData")) ?? null;

    if (userData == null) {
        alert("First you create account");
    } else {
        for (const data of userData) {
            if (data.userName == user.value) {
                console.log("user name match");
                if (data.password == pass.value) {
                    console.log("Password Match");
                    data.flag = true;
                    localStorage.setItem("userData", JSON.stringify(userData));
                    loginCheck(data.userName);
                    break;
                }
            }
        }
        if (!checkFlag()) {
            alert("UserName and Password not Found");
        }
    }
    ev.target.reset();
    document.getElementById("closeModal").click();
    ev.preventDefault();
});

function secondInputField(from, to, date) {
    let mForm = document.getElementById("mFrom");
    let mTo = document.getElementById("mTo");
    let mDate = document.getElementById("mDate");
    mForm.value = from;
    mTo.value = to;
    mDate.value = date;
}

document.getElementById("interchangeBtn").addEventListener("click", () => {
    let from = document.getElementById("from");
    let to = document.getElementById("to");
    let temp;
    temp = from.value;
    from.value = to.value;
    to.value = temp;
});

let allInput = document.querySelectorAll(".search");
allInput.forEach((elm, ind) => {
    elm.addEventListener("keyup", (e) => {
        let key = e.target.value;
        let input = e.target;
        key = key.toUpperCase();
        getSearch(key, input, ind);
    });
});

function getSearch(val, input, ind) {
    let url = `https://api.railwayapi.site/api/v1/stations?q=${val}`;
    fetch(url)
        .then((respone) => {
            return respone.json();
        })
        .then((result) => {
            showSearch(result.data, input, ind);
        })
        .catch((er) => {
            console.log(er);
        });
}

function showSearch(data, input, ind) {
    input.nextElementSibling.classList.add("active");
    let html = "";
    data.forEach((el) => {
        if (el.stationName == null) {
            return;
        } else {
            html += `<li class="list" onclick='inputStation("${el.stationCode}", "${ind}")'>${el.stationName}</li>`;
        }
    });
    input.nextElementSibling.innerHTML = html;
}
function inputStation(val, ind) {
    let inp = document.getElementsByClassName("search")[ind];
    inp.value = val;
    inp.nextElementSibling.classList.remove("active");
}

const trainSearchBtw1 = document.getElementById("trainSearchBtw1");
const trainSearchBtw2 = document.getElementById("trainSearchBtw2");
trainSearchBtw1.addEventListener("submit", (e) => {
    let contents = document.getElementsByClassName("content");
    contents[0].classList.remove("active");
    contents[1].classList.add("active");
    let from = e.target.from;
    let to = e.target.to;
    let date = e.target.date;
    from = from.value.toUpperCase();
    to = to.value.toUpperCase();
    date = date.value;
    getTrainList(from, to, date);
    secondInputField(from, to, date);

    e.target.reset();
    e.preventDefault();
});

trainSearchBtw2.addEventListener("submit", (ev) => {
    let mfrom = ev.target.mFrom;
    let mto = ev.target.mTo;
    let date = ev.target.mDate;

    mfrom = mfrom.value.toUpperCase();
    mto = mto.value.toUpperCase();
    getTrainList(mfrom, mto, date);

    ev.preventDefault();
});

function getTrainList(from, to, date) {
    const url = `https://api.railwayapi.site/api/v1/trainsBtwStations?fromStation=${from}&toStation=${to}&allTrains=true`;
    fetch(url)
        .then((response) => {
            if (response.status == 200) {
                return response.json();
            } else {
                alert("Sorry! No Direct Train....Search Another City");
                document.getElementsByTagName("tbody")[0].innerHTML =
                    "Please enter a valid details";
            }
        })
        .then((result) => {
            showTrain(result.data, date);
        })
        .catch((error) => {
            console.error(error);
        });
}

function showTrain(data, date) {
    let html = " ";

    data.forEach((elem) => {
        let trainName = elem.trainFullName;
        let trainNumber = elem.trainNumber;
        let stationFrom = elem.stationFrom.stationName;
        let stationTo = elem.stationTo.stationName;
        let departure = elem.stationFrom.departureTime;
        let arrival = elem.stationTo.arrivalTime;
        let duration = elem.duration;
        html += `
        <tr>
        <td style='font-weight: 700'>
        <img src="Icon/free-train-icon-1045-thumb.png" alt="train logo" class="img-fluid" style="height: 40px;">
        ${trainName}(${trainNumber})
        <h6 class="stationName text-primary" id="stationName">${stationFrom} <i class="bi bi-arrow-right-circle-fill"></i> ${stationTo}</h6>
        <h6 class="departs" id="departs">Departs on: All Day</h6>
        </td>
        <td class="text-center">
        <img src="Icon/sun.png" alt="" class="img-fluid mb-3" style="height: 60px;">
        <h5 id="departsTime">${departure}</h5>
        </td>
        <td class="text-center">
        <img src="Icon/rain.png" alt="" class="img-fluid mb-3" style="height: 60px;">
        <h5 id="arrivalTime">${arrival}</h5>
        </td>
        <td class="text-center">
        <img src="Icon/clock.png" alt="" class="img-fluid mb-3" style="height: 60px;">
        <h5 id="duration">${duration}</h5>
        </td>
        <td class="text-center" style="vertical-align: middle;">
        <button class="btn btn-primary" onclick='getTrain("${trainName}","${trainNumber}","${stationFrom}","${stationTo}","${departure}","${arrival}","${duration}","${date}")'>
        Book Now</button>
        </td>
        </tr>`;
    });

    document.getElementsByTagName("tbody")[0].innerHTML = html;
}

function pnrGenerator() {
    const random10DigitNumber =
        Math.floor(Math.random() * (9999999999 - 1000000000 + 1)) + 1000000000;
    return random10DigitNumber;
}

const randomNumber = pnrGenerator();

function getTrain(...values) {
    if (checkFlag()) {
        AllTRAINS = values;
        let contents = document.getElementsByClassName("content");
        contents[1].classList.remove("active");
        contents[2].classList.add("active");
        currentTrain(false, values);
    } else {
        alert("First you will be login to your account");
    }
}

function currentTrain(check, value) {
    if (check) {
        let preViewData = document.getElementById("preViewData");
        preViewData.innerHTML = `
        <div class="col-5">
        <h6 class="text-body-secondary">From Station</h6>
        <h4 class="fw-bold">${value[2]}</h4>
        <p class="text-body-secondary">
          Departure: ${value[7]} | ${value[4]}
        </p>
        </div>
        <div class="col-5">
        <h6 class="text-body-secondary">To Station</h6>
        <h4 class="fw-bold">${value[3]}</h4>
        <p class="text-body-secondary">
          Arrival: ${value[7]} | ${value[5]}
        </p>
        </div>
    `;
    }
}

let addPassenger = document.getElementById("addPassenger");
let index = 1;

addPassenger.addEventListener("click", () => {
    addForm(index++);
});

function addForm(ind) {
    let addFormBody = document.getElementById("addForm");
    pEmail();
    let html = `                       
     <div class="card pCard my-2 id="${ind}">
    <div class="card-body">
        <h5 class="mb-3">Person ${ind}</h5>
        <div class="row pForm" id=pForm${ind}>
            <div class="col">
                <input type="text" class="form-control" placeholder="Name" aria-label="Name" />
            </div>
            <div class="col">
                <input type="number" class="form-control" placeholder="Age" aria-label="Age" />
            </div>
            <div class="col">
                <select name="" id="pGender" class="form-select">
                    <option disabled selected hidden>Gender</option>
                    <option>Male</option>
                    <option>Female</option>
                    <option>Transgender</option>
                </select>
            </div>
        </div>
    </div>
    </div>`;

    addFormBody.insertAdjacentHTML("beforeend", html);
}

addForm(index++);

let savePassengers = document.getElementById("continue");

savePassengers.addEventListener("click", () => {
    let passengerList = createPData();
    let card = document.querySelectorAll(".pCard");

    card.forEach((elem, ind) => {
        let allTag = elem.querySelectorAll(".pCard input");
        let selectTag = elem.querySelector(".pCard select");
        let mNumber = document.getElementById("mNumber");
        passengerList.push({
            pnr: randomNumber,
            name: allTag[ind].value,
            age: allTag[ind + 1].value,
            mobile: mNumber.value,
            gender: selectTag.value,
        });
    });
    localStorage.setItem("passengerList", JSON.stringify(passengerList));

    currentTrain(true, AllTRAINS);

    passengerList = createPData();
    for (const data of passengerList) {
        let previewCardBody =
            document.getElementsByClassName("preview-card-body")[0];
        let html = "";
        html = `<h4 id="p1" class="py-3 pl-2 border-bottom">${data.name} <span class="fs-5">${data.age} | ${data.gender}</span></h4>`;
        previewCardBody.innerHTML += html;
    }
});

let confirmBook = document.getElementById("confirmBook");

confirmBook.addEventListener("submit", (e) => {
    e.preventDefault();

    let pnrData = JSON.parse(localStorage.getItem("pnr")) ?? [];
    let createPData = JSON.parse(localStorage.getItem("passengerList")) ?? [];
    let user = document.getElementById("showUserName");
    console.log(user);
    pnrData.push({
        pnr: createPData[0].pnr,
        status: "booked",
        trainNo: AllTRAINS[1],
        trainName: AllTRAINS[0],
        from: AllTRAINS[2],
        to: AllTRAINS[3],
        dTime: AllTRAINS[4],
        aTime: AllTRAINS[5],
        date: AllTRAINS[7],
        mobile: createPData[0].mobile,
        user: user.innerHTML,
    });
    localStorage.setItem("pnr", JSON.stringify(pnrData));
    // alert(`Your pnr no:-${createPData[0].pnr}`);
    let html = `<div class="alert alert-success alert-dismissible fade show" role="alert">
  <h4 class="alert-heading">PNR No:- ${createPData[0].pnr}</h4>
  <p>Congratulation, you successfully booked your ticket</p>
  <hr>
  <p class="mb-0 text-danger">Please rember your PNR No to Cancel and Find the ticket</p>
  <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close" onclick='reload()'></button>
</div>`;
    document.querySelector("body").innerHTML = html;
    localStorage.removeItem("passengerList");
    AllTRAINS = "";
});

function reload() {
    window.location.reload();
}

// Admi login validation check
let userName = "admin";
let passwed = "admin";

let aLog = document.getElementById("aLog");
aLog.addEventListener("submit", (e) => {
    let user = e.target.aUsername;
    let pass = e.target.aPassword;
    user = user.value;
    pass = pass.value;

    if (user == userName) {
        if (pass == passwed) {
            window.location.href = "admin.html";
        }
    }
    document.getElementById("closeModal1").click();

    e.preventDefault();
});

let slider = document.getElementsByClassName("slide");
let nextBtn = document.getElementsByClassName("nextBtn");
let bullet = document.getElementsByClassName("step");

Array.from(nextBtn).forEach((elem, index) => {
    // console.log(element);
    elem.addEventListener("click", () => {
        slider[index].style.marginLeft = `-25%`;
        bullet[index].classList.add("active");
        bullet[index].nextElementSibling.style.color = "var(--bs-blue)";
    });
});
