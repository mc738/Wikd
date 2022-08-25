const mainContent = document.getElementById("main");

async function fetchPage(url) {
    // For dynamic mode, fetch content via js.
    let res = await fetch(url);

    mainContent.innerHTML = await res.text();
    // Highlight code snippets with prism.
    window.Prism.highlightAll();
}

