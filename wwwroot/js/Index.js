/**
 * Bike Carousel Navigation Script
 * For THUEXEDAPHOIAN - Bike Rental Website
 */

document.addEventListener('DOMContentLoaded', function () {

    const tabButtons = document.querySelectorAll('.tab-button');
    const shopListings = document.querySelectorAll('.shop-listing');

    // Function to show tab content
    function showTab(tabId) {
        // First hide all shop listings
        shopListings.forEach(listing => {
            listing.style.display = 'none';
        });

        // Then show only the ones that belong to this tab
        // In a real app, we would filter based on data attributes
        // For now, we'll just show all of them
        shopListings.forEach((listing, index) => {
            // For demonstration purposes:
            // - "Gần tôi" tab shows all listings
            // - "Phổ biến" tab shows first and third listing
            // - "Đánh giá" tab shows second and fourth listing
            if (tabId === 'ganToi' ||
                (tabId === 'phoBien' && (index === 0 || index === 2)) ||
                (tabId === 'danhGia' && (index === 1 || index === 3))) {
                listing.style.display = 'flex';

                // Add a subtle animation
                listing.style.opacity = '0';
                listing.style.transform = 'translateY(10px)';

                setTimeout(() => {
                    listing.style.transition = 'opacity 0.3s ease, transform 0.3s ease';
                    listing.style.opacity = '1';
                    listing.style.transform = 'translateY(0)';
                }, 50 * index);
            }
        });
    }

    // Add click event to tab buttons
    tabButtons.forEach(button => {
        button.addEventListener('click', function () {
            // Remove active class from all tabs
            tabButtons.forEach(btn => btn.classList.remove('active'));

            // Add active class to clicked tab
            this.classList.add('active');

            // Show the corresponding tab content
            const tabId = this.getAttribute('data-tab');
            showTab(tabId);
        });
    });

    // Initialize with the first tab active
    showTab('ganToi');
    // Get carousel elements
    const carouselTrack = document.querySelector('.carousel-track');
    const prevBtn = document.querySelector('.prev-btn');
    const nextBtn = document.querySelector('.next-btn');
    const bikeCards = document.querySelectorAll('.bike-card');

    // Initialize carousel state
    let currentSlide = 0;
    let isDragging = false;
    let startPos = 0;
    let currentTranslate = 0;
    let previousTranslate = 0;

    // Function to determine number of slides visible based on screen width
    function getSlidesPerView() {
        if (window.innerWidth < 768) return 1;
        if (window.innerWidth < 1024) return 2;
        return 3;
    }

    // Calculate maximum slides
    function getMaxSlides() {
        return bikeCards.length - getSlidesPerView();
    }

    // Update carousel position
    function updateCarousel() {
        const slideWidth = 100 / getSlidesPerView();
        carouselTrack.style.transition = 'transform 0.3s ease-in-out';
        carouselTrack.style.transform = `translateX(-${currentSlide * slideWidth}%)`;

        // Update button states
        prevBtn.disabled = currentSlide === 0;
        prevBtn.classList.toggle('disabled', currentSlide === 0);

        nextBtn.disabled = currentSlide >= getMaxSlides();
        nextBtn.classList.toggle('disabled', currentSlide >= getMaxSlides());
    }

    // Go to previous slide
    function prevSlide() {
        if (currentSlide > 0) {
            currentSlide--;
            updateCarousel();
        }
    }

    // Go to next slide
    function nextSlide() {
        if (currentSlide < getMaxSlides()) {
            currentSlide++;
            updateCarousel();
        }
    }

    // Touch and mouse event handlers for dragging functionality
    

    function getPositionX(event) {
        return event.type.includes('mouse') ? event.pageX : event.touches[0].clientX;
    }

    function animation() {
        if (isDragging) {
            const slideWidth = 100 / getSlidesPerView();
            const translatePosition = currentTranslate - (currentSlide * slideWidth);
            carouselTrack.style.transform = `translateX(${translatePosition}%)`;
            requestAnimationFrame(animation);
        }
    }

    // Arrow key navigation
    function handleKeyDown(e) {
        if (e.key === 'ArrowLeft') {
            prevSlide();
        } else if (e.key === 'ArrowRight') {
            nextSlide();
        }
    }

    // Event listeners for button clicks
    prevBtn.addEventListener('click', prevSlide);
    nextBtn.addEventListener('click', nextSlide);

    // Event listeners for keyboard navigation
    document.addEventListener('keydown', handleKeyDown);

    // Event listeners for touch and mouse dragging
    carouselTrack.addEventListener('mousedown', touchStart);
    carouselTrack.addEventListener('touchstart', touchStart);

    carouselTrack.addEventListener('mousemove', touchMove);
    carouselTrack.addEventListener('touchmove', touchMove);

    carouselTrack.addEventListener('mouseup', touchEnd);
    carouselTrack.addEventListener('touchend', touchEnd);
    carouselTrack.addEventListener('mouseleave', () => {
        if (isDragging) {
            touchEnd();
        }
    });

    // Prevent context menu on right click for better dragging experience
    carouselTrack.addEventListener('contextmenu', e => {
        e.preventDefault();
        e.stopPropagation();
        return false;
    });

    // Event listener for window resize to adjust carousel
    window.addEventListener('resize', () => {
        if (currentSlide > getMaxSlides()) {
            currentSlide = getMaxSlides();
        }
        updateCarousel();
    });

    // Initialize carousel
    updateCarousel();

    // Start autoplay
    startAutoplay();
});