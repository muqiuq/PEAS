step = 1;
var spinner_opts = {
    lines: 12, length: 31, width: 21, radius: 58,
    scale: 0.65, corners: 1, speed: 1.8, rotate: 0,
    animation: 'spinner-line-fade-quick', direction: 1,
    color: '#55eb23', fadeColor: 'transparent',
    top: '50%', left: '50%', shadow: '0 0 10px transparent',
    zIndex: 2000000000, className: 'spinner', position: 'absolute',
};

function nextstep() {
    if (step == 1) {
        loginstep1();
    } else if (step == 2) {
        loginstep2();
    }
}

function loginstep2() {
    document.getElementById("submit").setAttribute("disabled", "disabled");
    document.getElementById("otp").setAttribute("disabled", "disabled");

    var target = document.getElementById('alerts');
    var spinner = new Spin.Spinner(spinner_opts).spin(target);

    fetch('/api/login', {
        method: "POST", body: JSON.stringify(
            {
                "AppToken": document.getElementById("apptoken").value,
                "EMail": document.getElementById("username").value,
                "OTP": document.getElementById("otp").value,
                "ReqId": document.getElementById("reqid").value
            })
    })
        .then(response => {
            if (!response.ok) {
                throw new Error("Server error (" + response.status + ")", { cause: response });
            }
            return response;
        })
        .then(response => response.json())
        .then(json => {
            spinner.stop();
            if (json["Success"] && json["Token"] != null && json["Token"] != "") {
                showAlert("Login successfull. Redirecting...");
                setTimeout(() => {
                    location.href = json["RedirectUrl"];
                }, 2000);
                step = 3;
            } else {
                showAlert(json["Message"], false);
                enableUsernameInput();
            }
        })
        .catch(error => {
            console.error(error);
            spinner.stop();
            showAlert(error.message, false);
            enableUsernameInput();
        });
}

function loginstep1() {
    document.getElementById("username").setAttribute("disabled", "disabled");
    document.getElementById("submit").setAttribute("disabled", "disabled");

    

    var target = document.getElementById('alerts');
    var spinner = new Spin.Spinner(spinner_opts).spin(target);

    fetch('/api/login', {
        method: "POST", body: JSON.stringify(
            { "EMail": document.getElementById("username").value, "AppToken": document.getElementById("apptoken").value })
        })
        .then(response => {
            if (!response.ok) {
                throw new Error("Email address rejected (unauthorized)", { cause: response });
            }
            return response;
        })
        .then(response => response.json())
        .then(json => {
            spinner.stop();
            if (json["Success"]) {
                showAlert("Sent e-mail with one time password. Please check you inbox and enter the code below.");
                document.getElementById("otpdiv").style["visibility"] = "visible";
                document.getElementById("otp").removeAttribute("disabled");
                document.getElementById("otp").focus();
                document.getElementById("submit").innerText = "Login";
                document.getElementById("submit").removeAttribute("disabled");
                step = 2;
            } else {
                showAlert("Email address rejected (unauthorized or too many failed login attempts).", false);
                enableUsernameInput();
            }
        })
        .catch(error => {
            console.error(error);
            spinner.stop();
            showAlert(error.message, false);
            enableUsernameInput();
        });
}

function enableUsernameInput() {
    document.getElementById("username").removeAttribute("disabled");
    document.getElementById("username").focus();
    document.getElementById("submit").removeAttribute("disabled");
    document.getElementById("otpdiv").style["visibility"] = "hidden";
    document.getElementById("otp").value = "";
    step = 1;
}

function showAlert(message, success = true) {
    document.getElementById("alerts").replaceChildren([]);
    emailsentalert = document.createElement("div");
    emailsentalert.setAttribute("class", "alert " + (success ? "alert-success" : "alert-danger"));
    emailsentalert.innerHTML = message;
    document.getElementById("alerts").appendChild(emailsentalert);
}



window.addEventListener("DOMContentLoaded", (event) => {
    fields = [document.getElementById("username"), document.getElementById("otp")];
    fields.forEach(x => {
        x.addEventListener("keyup", function (event) {
            event.preventDefault();
            if (event.keyCode === 13) {
                document.getElementById("submit").click();
            }
        });
    });
});