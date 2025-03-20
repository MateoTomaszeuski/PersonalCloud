window.downloadFile = (url) => {
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = '';
    document.body.appendChild(anchor);
    anchor.click();
    document.body.removeChild(anchor);
};
window.initLazyLoading = () => {
    console.log("in lazyloading")
    const images = document.querySelectorAll('img.lazy');
    const videos = document.querySelectorAll('video.lazy-video');
    let imagesc = document.querySelectorAll('img.lazy');
    console.log("Lazy load images found:", images.length);

    if (imagesc.length === 0) {
        console.warn("No images found, retrying in 100ms...");
        setTimeout(window.initLazyLoading, 100);
    } else {
        console.log("Initializing lazy loading...");
    const observer = new IntersectionObserver((entries, obs) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const el = entry.target;

                // Handle image lazy loading
                if (el.tagName === 'IMG' && el.dataset.src) {
                    el.src = el.dataset.src;
                    el.classList.remove('lazy');
                    el.removeAttribute('data-src');
                }

                // Handle video lazy loading
                if (el.tagName === 'VIDEO') {
                    const source = el.querySelector('source');
                    if (source && source.dataset.src) {
                        source.src = source.dataset.src;
                        el.load();
                        el.classList.remove('lazy-video');
                        source.removeAttribute('data-src');
                    }
                }

                observer.unobserve(el);
            }
        });
    });

    images.forEach(img => observer.observe(img));
    videos.forEach(video => observer.observe(video));
}
};
