document.addEventListener("DOMContentLoaded", () => {
    const junk = document.querySelector("junk");
    if (!junk) throw "Container <junk> not found";
    junk.innerHTML = "Тут будут удаленные сообщения";

    fetch("/api/article?del=true")
        .then(r => r.json())
        .then(j => {
            // console.log(j);
            let html = "";
            const tpl = "<p data-id='{{id}}'>{{moment}} {{topic}} {{text}} &#128472; <ins>&#x21ED;</ins> </p>";
            for (let article of j) {                
                html += tpl
                    .replaceAll("{{moment}}", article.deleteMoment)
                    .replaceAll("{{topic}}", article.topic.title)
                    .replaceAll("{{text}}", article.text.substring(0, 15))
                    .replaceAll("{{id}}", article.id);
            }
            junk.innerHTML = html;
            if (typeof j[0] !== 'undefined') {
                junk.setAttribute("data-user-id", j[0].authorId);
            }
            onArticleLoaded();
        })
});

function onArticleLoaded() {
    for (let ins of document.querySelectorAll("p ins")) {
        ins.onclick = insClick;
    }
}

function insClick(e) {
    const uid  = e.target.closest('p').getAttribute('data-id');
    const junk = document.querySelector("junk");
    // console.log(uid);
    fetch("/api/article?uid=" + uid, {
        method: "PURGE",
        headers: {
            "User-Id": junk.getAttribute('data-user-id')
        }
    })  .then(r => r.json())
        .then(j => {
            console.log(j);
            if (j.message == "Ok") {  // успешно восстановлена
                // а) обновить страницу либо б) удалить(скрыть) блок с публикацией

            }
            else {  // ошибка восстановления (на бэке)
                alert(j.message);
            }
        });
}