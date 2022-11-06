// Событие "DOMContentLoaded" подается тогда когда готов DOM
document.addEventListener("DOMContentLoaded", () => {
    // const app = document.getElementById("app");  // по ID <div id="app"></div>
    const app = document.querySelector("app");  // по имени тега <app></app>
    if (!app) throw "Forum  script: APP not found";
    // app.innerHTML = "APP will be here";
    loadTopics(app);
});

function loadTopics(elem) {
    fetch("/api/topic",  // API topics - get all
        {
            method: "GET",
            headers: {
                "User-Id": "",
                "Culture": ""
            },
            body: null
        })
        .then(r => r.json())
        .then(j => {
            if (j instanceof Array) {
                showTopics(elem, j);
            }
            else {
                throw "showTopics: Backend data invalid";
            }
        });
}

function showTopics(elem, j) {
    // elem.innerHTML = "topics will be here";
    /*
    for (let topic of j) {
        elem.innerHTML += `<div class='topic' data-id='${topic.id}'>
        <b>${topic.title}</b><i>${topic.description}</i></div>`;
    }
    */
    /* Так не делать - innerHTML должен меняться один раз
    elem.innerHTML = "<table border=1>";
    for (let topic of j) {
        elem.innerHTML += `<tr>
            <td>${topic.title}</td>
            <td>${topic.description}</td></tr>`;
    }
    elem.innerHTML += "</table>";
    */
    /*
    // 1. Формируем строку из скольки угодно частей, но один раз - HTML
    // 2. Желательно разделить разметку и данные, не смешивать
          шаблон HTML и имена переменных -- отдельно html, отдельно данные
    // 2.1. Это позволяет работать над разметкой отдельно
            (не заставляем дизайнеров "залезать" в код)
    // 2.2. А также вынести шаблон из кода и загрузить его AJAX

    var trTemplate = "<tr data-id='{{id}}'><td>*title</td><td>*descr</td></tr>";
    var appHtml = "<table border=1>";
    for (let topic of j) {
        appHtml +=
            trTemplate
                .replace("*title", topic.title)
                .replace("*descr", topic.description);
    }
    appHtml += "</table>";
    elem.innerHTML = appHtml;
    */

    // запрашиваем шаблон с сервера 
    // ~ var trTemplate = `...`;

    fetch("/templates/topic.html")
        .then(r => r.text())
        .then(trTemplate => {
            var appHtml = "";
            for (let topic of j) {  // topic - один объект из JSON
                let tpl = trTemplate;
                for (let prop in topic) {  // цикл по свойствам (ключам) объекта(id,title,description)
                    tpl = tpl.replaceAll(`{{${prop}}}`, topic[prop]);
                }
                appHtml += tpl;
                /*
                appHtml +=
                    trTemplate
                    .replaceAll("{{title}}", topic.title)
                    .replaceAll("{{description}}", topic.description)
                    .replaceAll("{{id}}", topic.id); */
            }
            elem.innerHTML = appHtml;
            topicLoaded();
        });
}
// Задача - по клику на Топик в консоли (или alert) выводится его id

async function topicLoaded() {
    for (let topic of document.querySelectorAll(".topic")) {
        topic.onclick = topicClick;
    }
}

function topicClick(e) {
    window.location = "/Forum/Topic/" +
        e.target.closest(".topic").getAttribute("data-id");
}