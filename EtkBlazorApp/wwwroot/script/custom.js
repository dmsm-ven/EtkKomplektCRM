function imageSizeValidation(inputFile) {
    let img = new Image()

    var uploadingFile = inputFile.files[0];

    img.src = window.URL.createObjectURL(uploadingFile)
    img.onload = () => {
        console.log(uploadingFile + " size : " + uploadingFile.size);
        console.log("res: " + img.width + "x" + img.height);
        return (img.width === 400 && img.height === 200);
    }
}

function getUserInfo() {
    return $.getJSON('https://api.db-ip.com/v2/free/self');
}