(function () {
  let userData = JSON.parse(localStorage.getItem("userData")) ?? [];
  for (const data of userData) {
    if (data.flag) {
      loginCheck(data.userName);
    }
  }
})();

function dateFun() {
  let date = new Date();
  document.getElementById("showCurrentTime").innerText = `${date.getDate()}-${
    date.getMonth() + 1
  }-${date.getFullYear()} [${
    date.getHours() > 12 ? date.getHours() - 12 : date.getHours()
  }:${date.getMinutes() < 10 ? "0" + date.getMinutes() : date.getMinutes()}:${
    date.getSeconds() < 10 ? "0" + date.getSeconds() : date.getSeconds()
  } ${date.getHours() < 12 ? "AM" : "PM"}]`;
}
setInterval(dateFun, 100);

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
            localStorage.setItem("userData", JSON.stringify(userData));
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

function checkPnrList() {
  let pnrData = JSON.parse(localStorage.getItem("pnr")) ?? [];
  return pnrData;
}

let getStatus = document.getElementById("getStatus");

getStatus.addEventListener("click", () => {
  let pnrInput = document.getElementById("pnrInput");
  checkPnrList(pnrInput.value);
});

function checkPnrList(value) {
  let pnrData = JSON.parse(localStorage.getItem("pnr")) ?? [];
  let pnrBody = document.getElementById("pnrBody");
  let html = "";
  for (const data of pnrData) {
    if (data.pnr == value) {
      html += `
            <td>${data.pnr}</td>
        <td>${data.status}</td>
        <td>${data.trainNo}</td>
        <td>${data.trainName}</td>
        <td>${data.from}</td>
        <td>${data.to}</td>
        <td>${data.dTime}</td>
        <td>${data.aTime}</td>
        <td>${data.date}</td>
        <td>${data.mobile}</td>
        <td>${data.user}</td>
        <td>
        <button class="btn btn-secondary" onclick=cancel('${data.pnr}')>Cancel Ticket</button>
        </td>
        `;
      break;
    }
  }

  pnrBody.innerHTML = html;
}

function cancel(pnrN) {
  let pnrData = checkPnrList();
  for (const data of pnrData) {
    if (data.status == pnrN) {
      data.status = "canceled";
      localStorage.setItem("pnr", JSON.stringify(pnrData));
    }
  }
}
let searchTrainBtn = document.getElementById("searchTrainBtn");
searchTrainBtn.addEventListener("submit", (e) => {
  getTrain(e.target.trainNumberInput.value);
  e.target.reset();
  e.preventDefault();
});

function getTrain(trainNumbers) {
  const url = `https://api.railwayapi.site/api/v1/trains/${trainNumbers}`;

  fetch(url)
    .then((response) => {
      if (response.status == 200) {
        return response.json();
      }
      if (response.status == 404) {
        alert("data not found");
        return;
      }
    })
    .then((result) => {
      showTrain(result.data);
    })
    .catch((error) => {
      console.error(error);
    });
}

function showTrain(data) {
  let searchTrainShow = document.getElementById("searchTrainShow");
  let html = "";
  data.forEach((elem, ind) => {
    html = `
        <th>${ind + 1}</th>
            <td>${elem.trainFullName}</td>
            <td>${elem.trainNumber}</td>
            <td>${elem.stationFrom.stationName}</td>
            <td>${elem.stationTo.stationName}</td>
            <td>${elem.departureTime}</td>
            <td>${elem.arrivalTime}</td>
        `;
  });
  searchTrainShow.innerHTML = html;
}
