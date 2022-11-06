// script for /Auth/Profile
document.addEventListener('DOMContentLoaded', () => {
    const userRealName = document.getElementById("userRealName");
    userRealName.onclick = editableClick;
    userRealName.onblur = e => {  // потеря фокуса - выход из элемента
        e.target.removeAttribute("contenteditable");
        // проверяем были ли изменения от сохраненного значения
        if (e.target.savedValue != e.target.innerText) {
            // если были - передаем изменения на сервер
            fetch("/Auth/ChangeRealName?NewName=" + e.target.innerText)
                .then(r => r.text())
                .then(t => alert(t));
        }
    };
    userRealName.onkeydown = editableKeydown;

    const userLogin = document.getElementById("userLogin");
    if (!userLogin) throw "userLogin not found in DOM";
    userLogin.onclick = editableClick;
    userLogin.onblur = userLoginBlur;
    userLogin.onkeydown = editableKeydown;

    const userEmail = document.getElementById("userEmail");
    if (!userEmail) throw "userEmail not found in DOM";
    userEmail.onclick = editableClick;
    userEmail.onblur = userEmailBlur;
    userEmail.onkeydown = editableKeydown;

    const avatar = document.getElementById("avatar");
    if (!avatar) throw "avatar not found in DOM";
    avatar.onchange = avatarChange;
});

function avatarChange(e) {
    // console.log(e.target.files);
    if (e.target.files.length > 0) {  // есть изменение файла
        const formData = new FormData();
        formData.append("userAvatar", e.target.files[0]);
        fetch("/Auth/ChangeAvatar", {
            method: "POST",
            body: formData
        }).then(r => r.json())
            .then(j => {
                if (j.status == "Ok") {
                    // нам приходит новое имя файла, заменяем на него источник <img id="userLogo">
                    userLogo.src = `/img/${j.message}`;

                    // TODO: обновить также аватар в заголовочной строке
                }
                else {
                    alert(j.message);
                }
            });
    }
}

function userEmailBlur(e) {
    e.target.removeAttribute("contenteditable");
    if (e.target.savedValue != e.target.innerText) {
        fetch("/Auth/ChangeEmail",
            {
                method: "PUT",
                headers: {
                    "Content-Type": "application/x-www-form-urlencoded"  // [FromForm]
                },
                body: `NewEmail=${e.target.innerText}`
            }
        ).then(r => r.json())
            .then(console.log);
    }
}

function editableClick(e) {
    // переводим поле в режим редактирования
    e.target.setAttribute("contenteditable", true);
    // запоминаем, что в нем было (чтобы потом проверить изменения были ли)
    e.target.savedValue = e.target.innerText;
}

function userLoginBlur(e) {
    e.target.removeAttribute("contenteditable");
    if (e.target.savedValue != e.target.innerText) {
        // alert(e.target.savedValue);
        fetch("/Auth/ChangeLogin",
            {
                method: "POST",
                headers: {
                    // "Content-Type": "application/x-www-form-urlencoded"  // [FromForm]
                    "Content-Type": "application/json"  // [FromBody]
                },
                body: JSON.stringify(e.target.innerText)
            }
        ).then(r => r.json())
            .then(console.log);
    }
}

function editableKeydown(e) {
    // console.log(e)
    if (e.key == "Enter") {
        e.preventDefault();
        e.target.blur();
    }
}