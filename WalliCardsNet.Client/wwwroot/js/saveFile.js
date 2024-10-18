﻿function saveAsFile(filename, bytesBase64) {
    let link = document.createElement('a');
    link.download = filename;
    link.href = 'data:text/csv;base64,' + bytesBase64;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}