window.downloadFile = (url) => {
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = '';
    document.body.appendChild(anchor);
    anchor.click();
    document.body.removeChild(anchor);
};

