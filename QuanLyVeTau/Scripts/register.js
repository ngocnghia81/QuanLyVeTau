const form = document.querySelector("#registerUser");
const allInputTag = document.querySelectorAll("#register .form-control");
const allSelectTag = document.querySelectorAll("#register .form-select");
const allCheckTag = document.querySelectorAll("#register .form-check-input");

form.addEventListener("submit", (ev) => {
  let check = [];
  let checkDuplicate = false;
  check = [
    ...onlyInputValidation(allInputTag),
    ...onlyInputValidation(allSelectTag),
  ];

  check.forEach((elem) => {
    console.log(elem);
  });

  if (
    check[0] &&
    check[1] &&
    check[2] &&
    check[3] &&
    (check[4] || check[5] || check[6]) &&
    check[7] &&
    check[8] &&
    check[9] &&
    check[10] &&
    check[11] &&
    check[12] &&
    check[13] &&
    check[14]
  ) {
    console.log("im run");
    let userData = JSON.parse(localStorage.getItem("userData")) ?? [];
    for (const data of userData) {
      if (data.userName == allInputTag[0].value) {
        checkDuplicate = true;
        break;
      }
    }
    if (checkDuplicate) {
      alert("UserName already Exists");
    } else {
      userData.push({
        flag: false,
        userName: allInputTag[0].value,
        password: allInputTag[1].value,
        security: {
          question: allSelectTag[0].value,
          answer: allInputTag[3].value,
        },
        name: {
          fName: allInputTag[4].value,
          mName: allInputTag[5].value,
          lName: allInputTag[6].value,
        },
        gender: allCheckTag.value,
        dob: allInputTag[7].value,
        email: allInputTag[8].value,
        number: allInputTag[9].value,
        address: allInputTag[10].value,
        pincode: allInputTag[11].value,
        city: allSelectTag[1].value,
        state: allSelectTag[2].value,
      });

      localStorage.setItem("userData", JSON.stringify(userData));

      ev.target.reset();
    }
  }

  console.log("im run");

  ev.preventDefault();
});

form.addEventListener("reset", (e) => {
  allInputTag.forEach((elem) => {
    elem.nextElementSibling.style.display = "none";
  });
  allSelectTag.forEach((elem) => {
    elem.nextElementSibling.style.display = "none";
  });
  e.target.reset();
});

allInputTag.forEach((elem) => {
  elem.addEventListener("keyup", () => {
    onlyInputValidation2(elem);
  });
});

function onlyInputValidation(input) {
  return Array.from(input).map((elem) => {
    if (elem.value == "") {
      elem.nextElementSibling.style.display = "block";
      return false;
    }
    if (elem.id == "inputPassword") {
      const regex =
        /^(?=.*[a-zA-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$/;
      if (regex.test(elem.value)) {
        elem.nextElementSibling.style.display = "none";
        return true;
      } else {
        elem.nextElementSibling.style.display = "block";
        return false;
      }
    }
    if (elem.id == "cnfPassword") {
      if (elem.value == allInputTag[1].value) {
        elem.nextElementSibling.style.display = "none";
        return true;
      } else {
        elem.nextElementSibling.style.display = "block";
        return false;
      }
    }
    if (elem.id == "inputEmail") {
      const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
      if (emailRegex.test(elem.value)) {
        elem.nextElementSibling.style.display = "none";
        return true;
      } else {
        elem.nextElementSibling.style.display = "block";
        return false;
      }
    } else {
      elem.nextElementSibling.style.display = "none";
      return true;
    }
  });
}

function onlyInputValidation2(elem) {
  if (elem.value == "") {
    elem.nextElementSibling.style.display = "block";
    return false;
  }
  if (elem.id == "inputPassword") {
    const regex =
      /^(?=.*[a-zA-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$/;
    if (regex.test(elem.value)) {
      elem.nextElementSibling.style.display = "none";
      return true;
    } else {
      elem.nextElementSibling.style.display = "block";
      return false;
    }
  }
  if (elem.id == "cnfPassword") {
    if (elem.value == allInputTag[1].value) {
      elem.nextElementSibling.style.display = "none";
      return true;
    } else {
      elem.nextElementSibling.style.display = "block";
      return false;
    }
  }
  if (elem.id == "inputEmail") {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (emailRegex.test(elem.value)) {
      elem.nextElementSibling.style.display = "none";
      return true;
    } else {
      elem.nextElementSibling.style.display = "block";
      return false;
    }
  } else {
    elem.nextElementSibling.style.display = "none";
    return true;
  }
}

function dateFun() {
  let date = new Date();
  document.getElementById(
    "showCurrentTime"
  ).innerText = `${date.getDate()}-${date.getMonth()+1}-${date.getFullYear()} [${date.getHours() > 12 ? date.getHours() - 12 : date.getHours()}:${date.getMinutes() < 10 ? '0'+date.getMinutes() : date.getMinutes()}:${date.getSeconds() < 10 ? '0'+date.getSeconds() : date.getSeconds()} ${date.getHours() < 12? 'AM':'PM'}]`;
}

setInterval(dateFun, 100);

// Admi login validation check
let userName = "admin";
let passwed = "admin";

let aLog = document.getElementById("aLog");
aLog.addEventListener("submit", (e) => {
  let user = e.target.aUsername;
  let pass = e.target.aPassword;
  console.log(user);
  console.log(pass);
  user = user.value;
  pass = pass.value;
  if (user == userName) {
    if (pass == passwed) {
      console.log("im run");
      window.location.href = "admin.html";
    }
  }
  document.getElementById("closeModal1").click();

  e.preventDefault();
});
