const mainContent = document.getElementById("main");
const indexItems = document.getElementsByClassName("index-item");

function init() {
    Array.from(indexItems).forEach(element => {
        let title = element.querySelector(".item-title");
        let indent = title.dataset.indent;
        title.style.paddingLeft = `calc(0.5rem * ${indent})`


        //element.querySelector(".item-title").classList.remove("selected");
    });
}

async function fetchPage(url, id) {
    const el = document.getElementById(id);
    // For dynamic mode, fetch content via js.
    let res = await fetch(url);

    Array.from(indexItems).forEach(element => {
        element.querySelector(".item-title").classList.remove("selected");
    });

    el.querySelector(".item-title").classList.add("selected");

    //window.history.pushState({}, "test", url)
    mainContent.innerHTML = await res.text();
    // Highlight code snippets with prism.
    window.Prism.highlightAll();
}

function expand(id) {
    const el = document.getElementById(id);
    const icon = document.getElementById(`${id}-icon`);



    if (el.classList.contains ("expand")) {
        el.classList.remove("expand");
        el.querySelector(".children").classList.remove("show");
        icon.classList.remove("fa-chevron-down");
        icon.classList.add("fa-chevron-right");
    }
    else {
        el.classList.add("expand");
        icon.classList.remove("fa-chevron-right");
        el.querySelector(".children").classList.add("show");
        icon.classList.add("fa-chevron-down");
    }
}