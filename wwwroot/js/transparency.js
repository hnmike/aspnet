document.addEventListener('DOMContentLoaded', function () {
    // Function to handle image transparency
    function makeImageTransparent(img) {
        if (img) {
            // Remove any background
            img.style.background = 'none';
            img.style.backgroundColor = 'transparent';

            // Set blend mode for transparency
            img.style.mixBlendMode = 'multiply';

            // Remove any filters that might affect transparency
            img.style.filter = 'none';

            // Ensure proper rendering
            img.style.isolation = 'isolate';

            // Add cross-origin attribute if needed
            img.setAttribute('crossorigin', 'anonymous');

            // Remove any backdrop filters
            img.style.backdropFilter = 'none';
            img.style.webkitBackdropFilter = 'none';
        }
    }

    // Handle all blended images
    const blendedImages = document.querySelectorAll('.blended-image');
    blendedImages.forEach(makeImageTransparent);

    // Handle image containers
    const imageContainers = document.querySelectorAll('.image-container');
    imageContainers.forEach(container => {
        container.style.background = 'transparent';
        container.style.backdropFilter = 'none';
    });

    // Create an observer for dynamically loaded images
    const observer = new MutationObserver((mutations) => {
        mutations.forEach((mutation) => {
            mutation.addedNodes.forEach((node) => {
                if (node.nodeType === 1 && node.matches('.blended-image')) {
                    makeImageTransparent(node);
                }
            });
        });
    });

    // Start observing the document for dynamic changes
    observer.observe(document.body, {
        childList: true,
        subtree: true
    });
});
