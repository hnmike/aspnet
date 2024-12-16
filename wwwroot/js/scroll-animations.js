document.addEventListener('DOMContentLoaded', function() {
    // Intersection Observer for all sections
    const sections = document.querySelectorAll('.sushi-section, .categories, .product.spad, .news, .banner');
    
    const sectionObserver = new IntersectionObserver((entries, observer) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('visible');
                
                // Special handling for product grid items
                if (entry.target.classList.contains('product')) {
                    const items = entry.target.querySelectorAll('.product-grid-item');
                    items.forEach((item, index) => {
                        setTimeout(() => {
                            item.style.opacity = '1';
                            item.style.transform = 'translateY(0)';
                        }, index * 100);
                    });
                }
            } else {
                // Only remove visible class when scrolling up for specific sections
                if (entry.target.classList.contains('categories') || 
                    entry.target.classList.contains('product') ||
                    entry.target.classList.contains('news') ||
                    entry.target.classList.contains('banner')) {
                    entry.target.classList.remove('visible');
                }
            }
        });
    }, {
        root: null,
        threshold: 0.2,
        rootMargin: '-50px'
    });

    sections.forEach(section => {
        sectionObserver.observe(section);
    });

    // Add scroll indicator to sushi section
    const sushiSection = document.querySelector('.sushi-section');
    if (sushiSection) {
        const scrollIndicator = document.createElement('div');
        scrollIndicator.className = 'scroll-indicator';
        scrollIndicator.innerHTML = `
            <svg width="30" height="30" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M7 13L12 18L17 13" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
                <path d="M7 7L12 12L17 7" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
            </svg>
        `;
        sushiSection.appendChild(scrollIndicator);
    }

    // Smooth scroll for anchor links
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();
            const targetId = this.getAttribute('href');
            if (targetId === '#') return;
            
            const targetElement = document.querySelector(targetId);
            if (targetElement) {
                targetElement.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            }
        });
    });
});
