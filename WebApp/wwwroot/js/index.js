window.onload = function () {
    console.log('onloaded')
    const colors = ["#f0f0f0", "#ffcccc", "#ccffcc", "#ccccff"];
    let index = 0;
    setInterval(() => {
        document.body.style.backgroundColor = colors[index];
        index = (index + 1) % colors.length;
    }, 1000);
}