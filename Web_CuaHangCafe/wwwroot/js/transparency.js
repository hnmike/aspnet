document.addEventListener('DOMContentLoaded', function () {
    function applyTransparency() {
        // Get the image element
        const image = document.querySelector('.blended-image');
        const container = document.querySelector('.image-container');
        const picture = document.querySelector('picture');

        if (image) {
            // Remove any background
            image.style.background = 'none';
            image.style.backgroundColor = 'transparent';

            // Set blend mode
            image.style.mixBlendMode = 'multiply';

            // Remove any filters
            image.style.filter = 'none';

            // Add other necessary styles
            image.style.maxWidth = '100%';
            image.style.height = 'auto';
            image.style.display = 'block';

            // Force browser to handle transparency correctly
            image.style.isolation = 'isolate';
            image.setAttribute('crossorigin', 'anonymous');
        }

        if (container) {
            container.style.background = 'transparent';
            container.style.backgroundColor = 'transparent';
            container.style.backdropFilter = 'none';
            container.style.webkitBackdropFilter = 'none';
        }

        if (picture) {
            picture.style.background = 'transparent';
            picture.style.backgroundColor = 'transparent';
        }
    }

    // Apply immediately
    applyTransparency();

    // Also apply when image loads
    const image = document.querySelector('.blended-image');
    if (image) {
        image.addEventListener('load', applyTransparency);
    }

    // Create an observer for dynamic content
    const observer = new MutationObserver((mutations) => {
        mutations.forEach((mutation) => {
            if (mutation.addedNodes.length) {
                applyTransparency();
            }
        });
    });

    // Start observing
    observer.observe(document.body, {
        childList: true,
        subtree: true
    });
});
