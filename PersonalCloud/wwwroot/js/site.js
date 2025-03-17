window.downloadFile = (url) => {
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = ''; // Let browser use the filename from URL
    document.body.appendChild(anchor);
    anchor.click();
    document.body.removeChild(anchor);
};
