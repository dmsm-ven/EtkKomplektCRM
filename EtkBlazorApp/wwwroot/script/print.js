function printMainContent(content) {
    var newWin = window.open('', 'Print-Window');
    newWin.document.write(content);
    newWin.document.close();
    setTimeout(function () { newWin.close(); }, 10);
}